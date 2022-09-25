namespace FNZ.Server.Services
{
	public static class FNEService
	{
		public static TileObjectAPI TileObject;
		public static EdgeObjectAPI EdgeObject;
		public static EntityEventAPI EntityEvent;
		public static EffectAPI Effect;
		public static EventAPI Event;
		public static FileAPI File;
		public static TileAPI Tile;

		static FNEService()
		{
			TileObject = new TileObjectAPI();
			EdgeObject = new EdgeObjectAPI();
			EntityEvent = new EntityEventAPI();
			Effect = new EffectAPI();
			Event = new EventAPI();
			File = new FileAPI();
			Tile = new TileAPI();
		}
	}
}