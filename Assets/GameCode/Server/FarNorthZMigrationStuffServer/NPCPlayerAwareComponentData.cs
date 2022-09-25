using FNZ.Shared.Model;
using System;
using System.Collections.Generic;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class NPCPlayerAwareComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(NPCPlayerAwareComponentServer);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return false;
		}
	}
}