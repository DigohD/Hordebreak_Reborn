using System;

namespace FNZ.Client.StaticData
{
	public enum NetMode
	{
		HOST = 0,
		JOIN = 1
	}

	public static class NetData
	{
		public static NetMode NET_MODE = NetMode.HOST;
		public static string LOCAL_PLAYER_NAME = "";
		public static string IP_ADRESS = "";
		public static int PORT = 0;
		public static DateTime PING_TIMESTAMP;
	}
}