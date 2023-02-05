using System;
using System.Diagnostics;
using FNZ.Server.FarNorthZMigrationStuff;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace FNZ.Server.Controller.Systems
{
    public struct HeapNode : IEquatable<HeapNode>, IComparable<HeapNode>
    {
        public byte GridCost;
        public int GridX;
        public int GridY;
        public int HeapIndex;
        public int ParentHeapIndex;
        public float PathDistanceCost;
        public float2 GoalDirection;

        public float TotalCost => PathDistanceCost + GridCost;

        public bool Equals(HeapNode other)
            => (GridX == other.GridX && GridY == other.GridY);
        
        public int CompareTo(HeapNode other)
        {
            var compare = TotalCost.CompareTo(other.TotalCost);

            if (compare == 0)
                compare = PathDistanceCost.CompareTo(other.PathDistanceCost);

            return -compare;
        }
    }

    [NativeContainerSupportsDeallocateOnJobCompletion]
    [NativeContainer]
    public struct NativeHeap : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private unsafe void* m_Buffer;
        private int m_Length;
        private int m_MinIndex;
        private int m_MaxIndex;
        //private AtomicSafetyHandle m_Safety;
        private Allocator m_AllocatorLabel;
        
        // [NativeSetClassTypeToNullOnSchedule]
        // private DisposeSentinel m_DisposeSentinel;
        private static int s_StaticSafetyId;
        
        private int m_CurrentItemCount;
        
        public int Length => m_Length;
        public int Count => m_CurrentItemCount; 

        public unsafe NativeHeap(int capacity, Allocator allocator)
        {
            Allocate(capacity, allocator, out this);
            UnsafeUtility.MemClear(m_Buffer, m_Length * (long) UnsafeUtility.SizeOf<HeapNode>());
            m_CurrentItemCount = 0;
        }
        
        private static unsafe void Allocate(int length, Allocator allocator, out NativeHeap array)
        {
            var num = UnsafeUtility.SizeOf<HeapNode>() * (long) length;

            array = new NativeHeap
            {
                m_Buffer = UnsafeUtility.Malloc(num, UnsafeUtility.AlignOf<HeapNode>(), allocator),
                m_Length = length,
                m_AllocatorLabel = allocator,
                m_MinIndex = 0,
                m_MaxIndex = length - 1
            };
            
            // DisposeSentinel.Create(out array.m_Safety, out array.m_DisposeSentinel, 1, allocator);
            // InitStaticSafetyId(ref array.m_Safety);
        }

        private unsafe HeapNode this[int index]
        {
            get => UnsafeUtility.ReadArrayElement<HeapNode>(m_Buffer, index);
            [WriteAccessRequired] set => UnsafeUtility.WriteArrayElement(m_Buffer, index, value);
        }
        
        // [BurstDiscard]
        // private static void InitStaticSafetyId(ref AtomicSafetyHandle handle)
        // {
        //     if (s_StaticSafetyId == 0)
        //         s_StaticSafetyId = AtomicSafetyHandle.NewStaticSafetyId<NativeHeap>();
        //     AtomicSafetyHandle.SetStaticSafetyId(ref handle, s_StaticSafetyId);
        // }

        public void Add(ref HeapNode item)
        {
            item.HeapIndex = m_CurrentItemCount;
            this[m_CurrentItemCount] = item;
            SortUp(item);
            m_CurrentItemCount++;
        }
        
        public HeapNode RemoveFirst()
        {
            var firstItem = this[0];
            m_CurrentItemCount--;
            this[0] = this[m_CurrentItemCount];
            var currentItem = this[0];
            currentItem.HeapIndex = 0;
            SortDown(ref firstItem);
            return firstItem;
        }
        
        public void UpdateItem(ref HeapNode item)
        {
            SortUp(item);
        }
        
        public bool Contains(HeapNode item)
        {
            for (var i = 0; i < m_CurrentItemCount; i++)
            {
                if (this[i].Equals(item))
                    return true;
            }

            return false;
        }
        
        private void SortUp(HeapNode item)
        {
            var parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                var parentItem = this[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }
        
        private void SortDown(ref HeapNode item)
        {
            while (true)
            {
                var childIndexLeft = item.HeapIndex * 2 + 1;
                var childIndexRight = item.HeapIndex * 2 + 2;
                var swapIndex = 0;

                if (childIndexLeft < m_CurrentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < m_CurrentItemCount)
                    {
                        if (this[childIndexLeft].CompareTo(this[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(this[swapIndex]) < 0)
                    {
                        Swap(item, this[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        
        private void Swap(HeapNode item1, HeapNode item2)
        {
            this[item1.HeapIndex] = item2;
            this[item2.HeapIndex] = item1;

            var item1Index = item1.HeapIndex;
            item1.HeapIndex = item2.HeapIndex;
            item2.HeapIndex = item1Index;
        }
        
        [WriteAccessRequired]
        public unsafe void Dispose()
        {
            if ((IntPtr) m_Buffer == IntPtr.Zero)
                throw new ObjectDisposedException("The NativeHeap is already disposed.");
            if (m_AllocatorLabel == Allocator.Invalid)
                throw new InvalidOperationException("The NativeHeap can not be Disposed because it was not allocated with a valid allocator.");
            if (m_AllocatorLabel > Allocator.None)
            {
                //DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
                UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
                m_AllocatorLabel = Allocator.Invalid;
            }
            m_Buffer = (void*) null;
            m_Length = 0;
        }
    }
    
    public struct FlowFieldRequestData
    {
        public float2 SourcePosition;
        public int Range;
    }
    
    //[DisableAutoCreation]
    // public class FlowFieldSystem : SystemBase
    // {
    //     private NativeQueue<FlowFieldRequestData> m_FlowFieldRequestQueue;
    //
    //     protected override void OnCreate()
    //     {
    //         m_FlowFieldRequestQueue = new NativeQueue<FlowFieldRequestData>(Allocator.Persistent);
    //     }
    //
    //     protected override void OnDestroy()
    //     {
    //         m_FlowFieldRequestQueue.Dispose();
    //     }
    //
    //     protected override void OnUpdate()
    //     {
    //         if (m_FlowFieldRequestQueue.Count <= 0) return;
    //
    //         var requestData = m_FlowFieldRequestQueue.Dequeue();
    //
    //         var sourcePosition = requestData.SourcePosition;
    //         var radius = requestData.Range;
    //         
    //         var tileWorldX = (int)sourcePosition.x;
    //         var tileWorldY = (int)sourcePosition.y;
    //
    //         var worldStartX = tileWorldX - radius;
    //         var worldStartY = tileWorldY - radius;
    //
    //         if (worldStartX < 0) worldStartX = 0;
    //         if (worldStartY < 0) worldStartY = 0;
    //
    //         var gridSizeX = radius * 2;
    //         var gridSizeY = radius * 2;
    //
    //         if (worldStartX + gridSizeX > GameServer.MainWorld.WIDTH) gridSizeX = GameServer.MainWorld.WIDTH - worldStartX;
    //         if (worldStartY + gridSizeY > GameServer.MainWorld.HEIGHT) gridSizeY = GameServer.MainWorld.HEIGHT - worldStartY;
    //
    //         var gridAreea = gridSizeX * gridSizeY;
    //
    //         var graph = new NativeArray<HeapNode>(gridAreea, Allocator.Temp);
    //         var vectorField = new NativeArray<float2>(gridAreea, Allocator.Temp);
    //         var openSet = new NativeHeap(gridAreea, Allocator.Temp);
    //         
    //         // Generate Distance Field
    //         for (var y = worldStartY; y < worldStartY + gridSizeY; y++)
    //         {
    //             for (var x = worldStartX; x < worldStartX + gridSizeX; x++)
    //             {
    //                 var gridX = x - worldStartX;
    //                 var gridY = y - worldStartY;
    //                 
    //                 var item = new HeapNode
    //                 {
    //                     GridX = gridX,
    //                     GridY = gridY,
    //                     GridCost = 1,
    //                     PathDistanceCost = 65535,
    //                     ParentHeapIndex = -1
    //                 };
    //
    //                 graph[gridX + gridY * gridSizeX] = item;
    //
    //                 openSet.Add(ref item);
    //             }
    //         }
    //         
    //         var goalX = (int)(sourcePosition.x) - worldStartX;
    //         var goalY = (int)(sourcePosition.y) - worldStartY;
    //
    //         var startNode = graph[goalX + goalY * gridSizeX];
    //         startNode.PathDistanceCost = 0;
    //         openSet.UpdateItem(ref startNode);
    //
    //         while (openSet.Count > 0)
    //         {
    //             var currentNode = openSet.RemoveFirst();
    //             
    //             var currentTile = new int2(currentNode.GridX, currentNode.GridY);
    //             var x = currentTile.x;
    //             var y = currentTile.y;
    //
    //             var neighbours = new FixedList32<int2>
    //             {
    //                 new int2(x, y - 1), //South
    //                 new int2(x - 1, y), //West
    //                 new int2(x, y + 1), //North
    //                 new int2(x + 1, y)  //East
    //             };
    //
    //             for (var i = 0; i < neighbours.Length; i++)
    //             {
    //                 var neighbour = neighbours[i];
    //                 
    //             }
    //             
    //         }
    //         
    //         // Generate Vector Field
    //
    //         var result = vectorField.ToArray();
    //
    //         graph.Dispose();
    //         vectorField.Dispose();
    //         openSet.Dispose();
    //     }

        // private struct GenerateFlowFieldJob : IJob
        // {
        //     public NativeArray<HeapNode> Graph;
        //     public NativeArray<float2> VectorField;
        //     public NativeHeap OpenSet;
        //     
        //     public void Execute()
        //     {
        //         
        //     }
        // }
        //
        // public void RequestFlowField(FlowFieldRequestData data)
        // {
        //     m_FlowFieldRequestQueue.Enqueue(data);
        // }
    // }
}