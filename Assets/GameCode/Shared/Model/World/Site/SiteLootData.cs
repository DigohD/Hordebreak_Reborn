using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Site 
{

	public enum AmountDefinition
	{
		[XmlEnum("Scarce")]
		SCARCE = 0,
		[XmlEnum("Moderate")]
		MODERATE = 1,
		[XmlEnum("Abundant")]
		ABUNDANT = 2,

		[XmlEnum("Event")]
		EVENT = 3,
		[XmlEnum("Occasional")]
		OCCASIONAL = 4,
	}



	[XmlType("SiteLootData")]
	public class SiteLootData
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amountDefinition")]
		public AmountDefinition amountDefinition;
	}
}