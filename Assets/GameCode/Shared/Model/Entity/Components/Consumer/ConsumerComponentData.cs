using FNZ.Shared.Model.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Consumer
{
	[XmlType("ConsumerComponentData")]
	public class ConsumerComponentData : DataComponent
	{
		[XmlArray("resources")]
		[XmlArrayItem(typeof(ResourceConsumptionData))]
		public List<ResourceConsumptionData> resources { get; set; }

		public override Type GetComponentType()
		{
			return typeof(ConsumerComponentShared);
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

            return true;
        }
    }
}