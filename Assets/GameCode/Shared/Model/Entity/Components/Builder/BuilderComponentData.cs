using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Builder
{
	[XmlType("BuilderComponentData")]
	public class BuilderComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(BuilderComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}