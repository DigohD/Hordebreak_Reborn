namespace FNZ.Server.WorldEvents
{
    public interface IWorldEvent
    {
        void OnTrigger();
        void OnFinished();
        void Tick(float deltaTime);
    }
}