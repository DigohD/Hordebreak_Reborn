using Unity.Entities;
using Unity.Jobs;

namespace FNZ.Server.Controller.Systems
{
	public class HordeSimulationSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{

			return inputDeps;
		}
	}
}