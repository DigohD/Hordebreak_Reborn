using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.TileObject
{
	[XmlType("TileObjectComponentData")]
	public class TileObjectComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(TileObjectComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
