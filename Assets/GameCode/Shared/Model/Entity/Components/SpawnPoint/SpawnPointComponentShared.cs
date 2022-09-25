using System;
using System.Xml.Serialization;
using Lidgren.Network;
using System.Collections.Generic;
 
namespace FNZ.Shared.Model.Entity.Components.SpawnPoint
{
	[XmlType("SpawnPointComponentData")]
	public class SpawnPointComponentData : DataComponent 
	{
		public override Type GetComponentType()
		{
			return typeof(SpawnPointComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return true;
		}
	}
 
	public class SpawnPointComponentShared : FNEComponent
	{
		public new SpawnPointComponentData m_Data => (SpawnPointComponentData) base.m_Data;
		public override void Init(){}
 
		public override void Serialize(NetBuffer bw){}
 
		public override void Deserialize(NetBuffer br){}
 
		public override ushort GetSizeInBytes(){ return 0; }
	}
}
