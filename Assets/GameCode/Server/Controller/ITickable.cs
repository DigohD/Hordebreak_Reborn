namespace FNZ.Server.Controller
{
	public interface ITickable
	{
		void Tick(float dt);
	}

	/*
	 * ILateTickable is used for components that do operations that affect the tickables list,
	 * e.g. excavatorComponent which destroys other tickable entities.
	 * 
	 * Can also be used for logic that should be updated after the original tick of all other
	 * entities has been executed.
	 */ 
	public interface ILateTickable : ITickable
	{
		void LateTick(float dt);
	}
}