using FNZ.Shared.Model.Items;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Model.Entity.Components.Player
{
	public class PlayerComponentShared : FNEComponent
	{
		public FNEComponent OpenedInteractable;
		public bool IsDead;

		protected bool m_IsOnSite;
		protected string m_CurrentSiteId;
		
		new public PlayerComponentData m_Data
		{
			get
			{
				return (PlayerComponentData)base.m_Data;
			}
		}

		protected Dictionary<string, long> m_MutedPlayers = new Dictionary<string, long>();

		protected List<string> unlockedBuildings = new List<string>();

		public bool Afk { get; set; }

		public float2 HomeLocation { get; set; }

		protected Item ItemOnCursor = null;

		public override void Init()
		{
			foreach (var building in m_Data.startingBuildings)
			{
				unlockedBuildings.Add(building);
			}
		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(ItemOnCursor != null);
			ItemOnCursor?.Serialize(bw);

			bw.Write(m_MutedPlayers.Keys.Count);
			foreach (string playerKey in m_MutedPlayers.Keys)
			{
				bw.Write(playerKey);
				bw.Write(m_MutedPlayers[playerKey]);
			}
			bw.Write(HomeLocation.x);
			bw.Write(HomeLocation.y);
			bw.Write(Afk);

			bw.Write(unlockedBuildings.Count);
			if (unlockedBuildings.Count > 0)
			{
				foreach (var buildingId in unlockedBuildings)
				{
					bw.Write(buildingId);
				}
			}
		}

		public override void Deserialize(NetBuffer br)
		{
			if (br.ReadBoolean())
				ItemOnCursor = Item.GenerateItem(br);
			else
				ItemOnCursor = null;

			m_MutedPlayers.Clear();
			int mutedPlayersCount = br.ReadInt32();
			for (int i = 0; i < mutedPlayersCount; i++)
			{
				m_MutedPlayers.Add(br.ReadString(), br.ReadInt64());
			}

			HomeLocation = new float2(br.ReadFloat(), br.ReadFloat());
			Afk = br.ReadBoolean();

			var buildingCount = br.ReadInt32();
			if (buildingCount > 0)
			{
				for (int i = 0; i < buildingCount; i++)
				{
					var buildingId = br.ReadString();
					if (!unlockedBuildings.Contains(buildingId))
					{
						unlockedBuildings.Add(buildingId);
					}
				}
			}
		}

		public override ushort GetSizeInBytes()
		{
			return sizeof(int);
		}

		public Item GetItemOnCursor()
		{
			return ItemOnCursor;
		}

		public Dictionary<string, long> GetMutedPlayers()
		{
			if (m_MutedPlayers.Count > 0)
				return m_MutedPlayers;

			return null;
		}

		public bool IsPlayerMuted(long Id)
		{
			return m_MutedPlayers.ContainsValue(Id);
		}

		public bool IsPlayerMuted(string name)
		{
			return m_MutedPlayers.ContainsKey(name.ToLower());
		}
	}
}
