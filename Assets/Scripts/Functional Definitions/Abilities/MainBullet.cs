using UnityEngine;

/// <summary>
/// The main bullet ability that every shellcore has as a basic ability (this will inherit from the base bullet ability)
/// </summary>
public class MainBullet : Bullet
{
    public GameObject muzzleFlash;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50;
        survivalTime = 0.25F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.MainBullet;
        cooldownDuration = 0.4F;
        energyCost = 10;
        description = $"Projectile that deals {GetDamage(abilityTier)} damage. \nStays with you no matter what.";
        abilityName = "Main Bullet";
        bulletSound = "clip_bullet";
        muzzleFlash = ResourceManager.GetAsset<GameObject>("main_bullet_muzzle_flash");
    }

    protected override void Start()
    {
        base.Start();
        damage = GetDamage(abilityTier);
    }

    public static int GetDamage(int tier)
    {
        return 100 + 50 * tier;
    }

    private GameObject muzzle;

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        muzzle = Instantiate(muzzleFlash, transform.position, Quaternion.identity);

        var deltaVector = targetPos - transform.position;
        float targetAngle = Mathf.Atan2(deltaVector.y, deltaVector.x) * Mathf.Rad2Deg;
        // float craftAngle = Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg;

        // float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));

        muzzle.transform.eulerAngles = new Vector3(0, 0, targetAngle);
        base.ActivationCosmetic(targetPos);
    }

    protected override bool FireBullet(Vector3 targetPos)
    {
        return base.FireBullet(targetPos);
    }

    void Update()
    {
        if (muzzle)
        {
            muzzle.transform.position = transform.position;
        }
    }
}
