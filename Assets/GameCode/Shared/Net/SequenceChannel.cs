namespace FNZ.Shared.Net
{
	public enum SequenceChannel : byte
	{
		DEFAULT = 0,
		WORLD_STATE = 1,
		WORLD_SETUP = 2,
		ENTITY_STATE = 3,
		PLAYER_STATE = 4,
		ITEM_STATE = 5,
		DEBUG = 6,
		CHAT = 7,
		EFFECT = 8,
		NOTIFICATION = 9,
		ERROR_MESSAGE = 10,
		WORLD_MAP_STATE = 11,
		QUEST,
	}
}

