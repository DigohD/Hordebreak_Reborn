using System.Collections.Generic;
using FNZ.Shared.Model.Entity;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace FNZ.Client.Systems
{
    public struct LoS_Tag : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(RenderMeshSpawnerSystem))]
    public class LineOfSightSystem : SystemBase
    {
        private const int c_Width = 59;
        private const int c_Height = 59;
        private const int c_TopDownCullingThreshold = 22;
        private const int c_LeftRightCullingThreshold = 43;
        private const int c_VisitStackCapacity = 2500;
        private const int c_Iterations = 46;
        private const float c_ScaleFactor = 0.5f;
        
        private BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
        
        private Entity m_EntityPrefab;
        private int2 m_PrevPlayerTile;
        
        private Stack<int2> m_VisitStack1;
        private Stack<int2> m_VisitStack2;

        private NativeArray<Entity> m_Entities;

        private NativeArray<byte> m_Seeing;
        private NativeArray<byte> m_Revealed;
        private NativeArray<byte> m_WasVisited;
        
        private NativeArray<int2> m_RevealPrio;
        private NativeList<int2> m_Visited;
        
        private FNEEntity m_Player;
        
        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = GameClient.ECS_ClientWorld
                .GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
            
            var size = c_Width * c_Height;
            
            m_VisitStack1 = new Stack<int2>(c_VisitStackCapacity);
            m_VisitStack2 = new Stack<int2>(c_VisitStackCapacity);
            
            m_Entities = new NativeArray<Entity>(size, Allocator.Persistent);
            
            m_Seeing     = new NativeArray<byte>(size, Allocator.Persistent);
            m_Revealed   = new NativeArray<byte>(size, Allocator.Persistent);
            m_WasVisited = new NativeArray<byte>(size, Allocator.Persistent);
            
            m_RevealPrio = new NativeArray<int2>(size, Allocator.Persistent);
            m_Visited    = new NativeList<int2>(Allocator.Persistent);
            
            m_PrevPlayerTile = new int2(0, 0);
            
            var prefab = Resources.Load<GameObject>("Plane");
            var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
            var material = meshRenderer.sharedMaterial;

            var desc = new RenderMeshDescription(
                meshFilter.sharedMesh,
                material,
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false
            );

            var entityManager = GameClient.ECS_ClientWorld.EntityManager;
            m_EntityPrefab = entityManager.CreateEntity();

            RenderMeshUtility.AddComponents(
                m_EntityPrefab,
                entityManager,
                desc
            );
            
            entityManager.AddComponentData(m_EntityPrefab, new LoS_Tag());
            entityManager.AddComponentData(m_EntityPrefab, new LocalToWorld());
            entityManager.AddComponentData(m_EntityPrefab, new Translation());
            entityManager.AddComponentData(m_EntityPrefab, new Rotation());
            entityManager.AddComponentData(m_EntityPrefab, new NonUniformScale
            {
                Value = new float3(1.0f, 1.65f * (1.0f + c_ScaleFactor), 1.0f)
            });

            entityManager.Instantiate(m_EntityPrefab, m_Entities);
        }

        protected override void OnDestroy()
        {
            m_Visited.Dispose();
            m_Entities.Dispose();
            m_Seeing.Dispose();
            m_Revealed.Dispose();
            m_WasVisited.Dispose();
            m_RevealPrio.Dispose();
        }

        protected override void OnUpdate()
        {
            if (GameClient.LocalPlayerEntity == null)
                return;

            if (m_Player == null)
            {
                m_Player = GameClient.LocalPlayerEntity;
                m_PrevPlayerTile = new int2((int) m_Player.Position.x, (int) m_Player.Position.y);
            }
            
            var originX = (int) m_Player.Position.x;
            var originY = (int) m_Player.Position.y;
            
            CalculateSeeing(originX, originY);
            
            var positions = new NativeArray<float3>(c_Height * c_Width, Allocator.TempJob);

            for (var y = 0; y < c_Height; y++)
            {
                for (var x = 0; x < c_Width; x++)
                {
                    positions[x + y * c_Width] = new float3(
                        originX - (c_Width / 2) + x + 0.5f, 
                        1.65f * (1.0f - c_ScaleFactor), 
                        originY - (c_Width / 2) + y + 0.5f
                    );
                }
            }

            Dependency = new ActivateAndDeactiveJob
            {
                Height = c_Height,
                Width = c_Width,
                TopDownCullingThreshold = c_TopDownCullingThreshold,
                LeftRightCullingThreshold = c_LeftRightCullingThreshold,
                MidpointX = c_Width / 2,
                MidpointY = c_Height / 2,
                Entities = m_Entities,
                Positions = positions,
                Revealed = m_Revealed,
                Seeing = m_Seeing,
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.Schedule(positions.Length, 128, Dependency);
            
            CompleteDependency();
            
            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            
            UpdateFog();
        }

        private unsafe void CalculateSeeing(int originX, int originY)
        {
            UnsafeUtility.MemSet(m_Seeing.GetUnsafePtr(), 0, m_Seeing.Length);
            UnsafeUtility.MemSet(m_WasVisited.GetUnsafePtr(), 0, m_WasVisited.Length);

            m_Visited.Clear();

            var world = GameClient.World;

            var midPointX = c_Width / 2;
            var midPointY = c_Height / 2;

            m_WasVisited[midPointX + midPointY * c_Width] = 1;

            var revealPrioCounter = 0;
            
            for (var y = 0; y < c_Height; y++)
            {
                for (var x = 0; x < c_Width; x++)
                {
                    m_RevealPrio[revealPrioCounter] = new int2(x, y);
                    revealPrioCounter++;
                }
            }

            var tomp = m_RevealPrio[0];
            m_RevealPrio[0] = new int2(midPointX, midPointY);
            m_RevealPrio[midPointX + midPointY * c_Width] = tomp;
            
            var iterations = 0;
            var seen = 0;

            m_VisitStack1.Clear();
            m_VisitStack2.Clear();

            var toVisit = m_VisitStack1;
            toVisit.Push(new int2(originX, originY));

            var visitNext = m_VisitStack2;
            
            revealPrioCounter = 0;
            while (toVisit.Count > 0)
            {
                while (toVisit.Count > 0)
                {
                    revealPrioCounter++;
                    
                    var tilePos = toVisit.Pop();

                    var seeingIndexX = tilePos.x + midPointX - originX;
                    var seeingIndexY = tilePos.y + midPointY - originY;

                    if (seeingIndexX < 1 || seeingIndexX > c_Width - 2)
                        continue;

                    if (seeingIndexY < 1 || seeingIndexY > c_Height - 2)
                        continue;

                    seen++;
                    
                    m_RevealPrio[revealPrioCounter] = new int2(seeingIndexX, seeingIndexY);
                    m_Seeing[seeingIndexX + seeingIndexY * c_Width] = (byte)((iterations < c_Iterations) ? 1 : 0);

                    m_Visited.Add(tilePos);

                    var playerInt2 = new int2(originX, originY);
                    
                    var northDiff = new int2(tilePos.x, tilePos.y + 1) - playerInt2;

                    var traverseNorth = false;
                    var traverseEast = false;
                    if (northDiff.x + northDiff.y < c_TopDownCullingThreshold)
                    {
                        traverseNorth = world.IsTileNorthEdgeSeeThrough(tilePos, iterations);
                        traverseEast = world.IsTileEastEdgeSeeThrough(tilePos, iterations);
                    }
                    
                    var southDiff = new int2(tilePos.x, tilePos.y - 1) - playerInt2;

                    var traverseSouth = false;
                    var traverseWest = false;
                    
                    if (southDiff.x + southDiff.y > -c_TopDownCullingThreshold)
                    {
                        if (tilePos.y - 1 >= 0)
                            traverseSouth = world.IsTileSouthEdgeSeeThrough(tilePos, iterations);
                        
                        if (tilePos.x - 1 >= 0)
                            traverseWest = world.IsTileWestEdgeSeeThrough(tilePos, iterations);
                    }

                    if (traverseNorth && m_WasVisited[seeingIndexX + (seeingIndexY + 1) * c_Width] == 0)
                    {
                        m_WasVisited[seeingIndexX + (seeingIndexY + 1) * c_Width] = 1;
                        visitNext.Push(new int2(tilePos.x, tilePos.y + 1));
                    }

                    if (traverseEast && m_WasVisited[(seeingIndexX + 1) + seeingIndexY * c_Width] == 0)
                    {
                        m_WasVisited[(seeingIndexX + 1) + seeingIndexY * c_Width] = 1;
                        visitNext.Push(new int2(tilePos.x + 1, tilePos.y));
                    }

                    if (traverseSouth && m_WasVisited[seeingIndexX + (seeingIndexY - 1) * c_Width] == 0)
                    {
                        m_WasVisited[seeingIndexX + (seeingIndexY - 1) * c_Width] = 1;
                        visitNext.Push(new int2(tilePos.x, tilePos.y - 1));
                    }


                    if (traverseWest && m_WasVisited[(seeingIndexX - 1) + seeingIndexY * c_Width] == 0)
                    {
                        m_WasVisited[(seeingIndexX - 1) + seeingIndexY * c_Width] = 1;
                        visitNext.Push(new int2(tilePos.x - 1, tilePos.y));
                    }
                }

                iterations++;
                var tmp = toVisit;
                toVisit = visitNext;
                visitNext = tmp;
            }

            if (originX != m_PrevPlayerTile.x || originY != m_PrevPlayerTile.y)
            {
                var xDiff = originX - m_PrevPlayerTile.x;
                var yDiff = originY - m_PrevPlayerTile.y;

                m_PrevPlayerTile = new int2(originX, originY);
                
                if (math.abs(xDiff) > 1 || math.abs(yDiff) > 1)
                {
                    for (var y = 0; y < c_Height; y++)
                    {
                        for (var x = 0; x < c_Width; x++)
                        {
                            m_Revealed[x + y * c_Width] = m_Seeing[x + y * c_Width];
                        }
                    }
                }
                else
                {
                    if (yDiff >= 0)
                    {
                        for (var y = 1; y < c_Height - 1; y++)
                        {
                            if(xDiff >= 0)
                                for (var x = 1; x < c_Width - 1; x++)
                                {
                                    m_Revealed[(x - xDiff) + (y - yDiff) * c_Width] = m_Revealed[x + y * c_Width];
                                    m_RevealPrio[(x - xDiff) + (y - yDiff) * c_Width] = m_RevealPrio[x + y * c_Width];
                                }
                            else for (var x = c_Width - 2; x > 1; x--)
                            {
                                m_Revealed[(x + 1) + (y - yDiff) * c_Width] = m_Revealed[x + y * c_Width];
                                m_RevealPrio[(x + 1) + (y - yDiff) * c_Width] = m_RevealPrio[x + y * c_Width];
                            }
                        }
                    }
                    else
                    {
                        for (var y = c_Height - 2; y > 1; y--)
                        {
                            if(xDiff >= 0)
                                for (var x = 1; x < c_Width - 1; x++)
                                {
                                    m_Revealed[(x - xDiff) + (y + 1) * c_Width] = m_Revealed[x + y * c_Width];
                                    m_RevealPrio[(x - xDiff) + (y + 1) * c_Width] = m_RevealPrio[x + y * c_Width];
                                }
                            else for (var x = c_Width - 2; x > 1; x--)
                            {
                                m_Revealed[(x + 1) + (y + 1) * c_Width] = m_Revealed[x + y * c_Width];
                                m_RevealPrio[(x + 1) + (y + 1) * c_Width] = m_RevealPrio[x + y * c_Width];
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private struct ActivateAndDeactiveJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            [ReadOnly] public int Width;
            [ReadOnly] public int Height;
            [ReadOnly] public int TopDownCullingThreshold;
            [ReadOnly] public int LeftRightCullingThreshold;
            [ReadOnly] public int MidpointX;
            [ReadOnly] public int MidpointY;

            [ReadOnly] 
            public NativeArray<Entity> Entities;
            
            [ReadOnly, DeallocateOnJobCompletion] 
            public NativeArray<float3> Positions;
            
            [ReadOnly] 
            public NativeArray<byte> Seeing;
            
            public NativeArray<byte> Revealed;

            public void Execute(int index)
            {
                if (Seeing[index] == 0)
                    Revealed[index] = 0;

                var x = index % Width;
                var y = index / Height;

                var xDiff = x - MidpointX;
                var yDiff = y - MidpointY;

                if (xDiff + yDiff < -TopDownCullingThreshold || xDiff + yDiff > TopDownCullingThreshold)
                    return;

                if (-xDiff + yDiff > LeftRightCullingThreshold || xDiff - yDiff > LeftRightCullingThreshold)
                    return;
                
                var entity = Entities[index];
                
                if (Revealed[index] == 0)
                {
                    CommandBuffer.SetComponent(index, entity, new Translation
                    {
                        Value = Positions[index]
                    });
                }
                else
                {
                    CommandBuffer.SetComponent(index, entity, new Translation
                    {
                        Value = new float3(0, 100, 0)
                    });
                }
            }
        }

        private void UpdateFog()
        {
            var find = 0;
            for (var i = 0; i < c_Height * c_Width; i++)
            {
                var index = m_RevealPrio[i];
                if (m_Seeing[index.x + index.y * c_Width] != m_Revealed[index.x + index.y * c_Width])
                {
                    m_Revealed[index.x + index.y * c_Width] = m_Seeing[index.x + index.y * c_Width];
                    find++;
                    if (find > 10)
                    {
                        return;
                    }
                }
            }
        }
    }
}