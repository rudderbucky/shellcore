using UnityEngine;

public class TowerAura : PassiveAbility
{
    protected float range;
    GameObject rangeCirclePrefab;
    CircleGraphic circle;
    protected override void Awake()
    {
        ID = AbilityID.Speed;
        base.Awake();
        abilityName = "Speed";
        description = "Passively increases speed.";

        rangeCirclePrefab = Instantiate(ResourceManager.GetAsset<GameObject>("range_circle_prefab"));
        rangeCirclePrefab.transform.SetParent(AbilityHandler.instance.transform.parent.Find("Circle Holder").transform);
        circle = rangeCirclePrefab.GetComponent<CircleGraphic>();
        circle.enabled = false;
        circle.dotted = true;
        circle.raycastTarget = false;
        circle.color = FactionManager.GetFactionColor(Core.GetFaction());
        range = 150;
    }

    public override void Deactivate()
    {
    }

    protected override void Execute()
    {
        circle.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position) * 1920 / Screen.width;

        var cameraPos = CameraScript.instance.transform.position;
        cameraPos.z = 0;
        var x = Camera.main.WorldToScreenPoint(cameraPos + new Vector3(0, range)).y - Camera.main.WorldToScreenPoint(cameraPos).y;
        circle.rectTransform.sizeDelta = new Vector2(x, x);
        circle.enabled = true;
    }

    public override void Tick()
    {
        base.Tick();
    }

    private void LateUpdate()
    {        
        var cameraPos = CameraScript.instance.transform.position;
        cameraPos.z = 0;
        var x = Camera.main.WorldToScreenPoint(cameraPos + new Vector3(0, range)).y - Camera.main.WorldToScreenPoint(cameraPos).y;
        x *= (float)1920 / Screen.width * 2;
        circle.rectTransform.sizeDelta = new Vector2(x, x);
        circle.enabled = Core as Tower;
        float rate = 45 * Time.deltaTime;
        circle.transform.Rotate(new Vector3(0, 0, rate));
        circle.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position) * 1920 / Screen.width;
    }
}
