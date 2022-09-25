using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Model.Entity.Components.Crafting;
using FNZ.Shared.Model.World.Site;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Shared.Model.World 
{

	public class MapManager
	{
		public struct RevealedSiteData
		{
			public float PosX;
			public float PosY;
			public bool FullReveal;
			public string SiteId;
			public string PrefixName;
			
			public void Deserialize(NetBuffer reader)
			{
				PosX = reader.ReadFloat();
				PosY = reader.ReadFloat();
				FullReveal = reader.ReadBoolean();
				if (FullReveal)
				{
					SiteId = IdTranslator.Instance.GetId<SiteData>(reader.ReadUInt16());
					PrefixName = reader.ReadString();
				}
			}

			public int GetSizeInBytes()
			{
				return 11 + (PrefixName.Length * 4);
			}

			public void Serialize(NetBuffer writer)
			{
				writer.Write(PosX);
				writer.Write(PosY);
				writer.Write(FullReveal);
				if (FullReveal)
				{
					writer.Write(IdTranslator.Instance.GetIdCode<SiteData>(SiteId));
					writer.Write(PrefixName);
				}
			}
		}
		
		protected bool[] visitedChunks;

		protected Dictionary<ushort, ushort> tileTypeMap = new Dictionary<ushort, ushort>();
		
		protected Dictionary<int, RevealedSiteData> m_RevealedSites = new Dictionary<int, RevealedSiteData>();
		
		public MapManager(int widthInChunks, int heightInChunks)
		{
			visitedChunks = new bool[widthInChunks * heightInChunks];
			m_RevealedSites = new Dictionary<int, RevealedSiteData>();
		}
		
		public Dictionary<int, RevealedSiteData> GetRevealedSiteMap()
		{
			return m_RevealedSites;
		}
	}
}