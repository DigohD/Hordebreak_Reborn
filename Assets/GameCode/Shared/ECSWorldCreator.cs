using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;

namespace FNZ.Shared
{
	public static class ECSWorldCreator
	{
		public static World CreateWorld(string name, WorldFlags flags, bool isDefaultGameObjectInjectionWorld)
        {
			var world = new World(name, flags);

			if (isDefaultGameObjectInjectionWorld)
				World.DefaultGameObjectInjectionWorld = world;

			return world;
		}

		public static void InitializeClientWorld(World clientWorld)
		{
			var systemList = GetAllSystems(WorldSystemFilterFlags.Default).ToList();
			var serverSystems = GetSystemsFromAssemblies("FNZ.Server");
			systemList.RemoveAll(x => serverSystems.Contains(x));

			AddSystemToRootLevelSystemGroupsInternal(clientWorld, systemList);
			ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(clientWorld);
		}

		public static void InitializeServerWorld(World serverWorld)
		{
			var systems = GetSystemsFromAssemblies("FNZ.Server");

			systems.Add(typeof(BeginInitializationEntityCommandBufferSystem));
			systems.Add(typeof(EndInitializationEntityCommandBufferSystem));
			systems.Add(typeof(BeginSimulationEntityCommandBufferSystem));
			systems.Add(typeof(EndSimulationEntityCommandBufferSystem));

			AddSystemToRootLevelSystemGroupsInternal(serverWorld, systems);
			ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(serverWorld);
		}

		static IReadOnlyList<Type> GetAllSystems(WorldSystemFilterFlags filterFlags)
		{
			return TypeManager.GetSystems(filterFlags, WorldSystemFilterFlags.Default);
		}

		static void AddSystemToRootLevelSystemGroupsInternal(World world, IEnumerable<Type> systemTypesOrig)
		{
			var initializationSystemGroup = world.GetOrCreateSystem<InitializationSystemGroup>();
			var simulationSystemGroup = world.GetOrCreateSystem<SimulationSystemGroup>();
			var presentationSystemGroup = world.GetOrCreateSystem<PresentationSystemGroup>();

			var managedTypes = new List<Type>();
			var unmanagedTypes = new List<Type>();

			foreach (var stype in systemTypesOrig)
			{
				if (typeof(ComponentSystemBase).IsAssignableFrom(stype))
					managedTypes.Add(stype);
				else if (typeof(ISystemBase).IsAssignableFrom(stype))
					unmanagedTypes.Add(stype);
				else
					throw new InvalidOperationException("Bad type");
			}

			var systems = world.GetOrCreateSystemsAndLogException(managedTypes.ToArray());

			// Add systems to their groups, based on the [UpdateInGroup] attribute.
			foreach (var system in systems)
			{
				if (system == null)
					continue;

				// Skip the built-in root-level system groups
				var type = system.GetType();
				if (type == typeof(InitializationSystemGroup) ||
					type == typeof(SimulationSystemGroup) ||
					type == typeof(PresentationSystemGroup))
				{
					continue;
				}

				var updateInGroupAttributes = TypeManager.GetSystemAttributes(system.GetType(), typeof(UpdateInGroupAttribute));
				if (updateInGroupAttributes.Length == 0)
				{
					simulationSystemGroup.AddSystemToUpdateList(system);
				}

				foreach (var attr in updateInGroupAttributes)
				{
					var group = FindGroup(world, type, attr);
					if (group != null)
					{
						group.AddSystemToUpdateList(system);
					}
				}
			}

			// Update player loop
			initializationSystemGroup.SortSystems();
			simulationSystemGroup.SortSystems();
			presentationSystemGroup.SortSystems();
		}

		static ComponentSystemGroup FindGroup(World world, Type systemType, Attribute attr)
		{
			var uga = attr as UpdateInGroupAttribute;

			if (uga == null)
				return null;

			if (!TypeManager.IsSystemAGroup(uga.GroupType))
			{
				throw new InvalidOperationException($"Invalid [UpdateInGroup] attribute for {systemType}: {uga.GroupType} must be derived from ComponentSystemGroup.");
			}
			if (uga.OrderFirst && uga.OrderLast)
			{
				throw new InvalidOperationException($"The system {systemType} can not specify both OrderFirst=true and OrderLast=true in its [UpdateInGroup] attribute.");
			}

			var groupSys = world.GetExistingSystem(uga.GroupType);
			if (groupSys == null)
			{
				// Warn against unexpected behaviour combining DisableAutoCreation and UpdateInGroup
				var parentDisableAutoCreation = TypeManager.GetSystemAttributes(uga.GroupType, typeof(DisableAutoCreationAttribute)).Length > 0;
				if (parentDisableAutoCreation)
				{
					Debug.LogWarning($"A system {systemType} wants to execute in {uga.GroupType} but this group has [DisableAutoCreation] and {systemType} does not. The system will not be added to any group and thus not update.");
				}
				else
				{
					Debug.LogWarning(
						$"A system {systemType} could not be added to group {uga.GroupType}, because the group was not created. Fix these errors before continuing. The system will not be added to any group and thus not update.");
				}
			}

			return groupSys as ComponentSystemGroup;
		}

		public static List<Type> GetSystemsFromAssemblies(params string[] assemblyNames)
		{
			var systemTypes = new List<Type>();

			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var assemblyName in assemblyNames)
				{
					if (ass.GetName().Name == assemblyName)
					{
						if (ass.ManifestModule.ToString() == "Microsoft.CodeAnalysis.Scripting.dll")
							continue;
						var allTypes = ass.GetTypes();

						var types = allTypes.Where(
							t => t.IsSubclassOf(typeof(ComponentSystemBase)) &&
							!t.IsAbstract &&
							!t.ContainsGenericParameters &&
							t.GetCustomAttributes(typeof(DisableAutoCreationAttribute), true).Length == 0);

						foreach (var type in types)
						{
							systemTypes.Add(type);
						}
					}
				}
			}

			return systemTypes;
		}

		public static ComponentSystemBase GetBehaviourManagerAndLogException(World world, Type type)
		{
			try
			{
				return world.GetOrCreateSystem(type);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return null;
		}

		private static void InsertManagerIntoSubsystemList<T>(PlayerLoopSystem[] subsystemList, int insertIndex, T mgr)
		   where T : ComponentSystemBase
		{
			var del = new DummyDelegateWrapper(mgr);
			subsystemList[insertIndex].type = typeof(T);
			subsystemList[insertIndex].updateDelegate = del.TriggerUpdate;
		}

		public static void Server_UpdatePlayerLoop(World world)
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

			if (currentPlayerLoop.subSystemList != null)
				playerLoop = currentPlayerLoop;

			if (world != null)
			{
				for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
				{
					int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;
					if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.FixedUpdate))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<SimulationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
					else if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<PresentationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
					else if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Initialization))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<InitializationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
				}
			}

			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		public static void Client_UpdatePlayerLoop(World world)
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

			if (currentPlayerLoop.subSystemList != null)
				playerLoop = currentPlayerLoop;

			if (world != null)
			{
				for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
				{
					int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;
					if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<SimulationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
					else if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.PreLateUpdate))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<PresentationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
					else if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Initialization))
					{
						var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];

						for (var j = 0; j < subsystemListLength; ++j)
							newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];

						InsertManagerIntoSubsystemList(newSubsystemList,
						   subsystemListLength + 0, world.GetOrCreateSystem<InitializationSystemGroup>());

						playerLoop.subSystemList[i].subSystemList = newSubsystemList;
					}
				}
			}

			PlayerLoop.SetPlayerLoop(playerLoop);
		}
	}

	internal class DummyDelegateWrapper
	{

		internal ComponentSystemBase System => m_System;
		private readonly ComponentSystemBase m_System;

		public DummyDelegateWrapper(ComponentSystemBase sys)
		{
			m_System = sys;
		}

		public void TriggerUpdate()
		{
			m_System.Update();
		}
	}
}

