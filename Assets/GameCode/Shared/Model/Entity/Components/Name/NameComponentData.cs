using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Name
{
	[XmlType("NameComponentData")]
	public class NameComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(NameComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
