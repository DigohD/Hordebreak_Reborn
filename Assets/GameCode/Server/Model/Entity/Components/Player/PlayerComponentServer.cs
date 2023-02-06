using FNZ.Server.Controller;
using FNZ.Server.Controller.Systems;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.World;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Player;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using FNZ.Server.Model.World.Blueprint;
using Unity.Mathematics;

namespace FNZ.Server.Model.Entity.Components.Player
{
	public class PlayerComponentServer : PlayerComponentShared, ITickable
	{
		public ServerWorldChunk LastChunk;
		private FNEFlowField ff = null;

		public int2 lastTile = new int2(256, 256);

		private int shotAlertTimer = 0;
		private int sightRange = 14;
		private int sightUpdateTimer = 0;

		public bool IsOP = false;
		private bool alertTimerStarted = false;

		private ServerWorld _world;

		public override void Init()
		{
			base.Init();

			_world = GameServer.GetWorldInstance(ParentEntity.WorldInstanceIndex);
		}

		public override void InitComponentLinks()
		{
			base.InitComponentLinks();

			ParentEntity.GetComponent<InventoryComponentServer>().AutoPlaceIfPossible(Item.GenerateItem("simple_safety_gun"));
		}

		public override void FileSerialize(NetBuffer bw)
		{
			base.Serialize(bw);
			bw.Write(IsOP);
		}

		public override void FileDeserialize(NetBuffer br)
		{
			base.Deserialize(br);
			IsOP = br.ReadBoolean();
		}

		public void MutePlayer(string name, long playerToMuteId)
		{
			if (!m_MutedPlayers.ContainsKey(name.ToLower()))
				m_MutedPlayers.Add(name.ToLower(), playerToMuteId);

			GameServer.NetAPI.Entity_UpdateComponent_STC(this, GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity));
		}

		public void UnmutePlayer(string name)
		{
			if (m_MutedPlayers.ContainsKey(name.ToLower()))
				m_MutedPlayers.Remove(name.ToLower());

			GameServer.NetAPI.Entity_UpdateComponent_STC(this, GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity));
		}

		public void UnlockBuilding(string buildingId)
		{
			if (!unlockedBuildings.Contains(buildingId))
				unlockedBuildings.Add(buildingId);
		}

		//Here for later if we need them.
		public List<string> GetUnlockedBuildings()
		{
			return unlockedBuildings;
		}

		public bool HasUnlockedBuilding(string buildingId)
		{
			return unlockedBuildings.Contains(buildingId);
		}

		public void KillPlayer()
		{
			IsDead = true;
			NE_Send_KillPlayer();
		}

		public void NE_Send_KillPlayer()
		{
			GameServer.NetAPI.BA_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.KILL_PLAYER
			);
		}
		
		public void NE_Send_CurrentSiteUpdated()
		{
			GameServer.NetAPI.BA_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.UPDATE_CURRENT_SITE,
				new UpdateCurrentSiteData(m_CurrentSiteId, m_IsOnSite)
			);
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((PlayerNetEvent)incMsg.ReadByte())
			{
				case PlayerNetEvent.ANIMATOR_DATA:
					GameServer.NetAPI.Client_Immediate_Forward_To_Other(incMsg.Data, incMsg.SenderConnection);
					break;

				case PlayerNetEvent.REVIVE_PLAYER:
					NE_Receive_RevivePlayer(incMsg);
					break;

				case PlayerNetEvent.RESPAWN_REQUEST:
					NE_Receive_RespawnRequest(incMsg);
					break;

				case PlayerNetEvent.REQUEST_THROW_ITEM:
					NE_Receive_RequestThrowItem(incMsg);
					break;

				case PlayerNetEvent.REQUEST_PICK_UP_ITEM:
					NE_Receive_RequestPickUpItem(incMsg);
					break;
			}
		}

		public void NE_Receive_RevivePlayer(NetIncomingMessage incMsg)
		{
			if (IsDead)
			{
				RevivePlayer();
			}
		}

		public void NE_Receive_RespawnRequest(NetIncomingMessage incMsg)
		{
			if (IsDead)
			{
				var basePos = float2.zero;
				if (GameServer.RoomManager.TryGetClosestBase(ParentEntity.Position, out basePos))
				{
					GameServer.NetAPI.Player_Teleport_STC(incMsg.SenderConnection, basePos);
				}
				else
				{
					GameServer.NetAPI.Player_Teleport_STC(incMsg.SenderConnection, HomeLocation);
				}

				RevivePlayer();
			}
		}

		public void NE_Receive_RequestThrowItem(NetIncomingMessage incMsg)
		{
			if (ItemOnCursor == null)
				return;

			GameServer.ItemsOnGroundManager.SpawnItemOnGround(ParentEntity.Position, ItemOnCursor);
			ItemOnCursor = null;

			GameServer.NetAPI.Entity_UpdateComponent_STC(
				this,
				GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity)
			);
		}

		public void NE_Receive_RequestPickUpItem(NetIncomingMessage incMsg)
		{
			var data = new RequestPickUpItemData();
			data.Deserialize(incMsg);

			var item = GameServer.ItemsOnGroundManager.GetItemOnGround(data.Identifier);
			var inventory = ParentEntity.GetComponent<InventoryComponentServer>();

			if(inventory.AutoPlaceIfPossible(item) == false)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(Localization.GetString("inventory_full_message"), incMsg.SenderConnection);
				return;
			}

			if (item != null)
			{
				GameServer.ItemsOnGroundManager.PopItemOnGround(data.Identifier);
				GameServer.NetAPI.BA_Remove_Item_On_Ground(data.Identifier);
				GameServer.NetAPI.Entity_UpdateComponent_BA(inventory);
			}
		}
		
		private void RevivePlayer()
		{
			IsDead = false;
			GameServer.NetAPI.BA_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.REVIVE_PLAYER
			);
			var statComp = ParentEntity.GetComponent<StatComponentServer>();
			statComp.Heal(statComp.MaxHealth * 0.25f);
			GameServer.NetAPI.Entity_UpdateComponent_STC(statComp, GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity));
		}

		public virtual void SetItemOnCursor(Item newItem)
		{
			ItemOnCursor = newItem;

			GameServer.NetAPI.Entity_UpdateComponent_STC(
				this,
				GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity)
			);
		}

		public void Tick(float dt)
		{
			if (alertTimerStarted)
			{
				if (shotAlertTimer >= 1 / ServerMainSystem.TARGET_SERVER_TICK_TIME * 20)
				{
					alertTimerStarted = false;
					shotAlertTimer = 0;
				}
				else
				{
					shotAlertTimer++;
				}
			}

			if (!IsDead)
			{
				int2 playerTile = new int2(ParentEntity.Position);
				CheckForEnemies(playerTile);
			}
		}

		private void CheckForEnemies(int2 currentTile)
		{
			List<int2> inVicinity = _world.GetSurroundingTilesInRadius(currentTile, sightRange);
			bool shouldGenerateNewFlowField = false;

			foreach (var tile in inVicinity)
			{
				var enemies = _world.GetTileEnemies(tile);
				if (enemies == null || enemies.Count == 0) continue;

				bool seesPlayer = FNEPathfinding.HasLineOfSight(_world, currentTile, tile);

				foreach (var enemy in enemies)
				{
					if (enemy.EntityType != EntityType.ECS_ENEMY)
						continue;

					var awareComp = enemy.GetComponent<NPCPlayerAwareComponentServer>();

					if (seesPlayer)
					{
						var ffComp = enemy.GetComponent<FlowFieldComponentServer>();
						shouldGenerateNewFlowField = true;

						if (ffComp != null && (ffComp.sightFlowField == null || ffComp.sightFlowField != ff))
						{
							ffComp.sightFlowField = ff;
							awareComp.SeenAlert(FNERandom.GetRandomIntInRange(3, 6));
							//enemy.Agent.target = ParentEntity;
						}
					}

					awareComp.isSeeingPlayer = seesPlayer;
				}
			}

			if (sightUpdateTimer >= (1 / ServerMainSystem.TARGET_SERVER_TICK_TIME) * 2)
			{
				if (shouldGenerateNewFlowField)
				{
					ff = new FNEFlowField(_world, ParentEntity.Position, sightRange);

					shouldGenerateNewFlowField = false;
				}

				sightUpdateTimer = 0;
			}

			sightUpdateTimer++;

			//sight
			if (!(currentTile.x == lastTile.x && currentTile.y == lastTile.y)) //playerTile != lastTile
			{
				if (shouldGenerateNewFlowField)
				{
					ff = new FNEFlowField(_world, ParentEntity.Position, sightRange);
					shouldGenerateNewFlowField = false;
				}
				var tilePlayers = _world.GetTilePlayers(lastTile);
				if (tilePlayers != null) 
				{
					tilePlayers.Remove(ParentEntity); 
				}

				_world.AddPlayerToTile(ParentEntity);
			}

			lastTile = currentTile;
		}

		public void SetCurrentSite(bool isOnSite, WorldBlueprintGen.SiteMetaData siteData)
		{
			if (isOnSite != m_IsOnSite)
			{
				m_IsOnSite = isOnSite;
				// Entered site
				if (m_IsOnSite)
				{
					m_CurrentSiteId = siteData.siteId;
				}
				// Left site
				else
				{
					m_CurrentSiteId = "";
				}
				
				NE_Send_CurrentSiteUpdated();
			}
		}
	}
}
