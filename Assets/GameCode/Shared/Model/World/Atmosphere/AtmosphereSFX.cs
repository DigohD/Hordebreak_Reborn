using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Atmosphere 
{
    [XmlType("AtmosphereSFXData")]
    public class AtmosphereSFXData
	{
        [XmlArray("sfxRefs")]
        [XmlArrayItem("sfxRef", typeof(string))]
        public List<string> sfxRefs { get; set; }

        [XmlElement("ambience")]        
        public string ambience { get; set; }

        [XmlElement("weight")]
        public byte weight { get; set; }

        [XmlElement("centerTime")]
        public byte centerTime { get; set; }

        [XmlElement("timeOffset")]
        public byte timeOffset { get; set; }
    }
}