using FNZ.Shared.Model;
using FNZ.Shared.Model.Items;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Utils
{

	[XmlType("LootTableData")]
	public class LootTableData
	{
		[XmlArray("table")]
		[XmlArrayItem(typeof(LootEntry))]
		public List<LootEntry> table { get; set; }

		[XmlElement("lootChanceInPercent")]
		public float lootChanceInPercent { get; set; }
		
		[XmlElement("minRolls")]
		public int minRolls { get; set; }

		[XmlElement("maxRolls")]
		public int maxRolls { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName)
        {
            foreach (var produceEntry in table)
            {
                XMLValidation.ValidateId<ItemData>(
                      errorMessages,
                      produceEntry.itemRef,
                      false,
                      fileName,
                      parentId,
                      "LootTableData produceentry",
                      compName
                );
            }
        }
	}

	[XmlType("LootEntry")]
	public class LootEntry
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("probability")]
		public int probability { get; set; }

		[XmlElement("guaranteed")]
		public bool guaranteed { get; set; }

		[XmlElement("unique")]
		public bool unique { get; set; }
	}
}