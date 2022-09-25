
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.SFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Refinement
{
	[XmlType("RefinementComponentData")]
	public class RefinementComponentData : DataComponent
	{
		[XmlArray("recipes")]
		[XmlArrayItem("recipeRef", typeof(string))]
		public List<string> recipes;

		[XmlElement("burnGradeRef")]
		public string burnGradeRef;

		[XmlElement("startSFXRef")]
		public string startSFXRef;

		[XmlElement("stopSFXRef")]
		public string stopSFXRef;

		[XmlElement("activeSFXLoopRef")]
		public string activeSFXLoopRef;

		public override Type GetComponentType()
		{
			return typeof(RefinementComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateIdList<RefinementRecipeData>(
                errorMessages,
                recipes,
                false,
                fileName,
                parentId,
                "recipes",
                compName
            );

            XMLValidation.ValidateId<BurnableData>(
                errorMessages,
                burnGradeRef,
                true,
                fileName,
                parentId,
                "burnGradeRef"
            );

            XMLValidation.ValidateId<SFXData>(
               errorMessages,
               startSFXRef,
               true,
               fileName,
               parentId,
               "startSFXRef"
           );

            XMLValidation.ValidateId<SFXData>(
              errorMessages,
              stopSFXRef,
              true,
              fileName,
              parentId,
              "stopSFXRef"
          );

            XMLValidation.ValidateId<SFXData>(
              errorMessages,
              activeSFXLoopRef,
              true,
              fileName,
              parentId,
              "activeSFXLoopRef"
          );

            return true;
        }
    }
}
