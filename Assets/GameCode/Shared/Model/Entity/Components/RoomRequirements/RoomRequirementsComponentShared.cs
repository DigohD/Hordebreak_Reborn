using System;
using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using Lidgren.Network;
using System.Collections.Generic;
 
namespace FNZ.Shared.Model.Entity.Components.RoomRequirements
{

	public class RoomRequirementsComponentShared : FNEComponent
	{
		public RoomRequirementsComponentData Data {
			get
			{
				return (RoomRequirementsComponentData) base.m_Data;
			}
		}

		public override void Init(){}
 
		public override void Serialize(NetBuffer bw){}
 
		public override void Deserialize(NetBuffer br){}
 
		public override ushort GetSizeInBytes(){ return 0; }
	}
}
