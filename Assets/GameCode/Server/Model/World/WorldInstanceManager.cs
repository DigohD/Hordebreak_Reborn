using System;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using UnityEngine;

namespace GameCode.Server.Model.World
{
    public class WorldInstanceManager
    {
        private readonly IDictionary<Guid, int> _worldInstanceIndices;
        private List<ServerWorld> _instances;

        public WorldInstanceManager()
        {
            _worldInstanceIndices = new Dictionary<Guid, int>();
            _instances = new List<ServerWorld>();
        }

        public int AddWorldInstance(Guid id, ServerWorld world)
        {
            if (_worldInstanceIndices.ContainsKey(id))
            {
                Debug.Log($"World instance already exist with Id {id}");
                return -1;
            }
            
            _instances.Add(world);
            var index = _instances.Count - 1;
            _worldInstanceIndices.Add(id, index);

            return index;
        }

        public ServerWorld GetWorldInstance(int index)
        {
            return _instances[index];
        }

        public ServerWorld GetWorldInstance(Guid id)
        {
            var success = _worldInstanceIndices.TryGetValue(id, out var worldIndex);
            if (success)
            {
                return _instances[worldIndex];
            }
            Debug.LogError($"World instance with Id {id} could not be found.");
            return null;
        }

        public void Tick(float dt)
        {
            foreach (var world in _instances)
            {
                if (world.ShouldTick)
                {
                    world.Tick(dt);
                }
            }
        }
    }
}