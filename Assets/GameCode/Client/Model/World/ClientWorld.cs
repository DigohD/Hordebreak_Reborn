using FNZ.Shared.Model.World;

namespace FNZ.Client.Model.World
{
	public class ClientWorld : GameWorld
	{
		public ClientMapManager WorldMap;

		public ClientWorld()
		{
			
			
		}
		
		public override void InitializeWorld<T>()
		{
			base.InitializeWorld<T>();
			
			WorldMap = new ClientMapManager();
		}
	}
}

