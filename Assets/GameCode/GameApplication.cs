using FNZ.Client;
using FNZ.Client.StaticData;
using FNZ.Server;
using UnityEngine;

public class GameApplication : MonoBehaviour
{
	public void Start()
	{
		if (NetData.NET_MODE == NetMode.HOST)
		{
			gameObject.AddComponent<GameServer>();
		}
		gameObject.AddComponent<GameClient>();
	}
}
