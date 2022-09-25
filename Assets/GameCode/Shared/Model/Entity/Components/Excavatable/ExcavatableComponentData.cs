using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Excavatable
{
	[XmlType("ExcavatableComponentData")]
	public class ExcavatableComponentData : DataComponent
	{
        [XmlElement("baseExcavateTime")]
        public float baseExcavateTime = 1f;

        [XmlElement("totalHits")]
		public int totalHits;

		[XmlElement("HitLootTable")]
		public LootTableData hitLootTable;

        [XmlArray("ExcavatableBonuses")]
        [XmlArrayItem(typeof(ExcavatableBonusData))]
        public List<ExcavatableBonusData> excavatableBonuses { get; set; }

        [XmlElement("DestroyLootTable")]
		public LootTableData destroyLootTable;

		[XmlElement("hitEffectRef")]
		public string hitEffectRef;

		[XmlElement("deathEffectRef")]
		public string deathEffectRef;

        [XmlElement("transformsOnExcavation")]
        public bool transformsOnExcavation { get; set; }

        [XmlElement("transformedEntityRef")]
        public string transformedEntityRef { get; set; }

        public override Type GetComponentType()
		{
			return typeof(ExcavatableComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            if (hitLootTable != null)
            {
                hitLootTable.ValidateXMLData(
                    errorMessages,
                    parentId,
                    fileName,
                    compName
                );
            }

            if (destroyLootTable != null)
            {
                destroyLootTable.ValidateXMLData(
                     errorMessages,
                     parentId,
                     fileName,
                     compName
                 );
            }

            XMLValidation.ValidateId<EffectData>(
                errorMessages,
                hitEffectRef,
                true,
                fileName,
                parentId,
                "hitEffectRef",
                compName
            );

            XMLValidation.ValidateId<EffectData>(
               errorMessages,
               deathEffectRef,
               true,
               fileName,
               parentId,
               "deathEffectRef",
               compName
           );

            if(transformedEntityRef != null && !transformsOnExcavation)
            {
                errorMessages.Add(
                    new Tuple<string, string>(
                        $"Error in '{fileName}'",
                        $"transformsOnExcavation in {compName} of '{parentId}' should be true since transformedEntityRef is specified."
                    )
                );
            }else if (transformedEntityRef == null && transformsOnExcavation)
            {
                errorMessages.Add(
                    new Tuple<string, string>(
                        $"Error in '{fileName}'",
                        $"transformedEntityRef in {compName} of '{parentId}' should be true since transformsOnExcavation is set to true."
                    )
                );
            }else if(transformedEntityRef != null && transformsOnExcavation)
            {
                XMLValidation.ValidateId<FNEEntityData>(
                    errorMessages,
                    transformedEntityRef,
                    true,
                    fileName,
                    parentId,
                    "transformedEntityRef",
                    compName
                );
            }

            return true;
        }
    }
}
