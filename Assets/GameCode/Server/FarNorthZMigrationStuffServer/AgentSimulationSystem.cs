using FNZ.Server.Model.Entity.Components;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World;
using FNZ.Shared.Constants;
using FNZ.Shared.FarNorthZMigrationStuff;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.Entity.Components.Health;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Lidgren.Network;
using RVO;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class AgentSimulationSystem
	{
		private static AgentSimulationSystem s_Instance;
		public static AgentSimulationSystem Instance => s_Instance ??= new AgentSimulationSystem();

		//private int generalFlowFieldTimer = 0;

		private readonly float m_SimulationTimeStep = 0.25f;
		private const float c_SimSpeedFactor = 3.0f;
		private const float c_NDist = 1.2f;
		private const float c_SightSpeedFactor = 3.0f;
		private const float c_SoundSpeedFactor = 2.0f;
		private const int c_MAXNeighbours = 4;
		private const float c_MAXSpeed = 2.0f;
		private const float c_TimeHorizon = 10.0f;
		private const float c_TimeHorizonObst = 10.0f;

		public bool SimulationStarted = false;

		public List<NPC_Agent> Agents = new List<NPC_Agent>();
		private List<HordeEntityUpdateNetData> m_EnemySendList = new List<HordeEntityUpdateNetData>();
		private List<HordeEntityAttackTargetData> m_AttackersList = new List<HordeEntityAttackTargetData>();
		private Dictionary<FNEEntity, List<HordeEntityUpdateNetData>> m_PlayerEntitiesMap = new Dictionary<FNEEntity, List<HordeEntityUpdateNetData>>();

		private AgentSimulationSystem()
		{
			m_SimulationTimeStep = m_SimulationTimeStep * c_SimSpeedFactor;
		}

		public NPC_Agent AddAgent(FNEEntity e, Vector2 position, float radius, float speed)
		{
			var agent = new NPC_Agent
			{
				position = position,
				entity = e,
				baseSpeed = speed
			};

			agent.agentID = Simulator.Instance.addAgent(position, c_NDist, c_MAXNeighbours,
					c_TimeHorizon, c_TimeHorizonObst, radius, c_MAXSpeed, new Vector2(0, 0));

			Agents.Add(agent);
			e.Agent = agent;
			agent.currentTile = new int2((int) position.x, (int) position.y);

			return agent;
		}

		public void RemoveAgent(FNEEntity agentToRemove)
		{
			int lastIndex = Simulator.Instance.agents_.Count - 1;
			int availableIndex = agentToRemove.Agent.agentID;

			if (lastIndex == availableIndex)
			{
				Simulator.Instance.agents_.RemoveAt(lastIndex);
				Agents.RemoveAt(lastIndex);
			}
			else
			{
				Agent last = Simulator.Instance.agents_[lastIndex];
				Simulator.Instance.agents_[availableIndex] = last;
				NPC_Agent lastNpc = Agents[lastIndex];
				Agents[availableIndex] = lastNpc;
				lastNpc.agentID = availableIndex;
				last.id_ = availableIndex;
				Simulator.Instance.agents_.RemoveAt(lastIndex);
				Agents.RemoveAt(lastIndex);
			}

			//Debug.Log($"[Server, Agent removed]: {agents.Count}");
		}

		public Obstacle AddObstacle(ObstacleComponentServer obstacle)
		{
			if (obstacle.isInSimulation) return obstacle.obstacle;
			
			var obstacleList = new List<Vector2>
			{
				new Vector2(obstacle.x, obstacle.y),
				new Vector2(obstacle.x + obstacle.width, obstacle.y),
				new Vector2(obstacle.x + obstacle.width, obstacle.y + obstacle.height),
				new Vector2(obstacle.x, obstacle.y + obstacle.height)
			};

			obstacle.isInSimulation = true;

			return Simulator.Instance.addObstacle(obstacleList);
		}

		public void RemoveObstacle(FNEEntity e)
		{
			int lastObstacleIndex = Simulator.Instance.obstacles_.Count - 4;
			ObstacleComponentServer comp = e.GetComponent<ObstacleComponentServer>();
			int availableIndex = comp.obstacle.id_;

			if (!comp.isInSimulation) return;
			comp.isInSimulation = false;

			if (availableIndex <= 0 || availableIndex >= Simulator.Instance.obstacles_.Count) return;

			if (lastObstacleIndex == availableIndex)
			{
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
			}
			else
			{
				Obstacle last0 = Simulator.Instance.obstacles_[lastObstacleIndex];
				Obstacle last1 = Simulator.Instance.obstacles_[lastObstacleIndex + 1];
				Obstacle last2 = Simulator.Instance.obstacles_[lastObstacleIndex + 2];
				Obstacle last3 = Simulator.Instance.obstacles_[lastObstacleIndex + 3];

				Simulator.Instance.obstacles_[availableIndex] = last0;
				Simulator.Instance.obstacles_[availableIndex + 1] = last1;
				Simulator.Instance.obstacles_[availableIndex + 2] = last2;
				Simulator.Instance.obstacles_[availableIndex + 3] = last3;

				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);
				Simulator.Instance.obstacles_.RemoveAt(Simulator.Instance.obstacles_.Count - 1);

				Simulator.Instance.obstacles_[availableIndex].id_ = availableIndex;
				Simulator.Instance.obstacles_[availableIndex + 1].id_ = availableIndex + 1;
				Simulator.Instance.obstacles_[availableIndex + 2].id_ = availableIndex + 2;
				Simulator.Instance.obstacles_[availableIndex + 3].id_ = availableIndex + 3;
			}
		}

		public void StartSimulation()
		{
			Simulator.Instance.setTimeStep(m_SimulationTimeStep);
			SimulationStarted = true;
		}

		public void StopSimulation()
		{
			Simulator.Instance.Clear();
		}

		int ticker = 0;

		public void Tick()
		{
			if (SimulationStarted)
			{
				// ticker++;
				// if (ticker % 5 == 0)
				// {
				// 	Debug.Log($"[SERVER, Agents in simulation]: {agents.Count+3}");
				//
				// 	ticker = 0;
				// }

				//Debug.Log("Ticking agents");

				//if (generalFlowFieldTimer >= Controller.TICKS_PER_SECOND * 10)
				//{
				//    FNEUtil.GenerateGeneralFlow();
				//    generalFlowFieldTimer = 0;
				//}
				//else
				//{
				//    generalFlowFieldTimer++;
				//}

				double start1 = FNEUtil.NanoTime();
				UpdateDesiredVelocitiesForAgents();
				double end1 = FNEUtil.NanoTime();
				double diff = (end1 - start1) / 1000000.0;

				if (m_AttackersList.Count > 0)
				{
					GameServer.NetAPI.Entity_AttackTarget_BAR(m_AttackersList);
					m_AttackersList.Clear();
				}

				start1 = FNEUtil.NanoTime();
				RunCollisionAvoidanceCorrection();
				end1 = FNEUtil.NanoTime();
				diff = (end1 - start1) / 1000000.0;
				//Debug.Log("RunCollisionAvoidanceCorrection execution time in ms: " + diff);

				start1 = FNEUtil.NanoTime();
				UpdateAgentPositions();
				end1 = FNEUtil.NanoTime();
				diff = (end1 - start1) / 1000000.0;
				//Debug.Log("UpdateAgentPositions execution time in ms: " + diff);

				ProcessEntitiesLoadAndUnload();
			}
		}

		//Gets called in doStep in the simulation
		public void SetAgentVelocity(int agentID, Vector2 velocity)
		{
			Agents[agentID].velocity = velocity;
		}

		private void UpdateDesiredVelocitiesForAgents()
		{
			foreach (var agent in Agents)
			{
				var chunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(agent.position);
				if (chunk == null) continue;
				if (!chunk.IsActive|| !chunk.IsInitialized)
				{
					agent.active = false;
					continue;
				}
				//if (chunk == null) continue;
				//agent.active = chunk.activeSimulation;
				if (!agent.active) continue;
				var comp = agent.entity.GetComponent<NPCPlayerAwareComponentServer>();
				if (comp == null) continue;
				UpdateDesiredVelocity(agent, comp);
			}
		}

		private void UpdateDesiredVelocity(NPC_Agent agent, NPCPlayerAwareComponentServer awareComp)
		{
			DamageTypeData dtd = DataBank.Instance.GetData<DamageTypeData>("zombie_melee");

			if (agent.isAttacking && DoAttack(agent))
			{
				var targetEntity = agent.target;
				if (targetEntity != null && targetEntity.Data.entityType.Equals("Player"))
				{
					Vector2 between = (Vector2)targetEntity.Position - agent.position;
					float range = agent.entity.Data.GetComponentData<EnemyStatsComponentData>().attackRange;
					if (between.magnitude <= range)
					{
						var healthComp = targetEntity.GetComponent<StatComponentServer>();
						if (healthComp.CurrentHealth > 0)
						{
							float damage = agent.entity.Data.GetComponentData<EnemyStatsComponentData>().damage;
							bool diedFromAttack = healthComp.Server_ApplyDamage(damage, dtd.Id);
							var conn = GameServer.NetConnector.GetConnectionFromPlayer(targetEntity);
							if (conn == null)
							{
								agent.target = null;
								agent.isAttacking = false;
							}
							else
							{
								GameServer.NetAPI.Entity_UpdateComponent_STC(healthComp, GameServer.NetConnector.GetConnectionFromPlayer(targetEntity));
							}
						}
					}
				}

				agent.isAttacking = false;
			}

			if (HasCooldown(agent))
			{
				agent.desiredVelocity = Vector2.zero;
				agent.velocity = Vector2.zero;
				Simulator.Instance.setAgentPrefVelocity(agent.agentID, agent.desiredVelocity);
				return;
			}


			int2 currentTile = new int2((int)agent.position.x, (int)agent.position.y);

			var neighbors = GameServer.World.GetTileStraightNeighbors(currentTile.x, currentTile.y);
			var players = new List<FNEEntity>();
			neighbors.Add(currentTile);

			foreach (var tile in neighbors)
			{
				var playersOnTile = GameServer.World.GetTilePlayers(tile);
				if (playersOnTile != null)
				{
					foreach (var player in playersOnTile)
					{
						players.Add(player);
					}
				}
			}

			var ffComp = agent.entity.GetComponent<FlowFieldComponentServer>();
			FNEEntity entityToAttack = null;
			agent.speed = agent.baseSpeed;

			if (players.Count > 0)
			{
				FNEEntity targetPlayer = null;

				foreach (var player in players)
				{
					if (!player.GetComponent<PlayerComponentServer>().IsDead)
					{
						targetPlayer = player;
						break;
					}
				}

				if (targetPlayer != null)
				{
					Vector2 between = (Vector2)targetPlayer.Position - agent.position;
					float range = agent.entity.Data.GetComponentData<EnemyStatsComponentData>().attackRange;
					if (between.magnitude <= range)
					{
						OnAttackPlayer(agent, targetPlayer);
						agent.target = targetPlayer;
					}
					else
					{
						agent.desiredVelocity = between.normalized;
						agent.speed = agent.baseSpeed * c_SightSpeedFactor;
						agent.target = null;
					}

					Simulator.Instance.setAgentPrefVelocity(agent.agentID, agent.desiredVelocity);
					return;
				}
			}

			if (awareComp.HasSeenPlayer() && ffComp.sightFlowField != null)
			{
				VectorFieldNode vfn = ffComp.sightFlowField.GetVectorFieldDirection((int)(agent.position.x), (int)(agent.position.y));

				agent.desiredVelocity = vfn.vector;
				entityToAttack = vfn.breakWall;
				if (agent.desiredVelocity.magnitude == 0)
				{
					awareComp.isSeeingPlayer = false;
				}
				agent.speed = agent.baseSpeed * c_SightSpeedFactor;
			}
			else if (awareComp.HasHeardSound() && ffComp.soundFlowField != null)
			{
				VectorFieldNode vfn = ffComp.soundFlowField.GetVectorFieldDirection((int)(agent.position.x),
						(int)(agent.position.y));

				agent.desiredVelocity = vfn.vector;
				entityToAttack = vfn.breakWall;
				agent.speed = agent.baseSpeed * c_SoundSpeedFactor;
			}
			else
			{
				if (ffComp.generalFlowField != null)
				{
					VectorFieldNode vfn = ffComp.generalFlowField.GetVectorFieldDirection((int)(agent.position.x),
						(int)(agent.position.y));

					agent.desiredVelocity = vfn.vector;
					entityToAttack = vfn.breakWall;
					agent.speed = agent.baseSpeed;
				}
				else
				{
					agent.desiredVelocity.x = 0;
					agent.desiredVelocity.y = 0;
				}
			}

			if (entityToAttack != null && !entityToAttack.IsDead
				//&& FNECollisionUtil.PolygonCircleCollision(entityToAttack.GetComponent<PolygonComponent>().GetWorldPolygon(Vector2.zero),
				//agent.position, agent.entity.data.GetComponentData<EnemyStatsComponentData>().agentRadius, agent.desiredVelocity * agent.speed).hit
				)
			{
				var enemyStats = agent.entity.Data.GetComponentData<EnemyStatsComponentData>();
				float coolDown = enemyStats.attackCooldown;
				long now = FNEUtil.NanoTime();

				if (agent.attackStartTimeStamp == 0 || (now - agent.attackStartTimeStamp) / 1000000000.0 > coolDown)
				{
					if (agent.entity != null)
					{
						var statComp = entityToAttack.GetComponent<StatComponentServer>();
						if (statComp != null)
						{
							if (statComp.Server_ApplyDamage(enemyStats.damage, dtd.Id))
							{
								m_AttackersList.Add(new HordeEntityAttackTargetData
								{
									AttackerNetId = agent.entity.NetId,
									TargetNetId = entityToAttack.NetId,
									AttackerPosition = agent.position
								});
							}
						}
					}

					agent.attackStartTimeStamp = now;
					agent.desiredVelocity.x = 0;
					agent.desiredVelocity.y = 0;
				}
			}
			else if (entityToAttack != null && entityToAttack.IsDead)
			{
				ffComp.soundFlowField?.RegenerateFlowField();
			}

			Simulator.Instance.setAgentPrefVelocity(agent.agentID, agent.desiredVelocity);
		}

		public bool HasCooldown(NPC_Agent agent)
		{
			float coolDown = agent.entity.Data.GetComponentData<EnemyStatsComponentData>().attackCooldown;
			long now = FNEUtil.NanoTime();
			return (now - agent.attackStartTimeStamp) / 1000000000.0 < coolDown;
		}

		public bool DoAttack(NPC_Agent agent)
		{
			float attackTime = agent.entity.Data.GetComponentData<EnemyStatsComponentData>().attackTimestamp;
			long now = FNEUtil.NanoTime();
			return (now - agent.attackStartTimeStamp) / 1000000000.0 >= attackTime;
		}

		private void OnAttackPlayer(NPC_Agent agent, FNEEntity targetPlayer)
		{
			agent.desiredVelocity = Vector2.zero;
			agent.velocity = Vector2.zero;

			if (HasCooldown(agent))
			{
				Simulator.Instance.setAgentPrefVelocity(agent.agentID, agent.desiredVelocity);
				return;
			}

			m_AttackersList.Add(new HordeEntityAttackTargetData
			{
				AttackerNetId = agent.entity.NetId,
				TargetNetId = targetPlayer.NetId,
				AttackerPosition = agent.position
			});

			agent.attackStartTimeStamp = FNEUtil.NanoTime();
			agent.isAttacking = true;
		}

		private void RunCollisionAvoidanceCorrection()
		{
			Simulator.Instance.doStep();
		}

		private void UpdateAgentPositions()
		{
			m_PlayerEntitiesMap.Clear();
			var players = GameServer.World.GetAllPlayers();

			foreach (var agent in Agents)
			{
				if (!agent.active || !agent.entity.Enabled) continue;
				var chunkIndices = GameServer.World.GetChunkIndices(agent.position);

				byte chunkX = (byte)chunkIndices.x;
				byte chunkY = (byte)chunkIndices.y;

				var chunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(chunkX, chunkY);
				if (chunk == null) continue;
				if (!chunk.IsActive || !chunk.IsInitialized) continue;

				float playerDist = float.MaxValue;
				FNEEntity closestPlayer = null;
				foreach (FNEEntity player in players)
				{
					float dist = (agent.position - (Vector2)player.Position).magnitude;
					if (closestPlayer == null || dist < playerDist)
					{
						closestPlayer = player;
						playerDist = dist;
					}
				}

				float proximitySpeedBonus = 1f;
				if (playerDist < 10f)
				{
					proximitySpeedBonus = ((1f - (playerDist / 10f)) + 1f);
				}

				float angle = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
				agent.entity.RotationDegrees = angle;
				Vector2 vel = agent.velocity * GameServer.DeltaTime * (agent.speed / c_SimSpeedFactor) * proximitySpeedBonus;

				if(float.IsNaN(agent.velocity.x) || float.IsNaN(agent.velocity.y))
				{
					vel = Vector2.zero;
				}

				agent.position += vel;
				agent.entity.Position = agent.position;

				if ((agent.lastUpdatedPos - agent.position).magnitude > 0.01f)
				{
					foreach (var player in players)
					{
						var playerConnection = GameServer.NetConnector.GetConnectionFromPlayer(player);
						if (playerConnection == null) 
							continue;

						if (!m_PlayerEntitiesMap.ContainsKey(player))
						{
							m_PlayerEntitiesMap.Add(player, new List<HordeEntityUpdateNetData>());
						}

						var playerState = GameServer.ChunkManager.GetPlayerChunkState(playerConnection);
						if (playerState.MovingEntitiesSynced.Contains(agent.entity.NetId) && playerState.CurrentlyLoadedChunks.Contains(chunk))
						{
							if (agent.position.x >= 0 && agent.position.y >= 0)
							{
								var agentData = new HordeEntityUpdateNetData
								{
									NetId = agent.entity.NetId,
									Position = agent.entity.Position
								};

								m_PlayerEntitiesMap[player].Add(agentData);
							}
						}
					}

					agent.lastUpdatedPos = agent.position;
				}

				var x = (int)agent.entity.Position.x;
				var y = (int)agent.entity.Position.y;


				var newPositionTile = new int2((int)agent.entity.Position.x, (int)agent.entity.Position.y);

				if (agent.currentTile.x != newPositionTile.x || agent.currentTile.y != newPositionTile.y)
				{
					var oldChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(agent.currentTile);
					var newChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(newPositionTile);

					if (newChunk != oldChunk)
					{
						foreach (var state in GameServer.ChunkManager.GetAllPlayersChunkStates().Values)
						{
							bool oldChunkLoadedState = state.IsChunkInLoadedState(oldChunk);

							if (oldChunkLoadedState != state.IsChunkInLoadedState(newChunk))
							{
								if (oldChunkLoadedState)
								{
									state.EntitiesToUnload.Add(new HordeEntityDestroyData
									{
										NetId = agent.entity.NetId,
										Position = agent.entity.Position
									});
								}
								else
								{
									state.EntitiesToLoad.Add(new HordeEntitySpawnData 
									{
										NetId = agent.entity.NetId,
										EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(agent.entity.EntityId),
										Position = agent.entity.Position,
										Rotation = agent.entity.RotationDegrees
									});
								}
							}
						}
					}
					
					GameServer.World.RemoveEnemyFromTile(agent.currentTile, agent.entity);
					GameServer.World.AddEnemyToTile(newPositionTile, agent.entity);
				}

				agent.currentTile = newPositionTile;

				Simulator.Instance.setAgentPosition(agent.agentID, agent.position);
			}

			if (m_PlayerEntitiesMap.Count > 0)
			{
				foreach (var player in m_PlayerEntitiesMap.Keys)
				{
					var conn = GameServer.NetConnector.GetConnectionFromPlayer(player);

					SendEnemies(conn, m_PlayerEntitiesMap[player]);
				}
			}
		}

		private void ProcessEntitiesLoadAndUnload()
		{
			var entitiesToLoad = new List<HordeEntitySpawnData>();
			var entitiesToUnLoad = new List<HordeEntityDestroyData>();

			foreach (var conn in GameServer.ChunkManager.GetAllPlayersChunkStates().Keys)
			{
				entitiesToLoad.Clear();
				entitiesToUnLoad.Clear();
				var state = GameServer.ChunkManager.GetPlayerChunkState(conn);

				if (state.EntitiesToLoad.Count > 0)
				{
					entitiesToLoad = state.EntitiesToLoad;
					state.EntitiesToLoad.Clear();
				}

				if (state.EntitiesToUnload.Count > 0)
				{
					entitiesToUnLoad = state.EntitiesToUnload;
					state.EntitiesToUnload.Clear();
				}

				if (entitiesToLoad.Count > 0)
					GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(entitiesToLoad);

				if (entitiesToUnLoad.Count > 0)
					GameServer.NetAPI.Entity_DestroyHordeEntity_Batched_BAR(entitiesToUnLoad);
			}
		}

		private void SendEnemies(NetConnection conn, List<HordeEntityUpdateNetData> entities)
		{
			m_EnemySendList.Clear();
			foreach (var entity in entities)
			{
				if (m_EnemySendList.Count < HordeEntityPacketHelperConstants.NumberOfEnemiesPerPacket)
				{
					m_EnemySendList.Add(entity);
				}
				else
				{
					GameServer.NetAPI.Entity_UpdateHordeEntity_Batched_STC(m_EnemySendList, conn);
					m_EnemySendList.Clear();
				}
			}

			if (m_EnemySendList.Count > 0)
			{
				GameServer.NetAPI.Entity_UpdateHordeEntity_Batched_STC(m_EnemySendList, conn);
				m_EnemySendList.Clear();
			}
		}
	}
}