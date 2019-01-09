using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class SectorCreatorMouse : MonoBehaviour {
	
	public Vector3 center = new Vector3(0,0,0);
	public Sector.SectorType type = Sector.SectorType.Neutral;
	public enum ObjectTypes {
		Outpost,
		Bunker,
		Carrier,
		PowerRock,
		Platform,
		ShellCore
	}

	public enum CommandTypes {
		Place,
		Remove,
		Center,
		ChangeFaction,
		Clear
	}

	public struct Command {
		public CommandTypes type;
		public PlaceableObject obj;
		public Vector3 position;
		public List<PlaceableObject> clearedList;
	}

	public Stack<Command> undoStack = new Stack<Command>();
	public Stack<Command> redoStack = new Stack<Command>();

	[System.Serializable]
	public struct PlaceableObject {
		public GameObject obj;
		public ObjectTypes type;
		public int faction;
		public int placeablesIndex;
		public Vector3 pos;
		public int rotation;
	}

	public struct SectorData {
		public string sectorjson;
		public string platformjson;
	}

	public bool windowEnabled = true;
	public int numberOfFactions;
	public PlaceableObject[] placeables;
	private float tileSize = 5F;
	Vector2 cursorOffset = new Vector2(2.5F, 2.5F);
	[HideInInspector]
	public List<PlaceableObject> objects;
	GUIWindowScripts mainMenu;
	GUIWindowScripts sectorProps;
	GUIWindowScripts hotkeyList;
	GUIWindowScripts readFile;
	int cursorCount = 0;
	string sctName;
	int x;
	int y;
	int height;
	int width;
	PlaceableObject cursor;
	Color currentColor = SectorColors.colors[0];
	// Update is called once per frame
	void Start() {
		for(int i = 0; i < placeables.Length; i++) {
			placeables[i].placeablesIndex = i;
		}
		windowEnabled = true;
		objects = new List<PlaceableObject>();
		cursor = placeables[0];
		cursor.obj = Instantiate(placeables[0].obj) as GameObject;
		cursor.obj.transform.position = cursorOffset;
		mainMenu = transform.Find("MenuBox").GetComponent<GUIWindowScripts>();
		sectorProps = transform.Find("SectorProps").GetComponent<GUIWindowScripts>();
		hotkeyList = transform.Find("Hotkey List").GetComponent<GUIWindowScripts>();
		readFile = transform.Find("ReadFile").GetComponent<GUIWindowScripts>();
		UpdateColors();
	}

	public void Undo() {
		if(undoStack.Count > 0) {
			Command com = undoStack.Pop();
			switch(com.type) {
				case CommandTypes.Place:
					foreach(PlaceableObject obj in objects) {
						if(obj.type == com.obj.type && obj.obj.transform.position == com.position) {
							objects.Remove(obj);
							Destroy(obj.obj);
							break;
						}
					}
					break;
				case CommandTypes.Remove:
					com.obj.obj = Instantiate(placeables[com.obj.placeablesIndex].obj, com.position, Quaternion.identity);
					objects.Add(com.obj);
					break;
				case CommandTypes.Center:
					Camera.main.transform.position = com.position;
					break;
				case CommandTypes.ChangeFaction:
					objects.Remove(com.obj);
					int newFaction = com.obj.faction - 1 < 0 ? numberOfFactions - 1 : com.obj.faction - 1;
					com.obj.faction = newFaction;
					if(com.obj.obj == null) {
						foreach(PlaceableObject place in objects) {
							if(place.type == com.obj.type && place.pos == com.obj.pos) {
								com.obj.obj = place.obj;
								objects.Remove(place);
								break;
							}
						}
					}
					objects.Add(com.obj);
					UpdateColors();
					break;
				case CommandTypes.Clear:
					foreach(PlaceableObject obj in com.clearedList) {
						PlaceableObject newObj = new PlaceableObject();
						newObj = obj;
						newObj.obj = Instantiate(placeables[obj.placeablesIndex].obj, obj.pos, Quaternion.identity);
						newObj.obj.transform.localEulerAngles = new Vector3(0,0,90 * newObj.rotation);
						objects.Add(newObj);
					}
					UpdateColors();
					break;
			}
			redoStack.Push(com);
		}
	}

	public void Redo() {
		if(redoStack.Count > 0) {
			Command com = redoStack.Pop();
			switch(com.type) {
				case CommandTypes.Place:
					com.obj.obj = Instantiate(placeables[com.obj.placeablesIndex].obj, com.position, Quaternion.identity);
					com.obj.pos = com.position;
					objects.Add(com.obj);
					break;
				case CommandTypes.Remove:
					foreach(PlaceableObject place in objects) {
						if(place.type == com.obj.type && place.pos == com.obj.pos) {
							objects.Remove(place);
							Destroy(place.obj);
							break;
						}
					}
					break;
				case CommandTypes.Center:
					Vector3 pos = center;
					pos.z -= 10;
					Camera.main.transform.position = pos;
					break;
				case CommandTypes.ChangeFaction:
					foreach(PlaceableObject place in objects) {
						if(place.pos == com.obj.pos && place.type == com.obj.type) {
							Destroy(place.obj);
							objects.Remove(place);
							break;
						}
					}
					com.obj.faction = (com.obj.faction + 1) % numberOfFactions;
					if(!com.obj.obj) com.obj.obj = Instantiate(placeables[com.obj.placeablesIndex].obj, com.position, Quaternion.identity);
					objects.Add(com.obj);
					UpdateColors();
					break;
				case CommandTypes.Clear:
					foreach(PlaceableObject oj in objects) {
						Destroy(oj.obj);
					}
					objects.Clear();
					break;
			}
			undoStack.Push(com);
		}
	}

	void UpdateColors() {
		foreach(Transform tile in GameObject.Find("Tile Holder").transform) {
			tile.GetComponent<SpriteRenderer>().color = currentColor;
		}
		if(cursor.type == ObjectTypes.Platform) {
			cursor.obj.GetComponent<SpriteRenderer>().color = currentColor + new Color(0.5F,0.5F,0.5F);
		}
		foreach(PlaceableObject placeable in placeables) {
			if(placeable.type == ObjectTypes.Platform) {
				placeable.obj.GetComponent<SpriteRenderer>().color = currentColor + new Color(0.5F,0.5F,0.5F);
			}
		}
		foreach(PlaceableObject placeable in objects) {
			if(placeable.type == ObjectTypes.Platform) {
				placeable.obj.GetComponent<SpriteRenderer>().color = currentColor + new Color(0.5F,0.5F,0.5F);
			} else if(GetIsFactable(placeable.type)) {
				foreach(SpriteRenderer renderer in placeable.obj.GetComponentsInChildren<SpriteRenderer>()) {
					renderer.color = FactionColors.colors[placeable.faction];
				}
				placeable.obj.GetComponent<SpriteRenderer>().color = Color.white;
			}
		}
	}
	bool GetIsFactable(ObjectTypes type) {
		return !(type == ObjectTypes.Platform || type == ObjectTypes.PowerRock);
	}
	void DeleteObject(){
		Command com = new Command();
		com.type = CommandTypes.Remove;
		PlaceableObject tile = new PlaceableObject();
		foreach(PlaceableObject obj in objects) { 
			// goes through every placeableobject, 
			// saves the platform for later as if it finds a non-platform it deletes the non-platform and returns
				if(obj.obj.transform.position == cursor.obj.transform.position) {
					if(obj.type == ObjectTypes.Platform) {
						tile = obj;
					} else {
						com.obj = obj;
						com.position = obj.obj.transform.position;
						undoStack.Push(com);
						redoStack.Clear();
						objects.Remove(obj);
						Destroy(obj.obj);
						return;
					}
				}
			}
		if(tile.obj) {
			com.obj = tile;
			com.position = tile.obj.transform.position;
			undoStack.Push(com);
			redoStack.Clear();
			objects.Remove(tile);
			Destroy(tile.obj);
		}
	}

	void Update () {
		windowEnabled = mainMenu.gameObject.activeSelf || sectorProps.gameObject.activeSelf || hotkeyList.gameObject.activeSelf
		|| readFile.gameObject.activeSelf;

		Vector3 mousePos = Input.mousePosition;
		mousePos.z += 10;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		if(cursor.type == ObjectTypes.Platform) {
			mousePos.x = cursorOffset.x + tileSize * (int)((mousePos.x - cursorOffset.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
			mousePos.y = cursorOffset.y + tileSize * (int)((mousePos.y - cursorOffset.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
		} else {
			mousePos.x = 0.5F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.5F * tileSize));
			mousePos.y = 0.5F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.5F * tileSize));
		}
		if(Input.GetKeyDown("g") && (!windowEnabled || mainMenu.gameObject.activeSelf)) {
			mainMenu.ToggleActive();
		} else {
			if(Input.GetKeyDown("g") && sectorProps.gameObject.activeSelf) {
				sectorProps.ToggleActive();
			}
			if(Input.GetKeyDown("g") && hotkeyList.gameObject.activeSelf) {
				hotkeyList.ToggleActive();
			}
			if(Input.GetKeyDown("g") && readFile.gameObject.activeSelf) {
				readFile.ToggleActive();
			}
		}

		if(!windowEnabled) {
			if(Input.GetKeyDown("space")) {
				Command com = new Command();
				com.position = Camera.main.transform.position;
				com.type = CommandTypes.Center;
				undoStack.Push(com);
				redoStack.Clear();
				Vector3 pos = center;
				pos.z -= 10;
				Camera.main.transform.position = pos;
			}
			if(Input.GetKeyDown("k")) {
				GetPlatformIndex(mousePos);	
			}
			if(Input.GetKeyDown("z")) {
				Undo();
			}
			if(Input.GetKeyDown("t")) {
				Redo();
			}
			if(Input.GetKeyDown("q")) {
				Destroy(cursor.obj);
				if(--cursorCount < 0) {
					cursorCount = placeables.Length - 1;
				}
				cursor = placeables[cursorCount % placeables.Length];
				cursor.obj = Instantiate(placeables[cursorCount % placeables.Length].obj, cursor.obj.transform.position, Quaternion.identity) as GameObject;
			}
			if(Input.GetKeyDown("c")) {
				Command com = new Command();
				List<PlaceableObject> oldList = new List<PlaceableObject>();
				foreach(PlaceableObject placeable in objects) {
					oldList.Add(placeable);
					Destroy(placeable.obj);
				}
				com.type = CommandTypes.Clear;
				com.clearedList = oldList;
				undoStack.Push(com);
				redoStack.Clear();
				objects.Clear();
			}
			if(Input.GetKeyDown("e")) {
				Destroy(cursor.obj);
				cursor = placeables[++cursorCount % placeables.Length];
				cursor.obj = Instantiate(placeables[cursorCount % placeables.Length].obj, cursor.obj.transform.position, Quaternion.identity) as GameObject;
			}
			if(Input.GetKeyDown("r") || Input.GetMouseButtonDown(1)) {
				DeleteObject();
			}
			if(Input.GetMouseButtonDown(0) || Input.GetKeyDown("f")) {

				Command com = new Command();
				bool found = false;
				foreach(PlaceableObject obj in objects) {
					if(obj.pos == cursor.obj.transform.position && cursor.type == obj.type) {
						if(GetIsFactable(cursor.type)) {
							com.type = CommandTypes.ChangeFaction;
							com.position = cursor.obj.transform.position;
							objects.Remove(obj);
							var newObj = obj;
							newObj.faction = (obj.faction + 1) % numberOfFactions;
							objects.Add(newObj);
							UpdateColors();
							com.obj = newObj;
							undoStack.Push(com);
						} else if(obj.type == ObjectTypes.Platform) {
							objects.Remove(obj);
							PlaceableObject newObj = obj;
							newObj.rotation = (newObj.rotation + 1) % 4;
							Vector3 rot = newObj.obj.transform.localEulerAngles;
							rot.z = 90 * newObj.rotation;
							newObj.obj.transform.localEulerAngles = rot;
							objects.Add(newObj);
						}
						else {
							com.type = CommandTypes.Remove;
							com.position = cursor.obj.transform.position;
							com.obj = obj;
							undoStack.Push(com);
							objects.Remove(obj);
							Destroy(obj.obj);
						}
						redoStack.Clear();
						found = true;
						break;
					}
				}
				if(!found) {
					com.type = CommandTypes.Place;
					PlaceableObject newo = cursor;
					newo.pos = cursor.obj.transform.position;
					com.position = newo.pos;
					newo.obj = Instantiate(cursor.obj, newo.pos, Quaternion.identity);
					com.obj = newo;
					undoStack.Push(com);
					redoStack.Clear();
					objects.Add(newo);
				}
			}
		cursor.obj.transform.position = mousePos;
		}
	}

	public void UpdateSector() {
		int.TryParse(GameObject.Find("Beginning X").GetComponentsInChildren<Text>()[1].text, out x);
		int.TryParse(GameObject.Find("Beginning Y").GetComponentsInChildren<Text>()[1].text, out y);
		int.TryParse(GameObject.Find("Height").GetComponentsInChildren<Text>()[1].text, out height);
		int.TryParse(GameObject.Find("Width").GetComponentsInChildren<Text>()[1].text, out width);
		sctName = GameObject.Find("Sector Name").GetComponentsInChildren<Text>()[1].text;
		center = new Vector3 {
			x = this.x + (width / 2),
			y = this.y + (height / 2),
			z = 0
		};
		var rend = GameObject.Find("SectorBorders").GetComponent<LineRenderer>();
		rend.SetPositions(new Vector3[]{
            new Vector3(x, y, 0),
            new Vector3(x + width, y, 0),
            new Vector3(x + width, y + height, 0),
            new Vector3(x, y + height, 0)
        });
		int secVal = GameObject.Find("Sector Type").GetComponent<Dropdown>().value;
		type = (Sector.SectorType)secVal;
		currentColor = SectorColors.colors[secVal];
		UpdateColors();
	}

	public void GetPlatformIndex(Vector3 pos) {
		Vector3 firstTilePos = new Vector3 {
			x = center.x,
			y = center.y,
		};
		Vector3 lastTilePos = new Vector3 {
			x = center.x,
			y = center.y,
		};
		foreach(PlaceableObject ojs in objects) {
			if(ojs.type == ObjectTypes.Platform) {
				Vector3 tilePos = ojs.obj.transform.position;
				if(tilePos.x < firstTilePos.x) firstTilePos.x = tilePos.x;
				if(tilePos.y > firstTilePos.y) firstTilePos.y = tilePos.y;
				if(tilePos.x > lastTilePos.x) lastTilePos.x = tilePos.x;
				if(tilePos.y < lastTilePos.y) lastTilePos.y = tilePos.y;
			}
		}
		int columns = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.x - center.x), Mathf.Abs(lastTilePos.x - center.x)) / tileSize) * 2;
		int rows = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.y - center.y), Mathf.Abs(lastTilePos.y - center.y)) / tileSize) * 2;
		Vector2 offset = new Vector2 {
			x = center.x + -(columns - 1) * tileSize/2,
			y = center.y + (rows - 1) * tileSize/2

		};
		int[] coordinates = new int[2];

		coordinates[1] = Mathf.RoundToInt((pos.x - offset.x) / tileSize);
		coordinates[0] = -Mathf.RoundToInt((pos.y - offset.y) / tileSize);
		Debug.Log("row: " + coordinates[0] + " column: " + coordinates[1] +  " of a square with "  + rows + " rows and "  + columns + " columns");
	}
	public void ToJSON() {
		SectorDataWrapper sct = new SectorDataWrapper();
		if(sctName == null || sctName == "") {
			Debug.Log("Name your damn sector!");
			return;
		}
		sct.sectorName = sctName;
		sct.type = type;
		sct.backgroundColor = currentColor;
		LandPlatformDataWrapper platform = new LandPlatformDataWrapper();

		Vector3 firstTilePos = new Vector3 {
			x = center.x,
			y = center.y
		};
		Vector3 lastTilePos = new Vector3 {
			x = center.x,
			y = center.y
		};
		foreach(PlaceableObject ojs in objects) {
			if(ojs.type == ObjectTypes.Platform) {
				Vector3 tilePos = ojs.obj.transform.position;
				if(tilePos.x < firstTilePos.x) firstTilePos.x = tilePos.x;
				if(tilePos.y > firstTilePos.y) firstTilePos.y = tilePos.y;
				if(tilePos.x > lastTilePos.x) lastTilePos.x = tilePos.x;
				if(tilePos.y < lastTilePos.y) lastTilePos.y = tilePos.y;
			}
		}
		int columns = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.x - center.x), Mathf.Abs(lastTilePos.x - center.x)) / tileSize) * 2;
		int rows = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.y - center.y), Mathf.Abs(lastTilePos.y - center.y)) / tileSize) * 2;
		Vector2 offset = new Vector2 {
			x = center.x - (columns - 1) * tileSize/2,
			y = center.y + (rows - 1) * tileSize/2
		};
		
		platform.rows = rows;
		platform.columns = columns;
		platform.tilemap = new int[rows * columns];
		platform.rotations = new int[rows * columns];
		platform.prefabs = new string[] {
			"4 Entry",
			"3 Entry",
			"2 Entry",
			"1 Entry",
			"0 Entry",
			"Junction"
		};

		for(int i = 0; i < platform.tilemap.Length; i++) {
			platform.tilemap[i] = -1;
		}

		IntRect rect = new IntRect();
		List<string> targetIDS = new List<string>();
		List<Sector.LevelEntity> ents = new List<Sector.LevelEntity>();
		rect.x = x;
		rect.y = y;
		rect.w = width;
		rect.h = height;
		sct.bounds = rect;
		int ID = 0;
		Vector3[] coreSpawnPointsByFaction = new Vector3[numberOfFactions];
		foreach(PlaceableObject oj in objects) {
			if(oj.type != ObjectTypes.Platform && oj.type != ObjectTypes.ShellCore) {
				Sector.LevelEntity ent = new Sector.LevelEntity();
				ent.ID = ID++ + "";
				ent.faction = oj.faction;
				ent.position = oj.obj.transform.position;
				switch(oj.type) {
					case ObjectTypes.Bunker:
						ent.assetID = "bunker_blueprint";
						ent.vendingID = "bunker_vending_blueprint";
						break;
					case ObjectTypes.Outpost:
						ent.assetID = "outpost_blueprint";
						ent.vendingID = "outpost_vending_blueprint";
						break;
					case ObjectTypes.Carrier:
						ent.assetID = "carrier_blueprint";
						targetIDS.Add(ent.ID);
						Vector3 pos = oj.obj.transform.position;
						pos.y -= 3;
						coreSpawnPointsByFaction[ent.faction] = pos;
						break;
					case ObjectTypes.PowerRock:
						ent.assetID = "energy_rock";
						break;
				}
				ent.name = ent.assetID + ent.ID;
				ents.Add(ent);
			} else if (oj.type == ObjectTypes.Platform) {
				int[] coordinates = new int[2];
				coordinates[1] = Mathf.RoundToInt((oj.obj.transform.position.x - offset.x) / tileSize);
				coordinates[0] = -Mathf.RoundToInt((oj.obj.transform.position.y - offset.y) / tileSize);
				
				platform.tilemap[coordinates[1] + platform.columns * coordinates[0]] = oj.placeablesIndex;
				platform.rotations[coordinates[1] + platform.columns * coordinates[0]] = oj.rotation;
			}
		}

		// create shellcores and assign spawn points

		foreach(PlaceableObject oj in objects) {
			if(oj.type == ObjectTypes.ShellCore) {
				Sector.LevelEntity ent = new Sector.LevelEntity();
				ent.ID = ID++ + "";
				ent.faction = oj.faction;
				ent.position = oj.pos;// = coreSpawnPointsByFaction[ent.faction];
				switch(oj.faction) 
				{
					case 0:
						ent.assetID = "shellcore_blueprint";
						break;
					case 1:
						ent.assetID = "demo_enemy_shellcore";
						break;
				}
				ent.name = ent.assetID + ent.ID;
				targetIDS.Add(ent.ID);
				ents.Add(ent);
			}
		}

		sct.entities = ents.ToArray();
		sct.targets = targetIDS.ToArray();
		sct.backgroundColor = currentColor;

		SectorData data = new SectorData();
		data.sectorjson = JsonUtility.ToJson(sct);
		data.platformjson = JsonUtility.ToJson(platform);

		string output = JsonUtility.ToJson(data);

		if(!System.IO.Directory.Exists(Application.dataPath + "\\..\\Sectors\\")) {
			System.IO.Directory.CreateDirectory(Application.dataPath + "\\..\\Sectors\\");
		}
		string path = Application.dataPath + "\\..\\Sectors\\" + sct.sectorName;
		System.IO.File.WriteAllText(path, output);
		System.IO.Path.ChangeExtension(path, ".json");
		Debug.Log("JSON written to location: " + Application.dataPath + "\\..\\Sectors\\" + sct.sectorName);
	}

	public void FromJSON() {
		string path = GameObject.Find("JSONPath").GetComponentsInChildren<Text>()[1].text;
		if(System.IO.File.Exists(path)) {
			SectorData data = JsonUtility.FromJson<SectorData>(System.IO.File.ReadAllText(path));
			SectorDataWrapper sectorDataWrapper = JsonUtility.FromJson<SectorDataWrapper>(data.sectorjson);
			LandPlatformDataWrapper platformDataWrapper = JsonUtility.FromJson<LandPlatformDataWrapper>(data.platformjson);

			type = sectorDataWrapper.type;
			currentColor = sectorDataWrapper.backgroundColor;
			sctName = sectorDataWrapper.sectorName;
			UpdateColors();

			int cols = platformDataWrapper.columns;
			int rows = platformDataWrapper.rows;

			x = sectorDataWrapper.bounds.x;
			y = sectorDataWrapper.bounds.y;
			width = sectorDataWrapper.bounds.w;
			height = sectorDataWrapper.bounds.h;

			sectorProps.ToggleActive();
			GameObject.Find("Beginning X").GetComponentInChildren<InputField>().text = x + "";
			GameObject.Find("Beginning Y").GetComponentInChildren<InputField>().text = "" + y;
			GameObject.Find("Height").GetComponentInChildren<InputField>().text = "" + height;
			GameObject.Find("Width").GetComponentInChildren<InputField>().text = "" + width;
			GameObject.Find("Sector Name").GetComponentInChildren<InputField>().text = sctName;
			GameObject.Find("Sector Type").GetComponent<Dropdown>().value = (int)sectorDataWrapper.type;
			sectorProps.ToggleActive();

			var rend = GameObject.Find("SectorBorders").GetComponent<LineRenderer>();
			rend.SetPositions(new Vector3[]{
            new Vector3(x, y, 0),
            new Vector3(x + width, y, 0),
            new Vector3(x + width, y + height, 0),
            new Vector3(x, y + height, 0)
        	});
			
			center = new Vector3 {
				x = this.x + width / 2F,
				y = this.y + height / 2F,
			};
			Debug.Log(center);

			Vector2 offset = new Vector2 
			{
				x = center.x -tileSize * (cols-1)/2,
				y = center.y +tileSize * (rows-1)/2
			};
			for(int i = 0; i < platformDataWrapper.tilemap.Length; i++) {
				switch(platformDataWrapper.tilemap[i]) {
					case -1:
						break;
					default:
						PlaceableObject obj = new PlaceableObject();
						obj.type = ObjectTypes.Platform;
						obj.placeablesIndex = platformDataWrapper.tilemap[i];
						obj.rotation = platformDataWrapper.rotations[i];
						Vector3 pos = new Vector3 {
							x = offset.x + (i % cols) * tileSize,
							y = offset.y - (i / cols) * tileSize,
							z = 0
						};
						obj.pos = pos;
						obj.obj = Instantiate(placeables[platformDataWrapper.tilemap[i]].obj, pos, Quaternion.identity);
						obj.obj.transform.localEulerAngles = new Vector3(0,0,90*platformDataWrapper.rotations[i]);
						objects.Add(obj);
						break;
				}
			}
			foreach(Sector.LevelEntity ent in sectorDataWrapper.entities) {
				PlaceableObject obj = new PlaceableObject();
				obj.pos = ent.position;
				obj.faction = ent.faction;

				// TODO: change this system once the shellcore editor is ready
				
				switch(ent.assetID) {
					case "outpost_blueprint":
						obj.type = ObjectTypes.Outpost;
						obj.placeablesIndex = 6;
						obj.obj = Instantiate(placeables[6].obj, obj.pos, Quaternion.identity);
						break;
					case "bunker_blueprint":
						obj.type = ObjectTypes.Bunker;
						obj.placeablesIndex = 7;
						obj.obj = Instantiate(placeables[7].obj, obj.pos, Quaternion.identity);
						break;
					case "carrier_blueprint":
						obj.type = ObjectTypes.Carrier;
						obj.placeablesIndex = 8;
						obj.obj = Instantiate(placeables[8].obj, obj.pos, Quaternion.identity);
						break;
					case "energy_rock":
						obj.type = ObjectTypes.PowerRock;
						obj.placeablesIndex = 9;
						obj.obj = Instantiate(placeables[9].obj, obj.pos, Quaternion.identity);
						break;
					case "shellcore_blueprint":
					case "demo_enemy_shellcore":
						obj.type = ObjectTypes.ShellCore;
						obj.placeablesIndex = 10;
						obj.obj = Instantiate(placeables[10].obj, obj.pos, Quaternion.identity);
						break;
					default:
						break;
				}
				if(GetIsFactable(obj.type)) {
					foreach(SpriteRenderer renderer in obj.obj.GetComponentsInChildren<SpriteRenderer>()) {
						renderer.color = FactionColors.colors[obj.faction];
					}
				obj.obj.GetComponent<SpriteRenderer>().color = Color.white;
				}
				objects.Add(obj);
			}
		}
	}
}
