using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Manager 
{
	public class SubViewAPI
	{
		public void QueueSubViewForSpawn(FNEEntity parentEntity, string viewDataRef, string subViewType)
		{
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewDataRef);

			if (viewData.viewIsGameObject)
			{
				var oppositeDir = parentEntity.GetComponent<EdgeObjectComponentClient>().OppositeMountedDirection;
				
				var rot = 0;
				
				// Vertical wall
				if (parentEntity.Position.x % 1 == 0)
				{
					if (!oppositeDir)
						rot = 90;
					else
						rot = 270;
				}
				// Horizontal wall
				else
				{
					if (!oppositeDir)
						rot = 0;
					else
						rot = 180;
				}
				
				QueueGameObjectSubViewForSpawn(
					parentEntity, 
					viewData.Id,
					"MountedObject",
					viewData,
					new Vector3(parentEntity.Position.x, 0, parentEntity.Position.y),
					rot
				);
			}
			else
			{
				float x = parentEntity.Position.x;
				float y = -viewData.heightPos;
				float z = parentEntity.Position.y;
				
				float3 position = new float3(x, y, z);

				var oppositeDir = parentEntity.GetComponent<EdgeObjectComponentClient>().OppositeMountedDirection;
				var rot = 0;
				// Vertical wall
				if (parentEntity.Position.x % 1 == 0)
				{
					if (!oppositeDir)
						rot = 90;
					else
						rot = 270;
				}
				// Horizontal wall
				else
				{
					if (!oppositeDir)
						rot = 0;
					else
						rot = 180;
				}
				
				Quaternion quat  = Quaternion.AngleAxis(rot, Vector3.up);

				QueueEntitySubViewForSpawn(viewData, position, quat, Vector3.one * viewData.scaleMod, parentEntity.NetId);
			}
		}
		
		private void QueueGameObjectSubViewForSpawn(
			FNEEntity parentEntity, 
			string subEntityId,
			string subEntityType,
			FNEEntityViewData viewData,
			Vector3 pos,
			float rot)
		{
			int parentNetId = parentEntity.NetId;
			Quaternion rotation = Quaternion.AngleAxis(rot, Vector3.up);
			Vector3 scale = Vector3.one * viewData.scaleMod;

			if (GameObjectPoolManager.HasObjectInstance(viewData.Id))
			{
				GameObject instance = GameObjectPoolManager.GetObjectInstance(
					viewData.Id,
					PrefabType.FNEENTIY,
					new float2(pos.x, pos.y)
				);
				
				if (subEntityType == "MountedObject")
				{
					instance.transform.position = pos;
					instance.transform.rotation = rotation;
				}

				// Unity normally convets blender scale (centimeters) to unity scale. (Multiply by 0.01)
				instance.transform.localScale = scale;
				GameClient.Spawner.QueueSubViewForActivation(new GameObjectActivationData
				{
					NetId = parentEntity.NetId,
					View = instance
				});
			}
			else
			{
				var spawnData = new GameObjectSpawnData
				{
					type = subEntityType,
					id = viewData.Id,
					netID = parentEntity.NetId,
					position = pos,
					rotation = rotation,
					scale = scale
				};

				GameClient.Spawner.QueueSubViewForSpawning(spawnData);
			}
		}
		
		public void ActivateSubViewGameObject(GameObjectActivationData activationData)
		{
			var instance = activationData.View;
			activationData.View.SetActive(true);
			GameClient.ViewConnector.AddSubViewGameObject(activationData.View, activationData.NetId);
			GameClient.WorldView.AddSubViewGameObject(instance);

			foreach (Transform child in instance.transform)
			{
				if (child.tag.Equals("Editor"))
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
		}
		
		public void SpawnSubViewGameObject(GameObjectSpawnData spawnData)
		{
			GameObject instance = GameObjectPoolManager.GetObjectInstance(spawnData.id, PrefabType.FNEENTIY, new float2(spawnData.position.x, spawnData.position.y));
			var entity = GameClient.NetConnector.GetEntity(spawnData.netID);
			
			var idHolder = instance.GetComponent<GameObjectIdHolder>();

			if (idHolder == null)
			{
				idHolder = instance.AddComponent<GameObjectIdHolder>();
			}
			
			idHolder.id = spawnData.id;
			
			instance.transform.position = spawnData.position;
			instance.transform.rotation = spawnData.rotation;
			instance.transform.localScale = spawnData.scale;
			
			GameClient.ViewConnector.AddSubViewGameObject(instance, spawnData.netID);
			GameClient.WorldView.AddSubViewGameObject(instance);
			
			foreach (Transform child in instance.transform)
			{
				if (child.tag.Equals("Editor"))
				{
                    Object.Destroy(child.gameObject);
				}
			}
		}
		
		public void QueueSubViewGameObjectForDeactivation(GameObject view)
		{
			if (view == null)
			{
				UIManager.ErrorHandler.NewErrorMessage("Du kan va en string", "Du, subview e null, Not Okay");
			}
			var data = new GameObjectDeactivationData
			{
				id = view.GetComponent<GameObjectIdHolder>().id,
				go = view
			};

			GameClient.Spawner.QueueSubViewForDeactivation(data);
		}
		
		public void QueueSubViewGameObjectsForDeactivation(List<GameObject> gameObjectSubViews)
		{
			foreach (var go in gameObjectSubViews)
			{
				QueueSubViewGameObjectForDeactivation(go);
			}
		}
		
		private void QueueEntitySubViewForSpawn(FNEEntityViewData viewData, float3 position, quaternion rotation, float3 scale, int netId = -1)
		{
			var prefab = PrefabBank.GetPrefab(null, viewData.Id);
			var entitySpawnData = GameClient.ViewAPI.ConvertGameObjectToEntitySpawnData(prefab, position, rotation, scale * viewData.scaleMod, netId);
			entitySpawnData.IsSubView = true;
			GameClient.ViewAPI.GetRenderMeshSpawnerSystem().AddEntitySpawnData(ref entitySpawnData);
		}
	}
}