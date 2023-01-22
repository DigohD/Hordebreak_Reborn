using System;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using UnityEngine;

namespace GameCode.Server.Model.World
{
    public class WorldInstanceManager
    {
        private readonly IDictionary<Guid, ServerWorld> _worldInstances;

        public WorldInstanceManager()
        {
            _worldInstances = new Dictionary<Guid, ServerWorld>();
        }

        public void AddWorldInstance(Guid id, ServerWorld world)
        {
            if (_worldInstances.ContainsKey(id))
            {
                Debug.Log($"World instance already exist with Id {id}");
                return;
            }
            
            _worldInstances.Add(id, world);
        }

        public ServerWorld GetWorldInstance(Guid id)
        {
            var success = _worldInstances.TryGetValue(id, out var world);
            if (success) return world;
            Debug.LogError($"World instance with Id {id} could not be found.");
            return null;
        }

        public void Tick(float dt)
        {
            foreach (var world in _worldInstances.Values)
            {
                world.Tick(dt);
            }
        }
    }
}