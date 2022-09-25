using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Inventory
{
	[XmlType("InventoryComponentData")]
	public class InventoryComponentData : DataComponent
	{
		[XmlElement("produceLootTable")]
		public LootTableData produceLootTable { get; set; }

		[XmlElement("destroyWhenEmpty")]
		public bool destroyWhenEmpty;

		[XmlElement("width")]
		public int width = 10;

		[XmlElement("height")]
		public int height = 5;

		[XmlElement("openAnimationName")]
		public string openAnimationName { get; set; }

		[XmlElement("closeAnimationName")]
		public string closeAnimationName { get; set; }

		public override Type GetComponentType()
		{
			return typeof(InventoryComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            if (produceLootTable != null)
            {
                produceLootTable.ValidateXMLData(
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
