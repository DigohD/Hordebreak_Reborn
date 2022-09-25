using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.GPUAnimationData
{
    [XmlType("GPUAnimationData")]
    public class GPUAnimationData
    {
        [XmlElement("isUsed")] 
        public bool IsUsed { get; set; }

        [XmlElement("action")] 
        public string Action { get; set; }

        [XmlElement("speed")] 
        public float Speed { get; set; }
    }
    
    [XmlType("GPUAnimationComponentData")]
    public class GPUAnimationComponentData : DataComponent
    {
        [XmlElement("blobShadowHeightOffset")]
        public float BlobShadowHeightOffset { get; set; }
        
        [XmlElement("blobShadowScale")]
        public float BlobShadowScale { get; set; }

        [XmlArray("animations")]
        [XmlArrayItem(typeof(GPUAnimationData))]
        public List<GPUAnimationData> Animations { get; set; }

        public override Type GetComponentType()
        {
            return typeof(GPUAnimationComponentShared);
        }

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}