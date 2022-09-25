using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.FarNorthZMigrationStuff;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientEntityNetworkManager : INetworkManager
	{
		public ClientEntityNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.SPAWN_ENTITY, OnEntitySpawn);
			GameClient.NetConnector.Register(NetMessageType.SPAWN_ENTITY_BATCH, OnEntitySpawnBatch);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_ENTITY, OnEntityUpdate);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_COMPONENT, OnComponentUpdate);
			GameClient.NetConnector.Register(NetMessageType.COMPONENT_NET_EVENT, OnComponentNetEvent);
			GameClient.NetConnector.Register(NetMessageType.DESTROY_ENTITY, OnEntityDestroy);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_COMPONENT_BATCH, OnComponentUpdateBatch);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_ENTITY_POS_AND_ROT, OnUpdateEntityPosAndRot);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_ENTITY_POS_AND_ROT_BATCH, OnUpdateEntityPosAndRotBatch);
		}

		private void OnEntitySpawn(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			int entityNetId = incMsg.ReadInt32();

			string entityId = IdTranslator.Instance.GetId<FNEEntityData>(incMsg.ReadUInt16());

			float2 pos = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
			float rot = incMsg.ReadFloat();

			string entityType = DataBank.Instance.GetData<FNEEntityData>(entityId).entityType;

			FNEEntity spawned = null;
			if (entityType == EntityType.TILE_OBJECT)
				spawned = GameClient.EntityFactory.CreateTileObject(entityId);
			else if (entityType == EntityType.EDGE_OBJECT)
				spawned = GameClient.EntityFactory.CreateEdgeObject(entityId);

			if (spawned == null)
			{
				Debug.LogError("COULD NOT SPAWN ENTITY ON CLIENT AS REQUESTED BY SERVER!");
				return;
			}

			spawned.Position = pos;
			spawned.RotationDegrees = rot;

			spawned.NetId = entityNetId;
			GameClient.NetConnector.SyncEntity(spawned);

			foreach (var component in spawned.Components)
			{
				component.Deserialize(incMsg);
			}

			if (entityType == EntityType.TILE_OBJECT)
			{
				GameClient.World.AddTileObject(spawned);
			}
			else if (entityType == EntityType.EDGE_OBJECT)
			{
				GameClient.World.AddEdgeObject(spawned);
			}

			var viewRef = FNEEntity.GetEntityViewVariationId(spawned.Data, spawned.Position);
			GameClient.ViewAPI.QueueViewForSpawn(spawned, viewRef);
		}

		private void OnEntitySpawnBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			ushort entitiesAmount = incMsg.ReadUInt16();
			int entityNetId;
			string entityId;
			float2 pos;
			float rot;
			string entityType;

			for (int i = 0; i < entitiesAmount; i++)
			{
				entityNetId = incMsg.ReadInt32();
				entityId = IdTranslator.Instance.GetId<FNEEntityData>(incMsg.ReadUInt16());

				pos = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
				rot = incMsg.ReadFloat();

				entityType = DataBank.Instance.GetData<FNEEntityData>(entityId).entityType;

				FNEEntity spawned = null;
				if (entityType == EntityType.TILE_OBJECT)
				{
					spawned = GameClient.EntityFactory.CreateTileObject(entityId);
				}
				else if (entityType == EntityType.EDGE_OBJECT)
				{
					spawned = GameClient.EntityFactory.CreateEdgeObject(entityId);
				}

				if (spawned == null)
				{
					Debug.LogError("COULD NOT SPAWN ENTITY ON CLIENT AS REQUESTED BY SERVER!");
					return;
				}

				spawned.Position = pos;
				spawned.RotationDegrees = rot;

				spawned.NetId = entityNetId;
				GameClient.NetConnector.SyncEntity(spawned);

				foreach (var component in spawned.Components)
				{
					component.Deserialize(incMsg);
				}

				if (entityType == EntityType.TILE_OBJECT)
				{
					GameClient.World.AddTileObject(spawned);
				}
				else if (entityType == EntityType.EDGE_OBJECT)
				{
					GameClient.World.AddEdgeObject(spawned);
				}

				var viewRef = FNEEntity.GetEntityViewVariationId(spawned.Data, spawned.Position);
				GameClient.ViewAPI.QueueViewForSpawn(spawned, viewRef);
			}
		}

		private void OnEntityUpdate(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var entity = net.GetEntity(incMsg.ReadInt32());

			if (entity == null)
			{
				Debug.LogError("RECEIVED COMPONENT UPDATE FOR NULL ENTITY");
				return;
			}

			foreach (var component in entity.Components)
			{
				component.Deserialize(incMsg);
			}

		}

		private void OnUpdateEntityPosAndRot(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var entity = net.GetEntity(incMsg.ReadInt32());

			entity.Position.x = incMsg.ReadFloat();
			entity.Position.y = incMsg.ReadFloat();
			entity.RotationDegrees = incMsg.ReadFloat();
		}

		private void OnUpdateEntityPosAndRotBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity;

			var amount = incMsg.ReadUInt16();
			for (int i = 0; i < amount; i++)
			{
				entity = net.GetEntity(incMsg.ReadInt32());

				var prevPos = new int2((int)entity.Position.x, (int)entity.Position.y);

				entity.Position.x = incMsg.ReadFloat();
				entity.Position.y = incMsg.ReadFloat();
				entity.RotationDegrees = -incMsg.ReadFloat(); //this is negative due to client side rotations being flipped.

				if (prevPos.x != (int)entity.Position.x || prevPos.y != (int)entity.Position.y)
				{
					GameClient.World.GetTileEnemies(prevPos).Remove(entity);
					GameClient.World.AddEnemyToTile(entity);
				}

				var go = GameClient.ViewConnector.GetGameObject(entity);
				if (go != null)
				{
					var comp = go.GetComponent<DEV_EnemyAI_DEV>();
					if (comp != null)
					{
						comp.NewTargetPos(entity.Position, entity.RotationDegrees);
					}
				}
			}

		}

		private void OnComponentUpdate(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var netId = incMsg.ReadInt32();
			if (netId < 0) return;
			FNEEntity entity = net.GetEntity(netId);
			Type compType = FNEComponent.ComponentIdTypeDict[incMsg.ReadUInt16()];

			if (entity == null)
			{
				//Debug.LogWarning("RECEIVED COMPONENT UPDATE FOR NULL ENTITY. ID: " + netId + " COMP: " + compType.ToString());
				return;
			}

			foreach (var component in entity.Components)
			{
				if (component.GetType().BaseType == compType)
				{
					component.Deserialize(incMsg);
					if (entity.EntityType == EntityType.EDGE_OBJECT)
					{
						entity.GetComponent<EdgeObjectComponentClient>().UpdateMountedView();
					}
					break;
				}
			}
		}

		private void OnComponentUpdateBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			ushort componentsAmount = incMsg.ReadUInt16();

			for (int i = 0; i < componentsAmount; i++)
			{
				var parentID = incMsg.ReadInt32();
				if (parentID <= 0)
					UnityEngine.Debug.LogError($"NetId:{parentID}");
				var parentEntity = net.GetEntity(parentID);

				var componentID = incMsg.ReadUInt16();
				var componentType = FNEComponent.ComponentIdTypeDict[componentID];

				//On spawn-in, 1-2 (on rare occasion: 3) Cropcomponent's parentEntities are null.
				//This is because their value in net.GetEntity()'s NetEntityList "list" is null the first frame because there has been no syncing going on yet.
				if (parentEntity == null)
				{
					//Debug.LogWarning($"Warning: EntityComponentBatch entity loop no: {i} had a null ParentEntity." +
					//	$"Entities no: {i + 1} to {componentsAmount} was discarded.");
					break;
				}

				foreach (var comp in parentEntity.Components)
				{
					if (comp.GetType().BaseType == componentType)
					{
						comp.Deserialize(incMsg);
						break;
					}
				}
			}
		}

		private void OnComponentNetEvent(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity = net.GetEntity(incMsg.ReadInt32());
			Type compType = FNEComponent.ComponentIdTypeDict[incMsg.ReadUInt16()];

			foreach (var component in entity.Components)
			{
				if (component.GetType().BaseType == compType)
				{
					component.OnNetEvent(incMsg);
					break;
				}
			}
		}

		private void OnEntityDestroy(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var fneEntity = net.GetEntity(incMsg.ReadInt32());
			if (fneEntity == null)
			{
				Debug.LogWarning("Entity to delete was null!");
				return;
			}
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();

			if (playerComp.OpenedInteractable != null && playerComp.OpenedInteractable.ParentEntity == fneEntity)
				playerComp.SetOpenedInteractable(null);

			if (fneEntity.EntityType == EntityType.ECS_ENEMY)
			{
				GameClient.ECS_ClientWorld.GetExistingSystem<DestroyEntitySystem>().QueueEntityForDestroy(fneEntity.NetId);
			}
			else
			{
				GameClient.EntityFactory.DestroyEntity(fneEntity);
			}
		}
	}
}