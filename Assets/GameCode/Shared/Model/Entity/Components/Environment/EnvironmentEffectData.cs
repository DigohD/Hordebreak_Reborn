using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Environment
{
    [XmlType("EnvironmentEffectData")]
    public class EnvironmentEffectData
    {
        [XmlElement("typeRef")]
        public string typeRef { get; set; }

        [XmlElement("amount")]
        public byte amount { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName = "def")
        {
            XMLValidation.ValidateId<EnvironmentData>(
                errorMessages,
                typeRef,
                false,
                fileName,
                parentId,
                "EnvironmentEffectData typeRef",
                compName
            );
        }
    }
}
