using FNZ.Client.Model.Entity.Components.AI;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.Model.Entity.Components.Excavatable;
using FNZ.Client.Model.Entity.Components.Name;
using FNZ.Client.Model.Entity.Components.Polygon;
using FNZ.Client.Model.Entity.Components.TileObject;
using FNZ.Client.Model.World;
using FNZ.Client.StaticData;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.Model
{
	public class ClientEntityFactory
	{
		private Dictionary<Type, Type> m_SharedCompToClientCompDictionary = new Dictionary<Type, Type>();

		private ClientWorld m_World;

		public ClientEntityFactory(ClientWorld world)
		{
			m_World = world;
			BuildTable();
		}

		public FNEEntity CreateEntity(string id)
		{
			var entity = new FNEEntity();
			entity.Init(id, new float2());

			var componentsFromXML = DataBank.Instance.GetData<FNEEntityData>(entity.EntityId).components;

			foreach (var compData in componentsFromXML)
			{
				entity.AddComponent(m_SharedCompToClientCompDictionary[compData.GetComponentType()], compData);
			}

			foreach (var comp in entity.Components)
			{
				comp.InitComponentLinks();
			}

			return entity;
		}

		public FNEEntity CreateTileObject(string id)
		{
			var entity = CreateEntity(id);

			var tileObjectComp = entity.AddComponent<TileObjectComponentClient>();
			tileObjectComp.cost = entity.Data.pathingCost;
			tileObjectComp.seeThrough = entity.Data.seeThrough;
			tileObjectComp.hittable = entity.Data.hittable;

			if (entity.Data.blocking || entity.Data.hittable || entity.HasComponent<ExcavatableComponentClient>())
			{
				PolygonComponentClient polyComp = entity.AddComponent<PolygonComponentClient>();
				if (entity.Data.smallCollisionBox)
				{
					polyComp.relativePoints.Add(new Vector2(-0.25f, -0.25f));
					polyComp.relativePoints.Add(new Vector2(-0.25f, 0.25f));
					polyComp.relativePoints.Add(new Vector2(0.25f, 0.25f));
					polyComp.relativePoints.Add(new Vector2(0.25f, -0.25f));
					polyComp.offsetToMiddle = new Vector2(0.5f, 0.5f);
				}
				else
				{
					polyComp.relativePoints.Add(new Vector2(-0.5f, -0.5f));
					polyComp.relativePoints.Add(new Vector2(-0.5f, 0.5f));
					polyComp.relativePoints.Add(new Vector2(0.5f, 0.5f));
					polyComp.relativePoints.Add(new Vector2(0.5f, -0.5f));
					polyComp.offsetToMiddle = new Vector2(0.5f, 0.5f);
				}
			}

			return entity;
		}

		public FNEEntity CreateEdgeObject(string id)
		{
			var entity = CreateEntity(id);
			entity.Scale = new float3(1, 1, 1);

			var eoc = entity.AddComponent<EdgeObjectComponentClient>();

			bool isPassable = true;
			if (entity.Data != null)
			{
				isPassable = entity.Data.pathingCost == 0; //this is a bit strange. I am not sure how to handle weather the wall should have a hitbox or not.
			}
			eoc.IsPassable = isPassable;
			eoc.IsDestroyed = false;
			eoc.IsHittable = entity.Data.hittable;

			if (!eoc.IsPassable)
			{
				//Test for polygon colliders
				var polyComp = entity.AddComponent<PolygonComponentClient>();
				polyComp.relativePoints.Add(new Vector2(-0.5f, -0.2f));
				polyComp.relativePoints.Add(new Vector2(-0.5f, 0.2f));
				polyComp.relativePoints.Add(new Vector2(0.5f, 0.2f));
				polyComp.relativePoints.Add(new Vector2(0.5f, -0.2f));
			}

			return entity;
		}

		public FNEEntity CreateEnemy(string entityId, int netId, float2 position, float rotation)
        {
			var entity = CreateEntity(entityId);

			entity.NetId = netId;
			entity.Position = position;
			entity.RotationDegrees = rotation;

			if (entity.EntityType == EntityType.GO_ENEMY)
			{
				InitGOEnemy(entity);
			}

			return entity;
		}

		private void InitGOEnemy(FNEEntity entity)
		{
			entity.AddComponent<AIComponentClient>();
		}

		public FNEEntity CreateLocalPlayerEntity(string id)
		{
			var localPlayer = new FNEEntity();

			localPlayer.Init(id, new float2());

			localPlayer.AddComponent<NameComponentClient>();
			localPlayer.GetComponent<NameComponentClient>().entityName = NetData.LOCAL_PLAYER_NAME;

			var componentsFromXML = DataBank.Instance.GetData<FNEEntityData>(localPlayer.EntityId).components;

			foreach (var compData in componentsFromXML)
			{
				var comp = localPlayer.AddComponent(m_SharedCompToClientCompDictionary[compData.GetComponentType()], compData);
			}

			foreach (var comp in localPlayer.Components)
			{
				comp.InitComponentLinks();
			}

			return localPlayer;
		}

		public FNEEntity CreateRemotePlayerEntity(string id)
		{
			var remotePlayer = new FNEEntity();

			remotePlayer.Init("player", new float2());

			var componentsFromXML = DataBank.Instance.GetData<FNEEntityData>(remotePlayer.EntityId).components;

			remotePlayer.AddComponent<NameComponentClient>();

			foreach (var compData in componentsFromXML)
			{
				var comp = remotePlayer.AddComponent(m_SharedCompToClientCompDictionary[compData.GetComponentType()], compData);
			}

			return remotePlayer;
		}

		public void DestroyEntity(FNEEntity entityToDestroy)
		{
			DestroyEntityView(entityToDestroy);
			entityToDestroy.Enabled = false;

			if (entityToDestroy.EntityType == EntityType.TILE_OBJECT)
			{
				GameClient.World.RemoveTileObject(entityToDestroy);
			}
			else if (entityToDestroy.EntityType == EntityType.EDGE_OBJECT)
			{
				GameClient.World.RemoveEdgeObject(entityToDestroy);
			}
			else if (entityToDestroy.EntityType == EntityType.GO_ENEMY || entityToDestroy.EntityType == EntityType.ECS_ENEMY)
			{
				var currentChunkCell = GameClient.World.GetChunkCellData((int)entityToDestroy.Position.x, (int)entityToDestroy.Position.y);

				if (currentChunkCell != null)
				{
					currentChunkCell.RemoveEnemy(entityToDestroy);
				}
			}

			GameClient.NetConnector.UnsyncEntity(entityToDestroy);
			//m_EntityPool.ReturnEntityToPool(entityToDestroy);
		}

		public void ReplaceEntityView(FNEEntity entityToReplace, string newViewRef)
		{
			DestroyEntityView(entityToReplace);
			GameClient.ViewAPI.QueueViewForSpawn(entityToReplace, newViewRef);
		}

		private void DestroyEntityView(FNEEntity entityToDestroy)
		{
			var entityGameObject = GameClient.ViewConnector.PopGameObjectView(entityToDestroy.NetId);
			if (entityGameObject != null)
			{
				GameClient.WorldView.RemoveGameObject(entityGameObject);
				GameClient.ViewAPI.QueueGameObjectForDeactivation(entityGameObject);
				var subViewGameObject = GameClient.ViewConnector.PopSubViewGameObjectView(entityToDestroy.NetId);
				if (subViewGameObject != null)
				{
					GameClient.WorldView.RemoveGameObject(entityGameObject);
					GameClient.ViewAPI.QueueGameObjectForDeactivation(entityGameObject);
				}
			}
			else
			{
				var entity = GameClient.ViewConnector.PopEntityView(entityToDestroy);
				if (entity != default)
				{
                    if (GameClient.WorldView != null)
						GameClient.WorldView.RemoveEntity(entity);
                    else
                        Debug.LogError("Chunkview was null when removing entity view from it!");

                    GameClient.ECS_ClientWorld.EntityManager.DestroyEntity(entity);
				}
				else
				{
					Debug.LogError("Tried to destroy view: " + entityToDestroy.EntityId + " but none was found in viewconnector");
				}
			}
		}

		private void BuildTable()
		{
			var allSharedTypes = typeof(DataComponent).Assembly.GetTypes();
			var allSharedComponenttypes = allSharedTypes.Where(t =>
				t.IsSubclassOf(typeof(FNEComponent))
			).ToList();

			foreach (var type in allSharedComponenttypes)
			{
				AddClientComponentType(type);
			}
		}

		private void AddClientComponentType(Type dataType)
		{
			var allClientTypes = typeof(ClientEntityFactory).Assembly.GetTypes();
			var matchingComponent = allClientTypes.Where(t =>
				{
					return t.IsSubclassOf(dataType);
				}
			).ToList();

			if (matchingComponent.Count == 0)
			{
				Debug.LogError("WARNING: " + dataType + " Does not have a matching client component!");
				return;
			}
			else if (matchingComponent.Count > 1)
			{
				Debug.LogError("WARNING: " + dataType + " Has more than one matching client component!");
				return;
			}

			m_SharedCompToClientCompDictionary.Add(dataType, matchingComponent.First());
		}
	}
}

