using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Crafting
{
	[XmlType("CraftingComponentData")]
	public class CraftingComponentData : DataComponent
	{
		[XmlArray("recipes")]
		[XmlArrayItem("recipeRef", typeof(string))]
		public List<string> recipes;

		public override Type GetComponentType()
		{
			return typeof(CraftingComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateIdList<CraftingRecipeData>(
                errorMessages,
                recipes,
                false,
                fileName,
                parentId,
                "recipes",
                compName
            );

            return true;
        }
    }
}