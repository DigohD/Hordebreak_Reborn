using FNZ.Shared.Model.Entity.Components.EdgeObject;
using Lidgren.Network;
using UnityEngine;
using FNZ.Client.View.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Entity.MountedObject;

namespace FNZ.Client.Model.Entity.Components.EdgeObject
{
	public class EdgeObjectComponentClient : EdgeObjectComponentShared
	{
		public GameObject MountedGameObjectView;
		public MountedObjectData PreviousMountedObject;

		public override void Deserialize(NetBuffer br)
		{
			PreviousMountedObject = MountedObjectData;

			base.Deserialize(br);
		}

		public void UpdateMountedView()
		{
			if (PreviousMountedObject != MountedObjectData)
			{
				if(MountedObjectData == null)
				{
					//Object.Destroy(MountedGameObjectView);
				}
				else
				{
					/*if (MountedGameObjectView != null)
						Object.Destroy(MountedGameObjectView);*/

					GameClient.SubViewAPI.QueueSubViewForSpawn(
						ParentEntity,
						MountedObjectData.viewVariations[0],
						"MountedObject"
					);
				}
			}
		}

		private void SpawnMountedView()
		{
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(MountedObjectData.viewVariations[0]);
			MountedGameObjectView = GameObjectPoolManager.GetObjectInstance(MountedObjectData.viewVariations[0], PrefabType.FNEENTIY, ParentEntity.Position);

			MountedGameObjectView.transform.position = new Vector3(ParentEntity.Position.x, viewData.heightPos, ParentEntity.Position.y);
			MountedGameObjectView.transform.localScale = Vector3.one * viewData.scaleMod;

			// Vertical wall
			if (ParentEntity.Position.x % 1 == 0)
			{
				if (!OppositeMountedDirection)
					MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 90, 0);
				else
					MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 270, 0);
			}
			// Horizontal wall
			else
			{
				if (!OppositeMountedDirection)
					MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 0, 0);
				else
					MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 180, 0);
			}
		}
	}
}
