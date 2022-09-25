using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components;
using FNZ.Server.Model.Entity.Components.Door;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.Entity.Components.Spawner;
using FNZ.Server.Model.World;
using FNZ.Server.Services.QuestManager;
using FNZ.Server.Utils;
using FNZ.Shared.Constants;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Misc;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.NetworkManager.Chat
{
	public class ChatCommands
	{
		public void ExecuteCommands(string[] playerStringParts, bool isOP, NetIncomingMessage incMsg)
		{
			string command = playerStringParts[0];
			string errorCommand;

			if (isOP)
				switch (command)
				{
					case ChatCommandConstants.COMMAND_AFK:
						AFK(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_BAN:
						Ban(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_DEBUG:
						Debug(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_DEOP:
						Deop(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_EFFECT:
						Effect(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_EMOTE:
						Emote(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_EVENT:
						Event(playerStringParts, incMsg.SenderConnection);
						break;

					//DEV_ONLY
					case ChatCommandConstants.COMMAND_FLOWFIELD:
						FlowField(playerStringParts, incMsg.SenderConnection);
						break;
					//DEV_ONLY

					case ChatCommandConstants.COMMAND_GIVE:
						Give(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_GLOBAL:
						Global(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_GODMODE:
						GodMode(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_HELP:
						Help(incMsg.SenderConnection, isOP);
						break;
					case ChatCommandConstants.COMMAND_HOME:
						Home(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_KICK:
						Kick(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_KILL:
						Kill(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_MATURECROPS:
						MatureCrops(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_SMITE:
					case ChatCommandConstants.COMMAND_MJOLNIR:
						Mjolnir(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_MUSIC:
						Music(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_MUTE:
						Mute(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_NOTIFICATION:
						Notification(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_OP:
						OP(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_OPALL:
						OPAll();
						break;
					case ChatCommandConstants.COMMAND_PARDON:
						Pardon(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_PING:
						Ping(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_PLAYERS:
						ListConnectedPlayers(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_PURGE:
						Purge(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_QUEST:
						Quest(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_REPLENISH:
						Replenish(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_RESTOCK:
						Restock(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_TESTPACKAGE:
						TestPackage(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.PINK:
						Pink(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.RED:
						Red(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.BLUE:
						Blue(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.BROWN:
						Brown(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_ROLL:
						Roll(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_SETHOME:
						SetHome(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_SPAWN:
						Spawn(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_TIME:
						Time(playerStringParts, incMsg.SenderConnection, isOP);
						break;
					case ChatCommandConstants.COMMAND_TPALL:
						TPAll(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_TP:
						TP(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_UNLOCKALL:
						UnlockAll(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_UNMUTE:
						Unmute(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_WHISPER:
						Whisper(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_KILL_ALL:
						KillAll();
						break;
					case ChatCommandConstants.COMMAND_SPAWNERS:
						if (playerStringParts[1] == "on") SpawnerComponentServer.runSpawn = true;
						if (playerStringParts[1] == "off") SpawnerComponentServer.runSpawn = false;
						break;
					case ChatCommandConstants.COMMAND_OPEN_GATE:
						var chunks = GameServer.ChunkManager.GetPlayerChunkState(incMsg.SenderConnection).CurrentlyLoadedChunks;
						foreach (var chunk in chunks)
						{
							foreach (var eo in chunk.SouthEdgeObjects)
							{
								if(eo == null)
									continue;
								
								if (eo.EntityId.Equals("eo_city_miscwalls_laser_fence_gate_l") ||
								    eo.EntityId.Equals("eo_city_miscwalls_laser_fence_gate_r"))
								{
									eo.GetComponent<DoorComponentServer>().OpenDoor();
								}
							}
							
							foreach (var eo in chunk.WestEdgeObjects)
							{
								if(eo == null)
									continue;
								
								if (eo.EntityId.Equals("eo_city_miscwalls_laser_fence_gate_l") ||
								    eo.EntityId.Equals("eo_city_miscwalls_laser_fence_gate_r"))
								{
									eo.GetComponent<DoorComponentServer>().OpenDoor();
								}
							}
						}
						break;
					default:
						errorCommand = Localization.GetString("wrong_emote_or_command_error_message");
						GameServer.NetAPI.Chat_SendMessage_STC(errorCommand, incMsg.SenderConnection, ChatColorMessage.MessageType.ERROR);
						break;
				}
			else
				switch (command)
				{
					case ChatCommandConstants.COMMAND_AFK:
						AFK(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_DEBUG:
						Debug(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_EMOTE:
						Emote(playerStringParts, incMsg.SenderConnection);
						break;
					//DEV_ONLY
					case ChatCommandConstants.COMMAND_FLOWFIELD:
						FlowField(playerStringParts, incMsg.SenderConnection);
						break;
					//DEV_ONLY
					case ChatCommandConstants.COMMAND_GLOBAL:
						Global(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_HELP:
						Help(incMsg.SenderConnection, isOP);
						break;
					case ChatCommandConstants.COMMAND_MUSIC:
						Music(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_MUTE:
						Mute(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_PING:
						Ping(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_PLAYERS:
						ListConnectedPlayers(incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_ROLL:
						Roll(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_TIME:
						Time(playerStringParts, incMsg.SenderConnection, isOP);
						break;
					case ChatCommandConstants.COMMAND_UNMUTE:
						Unmute(playerStringParts, incMsg.SenderConnection);
						break;
					case ChatCommandConstants.COMMAND_WHISPER:
						Whisper(playerStringParts, incMsg.SenderConnection);
						break;
					default:
						errorCommand = Localization.GetString("wrong_emote_or_command_error_message");
						GameServer.NetAPI.Chat_SendMessage_STC(errorCommand, incMsg.SenderConnection, ChatColorMessage.MessageType.ERROR);
						break;
				}
		}

		private void AFK(NetConnection senderConnection)
		{
			var playerComponent = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<PlayerComponentServer>();
			string afkString;

			afkString = playerComponent.Afk ? Localization.GetString("not_afk_message") : Localization.GetString("afk_message");
			GameServer.NetAPI.Chat_SendMessage_STC(afkString, senderConnection, ChatColorMessage.MessageType.COMMAND);

			playerComponent.Afk = !playerComponent.Afk;
			GameServer.NetAPI.Entity_UpdateComponent_BA(playerComponent);
		}

		private void Ban(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}
			else
			{
				//Code to ban a player from server here
			}
		}

		private void Debug(string[] playerStringParts, NetConnection senderConnection)
		{
			var debugOption = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("debug_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (debugOption != string.Empty)
			{
				switch (debugOption.ToLower())
				{
					default:
						break;
				}
			}

		}

		private void Deop(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerToDeOP = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
			string errorMessage, lostAdminMessage, removedMessage, notExistMessage, properSyntax;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerToDeOP != string.Empty)
			{
				var receivingPlayerConnection = GameServer.NetConnector.GetConnectionFromName(playerToDeOP);

				if (GameServer.NetConnector.GetServerHostConnection() == receivingPlayerConnection)
				{
					errorMessage = Localization.GetString("not_deop_host_message");
					GameServer.NetAPI.Chat_SendMessage_STC(errorMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
				}
				else if (receivingPlayerConnection != null)
				{
					var playerReceivingDeOP = GameServer.NetConnector.GetPlayerFromConnection(receivingPlayerConnection);
					playerReceivingDeOP.GetComponent<PlayerComponentServer>().IsOP = false;

					lostAdminMessage = Localization.GetString("lost_admin_message");
					removedMessage = Localization.GetString("removed_player_admin_message");

					removedMessage = string.Format(
						removedMessage,
						playerToDeOP
						);
					GameServer.NetAPI.Chat_SendMessage_STC(lostAdminMessage, receivingPlayerConnection, ChatColorMessage.MessageType.AFFECTED);
					GameServer.NetAPI.Chat_SendMessage_STC(removedMessage, senderConnection, ChatColorMessage.MessageType.COMMAND);
				}
				else
				{
					notExistMessage = Localization.GetString("not_exist_message");
					notExistMessage = string.Format(
						notExistMessage,
						playerToDeOP
						);
					GameServer.NetAPI.Chat_SendMessage_STC(notExistMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
				}
			}
		}

		private void Effect(string[] playerStringParts, NetConnection senderConnection)
		{
			string effectId = playerStringParts.Length >= 2 ? playerStringParts[1].ToLower() : string.Empty;
			var playerEntity = playerStringParts.Length >= 3 ? GameServer.NetConnector.GetPlayerFromName(playerStringParts[2]) : null;
			float2 coordinates = float2.zero;
			bool boolX = false;
			bool boolY = false;

			if (playerStringParts.Length >= 4)
			{
				boolX = float.TryParse(playerStringParts[2], out coordinates.x);
				boolY = float.TryParse(playerStringParts[3], out coordinates.y);
			}

			if (playerEntity != null)
				GameServer.NetAPI.Effect_SpawnEffect_BAR(effectId, playerEntity.Position, 0);
			else if (boolX && boolY)
				GameServer.NetAPI.Effect_SpawnEffect_BAR(effectId, coordinates, 0);
			else
			{
				string availableEffects = Localization.GetString("available_effects_header_mesage");
				var effectIds = DataBank.Instance.GetAllDataIdsOfType<EffectData>();

				foreach (var def in effectIds)
				{
					availableEffects += $"\n{def.Id}";
				}

				string effectLocation = Localization.GetString("effect_location_message");
				availableEffects += "\n" + effectLocation;

				GameServer.NetAPI.Chat_SendMessage_STC(availableEffects, senderConnection, ChatColorMessage.MessageType.COMMAND);
			}
		}

		private void Emote(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerString = playerStringParts.Length >= 2 ? string.Join(" ", playerStringParts) : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("emote_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			playerString = playerString.Remove(0, 2);
			FNEEntity emotingPlayer = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			string emotingPlayerName = emotingPlayer.GetComponent<NameComponentServer>().entityName + " ";
			emotingPlayerName = ChatColorMessage.ColorMessage(emotingPlayerName, ChatColorMessage.MessageType.PLAYERNAME);

			GameServer.NetAPI.Chat_SendMessage_BAR(emotingPlayerName + playerString, emotingPlayer.Position, ChatColorMessage.MessageType.EMOTE);
		}

		private void Event(string[] playerStringParts, NetConnection senderConnection)
		{
			string eventName = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("event_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			//Event code here!
		}

		private void FlowField(string[] playerStringParts, NetConnection senderConnection)
		{
			int radius = 3;
			if (playerStringParts.Length > 1 && playerStringParts[1] != "")
				int.TryParse(playerStringParts[1], out radius);

			var playerEntity = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var ff = new FNEFlowField(playerEntity.Position, radius);
			GameServer.NetAPI.World_SendFlowField_STC(ff, senderConnection);
		}

		private void Global(string[] playerStringParts, NetConnection senderConnection)
		{
			string message = playerStringParts.Length >= 2 ? string.Join(" ", playerStringParts) : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("global_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			message = message.Remove(0, 2);
			string sendingPlayerName = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<NameComponentServer>().entityName;
			string nameColorized = ChatColorMessage.ColorMessage($"{sendingPlayerName}: ", ChatColorMessage.MessageType.PLAYERNAME);

			GameServer.NetAPI.Chat_SendMessage_BA(nameColorized + message, ChatColorMessage.MessageType.GLOBAL, senderConnection.RemoteUniqueIdentifier);
		}

		private void Give(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("give_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerStringParts.Length >= 2)
			{
				string errorMessage;
				string itemString = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
				string receivingPlayerNameString = playerStringParts.Length >= 4 ? playerStringParts[3] : string.Empty;
				short amount = 1;

				if (playerStringParts.Length >= 3)
					short.TryParse(playerStringParts[2], out amount);

				//Checks to secure input.
				if (itemString == string.Empty)
				{
					errorMessage = Localization.GetString("not_specified_error_message");
					GameServer.NetAPI.Chat_SendMessage_STC(errorMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
					return;
				}
				else if (!DataBank.Instance.DoesIdExist<ItemData>(itemString))
				{
					errorMessage = Localization.GetString("not_correct_item_name_error_message");
					GameServer.NetAPI.Chat_SendMessage_STC(errorMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
					return;
				}
				else if (amount == 0)
				{
					errorMessage = Localization.GetString("not_applied_amount_error_message");
					GameServer.NetAPI.Chat_SendMessage_STC(errorMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
					return;
				}
				else
				{
					ItemData itemData = DataBank.Instance.GetData<ItemData>(itemString);

					//Convert excessive input amount to the item's maximum stack size.
					if (amount > DataBank.Instance.GetData<ItemData>(itemString).maxStackSize)
						amount = (short)DataBank.Instance.GetData<ItemData>(itemString).maxStackSize;
					Item item = Item.GenerateItem(itemString, amount, true);

					//is another player receiving the item?
					if (receivingPlayerNameString != string.Empty)
					{
						//is that player connected?
						var receivingPlayerConnection = GameServer.NetConnector.GetConnectionFromName(receivingPlayerNameString);
						if (receivingPlayerConnection != null)
							GiveItemToOther(item, itemData, receivingPlayerConnection, senderConnection);
						else
						{
							errorMessage = Localization.GetString("not_exist_message");
							errorMessage = string.Format(
								errorMessage,
								receivingPlayerNameString
							);

							GameServer.NetAPI.Chat_SendMessage_STC(errorMessage, senderConnection, ChatColorMessage.MessageType.ERROR);
						}
					}
					else
						GiveItemToSelf(item, itemData);
				}

				void GiveItemToSelf(Item item, ItemData itemData)
				{
					var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
					var playerInventoryComponent = player.GetComponent<InventoryComponentServer>();
					string message;
					ChatColorMessage.MessageType channel;

					//Is there enough room in inventory?
					if (playerInventoryComponent.AutoPlaceIfPossible(item))
					{
						message = Localization.GetString("give_item_was_added_message");
						message = string.Format(
							message,
							amount,
							Localization.GetString(itemData.nameRef)
						);
						channel = ChatColorMessage.MessageType.COMMAND;
					}
					else
					{
						message = Localization.GetString("give_item_inventory_full_error_message");
						message = string.Format(
							message,
							Localization.GetString(itemData.nameRef)
						);
						channel = ChatColorMessage.MessageType.ERROR;
					}

					GameServer.NetAPI.Entity_UpdateComponent_STC(playerInventoryComponent, senderConnection);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, channel);
				}

				void GiveItemToOther(Item item, ItemData itemData, NetConnection receivingPlayerConnection, NetConnection sendingPlayerConnection)
				{
					var receivingPlayer = GameServer.NetConnector.GetPlayerFromConnection(receivingPlayerConnection);
					var sendingPlayer = GameServer.NetConnector.GetPlayerFromConnection(sendingPlayerConnection);
					string receivingPlayerName = receivingPlayer.GetComponent<NameComponentServer>().entityName;
					string sendingPlayerName = sendingPlayer.GetComponent<NameComponentServer>().entityName;
					var receivingPlayerInventoryComponent = receivingPlayer.GetComponent<InventoryComponentServer>();

					string toSelf, toTarget;
					ChatColorMessage.MessageType channelToSelf, channelToTarget;

					//Is there enough room in inventory?
					if (receivingPlayerInventoryComponent.AutoPlaceIfPossible(item))
					{
						toSelf = Localization.GetString("give_item_to_other_message");
						toSelf = string.Format(
							toSelf,
							amount,
							Localization.GetString(itemData.nameRef),
							receivingPlayerName
						);

						toTarget = Localization.GetString("give_item_reciever_message");
						toTarget = string.Format(
							toTarget,
							amount,
							Localization.GetString(itemData.nameRef),
							sendingPlayerName
						);

						channelToSelf = ChatColorMessage.MessageType.COMMAND;
						channelToTarget = ChatColorMessage.MessageType.AFFECTED;

						GameServer.NetAPI.Entity_UpdateComponent_STC(receivingPlayerInventoryComponent, receivingPlayerConnection);
						GameServer.NetAPI.Chat_SendMessage_STC(toTarget, receivingPlayerConnection, channelToTarget);
					}
					else
					{
						toSelf = Localization.GetString("can_not_give_item_error_message");
						toSelf = string.Format(
							toSelf,
							receivingPlayerName
							);
						channelToSelf = ChatColorMessage.MessageType.ERROR;
					}

					GameServer.NetAPI.Chat_SendMessage_STC(toSelf, senderConnection, channelToSelf);
				}
			}
		}

		private void GodMode(NetConnection senderConnection)
		{
			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var playerStatComponent = player.GetComponent<StatComponentServer>();

			if (playerStatComponent.defenseTypeRefOverride != DefenseTypesConstants.DEFENSE_IMMUNE)
			{
				playerStatComponent.SetDefenseTypeOverride(DefenseTypesConstants.DEFENSE_IMMUNE);
				string message = Localization.GetString("godmode_activated");
				GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);
			}
			else
			{
				playerStatComponent.defenseTypeRefOverride = string.Empty;
				string message = Localization.GetString("godmode_deactivated");
				GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);
			}

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerStatComponent, senderConnection);
		}

		private void Help(NetConnection senderConnection, bool isOP)
		{
			string availibleCommands = Localization.GetString("available_commands_header_message");

			if (isOP)
			{
				string[] OPCommands = new string[] {
					$"\n{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_AFK,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("afk_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_BAN,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("ban_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_DEBUG,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("debug_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_DEOP,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("deop_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_EFFECT,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("effect_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_EMOTE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("emote_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_EVENT,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("event_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_GIVE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("give_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_GLOBAL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("global_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_GODMODE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("godmode_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_HELP,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("help_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_HOME,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("home_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_KICK,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("kick_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_KILL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("kill_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_MJOLNIR,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("mjolnir_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_MUTE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("mute_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_OP,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("op_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PARDON,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("pardon_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PING,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("ping_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PLAYERS,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("players_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PURGE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("purge_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_REPLENISH,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("replenish_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_RESTOCK,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("restock_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_ROLL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("roll_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_SETHOME,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("sethome_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_SPAWN,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("spawn_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_TIME,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("time_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_TP,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("teleport_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_TPALL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("teleportall_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_UNMUTE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("unmute_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_WHISPER,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("whisper_help_message")}",
					$"{ChatColorMessage.ColorMessage("/" + ChatCommandConstants.COMMAND_QUEST, ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("quest_help_message")}" };

				foreach (string command in OPCommands)
				{
					availibleCommands += command + "\n";
				}
			}
			else
			{
				string[] nonOPCommands = new string[] {
					$"\n{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_AFK,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("afk_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_EMOTE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("emote_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_GLOBAL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("global_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_HELP,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("help_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PING,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("ping_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_PLAYERS,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("players_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_ROLL,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("roll_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_TIME,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("time_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_UNMUTE,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("unmute_help_message")}",
					$"{ChatColorMessage.ColorMessage("/"+ChatCommandConstants.COMMAND_WHISPER,ChatColorMessage.MessageType.EMOTE)}: {Localization.GetString("whisper_help_message")}" };

				foreach (string command in nonOPCommands)
				{
					availibleCommands += command + "\n";
				}
			}

			GameServer.NetAPI.Chat_SendMessage_STC(availibleCommands, senderConnection, ChatColorMessage.MessageType.COMMAND);
		}

		private void Home(NetConnection senderConnection)
		{
			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var destination = player.GetComponent<PlayerComponentServer>().HomeLocation;

			GameServer.NetAPI.Player_Teleport_STC(senderConnection, destination);
		}

		private void Kick(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerStringParts.Length >= 2)
			{
				var playerToKickConnection = GameServer.NetConnector.GetConnectionFromName(playerStringParts[1]);

				if (playerToKickConnection != null)
				{
					playerToKickConnection.Disconnect(Localization.GetString("kicked_from_server_message"));
				}
			}
		}

		private void Kill(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerToKillString = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerToKillString != string.Empty)
			{
				var playerToKillConnection = GameServer.NetConnector.GetConnectionFromName(playerStringParts[1]);

				if (playerToKillConnection != null)
				{
					var playerToKill = GameServer.NetConnector.GetPlayerFromConnection(playerToKillConnection);
					var statComponent = playerToKill.GetComponent<StatComponentServer>();
					string adminKilled = Localization.GetString("killed_by_admin_message");

					statComponent.CurrentHealth = 0;

					GameServer.NetAPI.Entity_UpdateComponent_BAR(statComponent);
					GameServer.NetAPI.Chat_SendMessage_STC(adminKilled, playerToKillConnection, ChatColorMessage.MessageType.COMMAND);
				}
			}
		}

		private void ListConnectedPlayers(NetConnection senderConnection)
		{
			var connectedPlayers = GameServer.NetConnector.GetConnectedClientConnections();
			var offlinePlayers = GameServer.NetConnector.GetOfflineClients();
			string message = Localization.GetString("connected_players_message");
			string playerName;
			bool isAfk;

			foreach (var player in connectedPlayers)
			{
				playerName = GameServer.NetConnector.GetPlayerFromConnection(player).GetComponent<NameComponentServer>().entityName;
				offlinePlayers.Remove(playerName);
				playerName = ChatColorMessage.ColorMessage(playerName, ChatColorMessage.MessageType.PLAYERNAME);
				isAfk = GameServer.NetConnector.GetPlayerFromConnection(player).GetComponent<PlayerComponentServer>().Afk;

				if (isAfk)
					playerName = "*AFK* " + playerName;

				message += $"\n{playerName}";
			}

			message += ChatColorMessage.ColorMessage("\n\nOffline Players:", ChatColorMessage.MessageType.SERVER);

			foreach (var offlinePlayerName in offlinePlayers)
			{
				playerName = offlinePlayerName;
				playerName = ChatColorMessage.ColorMessage(playerName, ChatColorMessage.MessageType.SERVER);
				message += $"\n{playerName}";
			}

			GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.WARNING);
		}

		private void MatureCrops(NetConnection senderConnection)
		{
			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);

			var tiles = GameServer.World.GetSurroundingTilesInRadius((int2)player.Position, 2);
			foreach (var tile in tiles)
			{
				var to = GameServer.World.GetTileObject(tile.x, tile.y);
				if (to != null)
				{
					var cropComp = to.GetComponent<CropComponentServer>();
					if (cropComp != null)
						cropComp.growth = cropComp.Data.growthTimeTicks - 0.01f;
				}
			}
		}

		private void Mjolnir(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			var targetPlayerConnection = GameServer.NetConnector.GetConnectionFromName(playerStringParts[1]);
			if (targetPlayerConnection != null)
			{
				var targetPlayerEntity = GameServer.NetConnector.GetPlayerFromConnection(targetPlayerConnection);
				string mjolnirMessage = Localization.GetString("mjolnir_message");

				if (playerStringParts.Length >= 3 && int.TryParse(playerStringParts[2], out int mjolnirDamage))
				{
					var targetPlayerHealthComponent = targetPlayerEntity.GetComponent<StatComponentServer>();
					targetPlayerHealthComponent.Server_ApplyDamage(mjolnirDamage, DamageTypesConstants.TRUE_DAMAGE);

					GameServer.NetAPI.Entity_UpdateComponent_BAR(targetPlayerHealthComponent);
					GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.MJOLNIR, targetPlayerEntity.Position, 0);
					GameServer.NetAPI.Chat_SendMessage_STC(mjolnirMessage, targetPlayerConnection, ChatColorMessage.MessageType.WARNING);
				}
				else
					GameServer.NetAPI.Chat_SendMessage_STC(Localization.GetString("mjolnir_damage_not_specified_message"), targetPlayerConnection, ChatColorMessage.MessageType.ERROR);
			}
			else
			{
				var message = Localization.GetString("not_exist_message");
				message = string.Format(message, playerStringParts[1]);
				GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.ERROR);
			}
		}

		private void Music(string[] playerStringParts, NetConnection senderConnection)
		{
			string id = playerStringParts.Length >= 2 ? playerStringParts[1].ToLower() : string.Empty;
			string timer = playerStringParts.Length >= 3 ? playerStringParts[2] : string.Empty;

			if (float.TryParse(timer, out float time))
				GameServer.NetAPI.Audio_ChangeMusic_STC(senderConnection, id, time);
			else if (id != string.Empty)
				GameServer.NetAPI.Audio_ChangeMusic_STC(senderConnection, id);
		}

		private void Mute(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerToMute = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
			string message, properSyntax;

			if (playerStringParts.Length == 1)
			{
				properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerToMute != string.Empty)
			{
				//is that player connected?
				var playerToMuteConnection = GameServer.NetConnector.GetConnectionFromName(playerToMute);

				if (playerToMuteConnection == senderConnection)
				{
					message = Localization.GetString("cant_mute_yourself_message");
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.ERROR);
					return;
				}

				if (playerToMuteConnection != null)
				{
					long playerToMuteId = GameServer.NetConnector.GetUniqueIdFromConnection(playerToMuteConnection);
					var muterPlayerComponent = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<PlayerComponentServer>();

					if (muterPlayerComponent.IsPlayerMuted(playerToMute))
					{
						message = Localization.GetString("already_muted_message");
						GameServer.NetAPI.Chat_SendMessage_STC($"{playerToMute.ToLower()} " + message, senderConnection, ChatColorMessage.MessageType.COMMAND);
					}
					else
					{
						muterPlayerComponent.MutePlayer(playerToMute, playerToMuteId);
						message = Localization.GetString("has_been_muted_message");
						GameServer.NetAPI.Chat_SendMessage_STC($"{playerToMute.ToLower()} " + message, senderConnection, ChatColorMessage.MessageType.COMMAND);
					}
				}
				else
				{
					message = Localization.GetString("not_exist_message");
					message = string.Format(
						message,
						playerToMute
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.ERROR);
				}
			}
			else
			{
				var mutedPlayers = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<PlayerComponentServer>().GetMutedPlayers();
				string mutedPlayersMessage = Localization.GetString("following_players_muted_message") + "\n";

				if (mutedPlayers == null || mutedPlayers.Count == 0)
					mutedPlayersMessage = Localization.GetString("no_players_muted_message");
				else
				{
					foreach (var player in mutedPlayers)
					{
						mutedPlayersMessage += player.Key + "\n";
					}
				}

				GameServer.NetAPI.Chat_SendMessage_STC(mutedPlayersMessage, senderConnection, ChatColorMessage.MessageType.COMMAND);
			}
		}

		private void Notification(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string messageToPlayer = Localization.GetString("notification_header_message") +
										"\n" + Localization.GetString("notification_reference_message") +
										"\n" + Localization.GetString("notification_example_message");

				GameServer.NetAPI.Chat_SendMessage_STC(messageToPlayer, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			string spriteId = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
			string color = playerStringParts.Length >= 3 ? playerStringParts[2] : string.Empty;
			string isPermanent = playerStringParts.Length >= 4 ? playerStringParts[3] : string.Empty;
			string message = string.Join(" ", playerStringParts);

			if (playerStringParts.Length >= 5)
			{
				if (!DataBank.Instance.DoesIdExist<SpriteData>(spriteId))
					DefaultMessage();
				else if (!DataBank.Instance.DoesIdExist<ColorData>(color))
					DefaultMessage();
				else if (isPermanent.ToLower() != "true" && isPermanent.ToLower() != "false")
					DefaultMessage();
				else
				{
					int startIndex = playerStringParts[0].Length + 1 + playerStringParts[1].Length + 1 + playerStringParts[2].Length + 1 + playerStringParts[3].Length + 1;
					var trimmedMessage = message.Substring(startIndex);
					if (trimmedMessage != string.Empty)
						GameServer.NetAPI.Notification_SendNotification_BA(spriteId, color, isPermanent, trimmedMessage);
					else
						DefaultMessage();
				}
			}
			else
				DefaultMessage();

			void DefaultMessage()
			{
				string messageToPlayer = Localization.GetString("notification_header_message") +
														   "\n" + Localization.GetString("notification_reference_message") +
														   "\n" + Localization.GetString("notification_example_message");

				GameServer.NetAPI.Chat_SendMessage_STC(messageToPlayer, senderConnection, ChatColorMessage.MessageType.COMMAND);
			}
		}

		private void OP(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerToOP = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerToOP != string.Empty)
			{
				//Is that player connected?
				var receivingPlayerConnection = GameServer.NetConnector.GetConnectionFromName(playerToOP);
				ChatColorMessage.MessageType channel;
				string message;

				if (receivingPlayerConnection != null)
				{
					channel = ChatColorMessage.MessageType.COMMAND;
					var playerReceivingOP = GameServer.NetConnector.GetPlayerFromConnection(receivingPlayerConnection);
					playerReceivingOP.GetComponent<PlayerComponentServer>().IsOP = true;

					message = Localization.GetString("receiving_player_granted_admin_message");
					GameServer.NetAPI.Chat_SendMessage_STC(message, receivingPlayerConnection, channel);

					message = Localization.GetString("give_admin_to_player_message");
					message = string.Format(
						message,
						playerReceivingOP.GetComponent<NameComponentServer>().entityName
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, channel);
				}
				else
				{
					channel = ChatColorMessage.MessageType.ERROR;
					message = Localization.GetString("not_exist_message");
					message = string.Format(
						message,
						playerToOP
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, channel);
				}
			}
		}

		private void OPAll()
		{
			string message = Localization.GetString("all_players_granted_admin_message");
			ChatColorMessage.MessageType channel = ChatColorMessage.MessageType.COMMAND;

			foreach (var player in GameServer.NetConnector.GetConnectedClientEntities())
            {
				player.GetComponent<PlayerComponentServer>().IsOP = true;
            }

			GameServer.NetAPI.Chat_SendMessage_BA(message, channel);
		}

		private void Ping(NetConnection senderConnection)
		{
			GameServer.NetAPI.Chat_SendMessage_STC(ChatCommandConstants.COMMAND_PING, senderConnection, ChatColorMessage.MessageType.DEFAULT);
		}

		private void Pardon(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			//Unban code here
		}

		private void Purge(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("purge_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			//Not sure what this command is supposed to do
		}

		private void Replenish(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("replenish_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			//replenish code here
		}

		private void Restock(string[] playerStringParts, NetConnection senderConnection)
		{
			string[] woodPlanks = new string[] { "give", "item_wood_planks", "20" };
			string[] woodLog = new string[] { "give", "item_wood_log", "20" };
			string[] stone = new string[] { "give", "item_stone", "20" };
			string[] scrapMetal = new string[] { "give", "item_scrap_metal", "20" };
			//string[] ironIngot = new string[] { "give", "iron_ingot", "20" };
			//string[] ironPart = new string[] { "give", "iron_part", "20" };
			//string[] steelPart = new string[] { "give", "steel_part", "20" };
			//string[] pineCone = new string[] { "give", "pinecone", "20" };
			//string[] krytsberrySeed = new string[] { "give", "kryst_berry_seed", "20" };
			//string[] cotton = new string[] { "give", "cotton", "20" };
			//string[] potato = new string[] { "give", "potato", "20" };
			//string[] charcoal = new string[] { "give", "charcoal", "20" };
			//string[] ironOre = new string[] { "give", "iron_ore", "20" };
			//string[] handGun = new string[] { "give", "salvage_handgun", "1" };
			//string[] machineGun = new string[] { "give", "salvage_machinegun", "1" };

			Give(woodPlanks, senderConnection);
			Give(woodLog, senderConnection);
			Give(woodLog, senderConnection);
			Give(stone, senderConnection);
			Give(scrapMetal, senderConnection);
		}

		private void TestPackage(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1)
			{
				Give(new string[] { "give", "armor_hi_tech_helmet", "1" }, senderConnection);
				Give(new string[] { "give", "armor_hi_tech_chest", "1" }, senderConnection);
				Give(new string[] { "give", "armor_hi_tech_gloves", "1" }, senderConnection);
				Give(new string[] { "give", "armor_hi_tech_pants", "1" }, senderConnection);
				Give(new string[] { "give", "armor_hi_tech_boots", "1" }, senderConnection);

				Give(new string[] { "give", "weapon_energy_lancer", "1" }, senderConnection);
				Give(new string[] { "give", "weapon_explolauncher", "1" }, senderConnection);

				return;
			}

			switch (playerStringParts[1])
            {
				case "red":
					Give(new string[] { "give", "armor_hi_tech_helmet_red", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_chest_red", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_gloves_red", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_pants_red", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_boots_red", "1" }, senderConnection);
				break;

				case "yellow":
				case "gold":
					Give(new string[] { "give", "armor_hi_tech_helmet_gold", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_chest_gold", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_gloves_gold", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_pants_gold", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_boots_gold", "1" }, senderConnection);
				break;

				case "green":
					Give(new string[] { "give", "armor_hi_tech_helmet_green", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_chest_green", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_gloves_green", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_pants_green", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_boots_green", "1" }, senderConnection);
				break;

				case "blue":
				default:
					Give(new string[] { "give", "armor_hi_tech_helmet", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_chest", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_gloves", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_pants", "1" }, senderConnection);
					Give(new string[] { "give", "armor_hi_tech_boots", "1" }, senderConnection);
				break;
            }

			Give(new string[] { "give", "weapon_energy_lancer", "1" }, senderConnection);
			Give(new string[] { "give", "weapon_explolauncher", "1" }, senderConnection);
		}
		
		private void Pink(string[] playerStringParts, NetConnection senderConnection)
		{
			string[] chest = new string[] { "give", "pink_jacket", "1" };
			string[] pants = new string[] { "give", "pink_pants", "1" };
			string[] boots = new string[] { "give", "armor_start_gear_boots", "1" };

			Give(chest, senderConnection);
			Give(pants, senderConnection);
			Give(boots, senderConnection);
		}
		
		private void Blue(string[] playerStringParts, NetConnection senderConnection)
		{
			string[] chest = new string[] { "give", "blue_jacket", "1" };
			string[] pants = new string[] { "give", "blue_pants", "1" };
			string[] boots = new string[] { "give", "armor_start_gear_boots", "1" };

			Give(chest, senderConnection);
			Give(pants, senderConnection);
			Give(boots, senderConnection);
		}
		
		private void Red(string[] playerStringParts, NetConnection senderConnection)
		{
			string[] chest = new string[] { "give", "red_jacket", "1" };
			string[] pants = new string[] { "give", "red_pants", "1" };
			string[] boots = new string[] { "give", "armor_start_gear_boots", "1" };

			Give(chest, senderConnection);
			Give(pants, senderConnection);
			Give(boots, senderConnection);
		}
		
		private void Brown(string[] playerStringParts, NetConnection senderConnection)
		{
			string[] chest = new string[] { "give", "armor_start_gear_jacket", "1" };
			string[] pants = new string[] { "give", "armor_start_gear_pants", "1" };
			string[] boots = new string[] { "give", "armor_start_gear_boots", "1" };

			Give(chest, senderConnection);
			Give(pants, senderConnection);
			Give(boots, senderConnection);
		}

		private void Roll(string[] playerStringParts, NetConnection senderConnection)
		{
			string rollRandomString = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			int.TryParse(rollRandomString, out int maxRollValue);
			maxRollValue = maxRollValue != 0 ? maxRollValue : 100;
			int randomNumber = FNERandom.GetRandomIntInRange(0, maxRollValue) + 1;

			var playerEntity = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			string playerName = playerEntity.GetComponent<NameComponentServer>().entityName;
			playerName = ChatColorMessage.ColorMessage($"{playerName}", ChatColorMessage.MessageType.PLAYERNAME);

			string message = Localization.GetString("roll_message");
			message = string.Format(message, playerName, randomNumber, 1, maxRollValue);

			GameServer.NetAPI.Chat_SendMessage_BAR(message, playerEntity.Position, ChatColorMessage.MessageType.COMMAND);
		}

		private void SetHome(NetConnection senderConnection)
		{
			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var playerComponent = player.GetComponent<PlayerComponentServer>();
			playerComponent.HomeLocation = player.Position;
			string message = Localization.GetString("change_home_location_message");

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerComponent, senderConnection);
			GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);
		}

		private void Spawn(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("spawn_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			var xCoordinate = int.Parse(playerStringParts[1]);
			var yCoordinate = int.Parse(playerStringParts[2]);
			var xAmount = int.Parse(playerStringParts[3]);
			var yAmount = int.Parse(playerStringParts[4]);

			for (int y = 0; y < yAmount; y++)
			{
				for (int x = 0; x < xAmount; x++)
				{
					//var e = GameServer.EntityFactory.CreateEnemy("forest_shepherd", new float2(xCoordinate + x, yCoordinate + y), 0);
					//GameServer.NetAPI.Entity_SpawnEntity_BAR(e);
				}
			}

		}

		private void Time(string[] playerStringParts, NetConnection senderConnection, bool isOp)
		{
			if (isOp && playerStringParts.Length >= 2)
			{
				foreach (var word in playerStringParts)
				{
					if (bool.TryParse(word.ToLower(), out bool freezeTime))
						GameServer.World.Environment.FreezeTime = freezeTime;
					else if (byte.TryParse(word, out byte serverHour))
						GameServer.World.Environment.Hour = serverHour;
					else if (word.ToLower() == "freeze" || word.ToLower() == "stop")
						GameServer.World.Environment.FreezeTime = true;
					else if (word.ToLower() == "unfreeze" || word.ToLower() == "go")
						GameServer.World.Environment.FreezeTime = false;
				}
			}

			GameServer.NetAPI.Chat_SendMessage_STC($"Current time of day is: {GameServer.World.Environment.Hour}",
				senderConnection, ChatColorMessage.MessageType.SERVER);
		}

		private void TPAll(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("teleportall_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerStringParts.Length >= 2)
			{
				FNEEntity entity;
				float2 destination;

				if (playerStringParts[1].ToLower() == "home")
				{
					entity = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
					destination = entity.GetComponent<PlayerComponentServer>().HomeLocation;
				}
				else
				{
					entity = GameServer.NetConnector.GetPlayerFromName(playerStringParts[1]);

					if (entity != null)
						destination = entity.Position;
					else
						return;
				}

				foreach (var player in GameServer.NetConnector.GetConnectedClientConnections())
				{
					GameServer.NetAPI.Player_Teleport_STC(player, destination);
				}
			}
		}

		private void TP(string[] playersStringParts, NetConnection senderConnection)
		{
			if (playersStringParts.Length == 1 || playersStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("teleport_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playersStringParts.Length >= 3)
			{
				var playerToTeleportConnection = GameServer.NetConnector.GetConnectionFromName(playersStringParts[1]);
				var targetPlayer = GameServer.NetConnector.GetPlayerFromName(playersStringParts[1]);

				if (playerToTeleportConnection != null)
				{
					FNEEntity entity;
					float2 location = targetPlayer.Position;
					float2 teleportDestination;
					string teleportDestinationPlayerName = string.Empty;

					if (playersStringParts[2].ToLower() == "home" || playersStringParts[2].ToLower() == "myhome")
					{
						if (playersStringParts[2].ToLower() == "myhome")
							entity = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
						else
							entity = GameServer.NetConnector.GetPlayerFromConnection(playerToTeleportConnection);

						teleportDestinationPlayerName = $"{entity.GetComponent<NameComponentServer>().entityName}'s home";
						teleportDestination = entity.GetComponent<PlayerComponentServer>().HomeLocation;
					}
					else
					{
						entity = GameServer.NetConnector.GetPlayerFromName(playersStringParts[2]);

						if (entity != null)
						{
							teleportDestinationPlayerName = entity.GetComponent<NameComponentServer>().entityName;
							teleportDestination = entity.Position;
						}
						else
							return;
					}

					GameServer.NetAPI.Player_Teleport_STC(playerToTeleportConnection, teleportDestination);
					GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.TELEPORT, location, 0);

					var teleportedPlayerName = GameServer.NetConnector.GetPlayerFromConnection(playerToTeleportConnection).GetComponent<NameComponentServer>().entityName;
					string message = Localization.GetString("player_was_teleported_message");
					message = string.Format(message, teleportedPlayerName, teleportDestinationPlayerName);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);

					message = Localization.GetString("you_were_teleported_message");
					message = string.Format(message, teleportDestinationPlayerName);
					GameServer.NetAPI.Chat_SendMessage_STC(message, playerToTeleportConnection, ChatColorMessage.MessageType.AFFECTED);
				}
			}
		}

		private void UnlockAll(NetConnection senderConnection)
		{
			var playerComponent = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<PlayerComponentServer>();

			foreach (var buildingData in DataBank.Instance.GetAllDataIdsOfType<BuildingData>())
			{
				playerComponent.UnlockBuilding(buildingData.Id);
			}

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerComponent, senderConnection);
		}

		private void Unmute(string[] playerStringParts, NetConnection senderConnection)
		{
			string playerToUnMute = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
			string message, properSyntax;

			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				properSyntax = Localization.GetString("name_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			if (playerToUnMute != string.Empty)
			{
				var playerComp = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<PlayerComponentServer>();

				if (playerComp.IsPlayerMuted(playerToUnMute))
				{
					playerComp.UnmutePlayer(playerToUnMute);
					message = Localization.GetString("has_been_unmuted_message");
					message = string.Format(message, playerToUnMute.ToLower());
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);
				}
				else
				{
					message = Localization.GetString("is_not_muted_message");
					message = string.Format(message, playerToUnMute.ToLower());
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.COMMAND);
				}
			}
		}

		private void Whisper(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("whisper_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			string receivingPlayerName = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;
			string whisperMessage = playerStringParts.Length >= 3 ? string.Join(" ", playerStringParts) : string.Empty;
			string message;

			if (receivingPlayerName != string.Empty && whisperMessage != string.Empty)
			{
				string sendingPlayerName;
				bool whisperedIsAfk;
				whisperMessage = whisperMessage.Remove(0, 3 + receivingPlayerName.Length);

				//Is that player connected?
				var receivingPlayerConnection = GameServer.NetConnector.GetConnectionFromName(receivingPlayerName);

				if (receivingPlayerConnection != null)
				{
					sendingPlayerName = GameServer.NetConnector.GetPlayerFromConnection(senderConnection).GetComponent<NameComponentServer>().entityName;
					receivingPlayerName = GameServer.NetConnector.GetPlayerFromConnection(receivingPlayerConnection).GetComponent<NameComponentServer>().entityName;

					message = Localization.GetString("name_colon_message");
					message = string.Format(
						message,
						sendingPlayerName
						);
					sendingPlayerName = ChatColorMessage.ColorMessage(message, ChatColorMessage.MessageType.PLAYERNAME);
					message = Localization.GetString("to_name_colon_message");
					message = string.Format(
						message,
						receivingPlayerName
						);
					receivingPlayerName = ChatColorMessage.ColorMessage(message, ChatColorMessage.MessageType.PLAYERNAME);

					whisperedIsAfk = GameServer.NetConnector.GetPlayerFromConnection(receivingPlayerConnection).GetComponent<PlayerComponentServer>().Afk;

					if (whisperedIsAfk)
					{
						message = Localization.GetString("afk_text_message");
						receivingPlayerName = ChatColorMessage.ColorMessage(message, ChatColorMessage.MessageType.ERROR) + receivingPlayerName;
					}

					message = Localization.GetString("whisper_message");
					message = string.Format(
						message,
						receivingPlayerName,
						whisperMessage
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.WHISPER);

					message = Localization.GetString("whisper_message");
					message = string.Format(
						message,
						sendingPlayerName,
						whisperMessage
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, receivingPlayerConnection, ChatColorMessage.MessageType.WHISPER);
				}
				else
				{
					message = Localization.GetString("not_exist_message");
					message = string.Format(
						message,
						receivingPlayerName
						);
					GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.ERROR);
				}
			}
		}

		private void KillAll()
		{
			foreach (var c in GameServer.World.GetCurrentlyLoadedChunks())
			{
				foreach(var e in c.GetAllEnemies())
				{
					e.GetComponent<StatComponentServer>().Server_ApplyDamage(1000000, "");
				}
			}
		}

		private void Quest(string[] playerStringParts, NetConnection senderConnection)
		{
			if (playerStringParts.Length == 1 || playerStringParts[1] == "")
			{
				string properSyntax = Localization.GetString("quest_correct_syntax_message");
				GameServer.NetAPI.Chat_SendMessage_STC(properSyntax, senderConnection, ChatColorMessage.MessageType.COMMAND);
				return;
			}

			string questName = playerStringParts.Length >= 2 ? playerStringParts[1] : string.Empty;

			if (questName != string.Empty && DataBank.Instance.DoesIdExist<QuestData>(questName))
			{
				var questData = DataBank.Instance.GetData<QuestData>(questName);

				QuestManager.StartNewQuest(questData.Id);
			}
			else
			{
				string message = Localization.GetString("not_exist_message");
				message = string.Format(message, questName);
				GameServer.NetAPI.Chat_SendMessage_STC(message, senderConnection, ChatColorMessage.MessageType.ERROR);
			}
		}
	}
}