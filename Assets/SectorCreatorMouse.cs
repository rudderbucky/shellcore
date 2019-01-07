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
	[System.Serializable]
	public struct PlaceableObject {
		public GameObject obj;
		public ObjectTypes type;
		public int faction;
	}

	public struct SectorData {
		public string sectorjson;
		public string platformjson;
	}

	public bool windowEnabled = true;
	public int numberOfFactions;
	public PlaceableObject[] placeables;
	private float tileSize = 4.19F;
	[HideInInspector]
	public List<PlaceableObject> objects;
	GUIWindowScripts mainMenu;
	GUIWindowScripts sectorProps;
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
		windowEnabled = true;
		objects = new List<PlaceableObject>();
		cursor = placeables[0];
		cursor.obj = Instantiate(placeables[0].obj) as GameObject;
		mainMenu = transform.Find("MenuBox").GetComponent<GUIWindowScripts>();
		sectorProps = transform.Find("SectorProps").GetComponent<GUIWindowScripts>();
		UpdateColors();
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
			}
		}
	}
	bool GetIsFactable(ObjectTypes type) {
		return !(type == ObjectTypes.Platform || type == ObjectTypes.PowerRock);
	}
	void DeleteObject(){
		PlaceableObject tile = new PlaceableObject();
		foreach(PlaceableObject obj in objects) {
				if(obj.obj.transform.position == cursor.obj.transform.position) {
					if(obj.type == ObjectTypes.Platform) {
						tile = obj;
					} else {
						objects.Remove(obj);
						Destroy(obj.obj);
						return;
					}
				}
			}
		objects.Remove(tile);
		Destroy(tile.obj);
	}

	void Update () {
		windowEnabled = mainMenu.gameObject.activeSelf || sectorProps.gameObject.activeSelf;

		Vector3 mousePos = Input.mousePosition;
		mousePos.z += 10;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		mousePos.x = tileSize * (int)(mousePos.x / tileSize + (mousePos.x > 0 ? 0.5F : -0.5F));
		mousePos.y = tileSize * (int)(mousePos.y / tileSize + (mousePos.y > 0 ? 0.5F : -0.5F));

		if(Input.GetKeyDown("g") && (!windowEnabled || mainMenu.gameObject.activeSelf)) {
			mainMenu.ToggleActive();
		} else if(Input.GetKeyDown("g") && sectorProps.gameObject.activeSelf) {
				sectorProps.ToggleActive();
		}

		if(!windowEnabled) {
			if(Input.GetKeyDown("k")) {
				GetPlatformIndex(mousePos);	
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
				foreach(PlaceableObject placeable in objects) {
					Destroy(placeable.obj);
				}
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
				
				bool shouldInstantiate = true;
				PlaceableObject placed;
				int newFaction = 0;
				foreach(PlaceableObject obj in objects) {
					bool isFactable = GetIsFactable(obj.type);
					if(obj.obj.transform.position == cursor.obj.transform.position
						&& cursor.type == obj.type && isFactable) {
						newFaction = (obj.faction + 1) % numberOfFactions;
						objects.Remove(obj);
						Destroy(obj.obj);
						shouldInstantiate = true;
						break;
					} else if(obj.obj.transform.position == cursor.obj.transform.position
						&& ((cursor.type != obj.type && isFactable && GetIsFactable(cursor.type))
						|| (cursor.type == obj.type && !isFactable))) {
							objects.Remove(obj);
							Destroy(obj.obj);
							shouldInstantiate = false;
							break;
					}
				}
				if(shouldInstantiate) {
					placed = cursor;
					placed.faction = newFaction;
					placed.obj = Instantiate(cursor.obj, cursor.obj.transform.position, Quaternion.identity);
					if(GetIsFactable(placed.type)) {
						foreach(SpriteRenderer renderer in placed.obj.GetComponentsInChildren<SpriteRenderer>()) {
							renderer.color = FactionColors.colors[placed.faction];
						}
						placed.obj.GetComponent<SpriteRenderer>().color = Color.white;
					}
					objects.Add(placed);
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
		Debug.Log(x + " " + y + " " + width + " " + height + " ");
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
		int columns = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.x - center.x), Mathf.Abs(lastTilePos.x - center.x)) / tileSize) * 2 + 1;
		int rows = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.y - center.y), Mathf.Abs(lastTilePos.y - center.y)) / tileSize) * 2 + 1;
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
			x = 0,
			y = 0
		};
		Vector3 lastTilePos = new Vector3 {
			x = 0,
			y = 0
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
		int columns = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.x), Mathf.Abs(lastTilePos.x)) / tileSize) * 2 + 1;
		int rows = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(firstTilePos.y), Mathf.Abs(lastTilePos.y)) / tileSize) * 2 + 1;
		Vector2 offset = new Vector2 {
			x = -(columns - 1) * tileSize/2,
			y = (rows - 1) * tileSize/2
		};
		
		platform.rows = rows;
		platform.columns = columns;
		platform.tilemap = new int[rows * columns];
		for(int i = 0; i < platform.tilemap.Length; i++) {
			platform.tilemap[i] = -1;
		}

		IntRect rect = new IntRect();
		List<string> targetIDS = new List<string>();
		List<Sector.LevelEntity> ents = new List<Sector.LevelEntity>();
		List<Sector.LevelEntity> cores = new List<Sector.LevelEntity>();
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
				
				platform.tilemap[coordinates[1] + platform.columns * coordinates[0]] = 0;
			}
		}

		// create shellcores and assign spawn points

		foreach(PlaceableObject oj in objects) {
			if(oj.type == ObjectTypes.ShellCore) {
				Sector.LevelEntity ent = new Sector.LevelEntity();
				ent.ID = ID++ + "";
				ent.faction = oj.faction;
				ent.position = coreSpawnPointsByFaction[ent.faction];
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
		System.IO.File.WriteAllText(Application.dataPath + "\\..\\Sectors\\" + sct.sectorName, output);
		Debug.Log("JSON written to location: " + Application.dataPath + "\\..\\Sectors\\" + sct.sectorName);
	}
}
