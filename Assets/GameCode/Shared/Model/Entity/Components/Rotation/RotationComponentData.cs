using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Rotation
{
	[XmlType("RotationComponentData")]
	public class RotationComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(RotationComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
