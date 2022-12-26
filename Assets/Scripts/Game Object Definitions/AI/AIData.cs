using System.Collections.Generic;

public class AIData
{
    public static List<IInteractable> interactables = new List<IInteractable>();
    public static List<Entity> entities = new List<Entity>();
    public static List<EnergyRock> energyRocks = new List<EnergyRock>();
    public static List<EnergySphereScript> energySpheres = new List<EnergySphereScript>();
    public static List<IVendor> vendors = new List<IVendor>();
    public static List<TowerAura> auras = new List<TowerAura>();
    public static List<ShellPart> strayParts = new List<ShellPart>();
    public static List<Draggable> rockFragments = new List<Draggable>();
    public static List<Flag> flags = new List<Flag>();
}
