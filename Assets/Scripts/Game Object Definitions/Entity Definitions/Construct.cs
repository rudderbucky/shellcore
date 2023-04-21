using UnityEngine;
/// <summary>
/// Immobile entities are constructs. This includes outposts and carriers.
/// All constructs also do not respawn. (Some can convert, but that is not respawning)
/// </summary>
public class Construct : Entity
{
    protected override void Start()
    {
        base.Start();
        if (entityBody)
        {
            entityBody.drag = 25f;
        }
    }
    const float MAX_DAMAGE_PER_SECOND = 200;
    float damageLeft = MAX_DAMAGE_PER_SECOND;

    public override float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy)
    {
        var finalAmount = amount;
        if (this is ICarrier)
        {
            var cappedAmount = Mathf.Min(damageLeft, amount);
            damageLeft = Mathf.Max(0, damageLeft - cappedAmount);
            finalAmount = cappedAmount + (amount - cappedAmount) * 0.5F;
            Debug.Log(finalAmount + " " + damageLeft);
        }

        var residue = base.TakeShellDamage(finalAmount, shellPiercingFactor, lastDamagedBy);
        
        return residue;
    }

    protected override void Update()
    {
        damageLeft = Mathf.Min(MAX_DAMAGE_PER_SECOND, damageLeft + MAX_DAMAGE_PER_SECOND * Time.deltaTime);
        base.Update();
    }
}
