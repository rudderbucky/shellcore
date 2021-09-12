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
}
