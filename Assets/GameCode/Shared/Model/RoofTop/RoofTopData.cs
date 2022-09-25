using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.RoofTop 
{
	[XmlType("RoofTopData")]
	public class RoofTopData : DataDef
	{
		[XmlArray("baseWalls")]
		[XmlArrayItem(typeof(DataComponent))]
		public List<DataComponent> components { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			throw new NotImplementedException();
		}
	}
}