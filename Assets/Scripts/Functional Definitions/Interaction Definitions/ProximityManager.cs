using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityManager
{
    
    public static IInteractable GetClosestInteractable(Entity entity)
    {
        IInteractable closest = null;
        foreach (IInteractable interactable in AIData.interactables)
        {
            if (interactable as PlayerCore || interactable == null || !interactable.GetInteractible())
            {
                continue;
            }

            if (closest == null)
            {
                closest = interactable;
            }
            else if ((interactable.GetTransform().position - entity.transform.position).sqrMagnitude <=
                     (closest.GetTransform().position - entity.transform.position).sqrMagnitude)
            {
                closest = interactable;
            }
        }

        return closest;
    }

}
