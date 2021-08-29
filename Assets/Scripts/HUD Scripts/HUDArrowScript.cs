﻿using UnityEngine;

public class HUDArrowScript : MonoBehaviour
{
    public PlayerCore player;
    public SpriteRenderer spr;
    public static bool active = false;
    static HUDArrowScript instance;

    public bool init;

    // TODO: fix bug where arrow is disabled initially even though it was enabled in the main menu
    public void Initialize(PlayerCore player)
    {
        if (active)
        {
            this.player = player;
            init = true;
        }
    }

    void Awake()
    {
        instance = this;
        active = PlayerPrefs.GetString("HUDArrowScript_active", "False") == "True";
        Initialize(player);
    }

    public static void SetActive(bool act)
    {
        active = act;
        if (instance && act && instance.player)
        {
            instance.Initialize(instance.player);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            if (active && player.GetTargetingSystem().GetTarget())
            {
                Vector3 targpos = player.GetTargetingSystem().GetTarget().position;
                Vector3 viewpos = Camera.main.WorldToViewportPoint(targpos);
                if (viewpos.x > 1 || viewpos.x < 0 || viewpos.y < 0 || viewpos.y > 1)
                {
                    spr.enabled = true;
                    spr.sortingOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
                    var x = (-player.transform.position + targpos);
                    x.z = 0;
                    float magcheck = Mathf.Max(viewpos.x, 1 - viewpos.x, viewpos.y, 1 - viewpos.y);
                    transform.localScale = new Vector3(1 / magcheck, 1 / magcheck, 1);
                    if (player.IsMoving() || init)
                    {
                        init = false;
                        transform.position = player.transform.position + x.normalized * 10;
                        // TODO: check condition for adding/subbing 90
                        transform.eulerAngles = new Vector3(0, 0, (Mathf.Rad2Deg * Mathf.Atan(x.y / x.x) - (x.x > 0 ? 90 : -90)));
                    }
                }
                else
                {
                    spr.enabled = false;
                }
            }
            else
            {
                spr.enabled = false;
            }
        }
    }
}
