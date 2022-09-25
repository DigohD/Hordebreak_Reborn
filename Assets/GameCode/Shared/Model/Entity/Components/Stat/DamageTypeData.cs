using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	[XmlType("DamageTypeData")]
	public class DamageTypeData : DataDef
	{
		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = null;
			return true;
		}
	}

}
