using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipBuilder : GUIWindowScripts, IBuilderInterface {
	public GameObject SBPrefab;
	public Vector3 yardPosition;
	public Image shell;
	public Image core;
	public ShipBuilderCursorScript cursorScript;
	public GameObject buttonPrefab;
	public PlayerCore player;
	public Transform smallContents;
	public Transform mediumContents;
	public Transform largeContents;
	public Transform traderSmallContents;
	public Transform traderMediumContents;
	public Transform traderLargeContents;
	protected Transform[] contentsArray; // holds scroll view sub-sections by part size
	private Transform[] traderContentsArray;
	public GameObject smallText;
	public GameObject mediumText;
	public GameObject largeText;
	public GameObject traderSmallText;
	public GameObject traderMediumText;
	public GameObject traderLargeText;
	protected GameObject[] contentTexts;
	private GameObject[] traderContentTexts;
	private PresetButton[] presetButtons;
	protected string searcherString;
	private bool[] displayingTypes;
	public Image reconstructImage;
	public Text reconstructText;
	protected bool initialized;
	public TipsFromTheYard tips;
	public GameObject traderScrollView;
	protected Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;
	private Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> traderPartDict;
	public BuilderMode mode;
	private int[] abilityLimits;
	public SelectionDisplayHandler displayHandler;
	public GameObject currentPartHandler;
	public bool editorMode;
	public Text titleText;
	public GameObject editorModeButtons;
	public static WorldData.CharacterData currentCharacter;
	public GameObject editorModeAddPartSection;
	public static ShipBuilder instance;
	public static bool heavyCheat = false;
	private int editorCoreTier = 0;
		public BuilderMode GetMode() {
		return mode;
	}
	public GameObject partSelectContainer;
	public Transform[] partSelectTransforms;

	public bool ContainsParts(List<EntityBlueprint.PartInfo> parts) {
		Dictionary<EntityBlueprint.PartInfo, int> counts = new Dictionary<EntityBlueprint.PartInfo, int>();
		// get the part counts
		foreach(EntityBlueprint.PartInfo info in partDict.Keys) {
			var p = CullSpatialValues(info);
			if(!counts.ContainsKey(p)) {
				counts.Add(p, partDict[p].GetCount());
			}
		}
		foreach(ShipBuilderPart inf in cursorScript.parts) {
			var p = CullSpatialValues(inf.info);
			if(!counts.ContainsKey(p)) {
				counts.Add(p, 1);
			} else counts[p]++;
		}

		foreach(EntityBlueprint.PartInfo part in parts) {
			var p = CullSpatialValues(part);
			if(!counts.ContainsKey(p)) {
				return false;
			}
			else if(--counts[p] < 0) {
				return false;
			}
		}
		return true;
	}
	public bool DecrementPartButton(EntityBlueprint.PartInfo info) {
		if(partDict.ContainsKey(CullSpatialValues(info)) && partDict[CullSpatialValues(info)].GetCount() > 0) {
			partDict[CullSpatialValues(info)].DecrementCount();
			return true;
		} else return false;
	}
	public static EntityBlueprint.PartInfo CullSpatialValues(EntityBlueprint.PartInfo partToCull) {
		var part = new EntityBlueprint.PartInfo();
		part.partID = partToCull.partID;
		part.abilityID = partToCull.abilityID;
		if(part.abilityID == 10) part.secondaryData = partToCull.secondaryData;
		part.tier = partToCull.tier;
		part.shiny = partToCull.shiny;
		return part;
	}
	public enum TransferMode {
		Sell,
		Return,
		Buy
	}
	public void DispatchPart(ShipBuilderPart part, TransferMode transferMode) {
		var culledInfo = CullSpatialValues(part.info);
		Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> dict;
		Transform[] dictContentsArray;
		GameObject[] dictContentTexts;
		switch(transferMode) {
			case TransferMode.Sell:
				cursorScript.buildCost -= EntityBlueprint.GetPartValue(part.info);
				dictContentsArray = traderContentsArray;
				dict = traderPartDict;
				dictContentTexts = traderContentTexts;
				break;
			case TransferMode.Buy:
				dictContentsArray = contentsArray;
				dict = partDict;
				dictContentTexts = contentTexts;
				break;
			default: // transfer back to original inventory
				if(part.mode == BuilderMode.Trader) cursorScript.buildCost -= EntityBlueprint.GetPartValue(part.info);
				dict = (part.mode == BuilderMode.Yard ? partDict : traderPartDict);
				dictContentsArray = (part.mode == BuilderMode.Yard ? contentsArray : traderContentsArray);
				dictContentTexts = (part.mode == BuilderMode.Yard ? contentTexts : traderContentTexts);
				break;
		}
		if(!dict.ContainsKey(culledInfo)) {
			int size = ResourceManager.GetAsset<PartBlueprint>(part.info.partID).size;
			ShipBuilderInventoryScript dictInvButton = Instantiate(buttonPrefab, 
				dictContentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
			switch(transferMode) { // set the new button's transfer mode
				case TransferMode.Buy:
					dictInvButton.mode = BuilderMode.Yard;
					break;
				case TransferMode.Sell:
					dictInvButton.mode = BuilderMode.Trader;
					break;
				case TransferMode.Return:
					dictInvButton.mode = part.mode;
					break;
			}
			dict.Add(culledInfo, dictInvButton);
			dictContentTexts[size].SetActive(true);
			dict[culledInfo].part = culledInfo;
			dict[culledInfo].cursor = cursorScript;
		}
		dict[culledInfo].IncrementCount();
		cursorScript.buildValue -= EntityBlueprint.GetPartValue(part.info);
		cursorScript.parts.Remove(part);
		Destroy(part.gameObject);
	}

	public static Bounds GetRect(RectTransform rectTransform) {
		Bounds rect = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform);
		rect.center = rectTransform.anchoredPosition;
		rect.size = rect.size * 1F;
		return rect;
	}
	public enum ReconstructButtonStatus {
		Valid,
		PartInvalidPosition,
		NotEnoughCredits,
		PastSkillsLimit,
		PastSpawnsLimit,
		PastWeaponsLimit,
		PastPassivesLimit,
		PartTooHeavy
	}
	
	public ReconstructButtonStatus reconstructStatus;

	private void SetReconstructButton(ReconstructButtonStatus status) 
	{
		reconstructStatus = status;

		switch(status) {
			case ReconstructButtonStatus.Valid:
				reconstructText.color = Color.green;
				reconstructText.text = !editorMode ? "RECONSTRUCT" : "CONFIRM CHARACTER BLUEPRINT";
				break;
			case ReconstructButtonStatus.PartInvalidPosition:
				reconstructText.color = Color.red;
				reconstructText.text = "A PART IS IN AN INVALID POSITION";
				break;
			case ReconstructButtonStatus.NotEnoughCredits:
				reconstructText.color = Color.red;
				reconstructText.text = "NOT ENOUGH CREDITS";
				break;
			case ReconstructButtonStatus.PastSkillsLimit:
				reconstructText.color = Color.red;
				reconstructText.text = "TOO MANY SKILLS";
				break;
			case ReconstructButtonStatus.PastSpawnsLimit:
				reconstructText.color = Color.red;
				reconstructText.text = "TOO MANY SPAWNS";
				break;
			case ReconstructButtonStatus.PastWeaponsLimit:
				reconstructText.color = Color.red;
				reconstructText.text = "TOO MANY WEAPONS";
				break;
			case ReconstructButtonStatus.PastPassivesLimit:
				reconstructText.color = Color.red;
				reconstructText.text = "TOO MANY PASSIVES";
				break;
			case ReconstructButtonStatus.PartTooHeavy:
				reconstructText.color = Color.red;
				reconstructText.text = "A PART IS TOO HEAVY FOR YOUR CORE";				
				break;
		}
	}
	void UpdateChainHelper(ShipBuilderPart part) {
		var x = GetRect(part.rectTransform);
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(!shipPart.isInChain) {
				var y = GetRect(shipPart.rectTransform);
				y.Expand(0.001F);
				if(x.Intersects(y)) {
					shipPart.isInChain = true;
					UpdateChainHelper(shipPart);
				}
			}
		}
	}

	Vector2 GetImageBoundsBySprite(Image img)
	{
		return img.sprite.bounds.size * 100;
	}

	public bool CheckPartIntersectsWithShell(ShipBuilderPart shipPart)
	{
		// make sure calculations here are only with core1_shell
		// TODO: optimize by storing the sprite in a specific reference
		var oldSprite = shell.sprite;
		shell.sprite = ResourceManager.GetAsset<Sprite>("core1_shell");
		shell.rectTransform.sizeDelta = GetImageBoundsBySprite(shell);

		var shellRect = GetRect(shell.rectTransform);

		var partBounds = GetRect(shipPart.rectTransform);
		if(partBounds.Intersects(shellRect)) {
			bool z = Mathf.Abs(shipPart.rectTransform.anchoredPosition.x - shell.rectTransform.anchoredPosition.x) <=
			0.18F*(shipPart.rectTransform.sizeDelta.x + shell.rectTransform.sizeDelta.x) &&
			Mathf.Abs(shipPart.rectTransform.anchoredPosition.y - shell.rectTransform.anchoredPosition.y) <=
			0.18F*(shipPart.rectTransform.sizeDelta.y + shell.rectTransform.sizeDelta.y);
			shipPart.isInChain = !z;

			// reset sprite
			shell.sprite = oldSprite;
			shell.rectTransform.sizeDelta = GetImageBoundsBySprite(shell);
			return z;
		}

		shell.sprite = oldSprite;
		shell.rectTransform.sizeDelta = GetImageBoundsBySprite(shell);
		return false;
	}

	public void UpdateChain() {
		if(!editorMode)
			SetReconstructButton(cursorScript.buildCost > player.credits ? 
				ReconstructButtonStatus.NotEnoughCredits : ReconstructButtonStatus.Valid);
		else SetReconstructButton(ReconstructButtonStatus.Valid);

		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			shipPart.isInChain = false;
			CheckPartIntersectsWithShell(shipPart);
		}

		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(shipPart.isInChain) UpdateChainHelper(shipPart);
		}

		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			CheckPartIntersectsWithShell(shipPart);
		}

		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(!shipPart.isInChain || !shipPart.validPos) {
				SetReconstructButton(ReconstructButtonStatus.PartInvalidPosition);
				return;
			}
		}

		CheckPartSizes();
		CheckAbilityCaps();
	}

	private bool CheckPartSizes() {
		int maxTier = !editorMode && !heavyCheat ? CoreUpgraderScript.GetPartTierLimit(player.blueprint.coreShellSpriteID) : 3;
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(ResourceManager.GetAsset<PartBlueprint>(shipPart.info.partID).size > maxTier) {
				SetReconstructButton(ReconstructButtonStatus.PartTooHeavy);
				return false;
			}
		}
		return true;
	}

	private bool CheckAbilityCaps() {
		if(editorMode) return true;
		var currentAbilitynumbers = new int[] {0, 0, 0, 0, 0};

		foreach(ShipBuilderPart shipBuilderPart in cursorScript.parts) {
			var type = (int)AbilityUtilities.GetAbilityTypeByID(shipBuilderPart.info.abilityID);
			currentAbilitynumbers[type]++;
		}

		var extras = CoreUpgraderScript.GetExtraAbilities(player.blueprint.coreShellSpriteID);

		for(int i = 0; i < 4; i++) {
			if(currentAbilitynumbers[i] > abilityLimits[i] + extras[i]) {
				SetReconstructButton((ReconstructButtonStatus)(3 + i));
				return false;
			}
		}
		return true;
	}

	static Dictionary<string, List<(string, int)>> originsofParts = new Dictionary<string, List<(string, int)>>();

	public static void AddOriginToDictionary(ShellPart part)
	{
		if(!originsofParts.ContainsKey(part.droppedSectorName))
		{
			var list = new List<(string, int)>();
			list.Add((part.info.partID, part.info.abilityID));
			originsofParts.Add(part.droppedSectorName, list);
		} else originsofParts[part.droppedSectorName].Add((part.info.partID, part.info.abilityID));
	}

	public static bool CheckForOrigin(string sectorName, (string, int) tuple)
	{
		if(originsofParts.ContainsKey(sectorName))
		{
			return originsofParts[sectorName].Contains(tuple);
		} 
		else return false;
	}

	public static void RemoveOrigin(string sectorName, (string, int) tuple)
	{
		if(originsofParts.ContainsKey(sectorName))
		{
			if(originsofParts[sectorName].Contains(tuple)) originsofParts[sectorName].Remove(tuple);
		}
	}

	public void Initialize(BuilderMode mode, List<EntityBlueprint.PartInfo> traderInventory = null, EntityBlueprint blueprint = null) {
		SetSelectPartActive(false);
		// set editor mode if testing
		if(SectorManager.testJsonPath != null && !Input.GetKey(KeyCode.LeftShift) && mode == BuilderMode.Yard)
			editorMode = true;
		else if(SceneManager.GetActiveScene().name != "WorldCreator") editorMode = false;

		// initialize window on screen
		if(initialized) CloseUI(false); // prevent initializing twice by closing UI if already initialized
		initialized = true;
		instance = this;
		Activate();
		cursorScript.gameObject.SetActive(false);
		cursorScript.SetBuilder(this);

		GetComponentInChildren<ShipBuilderPartDisplay>().Initialize(this);

		// set up actual stats
		this.mode = mode;
		cursorScript.SetMode(mode);
		searcherString = "";
		contentsArray = new Transform[] {smallContents, mediumContents, largeContents};
		traderContentsArray = new Transform[] {traderSmallContents, traderMediumContents, traderLargeContents};
		contentTexts = new GameObject[] {smallText, mediumText, largeText};
		traderContentTexts = new GameObject[] {traderSmallText, traderMediumText, traderLargeText};
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}
		foreach(GameObject traderObj in traderContentTexts) {
			traderObj.SetActive(false);
		}
		displayingTypes = new bool[] {true, true, true, true, true};
		if(player)
			player.SetIsInteracting(true);
		partDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();
		traderPartDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();

		// set shell sprite and color
		if(!editorMode && !blueprint) blueprint = player.blueprint;
		if(!editorMode)
		{
			shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
			core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);	
		}
		else
		{
			shell.sprite = ResourceManager.GetAsset<Sprite>("core1_shell");
			core.sprite = ResourceManager.GetAsset<Sprite>("core1_light");
		}

		shell.color = FactionManager.GetFactionColor(0);
		shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;

		// orient shell image so relative center stays the same regardless of shell tier
		shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
		core.rectTransform.anchoredPosition = -shell.rectTransform.anchoredPosition;
		
		core.material = ResourceManager.GetAsset<Material>("material_color_swap");
		core.color = FactionManager.GetFactionColor(0);
		core.preserveAspect = true;
		core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
		List<EntityBlueprint.PartInfo> parts = new List<EntityBlueprint.PartInfo>();
		if(!editorMode)
		{
			parts = player.GetInventory();
			cursorScript.player = player;
			cursorScript.handler = player.GetAbilityHandler();

			abilityLimits = player.abilityCaps;
		}
		else
		{
			/* Adds all part/ability/tier/drone permutations to the player's inventory. Uncomment to cheat.
			for(int i = 0; i < 8; i++) 
			{
				EntityBlueprint.PartInfo info = new EntityBlueprint.PartInfo();
				info.partID = "SmallCenter1";
				info.abilityID = 10;
				DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)i);
				info.secondaryData = JsonUtility.ToJson(data);
				if(info.abilityID == 0 || info.abilityID == 10) info.tier = 0;
				parts.Add(info);
			}
			
			if(parts.Count == 0) {
				EntityBlueprint.PartInfo info = new EntityBlueprint.PartInfo();
				foreach(string name in ResourceManager.allPartNames) {
					for(int i = 0; i < 22; i++) 
					{
						info.partID = name;
						info.abilityID = i;
						if((info.abilityID >= 14 && info.abilityID <= 16) || info.abilityID == 3) info.abilityID = 0;
						if(info.abilityID == 10) {
							DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)Random.Range(0, 8));
							info.secondaryData = JsonUtility.ToJson(data);
						}
						if(info.abilityID == 0 || info.abilityID == 10 || info.abilityID == 21) info.tier = 0;
						else info.tier = 1;
						parts.Add(info);
						parts.Add(info);
						parts.Add(info);
					}
				}
			}*/
		}

		if(mode == BuilderMode.Trader)
		{
			var traderInv = new List<EntityBlueprint.PartInfo>();
			foreach(var part in traderInventory)
			{
				traderInv.Add(CullSpatialValues(part));

				// Player has now seen all the parts in the trader's inventory.
				player.cursave.partsSeen.Add(PartIndexScript.CullToPartIndexValues(part));
			}
			traderInventory = traderInv;
		}
		
		// hide the buttons and yard tips if interacting with a trader

		tips.gameObject.SetActive(mode == BuilderMode.Yard);
		traderScrollView.gameObject.SetActive(mode == BuilderMode.Trader);

		/*
		if(traderInventory.Count == 0) {
			EntityBlueprint.PartInfo info = new EntityBlueprint.PartInfo();
			foreach(string name in ResourceManager.allPartNames) {
				for(int i = 0; i < 3; i++) 
				{
					info.partID = name;
					info.abilityID = Random.Range(0,21);
					if((info.abilityID >= 14 && info.abilityID <= 16) || info.abilityID == 3) info.abilityID = 0;
					if(info.abilityID == 10) {
						DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)Random.Range(0, 8));
						info.secondaryData = JsonUtility.ToJson(data);
					}
					if(info.abilityID == 0 || info.abilityID == 10) info.tier = 0;
					else info.tier = Random.Range(1, 4);
					traderInventory.Add(info);
				}
			}
		}
		*/

		if(traderInventory != null) {
			foreach(EntityBlueprint.PartInfo info in traderInventory) { // TODO: cull values to prevent possible problems
				if(!traderPartDict.ContainsKey(info)) {
					int size = ResourceManager.GetAsset<PartBlueprint>(info.partID).size;
					ShipBuilderInventoryScript traderInvButton = Instantiate(buttonPrefab, // instantiate part button for trader
						traderContentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
						traderContentTexts[size].SetActive(true);
						traderInvButton.part = info;
						traderInvButton.cursor = cursorScript;
						traderInvButton.mode = BuilderMode.Trader;
					traderPartDict.Add(info, traderInvButton);
					for(int i = 0; i < 500; i++) traderInvButton.IncrementCount();
				}
			}
		}

		if(parts != null) {
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = CullSpatialValues(parts[i]);
			}
		}

		foreach(EntityBlueprint.PartInfo part in parts) {
			AddPart(part);
		}

		var partsToAdd = new List<ShellPart>();
		if(!editorMode)
		{
			var initialShards = player.shards;

			foreach(Entity ent in player.GetUnitsCommanding()) {
				if(!(ent as Drone)) continue;
				var beam = ent.GetComponentInChildren<TractorBeam>();
				if(beam)
				{
					var target = beam.GetTractorTarget();
					if (target && target.GetComponent<ShellPart>())
					{
						partsToAdd.Add(target.GetComponent<ShellPart>());
						AddOriginToDictionary(target.GetComponent<ShellPart>());

					} else if(target && target.GetComponent<Shard>()) {
						AddShard(target.GetComponent<Shard>());
						Destroy(target.gameObject);
					}
				}
			}

			var playerTarget = player.GetTractorTarget();
			if(playerTarget && playerTarget.GetComponent<ShellPart>()) {
				partsToAdd.Add(playerTarget.GetComponent<ShellPart>());
				AddOriginToDictionary(playerTarget.GetComponent<ShellPart>());
			} else if(playerTarget && playerTarget.GetComponent<Shard>()) {
				AddShard(playerTarget.GetComponent<Shard>());
				Destroy(playerTarget.gameObject);
			}

			if(initialShards != player.shards) {
				ShardCountScript.DisplayCount(player.shards);
			}

			foreach(ShellPart part in partsToAdd) {
				var info = part.info;
				info = ShipBuilder.CullSpatialValues(info);
				AddPart(info);
				player.cursave.partInventory.Add(info);

				PartIndexScript.AttemptAddToPartsObtained(info);
				PartIndexScript.AttemptAddToPartsSeen(info);
				Destroy(part.gameObject);
			}
			LoadBlueprint(blueprint);
		}

		// activate windows
		gameObject.SetActive(true);

		if(SceneManager.GetActiveScene().name == "SampleScene" && (presetButtons == null || presetButtons.Length == 0)) 
		{
			presetButtons = GetComponentsInChildren<PresetButton>();
			foreach(PresetButton button in presetButtons) {
				button.SBPrefab = SBPrefab;
				button.player = player;
				button.cursorScript = cursorScript;
				button.builder = this;
				button.displayHandler = displayHandler;
				button.currentPartHandler = currentPartHandler;
				button.Initialize();
			}
		}

		if(!editorMode)
		{
			if(editorModeButtons) editorModeButtons.SetActive(false);

			foreach(PresetButton button in presetButtons) {
				button.gameObject.SetActive(mode == BuilderMode.Yard);
				button.CheckValid();
			}
		}
		else
		{
			presetButtons = GetComponentsInChildren<PresetButton>();
			foreach(PresetButton button in presetButtons) {
				button.gameObject.SetActive(false);
			}	

			editorModeButtons.SetActive(true);
		}

		// set title text
		switch(mode)
		{
			case BuilderMode.Yard:
				if(!editorMode)
				{
					titleText.text = "SHIP BUILDER - YARD";
				}
				else
				{
					titleText.text = "SHIP BUILDER - SUPER DUPER EDITOR MODE";
				}
				break;
			case BuilderMode.Trader:
				titleText.text = "SHIP BUILDER - TRADER";
				break;
			case BuilderMode.Workshop:
				titleText.text = "SHIP BUILDER - DRONE WORKSHOP";
				break;
			default:
				break;
		}

		var dropdown = editorModeAddPartSection.transform.Find("Ability ID").GetComponent<Dropdown>();
		if(editorMode && dropdown.options.Count == 0)
		{
			foreach(var id in System.Enum.GetValues(typeof(AbilityID)))
			{
				dropdown.options.Add(new Dropdown.OptionData
				(id.ToString()));
			}
		}

		cursorScript.gameObject.SetActive(true);
		cursorScript.UpdateHandler();
	}
    public void AddShard(Shard shard) {
        var tiers = new int[] {1, 5, 20};
        player.shards += tiers[shard.tier];
    }
	private void AddPart(EntityBlueprint.PartInfo part) {
		if(!partDict.ContainsKey(part)) {
			int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
			ShipBuilderInventoryScript invButton = Instantiate(buttonPrefab, 
				contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
			partDict.Add(part, invButton);
			contentTexts[size].SetActive(true);
			invButton.part = part;
			invButton.cursor = cursorScript;
			invButton.IncrementCount();
			invButton.mode = BuilderMode.Yard;
		} else partDict[part].IncrementCount();

		if(editorMode) {
			for(int i = 0; i < 100; i++)
				partDict[part].IncrementCount();
		}
	}
	
	public void AddPartByEditorSection() {
		var part = new EntityBlueprint.PartInfo();
		
		if(int.TryParse(editorModeAddPartSection.transform.Find("Ability Tier").GetComponent<InputField>().text, out part.tier)) {
			
			var secondaryData = editorModeAddPartSection.transform.Find("Secondary Data").GetComponent<InputField>().text;
			part.secondaryData = secondaryData != null ? secondaryData : "";
			part.abilityID = editorModeAddPartSection.transform.Find("Ability ID").GetComponent<Dropdown>().value;
			var x = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>().text;
			if(ResourceManager.allPartNames.Contains(x)) {
				part.partID = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>().text;
				AddPart(part);
			}

		}
	}

	public void ChangePartImage(string id) {
		if(ResourceManager.allPartNames.Contains(id)) {
			var im = editorModeAddPartSection.transform.Find("Part Image").GetComponent<Image>();
			im.sprite = ResourceManager.GetAsset<Sprite>(id + "_sprite");
			im.rectTransform.sizeDelta = im.sprite.bounds.size * 50	;
		}
	}

	public override void CloseUI() {
		CloseUI(false);
	}

	public void CloseUI(bool validClose) {
		if(editorMode)
			SetSelectPartActive(false);
		if(!validClose) AudioManager.PlayClipByID("clip_back");
		else AudioManager.PlayClipByID("clip_select");
		initialized = false;
		if(player) player.SetIsInteracting(false);
		gameObject.SetActive(false);
		if(!editorMode && validClose) {

			// try adding parts in the player's inventory and on their ship into the part index obtained list.
			player.cursave.partInventory = new List<EntityBlueprint.PartInfo>();
			foreach(EntityBlueprint.PartInfo info in partDict.Keys) {
				if(partDict[info].GetCount() > 0) {
					for(int i = 0; i < partDict[info].GetCount(); i++)
					{
						player.cursave.partInventory.Add(info);
						PartIndexScript.AttemptAddToPartsObtained(info);
					}
				}
			}

			foreach(EntityBlueprint.PartInfo info2 in player.blueprint.parts)
			{
				PartIndexScript.AttemptAddToPartsObtained(info2);
			}
		}

		foreach(ShipBuilderInventoryScript button in partDict.Values) {
			Destroy(button.gameObject);
		}
		foreach(ShipBuilderInventoryScript traderButton in traderPartDict.Values) {
			Destroy(traderButton.gameObject);
		}
		foreach(ShipBuilderPart part in cursorScript.parts) {
			Destroy(part.gameObject);
		}
		cursorScript.parts = new List<ShipBuilderPart>();
		if((!editorMode || player) && !validClose) {
			AbilityHandler handler = player.GetAbilityHandler(); // reset handler to correct representation
			handler.Deinitialize();
			handler.Initialize(player);
		}
	}
	public void LoadBlueprint(EntityBlueprint blueprint) {
		foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
			var p = Instantiate(SBPrefab, cursorScript.transform.parent).GetComponent<ShipBuilderPart>();
			p.cursorScript = cursorScript;
			cursorScript.parts.Add(p);
			p.info = part;
			p.SetLastValidPos(part.location);
			p.isInChain = true;
			p.validPos = true;
		}

		if(editorMode)
		{
			for(int i = 0; i < CoreUpgraderScript.GetCoreNames().Length; i++)
			{
				if(CoreUpgraderScript.GetCoreNames()[i] == blueprint.coreShellSpriteID)
				{
					editorCoreTier = i;
				}
			}
		}
		
		core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
		shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
		shell.color = FactionManager.GetFactionColor(0);
		shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
	}
	public static void SaveBlueprint(EntityBlueprint blueprint = null, string fileName = null, string json = null) {
		if(fileName != null) 
			System.IO.File.WriteAllText(fileName, json);
		#if UNITY_EDITOR
		else
			AssetDatabase.CreateAsset(blueprint, "Assets\\SavedPrint.asset");
		#endif
	}

	private List<ShipBuilderInventoryNameSelect> nameSelectObjs = new List<ShipBuilderInventoryNameSelect>();
	public void SetSelectPartActive(bool active)
	{
		if(partSelectContainer.activeSelf && active) return;
		partSelectContainer.SetActive(active);

		if(active)
		{
			nameSelectObjs.Clear();
			foreach(var name in ResourceManager.allPartNames)
			{
				var size = ResourceManager.GetAsset<PartBlueprint>(name).size;
				GameObject invButton = Instantiate(buttonPrefab, 
				partSelectTransforms[size]);

				// remove the inventory button and add a name select button. Carry over the refs in the inventory button
				var oldComp = invButton.GetComponent<ShipBuilderInventoryScript>();
				var shiny = oldComp.isShiny;
				Destroy(oldComp);
				var comp = invButton.AddComponent<ShipBuilderInventoryNameSelect>();
				comp.part = new EntityBlueprint.PartInfo();
				comp.part.partID = name;
				comp.isShiny = shiny;
				comp.builder = this;
				comp.field = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>();
				nameSelectObjs.Add(comp);
			}
		}
		else
		{
			foreach(var obj in nameSelectObjs)
			{
				Destroy(obj.gameObject);
			}
			nameSelectObjs.Clear();
		}
	}

	public void LoadBlueprint(string json) {
		EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
		JsonUtility.FromJsonOverwrite(json, blueprint);
		LoadBlueprint(blueprint);
	}



	#if UNITY_EDITOR
	public void SavePrintWithPrompt() {
		var path = UnityEditor.EditorUtility.SaveFilePanel("Save Blueprint", Application.streamingAssetsPath + "\\Entities", 
			"DefaultPrint", "json");
		SaveBlueprint(null, path, GetCurrentJSON());
	}

	public void LoadPrintWithPrompt() {
		var path = UnityEditor.EditorUtility.OpenFilePanel("Load Blueprint", Application.streamingAssetsPath + "\\Entities", "json");
		LoadBlueprint(System.IO.File.ReadAllText(path));
	}

	#endif
	public InputField inField;
	public void SetBlueprint() {
		if(inField.text == "") return;
		var blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
		JsonUtility.FromJsonOverwrite(inField.text, blueprint);
		if(!blueprint) return;
		CloseUI(false);
		inField.text = "";
		Initialize(BuilderMode.Yard, null, blueprint);
		#if UNITY_EDITOR
			SaveBlueprint(blueprint); // creates an asset of that blueprint for later use
		#endif
	}
	public void Deinitialize() {
		if(!editorMode && cursorScript.buildCost > player.credits) return;
		if(!CheckAbilityCaps()) return;
		if(!CheckPartSizes()) return;
		bool invalidState = false;
		foreach(ShipBuilderPart part in cursorScript.parts) {
			if(!part.validPos || !part.isInChain) {
				invalidState = true;
				break;
			}
		}
		if(!invalidState) {
			Export();
			CloseUI(true);
		}
	}

	public void Export() {
		if(player)
		{
			player.credits -= cursorScript.buildCost;
			player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
			foreach(ShipBuilderPart part in cursorScript.parts) {
				player.blueprint.parts.Add(part.info);
			}
			player.Rebuild();
		}

		#if UNITY_EDITOR
		if(editorMode && currentCharacter != null) 
		{
			currentCharacter.blueprintJSON = GetCurrentJSON();
			WorldCreatorCursor.instance.characterHandler.reflectData();
			
			// null character so another change doesn't accidentally happen
			currentCharacter = null;
		}
			
		#endif
        
		NodeEditorFramework.Standard.UsePartCondition.OnPlayerReconstruct.Invoke();
	}

	protected override void Update() {
		base.Update();
		if(!editorMode)
		{
			if((player.transform.position - yardPosition).sqrMagnitude > 200)
				CloseUI(false);
		}

		if(editorMode && Input.GetKeyDown(KeyCode.H))
		{
			TraderInventory trader = new TraderInventory();
			trader.parts = new List<EntityBlueprint.PartInfo>();
			foreach(var part in partDict.Keys)
			{
				Debug.Log("a");
				trader.parts.Add(CullSpatialValues(part));
			}
			Debug.Log(JsonUtility.ToJson(trader));
		}

		if(editorMode && Input.GetKeyDown(KeyCode.X))
		{
			foreach(var input in GetComponentsInChildren<InputField>())
			{
				if(input.isFocused) return;
			}

			var cores = new List<string>(CoreUpgraderScript.GetCoreNames());
			cores.Add("groundcarriershell");
			cores.Add("drone_shell");
			
			editorCoreTier++;
			editorCoreTier %= cores.Count;
			shell.sprite = ResourceManager.GetAsset<Sprite>(cores[editorCoreTier]);
			if(editorCoreTier == cores.Count - 2)
				core.sprite = ResourceManager.GetAsset<Sprite>("groundcarriercore");
			else if(editorCoreTier == cores.Count - 1)
				core.sprite = ResourceManager.GetAsset<Sprite>("drone_light");
			else
				core.sprite = ResourceManager.GetAsset<Sprite>("core1_light");				
			
			shell.color = FactionManager.GetFactionColor(0);
			shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;

			// orient shell image so relative center stays the same regardless of shell tier
			shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
			core.rectTransform.anchoredPosition = -shell.rectTransform.anchoredPosition;
			
			core.material = ResourceManager.GetAsset<Material>("material_color_swap");
            core.color = FactionManager.GetFactionColor(0);

            core.preserveAspect = true;
			core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
		}
		
	}

    [System.Serializable]
    public struct TraderInventory
    {
        public List<EntityBlueprint.PartInfo> parts;
    }

	public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn() {
		foreach(ShipBuilderInventoryScript inv in partDict.Values) {
			if(RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition) 
				&& inv.gameObject.activeSelf) {
				return inv.part;
			}
		}
		foreach(ShipBuilderInventoryScript traderInv in traderPartDict.Values) {
			if(RectTransformUtility.RectangleContainsScreenPoint(traderInv.GetComponent<RectTransform>(), Input.mousePosition) 
				&& traderInv.gameObject.activeSelf) {
				return traderInv.part;
			}
		}
		return null;
	}
	public void ChangeDisplayFactors() {
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}
		foreach(ShipBuilderInventoryScript inv in partDict.Values) {
			string partName = inv.part.partID.ToLower();
			string abilityName = AbilityUtilities.GetAbilityNameByID(inv.part.abilityID, inv.part.secondaryData).ToLower();
			if(partName.Contains(searcherString) || abilityName.Contains(searcherString) || searcherString == "") {
				if(displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(inv.part.abilityID)]) {
					inv.gameObject.SetActive(true);
					contentTexts[ResourceManager.GetAsset<PartBlueprint>(inv.part.partID).size].SetActive(true);
				}
				else inv.gameObject.SetActive(false);
			} else inv.gameObject.SetActive(false);
		}
	}

	public string GetCurrentJSON() {
		EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();	
		if(!editorMode)
		{
			blueprint.coreShellSpriteID = player.blueprint.coreShellSpriteID;
			blueprint.coreSpriteID = player.blueprint.coreSpriteID;
		}
		else
		{
			var cores = new List<string>(CoreUpgraderScript.GetCoreNames());
			cores.Add("groundcarriershell");
			cores.Add("drone_shell");
			blueprint.coreShellSpriteID = cores[editorCoreTier % cores.Count];
			if(editorCoreTier == cores.Count - 2)
				blueprint.coreSpriteID = "groundcarriercore";
			else if(editorCoreTier == cores.Count - 1)
				blueprint.coreSpriteID = "drone_light";
			else
				blueprint.coreSpriteID = "core1_light";
		}
		blueprint.parts = new List<EntityBlueprint.PartInfo>();
		foreach(ShipBuilderPart part in cursorScript.parts) {
			blueprint.parts.Add(part.info);
		}
		return JsonUtility.ToJson(blueprint);
	}
	
	public void SetSearcherString(string searcher) {
		searcherString = searcher.ToLower();
		ChangeDisplayFactors();
	}
	public void UpdateDisplayingCategories(int type) {
		displayingTypes[type] = !displayingTypes[type];
		ChangeDisplayFactors();
	}

	public override bool GetActive() {
		return gameObject.activeSelf;
	}

	/// prevent dragging the window if the mouse is on the grid
	public override void OnPointerDown(PointerEventData eventData) {
		if(RectTransformUtility.RectangleContainsScreenPoint(cursorScript.grid2mask, Input.mousePosition)) return;
		base.OnPointerDown(eventData);
	}

	public EntityBlueprint.PartInfo? RequestInventoryMouseOverInfo()
	{
		foreach(var part in partDict) {
			if(RectTransformUtility.RectangleContainsScreenPoint(part.Value.GetComponent<RectTransform>(), Input.mousePosition) &&
				part.Value.gameObject.activeSelf) {
				return part.Key;
			}
		}

		if(traderPartDict != null)
			foreach(var part in traderPartDict) {
				if(RectTransformUtility.RectangleContainsScreenPoint(part.Value.GetComponent<RectTransform>(), Input.mousePosition) &&
					part.Value.gameObject.activeSelf) {
					return part.Key;
				}
			}

		return null;
	}
}
