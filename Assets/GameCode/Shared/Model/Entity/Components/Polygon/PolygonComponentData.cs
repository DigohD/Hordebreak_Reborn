using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Polygon
{
	[XmlType("PolygonComponentData")]
	public class PolygonComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(PolygonComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
