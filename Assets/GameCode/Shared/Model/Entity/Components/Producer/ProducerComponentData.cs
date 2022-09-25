using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Producer
{
	[XmlType("ProducerComponentData")]
	public class ProducerComponentData : DataComponent
	{
		[XmlArray("resources")]
		[XmlArrayItem(typeof(ResourceProductionData))]
		public List<ResourceProductionData> resources { get; set; }

		public override Type GetComponentType()
		{
			return typeof(ProducerComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            foreach (var Data in resources)
            {
                Data.ValidateXMLData(
                    errorMessages,
                    parentId,
                    fileName,
                    compName
                );
            }

            return false;
        }
    }
}
