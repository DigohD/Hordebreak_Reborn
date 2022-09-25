using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Player
{
	public enum PlayerNetEvent
	{
		ANIMATOR_DATA = 0,
		KILL_PLAYER = 1,
		REVIVE_PLAYER = 2,
		RESPAWN_REQUEST = 3,
		RESPAWN_GRANTED = 4,
		REQUEST_THROW_ITEM = 5,
		REQUEST_PICK_UP_ITEM = 6,
		UPDATE_CURRENT_SITE = 7,
	}

	public struct PlayerAnimatorData : IComponentNetEventData
	{
		public WeaponPosture WeaponPosture;

		public bool Sprinting;
		public bool Idle;

		public float MovementX;
		public float MovementY;

		public void Deserialize(NetBuffer reader)
		{
			WeaponPosture = (WeaponPosture)reader.ReadByte();

			Sprinting = reader.ReadBoolean();
			Idle = reader.ReadBoolean();

			MovementX = reader.ReadFloat();
			MovementY = reader.ReadFloat();
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write((byte)WeaponPosture);

			writer.Write(Sprinting);
			writer.Write(Idle);

			writer.Write(MovementX);
			writer.Write(MovementY);
		}

		public int GetSizeInBytes()
		{
			return 11;
		}
	}

	public struct RequestPickUpItemData : IComponentNetEventData
	{
		public long Identifier;

		public void Deserialize(NetBuffer reader)
		{
			Identifier = reader.ReadInt64();
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(Identifier);
		}

		public int GetSizeInBytes()
		{
			return 8;
		}
	}
	
	public struct UpdateCurrentSiteData : IComponentNetEventData
	{
		public string  CurrentSiteId;
		public bool IsOnSite;

		public UpdateCurrentSiteData(string currentSiteId, bool isOnSite)
		{
			CurrentSiteId = currentSiteId;
			IsOnSite = isOnSite;
		}
		
		public void Deserialize(NetBuffer reader)
		{
			IsOnSite = reader.ReadBoolean();
			if (IsOnSite)
			{
				CurrentSiteId = IdTranslator.Instance.GetId<SiteData>(reader.ReadUInt16());
			}
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(IsOnSite);
			if (IsOnSite)
			{
				writer.Write(IdTranslator.Instance.GetIdCode<SiteData>(CurrentSiteId));
			}
		}

		public int GetSizeInBytes()
		{
			return 3;
		}
	}
}