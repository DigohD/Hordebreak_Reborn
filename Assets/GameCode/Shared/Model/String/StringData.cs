using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.String
{
	[XmlType("StringData")]
	public class StringData : DataDef
	{
		[XmlElement("sv")]
		public string sv { get; set; }

		[XmlElement("en")]
		public string en { get; set; }

		[XmlElement("de")]
		public string de { get; set; }

		[XmlElement("fr")]
		public string fr { get; set; }

		[XmlElement("es")]
		public string es { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = null;
			return false;
		}
	}
}