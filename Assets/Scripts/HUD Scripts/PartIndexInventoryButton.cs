using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartIndexInventoryButton : ShipBuilderInventoryBase
{
    public PartIndexScript.PartStatus status;

    protected override void Start()
    {

    }
    public void Initialize()
    {
        base.Start();
        val.enabled = false;

        switch(status)
        {
            case PartIndexScript.PartStatus.Unseen:
                image.enabled = false;
                if(shooter) shooter.enabled = false;
                break;
            case PartIndexScript.PartStatus.Seen:
                image.color = Color.gray;
                if(shooter) shooter.color = Color.gray;
                break;
            default:
                break;
        }
    }
}
