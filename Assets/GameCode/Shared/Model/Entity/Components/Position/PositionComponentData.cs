using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Position
{
	[XmlType("PositionComponentData")]
	public class PositionComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(PositionComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
