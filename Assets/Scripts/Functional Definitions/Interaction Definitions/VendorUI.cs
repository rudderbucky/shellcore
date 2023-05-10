using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IVendor
{
    VendingBlueprint GetVendingBlueprint();
    int GetFaction();
    Vector3 GetPosition();
    Transform GetTransform();
    bool NeedsAlliedFaction();
    EntityNetworkAdapter GetAdapter();
}

public class VendorUI : MonoBehaviour, IDialogueable, IWindow
{
    public VendingBlueprint blueprint;
    public GameObject UIPrefab;
    public GameObject buttonPrefab;
    public PlayerCore player;
    private IVendor vendor;
    private GameObject UI;
    private Transform background;
    private bool opened;
    private GameObject[] buttons;
    private Text costInfo;
    private Text nameInfo;
    public int range;
    public GameObject tooltipPrefab;
    public static VendorUI instance;

    public bool GetActive()
    {
        return UI && UI.activeSelf;
    }

    public bool IsOpen()
    {
        return opened;
    }

    public void openUI()
    {
        if (opened)
        {
            var vendor = this.vendor;
            var player = this.player;
            CloseUI();
            SetVendor(vendor, player);
        }

        if (!blueprint)
        {
            Debug.Log("No blueprint!");
            return;
        }

        if (!UIPrefab)
        {
            UIPrefab = ResourceManager.GetAsset<GameObject>("vendor_ui");
        }

        if (!buttonPrefab)
        {
            buttonPrefab = ResourceManager.GetAsset<GameObject>("vendor_button");
        }

        UI = Instantiate(UIPrefab);
        UI.GetComponentInChildren<GUIWindowScripts>().Activate();
        instance = this;
        background = UI.transform.Find("Container").Find("Background");
        Button close = background.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(CloseUI);
        costInfo = background.transform.Find("Cost").GetComponent<Text>();
        nameInfo = background.transform.Find("Name").GetComponent<Text>();
        costInfo.text = "";
        range = blueprint.range;


        buttons = new GameObject[blueprint.items.Count];
        for (int i = 0; i < blueprint.items.Count; i++)
        {
            int index = i;
            buttons[i] = Instantiate(buttonPrefab);

            RectTransform rt = buttons[i].GetComponent<RectTransform>();
            rt.SetParent(background, false);
            rt.anchoredPosition = new Vector2(185 + (i % 5) * 64, -40 - (i > 4 ? 64 : 0));

            Button button = buttons[i].GetComponent<Button>();
            button.onClick.AddListener(() => { onButtonPressed(index); });

            VendorUIButton vendorUIButton = buttons[i].GetComponent<VendorUIButton>();

            if (player.GetPower() < blueprint.items[i].cost)
            {
                buttons[i].GetComponent<Image>().color = new Color(0, 0, 0.4F);
            }
            if (blueprint.items[i].entityBlueprint == null)
            {
                blueprint.items[i].entityBlueprint = SectorManager.TryGettingEntityBlueprint(blueprint.items[i].json); 
            }
            vendorUIButton.blueprint = blueprint.items[i].entityBlueprint;
            vendorUIButton.costText = $"POWER COST: <color=cyan>{blueprint.items[i].cost}</color>";
            vendorUIButton.descriptionText = blueprint.items[i].description;
            vendorUIButton.tooltipPrefab = tooltipPrefab;
            vendorUIButton.costInfo = costInfo;
            vendorUIButton.nameInfo = nameInfo;
            vendorUIButton.handler = UI.GetComponentInChildren<SelectionDisplayHandler>();

            Image sr = buttons[i].transform.Find("Icon").GetComponent<Image>();
            sr.sprite = ResourceManager.GetAsset<Sprite>(blueprint.items[i].icon);

            Text[] texts = buttons[i].GetComponentsInChildren<Text>();
            texts[0].text = (i + 1).ToString();
            texts[1].text = blueprint.items[i].cost.ToString();
            texts[1].color = Color.cyan;
        }

        opened = true;
    }

    public void SetVendor(IVendor vendor, PlayerCore player)
    {
        this.player = player;
        this.vendor = vendor;
        blueprint = vendor.GetVendingBlueprint();
    }

    private void ClearVendor()
    {
        this.player = null;
        this.vendor = null;
        this.blueprint = null;
    }

    private void Update()
    {
        if (!opened) return;
        if (vendor == null || vendor.Equals(null))
        {
            Debug.Log("No vendor!");
            return;
        }

        if (player == null)
        {
            Debug.Log("No player set! Using default player.");
            player = PlayerCore.Instance;
        }

        if (player && vendor.NeedsAlliedFaction() && !FactionManager.IsAllied(vendor.GetFaction(), player.faction))
        {
            Debug.Log("Vendor faction changed");
            CloseUI();
            return;
        }

        if (player.GetIsDead())
        {
            Debug.Log("Player is dead!");
            CloseUI();
            return;
        }

        if ((vendor.GetPosition() - player.transform.position).sqrMagnitude > range)
        {
            Debug.Log("Player moved out of the vendor range");
            CloseUI();
            return;
        }

        for (int i = 0; i < blueprint.items.Count; i++)
        {
            if (player.GetPower() < blueprint.items[i].cost)
            {
                buttons[i].GetComponent<Image>().color = new Color(0, 0, 0.4F);
            }
            else
            {
                buttons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void CloseUI()
    {
        UI.GetComponentInChildren<GUIWindowScripts>().CloseUI();
        opened = false;
        Destroy(UI);
        ClearVendor();
    }

    public static Entity BuyItem(ShellCore core, int index, IVendor vendor)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            if (!core.networkAdapter) return null;
            core.networkAdapter.ExecuteVendorPurchaseServerRpc(index, vendor.GetAdapter().NetworkObjectId);
            return null;
        }

        // TODO: these booleans can be used this way right now but a new IVendor state should be created for if commanding count is needed
        if (vendor.NeedsAlliedFaction() && core.unitsCommanding.Count >= core.GetTotalCommandLimit())
        {
            return null;
        }

        GameObject creation = new GameObject();
        creation.transform.position = vendor.GetPosition();
        var blueprint = vendor.GetVendingBlueprint();
        
        if (index < 0 || blueprint.items.Count <= index) return null;
        if (core.GetPower() < blueprint.items[index].cost) return null;

        if (blueprint.items[index].entityBlueprint == null)
        {
            blueprint.items[index].entityBlueprint = SectorManager.TryGettingEntityBlueprint(blueprint.items[index].json); 
        }
        bool tractor = false;
        switch (blueprint.items[index].entityBlueprint.intendedType)
        {
            case EntityBlueprint.IntendedType.Turret:
                Turret tur = creation.AddComponent<Turret>();
                tur.blueprint = blueprint.items[index].entityBlueprint;
                tractor = true;
                tur.SetOwner(core);

                if (SectorManager.instance && SectorManager.instance.GetComponentInChildren<BattleZoneManager>())
                {
                    var stats = SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Find(s => s.faction == core.faction);
                    if (stats == null)
                    {
                        stats = new BattleZoneManager.Stats(core.faction);
                        SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Add(stats);
                    }
                    stats.turretSpawns++;
                }
                break;
            case EntityBlueprint.IntendedType.Tank:
                Tank tank = creation.AddComponent<Tank>();
                tank.blueprint = blueprint.items[index].entityBlueprint;
                tank.SetOwner(core);
                break;
            case EntityBlueprint.IntendedType.Drone:
                Drone drone = creation.AddComponent<Drone>();
                drone.blueprint = blueprint.items[index].entityBlueprint;
                drone.SetOwner(core);
                break;
            case EntityBlueprint.IntendedType.ShellCore:
                ShellCore shellCore = creation.AddComponent<ShellCore>();
                shellCore.blueprint = blueprint.items[index].entityBlueprint;
                shellCore.sectorMngr = SectorManager.instance;
                break;
            case EntityBlueprint.IntendedType.Tower:
                Tower tower = creation.AddComponent<Tower>();
                tower.blueprint = blueprint.items[index].entityBlueprint;
                tower.sectorMngr = SectorManager.instance;
                (vendor as TowerBase).SetCurrentTower(tower);
                break;
            default:
                break;
        }
        var ent = creation.GetComponent<Entity>();
        ent.spawnPoint = vendor.GetPosition();
        ent.faction = core.faction;
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
        {
            ent.ID = SectorManager.instance.GetFreeEntityID();
            ent.blueprintString = JsonUtility.ToJson(blueprint.items[index].entityBlueprint);
            ent.AttemptCreateNetworkObject(false);
        }
        if (tractor) core.SetTractorTarget(creation.GetComponent<Draggable>());
        creation.name = blueprint.items[index].entityBlueprint.name;
        core.sectorMngr.InsertPersistentObject(blueprint.items[index].entityBlueprint.name, creation);
        core.AddPower(-blueprint.items[index].cost);
        return ent;
    }

    public void onButtonPressed(int index)
    {
        if (player.GetPower() >= blueprint.items[index].cost && (!vendor.NeedsAlliedFaction() || FactionManager.IsAllied(player.faction, vendor.GetFaction()))
                                                             && (!vendor.NeedsAlliedFaction() || player.unitsCommanding.Count < player.GetTotalCommandLimit()))
        {
            BuyItem(player, index, vendor);
            if (GetActive())
            {
                CloseUI();
            }

            ClearVendor();
        }
        else if (player && (vendor.NeedsAlliedFaction() && player.GetUnitsCommanding().Count >= player.GetTotalCommandLimit()))
        {
            player.alerter.showMessage("Unit limit reached!", "clip_alert");
        }
    }

    public UnityEvent GetOnCancelled()
    {
        return null;
    }
}
