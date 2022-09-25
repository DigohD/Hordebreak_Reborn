using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Net;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.Builder
{
	public class BuilderComponentNet : MonoBehaviour
	{
		public enum BuilderNetEvent
		{
			BUILD = 0,
			BUILD_WALL = 1,
			BUILD_TILES = 2,
			BUILD_ADDON = 3,
			BUILD_MOUNTED_OBJECT = 4,
			REMOVE_MOUNTED_OBJECT = 5
		}

		public struct BuildData : IComponentNetEventData
		{
			public float x;
			public float y;
			public int rot;
			public string recipeId;

			public void Deserialize(NetBuffer reader)
			{
				x = reader.ReadFloat();
				y = reader.ReadFloat();
				rot = reader.ReadInt32();
				recipeId = IdTranslator.Instance.GetId<BuildingData>(reader.ReadUInt16());
			}

			public int GetSizeInBytes()
			{
				return 4 + 4 + 4 + 2;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(rot);
				writer.Write(IdTranslator.Instance.GetIdCode<BuildingData>(recipeId));
			}
		}

		public struct BuildAddonData : IComponentNetEventData
		{
			public float x;
			public float y;
			public int rot;
			public string addonId;

			public void Deserialize(NetBuffer reader)
			{
				x = reader.ReadFloat();
				y = reader.ReadFloat();
				rot = reader.ReadInt32();
				addonId = IdTranslator.Instance.GetId<BuildingAddonData>(reader.ReadUInt16());
			}

			public int GetSizeInBytes()
			{
				return 4 + 4 + 4 + 2;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(rot);
				writer.Write(IdTranslator.Instance.GetIdCode<BuildingAddonData>(addonId));
			}
		}

		public struct BuildMountedObjectData : IComponentNetEventData
		{
			public float x;
			public float y;
			public bool oppositeDirection;
			public string buildingId;

			public void Deserialize(NetBuffer reader)
			{
				x = reader.ReadFloat();
				y = reader.ReadFloat();
				oppositeDirection = reader.ReadBoolean();
				buildingId = IdTranslator.Instance.GetId<BuildingData>(reader.ReadUInt16());
			}

			public int GetSizeInBytes()
			{
				return 4 + 4 + 1 + 2;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(oppositeDirection);
				writer.Write(IdTranslator.Instance.GetIdCode<BuildingData>(buildingId));
			}
		}

		public struct RemoveMountedObjectData : IComponentNetEventData
		{
			public float x;
			public float y;

			public void Deserialize(NetBuffer reader)
			{
				x = reader.ReadFloat();
				y = reader.ReadFloat();
			}

			public int GetSizeInBytes()
			{
				return 4 + 4;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(x);
				writer.Write(y);
			}
		}

		public struct BuildWallData : IComponentNetEventData
		{
			public string recipeId;
			public int startX;
			public int startY;
			public int endX;
			public int endY;

			public void Deserialize(NetBuffer reader)
			{
				recipeId = IdTranslator.Instance.GetId<BuildingData>(reader.ReadUInt16());
				startX = reader.ReadInt32();
				startY = reader.ReadInt32();
				endX = reader.ReadInt32();
				endY = reader.ReadInt32();
			}

			public int GetSizeInBytes()
			{
				return 2 + 4 + 4 + 4 + 4;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(IdTranslator.Instance.GetIdCode<BuildingData>(recipeId));
				writer.Write(startX);
				writer.Write(startY);
				writer.Write(endX);
				writer.Write(endY);
			}
		}

		public struct BuildTileData : IComponentNetEventData
		{
			public string recipeId;
			public int startX;
			public int startY;
			public int endX;
			public int endY;

			public void Deserialize(NetBuffer reader)
			{
				recipeId = IdTranslator.Instance.GetId<BuildingData>(reader.ReadUInt16());
				startX = reader.ReadInt32();
				startY = reader.ReadInt32();
				endX = reader.ReadInt32();
				endY = reader.ReadInt32();
			}

			public int GetSizeInBytes()
			{
				return 2 + 4 + 4 + 4 + 4;
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(IdTranslator.Instance.GetIdCode<BuildingData>(recipeId));
				writer.Write(startX);
				writer.Write(startY);
				writer.Write(endX);
				writer.Write(endY);
			}
		}
	}
}