using FNZ.Shared.Model.World.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Environment
{
	[XmlType("EnvironmentComponentData")]
	public class EnvironmentComponentData : DataComponent
	{
		[XmlArray("environment")]
		[XmlArrayItem(typeof(EnvironmentEffectData))]
		public List<EnvironmentEffectData> environment { get; set; }

		public override Type GetComponentType()
		{
			return typeof(EnvironmentComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();
            var errorTitle = parentId + " Comp: " + compName;

            if (environment != null)
            {
                foreach (var env in environment)
                {
                    env.ValidateXMLData(
                        errorMessages,
                        parentId,
                        fileName,
                        compName
                    );
                }
            }
            else
            {
                errorMessages.Add(
                    new Tuple<string, string>(
                        errorTitle,
                        $"environment of '{compName}' in '{parentId}' must be specified."
                    )
                );
            }

            return true;
        }
    }
}
