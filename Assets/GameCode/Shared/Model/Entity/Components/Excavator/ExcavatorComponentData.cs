using FNZ.Shared.Model.Entity.EntityViewData;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.Excavator
{
	[XmlType("ExcavatorComponentData")]
	public class ExcavatorComponentData : DataComponent
	{
		[XmlElement("baseMaximumFuel")]
		public int BaseFuel;

		[XmlElement("baseCooldownFueled")]
		public float BaseCooldownFueled;

		[XmlElement("baseCooldownDry")]
		public float BaseCooldownDry;

		[XmlElement("baseRange")]
		public float BaseRange;

        [XmlElement("excavatorMeshData")]
        public string excavatorMeshData { get; set; }
        
        [XmlElement("excavatorTextureData")]
        public string excavatorTextureData { get; set; }

        public override Type GetComponentType()
		{
			return typeof(ExcavatorComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                excavatorMeshData,
                false,
                fileName,
                parentId,
                "excavatorMeshData",
                compName
            );

            return true;
        }
    }
}
