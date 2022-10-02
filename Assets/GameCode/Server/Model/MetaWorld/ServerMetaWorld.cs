using FNZ.Shared.Model.World.MetaWorld;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FNZ.Server.Model.MetaWorld 
{
	public class ServerMetaWorld : MetaWorldModel
	{
		public ServerMetaWorld()
        {

        }

		public void CreateNewWorld()
        {
			Places = MetaWorldGenerator.GenerateStartWorld();
        }

		public void LoadFromFile()
        {
			var path = GameServer.FilePaths.GetMetaWorldFilePath();
			if (File.Exists(path))
			{
				var netBuffer = new NetBuffer
				{
					Data = FileUtils.ReadFile(path)
				};

				DeserializeMetaWorld(netBuffer);
			}
		}
	}
}