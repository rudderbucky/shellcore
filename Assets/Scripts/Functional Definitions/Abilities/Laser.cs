using UnityEngine;

public class Laser : Bullet
{
    public static readonly int laserDamage = 55;
    public static readonly float laserPierceFactor = 0F;

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
        bonusDamageType = typeof(Tank);
        pierceFactor = laserPierceFactor;
        bulletSound = "clip_laser";
    }

    protected override void Start()
    {
        base.Start();
        bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
    }

    protected override bool FireBullet(Vector3 targetPos)
    {
        time = bulletFrequency;
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
        time -= Time.deltaTime;
        var target = targetingSystem.GetTarget();
        if (bulletsLeft > 0 && (time < 0) && target && !Core.IsInvisible && !Core.isAbsorbing)
        {
            if (!isEnabled) 
            {
                bulletsLeft = 0;
                return;
            }
            time = bulletFrequency;
            bulletsLeft -= 1;
            base.FireBullet(target.position);
        }
    }
}
