using FNZ.Shared.Model;
using System;
using System.Collections.Generic;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FlowFieldComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(FlowFieldComponentServer);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return false;
		}
	}
}