using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FNZ.Shared.Model.Items
{
	public abstract class ItemComponentData
	{
		public abstract Type GetComponentType();

		public abstract bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName);
	}

	public abstract class ItemComponent
	{
		public Item ParentItem;
		protected ItemComponentData m_Data;

		public static Dictionary<ushort, Type> ComponentIdTypeDict = new Dictionary<ushort, Type>();

		static ItemComponent()
		{
			var compTypes = Assembly.GetAssembly(typeof(Item)).GetTypes().Where(t => t.BaseType == typeof(ItemComponent));
			ushort compIndex = 0;
			foreach (var compType in compTypes)
			{
				ComponentIdTypeDict.Add(compIndex++, compType);
			}
		}

		public ItemComponentData GetData()
		{
			return m_Data;
		}

		public void SetData(ItemComponentData data)
		{
			m_Data = data;
		}

		public abstract void Init();

		public virtual void Serialize(NetBuffer bw) { }
		public virtual void Deserialize(NetBuffer br) { }
	}
}

