using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	public partial class StatModData
	{
		public enum StatType
		{
			[XmlEnum(MAX_HEALTH)]
			MaxHealth,
			[XmlEnum(ARMOR)]
			Armor,
			[XmlEnum(MAX_SHIELDS)]
			MaxShields
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();
            var errorTitle = parentId + " Comp: " + compName;
            errorMessages.Add(new Tuple<string, string>(errorTitle, "VALIDATION NOT IMPLEMENTED"));
            return false;
        }
    }
}
