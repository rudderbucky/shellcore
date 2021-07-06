public abstract class AIModule
{
    public Craft craft;
    public AirCraftAI ai;
    public IOwner owner;
    protected bool initialized;

    public abstract void Init();
    public abstract void StateTick();
    public abstract void ActionTick();
}
