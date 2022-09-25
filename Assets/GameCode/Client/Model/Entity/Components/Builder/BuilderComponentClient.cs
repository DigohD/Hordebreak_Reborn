using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.Entity.Components.Builder;
using static FNZ.Shared.Model.Entity.Components.Builder.BuilderComponentNet;

namespace FNZ.Client.Model.Entity.Components.Builder
{
	public class BuilderComponentClient : BuilderComponentShared
	{
		public override void Init()
		{
			base.Init();

			foreach (var buildingData in DataBank.Instance.GetAllDataIdsOfType<BuildingData>())
			{
				BuildingCategoryLists[buildingData.categoryRef].Add(buildingData);
			}
		}

		public void NE_Send_Build(float x, float y, int rot, string recipeId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.BUILD,
				new BuildData
				{
					x = x,
					y = y,
					rot = rot,
					recipeId = recipeId
				}
			);
		}

		public void NE_Send_Build_Addon(float x, float y, int rot, string addonId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.BUILD_ADDON,
				new BuildAddonData
				{
					x = x,
					y = y,
					rot = rot,
					addonId = addonId
				}
			);
		}

		public void NE_Send_Build_Mounted_Object(float x, float y, bool oppositeDirection, string buildingId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.BUILD_MOUNTED_OBJECT,
				new BuildMountedObjectData
				{
					x = x,
					y = y,
					oppositeDirection = oppositeDirection,
					buildingId = buildingId
				}
			);
		}

		public void NE_Send_Remove_Mounted_Object(float x, float y)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.REMOVE_MOUNTED_OBJECT,
				new RemoveMountedObjectData
				{
					x = x,
					y = y
				}
			);
		}

		public void NE_Send_Build_Wall(int startX, int startY, int endX, int endY, string recipeId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.BUILD_WALL,
				new BuildWallData
				{
					startX = startX,
					startY = startY,
					endX = endX,
					endY = endY,
					recipeId = recipeId
				}
			);
		}

		public void NE_Send_Build_Tiles(int startX, int startY, int endX, int endY, string recipeId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)BuilderNetEvent.BUILD_TILES,
				new BuildTileData
				{
					startX = startX,
					startY = startY,
					endX = endX,
					endY = endY,
					recipeId = recipeId
				}
			);
		}
	}
}
