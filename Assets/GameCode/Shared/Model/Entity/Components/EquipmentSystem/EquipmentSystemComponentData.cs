using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.EquipmentSystem
{
	[XmlType("EquipmentSystemComponentData")]
	public class EquipmentSystemComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(EquipmentSystemComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
