using UnityEngine;

public class Laser : Bullet
{
    public static readonly int laserDamage = 60;
    public static readonly float laserPierceFactor = 0.25F;

    private int bulletsLeft = 0;
    private float bulletFrequency = 0.15F;
    private float time;
    private Vector3 targetPos;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 100;
        survivalTime = 0.1F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Laser;
        cooldownDuration = 3F;
        energyCost = 50;
        damage = laserDamage;
        prefabScale = Vector2.one;
        terrain = Entity.TerrainType.All;
        category = Entity.EntityCategory.Unit;
        pierceFactor = laserPierceFactor;
        bulletSound = "clip_laser";
        bonusDamageType = null;
    }

    protected override void Start()
    {
        base.Start();
        bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
    }

    protected override bool FireBullet(Vector3 targetPos)
    {
        time = Time.time;
        bulletsLeft = 5;
        this.targetPos = targetPos;
        return true;
    }

    public override void SetDestroyed(bool input)
    {
        if (input)
        {
            bulletsLeft = 0;
        }
        base.SetDestroyed(input);
    }

    protected void Update()
    {
        var target = targetingSystem.GetTarget();
        if (bulletsLeft > 0 && (Time.time - time > bulletFrequency) && target)
        {
            time = Time.time;
            bulletsLeft -= 1;
            base.FireBullet(target.position);
        }
    }
}
