using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.EntityViewData
{
    [XmlType("FNEEntityTextureData")]
    public class FNEEntityTextureData : DataDef
    {
        [XmlElement("assetBundlePath")]
        public string AssetBundlePath { get; set; }
        
        [XmlElement("assetBundleName")]
        public string AssetBundleName { get; set; }
        
        [XmlElement("albedoName")]
        public string AlbedoName { get; set; }
        
        [XmlElement("normalMapName")]
        public string NormalMapName { get; set; }
        
        [XmlElement("maskMapName")]
        public string MaskMapName { get; set; }
                
        [XmlElement("emissiveMapName")]
        public string EmissiveMapName { get; set; }
        
        [XmlElement("albedoPath")]
        public string AlbedoPath { get; set; }

        [XmlElement("maskMapPath")]
        public string MaskMapPath { get; set; }

        [XmlElement("normalPath")]
        public string NormalPath { get; set; }

        [XmlElement("emissivePath")]
        public string EmissivePath { get; set; }

        [XmlElement("emissiveFactor")] 
        public float EmissiveFactor { get; set; } = 1.0f;
        
        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
        {
            errorMessages = new List<Tuple<string, string>>();
            return true;
        }
    }
}