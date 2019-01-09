using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIModule
{
    public Craft craft;
    public AirCraftAI ai;
    public IOwner owner;
    protected bool initialized;

    public abstract void Init();
    public abstract void Tick();
}
