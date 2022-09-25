using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Site 
{
	[XmlType("SiteEntry")]
	public class SiteEntry
	{
		[XmlElement("siteRef")]
		public string siteRef { get; set; }

		[XmlElement("weight")]
		public int weight { get; set; }
	}
}