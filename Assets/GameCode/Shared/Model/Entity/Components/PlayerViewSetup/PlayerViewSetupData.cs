using FNZ.Shared.Model.Entity.EntityViewData;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.PlayerViewSetup
{
	[XmlType("PlayerViewData")]
	public class PlayerViewData
	{
		[XmlElement("headRef")]
		public string headRef { get; set; }

		[XmlElement("hairRef")]
		public string hairRef { get; set; }

		[XmlElement("torsoRef")]
		public string torsoRef { get; set; }

		[XmlElement("handsRef")]
		public string handsRef { get; set; }

		[XmlElement("legsRef")]
		public string legsRef { get; set; }

		[XmlElement("feetRef")]
		public string feetRef { get; set; }

		public bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName)
		{
			errorMessages = new List<Tuple<string, string>>();

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                headRef,
                false,
                fileName,
                parentId,
                "headRef"
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                hairRef,
                false,
                fileName,
                parentId,
                "hairRef"
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                torsoRef,
                false,
                fileName,
                parentId,
                "torsoRef"
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                handsRef,
                false,
                fileName,
                parentId,
                "handsRef"
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                legsRef,
                false,
                fileName,
                parentId,
                "legsRef"
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                feetRef,
                false,
                fileName,
                parentId,
                "feetRef"
            );

			return true;
		}
	}
}