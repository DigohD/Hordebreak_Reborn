using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	[XmlType("StatComponentData")]
	public class StatComponentData : DataComponent
	{
		[XmlElement("startHealth")]
		public float startHealth;

		[XmlElement("startShields")]
		public float startShields;

		[XmlElement("startShieldsRegeneration")]
		public float startShieldsRegeneration;

		[XmlElement("startArmor")]
		public float startArmor;

		[XmlElement("defenseTypeRef")]
		public string defenseTypeRef;

		public override Type GetComponentType()
		{
			return typeof(StatComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateId<DefenseTypeData>(
                errorMessages,
                defenseTypeRef,
                false,
                fileName,
                parentId,
                "defenseTypeRef",
                compName
            );

            return false;
        }
    }
}
