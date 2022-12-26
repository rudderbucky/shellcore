using UnityEngine;

public class TowerAura : PassiveAbility
{
    protected float range;
    GameObject rangeCirclePrefab;
    CircleGraphic circle;
    protected override void Awake()
    {
        base.Awake();
        abilityName = "Tower Aura";
        description = "Creates various passive effect auras.";

        rangeCirclePrefab = Instantiate(ResourceManager.GetAsset<GameObject>("range_circle_prefab"));
        if(AbilityHandler.instance && AbilityHandler.instance.transform.parent.Find("Circle Holder"))
            rangeCirclePrefab.transform.SetParent(AbilityHandler.instance.transform.parent.Find("Circle Holder").transform, false);
        circle = rangeCirclePrefab.GetComponent<CircleGraphic>();
        circle.enabled = false;
        circle.dotted = true;
        circle.raycastTarget = false;
        circle.color = FactionManager.GetFactionColor(Core.GetFaction());
        range = 30;
        if (!AIData.auras.Contains(this)) AIData.auras.Add(this);
    }


    public void Initialize()
    {
        switch (type)
        {
            case AuraType.Heal:
                ID = AbilityID.HealAura;
                break;
            case AuraType.Speed:
                ID = AbilityID.SpeedAura;
                range = 450;
                break;
            case AuraType.Energy:
                ID = AbilityID.EnergyAura;
                break;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (AIData.auras.Contains(this)) AIData.auras.Remove(this);
        if (circle.gameObject) Destroy(circle.gameObject);
    }

    public enum AuraType
    {
        Heal,
        Speed,
        Energy
    }

    public AuraType type;


    public override float GetRange()
    {
        return range; // get range
    }

    private void LateUpdate()
    {        
        var cameraPos = CameraScript.instance ? CameraScript.instance.transform.position : Vector3.zero;
        cameraPos.z = 0;
        var x = Camera.main.WorldToScreenPoint(cameraPos + new Vector3(0, range)).y - Camera.main.WorldToScreenPoint(cameraPos).y;
        x *= (float)1920 / Screen.width * 2;
        float rate = 45 * Time.deltaTime;
        if (!circle) return;
        circle.transform.Rotate(new Vector3(0, 0, rate));
        circle.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position) * 1920 / Screen.width;
        circle.rectTransform.sizeDelta = new Vector2(x, x);
        circle.enabled = Core as Tower;
    }
}
