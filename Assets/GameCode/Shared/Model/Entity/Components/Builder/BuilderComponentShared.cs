using FNZ.Shared.Model.Building;
using Lidgren.Network;
using System.Collections.Generic;

namespace FNZ.Shared.Model.Entity.Components.Builder
{
	public class BuilderComponentShared : FNEComponent
	{
		new public BuilderComponentData m_Data
		{
			get
			{
				return (BuilderComponentData) base.m_Data;
			}
		}

		public Dictionary<string, List<BuildingData>> BuildingCategoryLists = new Dictionary<string, List<BuildingData>>();

		public override void Init()
		{
			foreach (var cat in DataBank.Instance.GetAllDataIdsOfType<BuildingCategoryData>())
			{
				BuildingCategoryLists.Add(cat.Id, new List<BuildingData>());
			}
		}

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
