using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Crop
{
	[XmlType("CropComponentData")]
	public class CropComponentData : DataComponent
	{
		[XmlElement("produceLootTable")]
		public LootTableData produceLootTable { get; set; }

		[XmlElement("consumedOnHarvest")]
		public bool consumedOnHarvest { get; set; }

        [XmlElement("transformsOnHarvest")]
        public bool transformsOnHarvest { get; set; }

        [XmlElement("transformedEntityRef")]
        public string transformedEntityRef { get; set; }

        [XmlElement("matureEntityRef")]
		public string matureEntityRef { get; set; }

		[XmlElement("growthTimeTicks")]
		public float growthTimeTicks { get; set; }

		[XmlElement("harvestableViewRef")]
		public string harvestableViewRef { get; set; }

        [XmlElement("harvestEffectRef")]
        public string harvestEffectRef { get; set; }

        [XmlArray("environmentSpans")]
		[XmlArrayItem(typeof(EnvironmentSpanData))]
		public List<EnvironmentSpanData> environmentSpans { get; set; }

		public override Type GetComponentType()
		{
			return typeof(CropComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();
            
            if(produceLootTable != null)
            {
                produceLootTable.ValidateXMLData(errorMessages, parentId, fileName, compName);
            }

            XMLValidation.ValidateId<FNEEntityData>(
                errorMessages,
                matureEntityRef,
                true,
                fileName,
                parentId,
                "matureEntityRef",
                compName
            );

            XMLValidation.ValidateId<EffectData>(
                errorMessages,
                harvestEffectRef,
                true,
                fileName,
                parentId,
                "harvestEffectRef",
                compName
            );

            XMLValidation.ValidateId<FNEEntityViewData>(
               errorMessages,
               harvestableViewRef,
               true,
               fileName,
               parentId,
               "harvestableViewRef",
               compName
           );

            if (environmentSpans != null)
            {
                foreach (var span in environmentSpans)
                {
                    span.ValidateXMLData(
                        errorMessages, 
                        parentId, 
                        fileName, 
                        compName    
                    );
                }
            }

            return true;
        }
    }
}