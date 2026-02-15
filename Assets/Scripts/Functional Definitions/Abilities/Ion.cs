using UnityEngine;

public class Ion : WeaponAbility
{
    IonLineController lineController;

    protected override void Awake()
    {
        GameObject gObj = new GameObject("Line Renderer");
        gObj.transform.SetParent(this.transform, false);
        lineController = gObj.AddComponent<IonLineController>();
        ID = AbilityID.Ion;
        abilityName = "Ion";
        cooldownDuration = 15f;
        range = 15;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.All;
        base.Awake();
    }

    public float GetBeamAngle() 
    {
        return lineController.GetBeamAngle();
    }

    protected override void Start()
    {
        base.Start();
        lineController.Init(targetingSystem, Core, range, part, abilityTier, type == WeaponDiversityType.Torpedo ? Entity.TerrainType.Ground : Entity.TerrainType.Air);
    }

    public override void SetDestroyed(bool destroyed)
    {
        if (destroyed && lineController)
        {
            Destroy(lineController.gameObject);
        }

        base.SetDestroyed(destroyed);
    }

    void OnDestroy()
    {
        if (lineController)
        {
            Destroy(lineController.gameObject);
        }
        if (source)
        {
            Destroy(source);
        }
    }

    void LateUpdate()
    {
        if (lineController)
        {
        }
    }

    void Update()
    {
        lineController.gasBoosted = gasBoosted;
        if (lineController.GetDuration() <= 0 && source)
        {
            Destroy(source);
        }
    }

    Vector3 GetMousePos()
    {
        Vector3 vec = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraScript.zLevel));
        vec.z = 0;
        return vec;
    }

    GameObject source;
    protected override bool Execute(Vector3 victimPos)
    {
        /*
        if(targetingSystem.GetTarget() && !firing)
        {
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, targetingSystem.GetTarget().position);
            firing = true;
        }
        */
        if (!lineController.GetFiring()) // TODO: Use AbilityState.Charging instead
        {
            source = AudioManager.PlayClipByID("clip_ion", transform.position);
            lineController.StartFiring(5);
            return true;
        }

        return false;
    }
}
