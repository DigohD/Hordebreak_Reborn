using FNZ.Server.Model.Entity.Components.Consumer;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.Entity.Components.Environment;
using FNZ.Server.Model.Entity.Components.Producer;
using FNZ.Server.Model.Entity.Components.RoomRequirements;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.RoomRequirements;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;

namespace FNZ.Server.Model.World
{
	public class ServerRoom : Room
	{
		public ServerRoom(long newId) : base(newId)
		{

		}

		private Dictionary<string, byte> m_TilePropertyCounts = new Dictionary<string, byte>();
		private Dictionary<string, byte> m_WallPropertyCounts = new Dictionary<string, byte>();
		private List<int> m_RoomWalls = new List<int>();

		private List<FNEEntity> m_TileObjectsToCalculate = new List<FNEEntity>();
		private List<EdgeObjectComponentServer> m_MountedObjectsToCalculate = new List<EdgeObjectComponentServer>();

		public void InitRoomRecalculation()
		{
			// Clear values
			roomEnvironmentValues.Clear();
			roomEnvironmentAffectors.Clear();
			RoomProperties.Clear();
			m_TilePropertyCounts.Clear();
			m_TileObjectsToCalculate.Clear();
			m_MountedObjectsToCalculate.Clear();
            m_WallPropertyCounts.Clear();
            m_RoomWalls.Clear();
			Resources.Clear();

			// Calculate room properties
			foreach (var tilePos in Tiles)
			{
				var tileData = GameServer.MainWorld.GetTileData(tilePos.x, tilePos.y);

				foreach (var rp in tileData.roomPropertyRefs)
				{
					if (!m_TilePropertyCounts.ContainsKey(rp))
						m_TilePropertyCounts.Add(rp, 1);
					else
						m_TilePropertyCounts[rp]++;
				}

				var walls = GameServer.MainWorld.GetStraightDirectionsEdgeObjects(tilePos);
				foreach (var wall in walls)
				{
					if (wall == null)
						continue;

					if (!m_RoomWalls.Contains(wall.NetId))
					{
						m_RoomWalls.Add(wall.NetId);
						foreach (var rp in wall.Data.roomPropertyRefs)
						{
							if (!m_WallPropertyCounts.ContainsKey(rp))
								m_WallPropertyCounts.Add(rp, 1);
							else
								m_WallPropertyCounts[rp]++;
						}
					}
				}
			}

			// Add properties based on floor
			foreach (var tpc in m_TilePropertyCounts)
			{
				if (tpc.Value == Tiles.Count)
					RoomProperties.Add(tpc.Key, 1);
			}

			// Add or increase property level based on walls
			foreach (var wpc in m_WallPropertyCounts)
			{
				if (wpc.Value == m_RoomWalls.Count)
				{
					if (!RoomProperties.ContainsKey(wpc.Key))
					{
						RoomProperties.Add(wpc.Key, 1);
					}
					else
					{
						RoomProperties[wpc.Key]++;
					}
				}
			}

			// Add properties based on absence of other properties, for example, add outdoors if indoors is not present
			foreach (var rp in DataBank.Instance.GetAllDataIdsOfType<RoomPropertyData>())
			{
				if (!string.IsNullOrEmpty(rp.absencePropertyRef) && !RoomProperties.ContainsKey(rp.Id))
				{
					RoomProperties.Add(rp.absencePropertyRef, 2);
				}
			}

			// Calculate tile object effects in room
			foreach (var tilePos in Tiles)
			{
				var tileObject = GameServer.MainWorld.GetTileObject(tilePos.x, tilePos.y);
				if (tileObject != null)
				{
					m_TileObjectsToCalculate.Add(tileObject);
				}
			}

			foreach (var wallId in m_RoomWalls)
			{
				var wallEntity = GameServer.NetConnector.GetFneEntity(wallId);
				var edgeComp = wallEntity.GetComponent<EdgeObjectComponentServer>();

				if (edgeComp.MountedObjectData != null)
				{
					m_MountedObjectsToCalculate.Add(edgeComp);
				}
			}
		}

		// This function returns true if anything is successfully calculated. When all rooms return false,
		// the base calculation is complete, and has either succeeded or failed.
		public bool CalculateRoomStatus()
		{
			bool toReturn = false;

			// Calculate tile object effects in room
			for(int i = 0; i < m_TileObjectsToCalculate.Count; i++)
			{
				var tileObject = m_TileObjectsToCalculate[i];
				var comps = tileObject.Components;
				var requirementsCompData = tileObject.GetComponent<RoomRequirementsComponentServer>()?.Data;

				if (requirementsCompData == null || DoesRoomFulfillRequirements(requirementsCompData.propertyRequirements))
				{
					var consumerCompData = tileObject.GetComponent<ConsumerComponentServer>()?.Data;
					if(consumerCompData != null)
					{
						var hasResources = true;
						foreach (var rcd in consumerCompData.resources)
						{
							if (!Resources.ContainsKey(rcd.resourceRef) || Resources[rcd.resourceRef] < rcd.amount)
							{
								hasResources = false;
								break;
							}
						}

						if (!hasResources)
						{
							continue;
						}

						foreach (var rcd in consumerCompData.resources)
						{
							Resources[rcd.resourceRef] -= rcd.amount;
						}
					}

					for (int j = 0; j < comps.Count; j++)
					{
						if (comps[j] is ProducerComponentServer)
						{
							var producerComp = (ProducerComponentServer)comps[j];
							foreach (var rpd in producerComp.Data.resources)
							{
								if (!Resources.ContainsKey(rpd.resourceRef))
									Resources.Add(rpd.resourceRef, rpd.amount);
								else
									Resources[rpd.resourceRef] += rpd.amount;
							}
						}

						if (comps[j] is EnvironmentComponentServer)
						{
							var envComp = (EnvironmentComponentServer)comps[j];
							foreach (var envData in envComp.Data.environment)
							{
								if (!roomEnvironmentAffectors.ContainsKey(envData.typeRef))
									roomEnvironmentAffectors.Add(envData.typeRef, 0);

								roomEnvironmentAffectors[envData.typeRef] += envData.amount;
							}
						}
					}

					m_TileObjectsToCalculate.RemoveAt(i);
					i--;
					toReturn = true;
				}

				// Did room fail property requirements? Remove it from future calculation!
				else if (requirementsCompData != null)
				{
					m_TileObjectsToCalculate.RemoveAt(i);
					i--;
				}
			}

			for (int i = 0; i < m_MountedObjectsToCalculate.Count; i++)
			{
				long connectedRoomId = -1;
				bool isGiver = false;

				var edgeComp = m_MountedObjectsToCalculate[i];
				var wallEntity = edgeComp.ParentEntity;

				var isVertical = wallEntity.Position.x % 1 == 0;
				if (isVertical)
				{
					var isWest = Tiles.Contains((int2) wallEntity.Position);
					if (isWest)
					{
						connectedRoomId = GameServer.MainWorld.GetTileRoom(
							new float2(wallEntity.Position.x - 1, wallEntity.Position.y)
						);
						isGiver = !edgeComp.OppositeMountedDirection;
					}
					else
					{
						connectedRoomId = GameServer.MainWorld.GetTileRoom(
							new float2(wallEntity.Position.x, wallEntity.Position.y)
						);
						isGiver = edgeComp.OppositeMountedDirection;
					}
				}
				else
				{
					var isSouth = Tiles.Contains((int2) wallEntity.Position);
					if (isSouth)
					{
						connectedRoomId = GameServer.MainWorld.GetTileRoom(
							new float2(wallEntity.Position.x, wallEntity.Position.y - 1)
						);
						isGiver = !edgeComp.OppositeMountedDirection;
					}
					else
					{
						connectedRoomId = GameServer.MainWorld.GetTileRoom(
							new float2(wallEntity.Position.x, wallEntity.Position.y)
						);
						isGiver = edgeComp.OppositeMountedDirection;
					}
				}

				if (isGiver && !edgeComp.MountedObjectData.generateFromOutdoors)
				{
					var envTransfers = edgeComp.MountedObjectData.environmentTransfers;
					bool doesEnvironmentValuesExist = true;
					foreach (var envTransfer in envTransfers)
					{
						if (!roomEnvironmentAffectors.ContainsKey(envTransfer.typeRef) || roomEnvironmentAffectors[envTransfer.typeRef] < envTransfer.amount)
						{
							doesEnvironmentValuesExist = false;
						}                  
					}

					var resTransfers = edgeComp.MountedObjectData.resourceTransfers;
					bool doResourcesExist = true;
					foreach (var resTransfer in resTransfers)
					{
						if (!Resources.ContainsKey(resTransfer.resourceRef) || Resources[resTransfer.resourceRef] < resTransfer.amount)
						{
							doResourcesExist = false;
						}
					}

					if(!doesEnvironmentValuesExist || !doResourcesExist)
					{
						continue;
					}

					foreach (var envTransfer in envTransfers)
					{
						if (!roomEnvironmentAffectors.ContainsKey(envTransfer.typeRef))
							roomEnvironmentAffectors.Add(envTransfer.typeRef, 0);

						roomEnvironmentAffectors[envTransfer.typeRef] -= envTransfer.amount;
					}

					foreach (var resourceTransfer in resTransfers)
					{
						if (!Resources.ContainsKey(resourceTransfer.resourceRef))
							Resources.Add(resourceTransfer.resourceRef, 0);

						Resources[resourceTransfer.resourceRef] -= resourceTransfer.amount;
					}

					if(connectedRoomId > 0)
					{
						var connectedRoom = (ServerRoom) GameServer.RoomManager.GetRoom(connectedRoomId);

						foreach (var envTransfer in edgeComp.MountedObjectData.environmentTransfers)
						{
							if (!connectedRoom.roomEnvironmentAffectors.ContainsKey(envTransfer.typeRef))
								connectedRoom.roomEnvironmentAffectors.Add(envTransfer.typeRef, 0);

							connectedRoom.roomEnvironmentAffectors[envTransfer.typeRef] += envTransfer.amount;
						}

						foreach (var resourceTransfer in edgeComp.MountedObjectData.resourceTransfers)
						{
							if (!connectedRoom.Resources.ContainsKey(resourceTransfer.resourceRef))
								connectedRoom.Resources.Add(resourceTransfer.resourceRef, 0);

							connectedRoom.Resources[resourceTransfer.resourceRef] += resourceTransfer.amount;
						}
					}

					m_MountedObjectsToCalculate.RemoveAt(i);
					i--;
					toReturn = true;
				}
				else if(!isGiver && edgeComp.MountedObjectData.generateFromOutdoors)
				{
					if (connectedRoomId <= 0 && edgeComp.MountedObjectData.generateFromOutdoors)
					{
						foreach (var envTransfer in edgeComp.MountedObjectData.environmentTransfers)
						{
							if (!roomEnvironmentAffectors.ContainsKey(envTransfer.typeRef))
								roomEnvironmentAffectors.Add(envTransfer.typeRef, 0);

							roomEnvironmentAffectors[envTransfer.typeRef] += envTransfer.amount;
						}

						foreach (var resourceTransfer in edgeComp.MountedObjectData.resourceTransfers)
						{
							if (!Resources.ContainsKey(resourceTransfer.resourceRef))
								Resources.Add(resourceTransfer.resourceRef, 0);

							Resources[resourceTransfer.resourceRef] += resourceTransfer.amount;
						}
					}

					m_MountedObjectsToCalculate.RemoveAt(i);
					i--;
					toReturn = true;
				}
				else
				{
					m_MountedObjectsToCalculate.RemoveAt(i);
					i--;
				}
			}

			roomEnvironmentValues.Clear();
			foreach (var env in DataBank.Instance.GetAllDataIdsOfType<EnvironmentData>())
            {
                if (!roomEnvironmentAffectors.ContainsKey(env.Id))
                    roomEnvironmentAffectors.Add(env.Id, 0);

                roomEnvironmentValues.Add(env.Id, (roomEnvironmentAffectors[env.Id] / Tiles.Count));
            }

			return toReturn;
        }

		public void AddRoomCalculationFailsToLists(List<FNEEntity> failedEntities)
		{
			failedEntities.AddRange(m_TileObjectsToCalculate);
			foreach (var comp in m_MountedObjectsToCalculate)
				failedEntities.Add(comp.ParentEntity);
		}
	}
}