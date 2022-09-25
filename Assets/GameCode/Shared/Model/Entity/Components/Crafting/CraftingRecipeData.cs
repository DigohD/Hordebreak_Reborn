using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.SFX;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FNZ.Shared.Model.VFX;

namespace FNZ.Shared.Model.Entity.Components.Crafting
{
	[XmlType("CraftingRecipeData")]
	public class CraftingRecipeData : DataDef
	{
		[XmlArray("requiredMaterials")]
		[XmlArrayItem(typeof(MaterialDef))]
		public List<MaterialDef> requiredMaterials { get; set; }

		[XmlElement("productRef")]
		public string productRef { get; set; }

		[XmlElement("productAmount")]
		public int productAmount { get; set; }

        [XmlElement("craftSFXRef")]
        public string craftSFXRef { get; set; }
        
        [XmlElement("craftVFXRef")]
        public string craftVFXRef { get; set; }

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (requiredMaterials == null || requiredMaterials.Count == 0)
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'requiredMaterials' for '{Id}' must exist and not be empty."));
			else
			{
				foreach (var materialDef in requiredMaterials)
				{
                    materialDef.ValidateXMLData(errorMessages, Id, fileName);
				}
			}

            XMLValidation.ValidateId<ItemData>(
                errorMessages,
                productRef,
                false,
                fileName,
                Id,
                "productRef"
            );

            XMLValidation.ValidateId<SFXData>(
               errorMessages,
               craftSFXRef,
               true,
               fileName,
               Id,
               "craftSFXRef"
            );
            
            XMLValidation.ValidateId<VFXData>(
	            errorMessages,
	            craftVFXRef,
	            true,
	            fileName,
	            Id,
	            "craftVFXRef"
            );

            return errorMessages.Count > 0;
		}
	}
}