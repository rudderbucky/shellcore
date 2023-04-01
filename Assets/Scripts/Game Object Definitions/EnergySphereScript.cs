using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergySphereScript : MonoBehaviour
{
    // TODO: make undraggable if already being dragged
    private bool collected = false;
    public static bool dirty = false;

    private void OnEnable()
    {
        AIData.energySpheres.Add(this);
        if (SceneManager.GetActiveScene().name != "SampleScene" || MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
        {
            GetComponent<NetworkPowerOrbWrapper>().enabled = false;
            GetComponent<NetworkObject>().enabled = false;
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
            GetComponent<NetworkObject>().Spawn();
            GetComponent<NetworkPowerOrbWrapper>().SetPositionClientRpc(transform.position);
            if (!callbackAdded)
            {
                callbackAdded = true;
                NetworkManager.Singleton.OnClientConnectedCallback += callback;
            }
        }
    }
    private static System.Action<ulong> callback = (u) =>
    {
        dirty = true;
        dirtyTimer = Time.time + 1;
    };    

    private static bool callbackAdded;

    private void OnDestroy()
    {
        AIData.energySpheres.Remove(this);
    }

    float timer;
    static float dirtyTimer = 0;

    private void Update()
    {
        if (dirtyTimer - Time.time < 0)
        {
            dirty = false;
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
            GetComponent<NetworkPowerOrbWrapper>().enabled = GetComponent<NetworkObject>().enabled = timer < 0.1F || dirty || AIData.shellCores.Exists(s => (s.transform.position - transform.position).sqrMagnitude < MasterNetworkAdapter.POP_IN_DISTANCE);


        timer += Time.deltaTime;
        if (timer > 20 && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client)
        {
            Destroy(gameObject);
        }
        else if (timer > 17)
        {
            Blink();
        }
    }

    /// <summary>
    /// Makes the energy blink like in the original game
    /// </summary>
    void Blink()
    {
        GetComponent<SpriteRenderer>().enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        if (((collision.gameObject.name == "Shell Sprite" && collision.GetComponentInParent<IHarvester>() != null)
             || collision.transform.root.GetComponentInChildren<Harvester>() != null) && !collected)
        {
            AudioManager.PlayClipByID("clip_powerup", transform.position);
            var harvester = collision.GetComponentInParent<IHarvester>();
            if (harvester == null)
            {
                harvester = collision.transform.root.GetComponentInChildren<IHarvester>();
            }

            collected = true;
            harvester.AddPower(20);
            harvester.PowerHeal();
            Destroy(gameObject);
        }
    }
}
