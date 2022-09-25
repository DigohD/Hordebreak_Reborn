using System;
using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.BuildingAddon;

namespace FNZ.Shared.Model.Entity.Components.BuildingAddon
{
	[XmlType("BuildingAddonComponentData")]
	public class BuildingAddonComponentData : DataComponent 
	{
		[XmlArray("addonRefs")]
		[XmlArrayItem("addonRef", typeof(string))]
		public List<string> addonRefs { get; set; }

		public override Type GetComponentType()
		{
			return typeof(BuildingAddonComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateIdList<BuildingAddonData>(
                errorMessages,
                addonRefs,
                false,
                fileName,
                parentId,
                "addonRefs",
                compName
            );

            return true;
        }
    }
 
	public class BuildingAddonComponentShared : FNEComponent
	{
		new public BuildingAddonComponentData m_Data {
			get
			{
				return (BuildingAddonComponentData) base.m_Data;
			}
		}
		public override void Init(){}
 
		public override void Serialize(NetBuffer bw){}
 
		public override void Deserialize(NetBuffer br){}
 
		public override ushort GetSizeInBytes(){ return 0; }
	}
}
