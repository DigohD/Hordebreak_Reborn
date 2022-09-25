using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Atmosphere 
{
    
    public enum FogDensity {
        [XmlEnum("Lightest")]
        LIGHTEST = 600,
		
        [XmlEnum("SuperDense")]
        SUPER_DENSE = 40,
        [XmlEnum("Dense")]
        DENSE = 85,
        [XmlEnum("Moderate")]
        MODERATE = 125,
        [XmlEnum("Light")]
        LIGHT = 200,
        [XmlEnum("SuperLight")]
        SUPER_LIGHT = 450,
    }
    
    [XmlType("AtmosphereData")]
    public class AtmosphereData : DataDef
    {
        [XmlArray("sfxList")]
        [XmlArrayItem(typeof(AtmosphereSFXData))]
        public List<AtmosphereSFXData> sfxList { get; set; }

        [XmlElement("fogTint")]
        public string fogTint { get; set; } = "#FFFFFF";
        
        [XmlElement("sunTint")]
        public string sunTint { get; set; } = "#FFFFFF";
        
        [XmlElement("moonTint")]
        public string moonTint { get; set; } = "#FFFFFF";
        
        [XmlElement("waterTint")]
        public string waterTint { get; set; } = "#FFFFFF";
        
        [XmlElement("fogThickness")]
        public FogDensity fogThickness { get; set; } = FogDensity.LIGHTEST;

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
        {
            errorMessages = null;
            return false;
        }
    }
}