using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.EdgeObject
{
	[XmlType("EdgeObjectComponentData")]
	public class EdgeObjectComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(EdgeObjectComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
