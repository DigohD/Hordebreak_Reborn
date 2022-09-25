using FNZ.Shared.Model.Entity.Components.Environment;
using FNZ.Shared.Model.Entity.Components.Producer;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.MountedObject 
{
	[XmlType("MountedObjectData")]
	public class MountedObjectData : DataDef
	{
		[XmlArray("viewVariations")]
		[XmlArrayItem("viewRef", typeof(string))]
		public List<string> viewVariations { get; set; }

		[XmlArray("environmentTransfers")]
		[XmlArrayItem(typeof(EnvironmentEffectData))]
		public List<EnvironmentEffectData> environmentTransfers { get; set; }

		[XmlArray("resourceTransfers")]
		[XmlArrayItem(typeof(ResourceProductionData))]
		public List<ResourceProductionData> resourceTransfers { get; set; }

        [XmlElement("mayGenerateFromOutdoors")]
        public bool generateFromOutdoors { get; set; } = false;

        [XmlElement("editorName")]
        public string editorName { get; set; }

        [XmlElement("editorCategoryName")]
        public string editorCategoryName { get; set; }

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (viewVariations != null && viewVariations.Count > 0)
			{
                XMLValidation.ValidateIdList<FNEEntityViewData>(
                    errorMessages,
                    viewVariations,
                    false,
                    fileName,
                    Id,
                    "viewVariations"
                );
			}
			else
			{
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"viewVariations of {Id} must be specified."));
			}

            if (environmentTransfers != null)
            {
                foreach (var env in environmentTransfers)
                {
                    env.ValidateXMLData(
                        errorMessages,
                        Id,
                        fileName
                    );
                }
            }

            if (resourceTransfers != null)
            {
                foreach (var res in resourceTransfers)
                {
                    res.ValidateXMLData(
                        errorMessages,
                        Id,
                        fileName
                    );
                }
            }

            return errorMessages.Count > 0;
		}
	}
}