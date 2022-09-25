using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.BaseTerminal 
{

	[XmlType("BaseTerminalComponentData")]
	public class BaseTerminalComponentData : DataComponent 
	{
		public override Type GetComponentType()
		{
			return typeof(BaseTerminalComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return true;
		}
	}
}