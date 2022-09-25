using FNZ.Shared.Model.Entity.Components;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FlowFieldComponentServer : FNEComponent
	{
		public FNEFlowField sightFlowField = null;
		public FNEFlowField soundFlowField = null;
		public FNEFlowField generalFlowField = null;

		public void ResetAllFlowFields()
		{
			sightFlowField = null;
			soundFlowField = null;
			generalFlowField = null;
		}

		public bool IsAllFlowFieldsInactive()
		{
			return sightFlowField == null && soundFlowField == null && generalFlowField == null;
		}

		public override ushort GetSizeInBytes() { return 0; }
	}
}