using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FNZ.Shared.Model.Entity.Components
{
	public enum FNEComponentMessage
	{
		REACHED_TARGET,
		ZERO_HEALTH
	}

	public abstract class FNEComponent
	{
		public static Dictionary<ushort, Type> ComponentIdTypeDict = new Dictionary<ushort, Type>();

		static FNEComponent()
		{
			var compTypes = Assembly.GetAssembly(typeof(FNEEntity)).GetTypes().Where(t => t.BaseType == typeof(FNEComponent));
			ushort compIndex = 0;
			foreach (var compType in compTypes)
			{
				ComponentIdTypeDict.Add(compIndex++, compType);
			}
		}

		public static ushort GetTypeIdOfComponent(FNEComponent component)
		{
			Type t = component.GetType().BaseType;
			foreach (var key in ComponentIdTypeDict.Keys)
				if (ComponentIdTypeDict[key] == t)
					return key;

			throw new Exception("Component index not present for component of type: " + t);
		}

		protected bool m_Enabled = true;

		public FNEEntity ParentEntity;
		public bool Enabled { get; set; } = true;
		protected virtual DataComponent m_Data { get; set; }

		public void SetData(DataComponent newData)
		{
			m_Data = newData;
		}

		public virtual void Init() { }

		public virtual void InitComponentLinks() { }

		public virtual void Receive(FNEComponentMessage message) { }
		public virtual void Serialize(NetBuffer writer) { }
		public virtual void Deserialize(NetBuffer reader) { }

		public bool IsEnabled() { return m_Enabled; }

		public virtual void OnNetEvent(NetIncomingMessage incMsg) { }

		public virtual void NetSerialize(NetBuffer writer) { Serialize(writer); }
		public virtual void NetDeserialize(NetBuffer reader) { Deserialize(reader); }
		public virtual void FileSerialize(NetBuffer writer) { Serialize(writer); }
		public virtual void FileDeserialize(NetBuffer reader) { Deserialize(reader); }
		public void Enable() { Enabled = true; }
		public void Disable() { Enabled = false; }

		public abstract ushort GetSizeInBytes();

		public virtual void OnReplaced(FNEEntity replacement)
		{
			
		}
	}
}

