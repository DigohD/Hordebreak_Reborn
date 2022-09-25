using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	[XmlType("DefenseTypeData")]
	public class DefenseTypeData : DataDef
	{
		[XmlType("DamageEntry")]
		public struct DamageEntry
		{
			[XmlElement("damageRef")]
			public string damageRef { get; set; }

			[XmlElement("amplifier")]
			public float amplification { get; set; }
		}

		[XmlArray("damagedBy")]
		[XmlArrayItem("damageEntry", typeof(DamageEntry))]
		public List<DamageEntry> damagedByList { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (damagedByList == null || damagedByList.Count == 0)
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"Xml entry 'damagedByList' for {Id} was null or empty."));
			else
			{
				foreach (var damageEntry in damagedByList)
				{
					if (!DataBank.Instance.DoesIdExist<DamageTypeData>(damageEntry.damageRef))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
							$"damageTypeRef: '{damageEntry.damageRef}' of {Id} was not found in database."));
				}
			}

			return errorMessages.Count > 0;
		}
	}
}