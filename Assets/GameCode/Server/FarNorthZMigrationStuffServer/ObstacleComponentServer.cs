using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Shared.Model.Entity.Components;
using RVO;

public class ObstacleComponentServer : FNEComponent
{
	public float x, y, width, height;
	public Obstacle obstacle;
	public bool isInSimulation;

	public override void Init() { }

	public void InitObstacle()
	{
		obstacle = AgentSimulationSystem.Instance.AddObstacle(this);
	}

	public override ushort GetSizeInBytes()
	{
		return 0;
	}

}
