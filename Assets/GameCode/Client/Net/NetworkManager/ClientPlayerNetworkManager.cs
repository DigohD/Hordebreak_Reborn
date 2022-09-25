using FNZ.Client.Model.Entity.Components.Name;
using FNZ.Client.Model.Entity.Components.Position;
using FNZ.Client.Model.Entity.Components.Rotation;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.Audio;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.PlayerStitching;
using FNZ.Client.View.Player.Systems;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientPlayerNetworkManager : INetworkManager
	{
		public ClientPlayerNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.SPAWN_LOCAL_PLAYER, OnSpawnLocalPlayer);
			GameClient.NetConnector.Register(NetMessageType.SPAWN_REMOTE_PLAYER, OnSpawnRemotePlayer);
			GameClient.NetConnector.Register(NetMessageType.REMOVE_REMOTE_PLAYER, OnRemoveRemotePlayer);
			GameClient.NetConnector.Register(NetMessageType.PLAYER_SERVER_CONNECTION_ERROR, OnPlayerServerConnectionError);
			GameClient.NetConnector.Register(NetMessageType.UPDATE_PLAYER_POS_AND_ROT, OnUpdatePlayerPosAndRot);
			GameClient.NetConnector.Register(NetMessageType.PLAYER_ANIMATION_EVENT, OnPlayerAnimationEvent);
			GameClient.NetConnector.Register(NetMessageType.PLAYER_TELEPORT, OnPlayerTeleportEvent);
		}

		private void OnSpawnLocalPlayer(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			string entityId = IdTranslator.Instance.GetId<FNEEntityData>(incMsg.ReadUInt16());
			var localPlayer = GameClient.EntityFactory.CreateLocalPlayerEntity(entityId);
			localPlayer.NetDeserialize(incMsg);

			GameClient.LocalPlayerEntity = localPlayer;

			GameObject view = GameClient.ViewAPI.SpawnPlayerView();
			view.GetComponent<PlayerController>().Init(localPlayer);

			GameClient.LocalPlayerView = view;

			net.SyncEntity(localPlayer);
			GameClient.ViewConnector.AddGameObject(view, localPlayer.NetId);
		}

		private void OnSpawnRemotePlayer(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			string entityId = IdTranslator.Instance.GetId<FNEEntityData>(incMsg.ReadUInt16());
			var remotePlayer = GameClient.EntityFactory.CreateRemotePlayerEntity(entityId);
			remotePlayer.NetDeserialize(incMsg);

			GameObject view = GameClient.ViewAPI.SpawnPlayerView();
			GameObject.Destroy(view.GetComponent<PlayerController>());

			view.AddComponent(typeof(PlayerControllerRemote));
			view.GetComponent<PlayerControllerRemote>().Init(remotePlayer);
			
			UIManager.Instance.NewUIArrow(remotePlayer.GetComponent<NameComponentClient>().entityName, remotePlayer.Position);
			GameClient.RemotePlayerViews.Add(view);
			GameClient.RemotePlayerEntities.Add(remotePlayer);

			net.SyncEntity(remotePlayer);
			GameClient.ViewConnector.AddGameObject(view, remotePlayer.NetId);

			view.tag = "RemotePlayer";
		}

		private void OnRemoveRemotePlayer(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var remotePlayer = net.GetEntity(incMsg.ReadInt32());
			foreach (var go in GameClient.RemotePlayerViews)
			{
				if (go.GetComponent<PlayerControllerRemote>().m_PlayerEntity == remotePlayer)
				{
					GameClient.RemotePlayerViews.Remove(go);
					GameObject.Destroy(go);
					break;
				}
			}

			UIManager.Instance.RemovePlayerFromMap(remotePlayer);
			UIManager.Instance.RemoveUIArrow(remotePlayer.Data.Id);
			GameClient.RemotePlayerEntities.Remove(remotePlayer);
			GameClient.ViewConnector.RemoveView(remotePlayer.NetId);

			net.UnsyncEntity(remotePlayer);
		}

		private void OnUpdatePlayerPosAndRot(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			// Koppla entities till motsvarande GameObject/ECS-Entity och uppdatera vyn till följd
			// av mottagna nätverkspaket. Gör det i dictionaries där NetId pekar på vy. Använd
			// XML-data för att ta reda på om entity är GO eller ECS.

			FNEEntity entity = net.GetEntity(incMsg.ReadInt32());

			if (entity == null) return;

			entity.DeserializePosition(incMsg);
			entity.DeserializeRotation(incMsg);

			UIManager.Instance.UpdateUIArrow(entity.Data.Id, entity.Position);
		}

		private void OnPlayerAnimationEvent(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity = net.GetEntity(incMsg.ReadInt32());

			if (entity == null) return;

			var view = GameClient.ViewConnector.GetGameObject(entity);

			view.GetComponent<PlayerControllerRemote>().PlayOneShotAnimation(
				(OneShotAnimationType)incMsg.ReadByte()
			);
		}

		private void OnPlayerTeleportEvent(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			float destinationX = incMsg.ReadFloat();
			float destinationY = incMsg.ReadFloat();

			var self = GameClient.LocalPlayerEntity;
			self.Position.x = destinationX;
			self.Position.y = destinationY;
		}

		private void OnPlayerServerConnectionError(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var errorName = incMsg.ReadString();
			var errorMessage = incMsg.ReadString();

			if (errorName.ToLower().Contains("name"))
			{
				UI_StartMenuErrors.NameTaken(errorName, errorMessage);
				AudioManager.Instance.PlayMusic("intro_music", 1);
				AudioManager.Instance.PlayAmbience("night", 1);
				SceneManager.LoadSceneAsync("StartMenu");
			}
		}
	}
}