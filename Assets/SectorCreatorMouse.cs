using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class SectorCreatorMouse : MonoBehaviour {
	
	public Sector.SectorType type;
	public enum ObjectTypes {
		Outpost,
		Bunker,
		Carrier,
		PowerRock,
		Platform
	}
	[System.Serializable]
	public struct PlaceableObject {
		public GameObject obj;
		public ObjectTypes type;
		public int faction;
	}
	public bool windowEnabled = true;
	public int numberOfFactions;
	public PlaceableObject[] placeables;
	private float tileSize = 4.19F;
	[HideInInspector]
	public List<PlaceableObject> objects;
	int cursorCount = 0;
	string sctName;
	int x;
	int y;
	int height;
	int width;
	PlaceableObject cursor;
	// Update is called once per frame
	void Start() {
		windowEnabled = true;
		objects = new List<PlaceableObject>();
		cursor = placeables[0];
		cursor.obj = Instantiate(placeables[0].obj) as GameObject;
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
		Vector3 mousePos = Input.mousePosition;
		mousePos.z += 10;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		mousePos.x = tileSize * (int)(mousePos.x / tileSize + (mousePos.x > 0 ? 0.5F : -0.5F));
		mousePos.y = tileSize * (int)(mousePos.y / tileSize + (mousePos.y > 0 ? 0.5F : -0.5F));

		if(Input.GetKeyDown("g")) {
			windowEnabled = !windowEnabled;
			transform.Find("DialogueBox").gameObject.SetActive(windowEnabled);
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
		Debug.Log(x + " " + y + " " + width + " " + height + " ");
		var rend = GameObject.Find("SectorBorders").GetComponent<LineRenderer>();
		rend.SetPositions(new Vector3[]{
            new Vector3(x, y, 0),
            new Vector3(x + width, y, 0),
            new Vector3(x + width, y + height, 0),
            new Vector3(x, y + height, 0)
        });
	}

	public void GetPlatformIndex(Vector3 pos) {
		int columns = Mathf.FloorToInt(width / tileSize) - ((Mathf.Abs(x) % tileSize) < (tileSize) / 2 ? 1 : 0);
		var rows = Mathf.FloorToInt(height / tileSize) - ((Mathf.Abs(y) % tileSize) < (tileSize) / 2 ? 1 : 0);
		Vector2 offset = new Vector2 {
			x = -(rows) * tileSize / 2,
			y = -(columns) * tileSize / 2
		};
		int[] coordinates = new int[2];
		coordinates[1] = -Mathf.RoundToInt((pos.y + offset.y) / 4.19F);
		coordinates[0] =  columns - 1 + Mathf.RoundToInt((pos.x + offset.x) / 4.19F);
		Debug.Log("row: " + coordinates[1] + " column: " + coordinates[0] + " of a square with " + rows + " rows and " + columns + " columns");
	}
	public void ToJSON() {
		Sector sct = ScriptableObject.CreateInstance<Sector>();
		if(sctName == null || sctName == "") {
			Debug.Log("Name your damn sector!");
			return;
		}
		sct.name = sctName;
		sct.type = type;
		switch(sct.type) {
			case Sector.SectorType.BattleZone:
				sct.backgroundColor = new Color(0.5F,0,0);
				break;
		}
		LandPlatform platform = ScriptableObject.CreateInstance<LandPlatform>();
		int columns = Mathf.FloorToInt(width / tileSize) - ((Mathf.Abs(x) % tileSize) < (tileSize) / 2 ? 1 : 0);
		int rows = Mathf.FloorToInt(height / tileSize) - ((Mathf.Abs(y) % tileSize) < (tileSize) / 2 ? 1 : 0);
		platform.rows = rows;
		platform.columns = columns;
		platform.tilemap = new int[rows * columns];
		for(int i = 0; i < platform.tilemap.Length; i++) {
			platform.tilemap[i] = -1;
		}
		Vector2 offset = new Vector2 {
			x = -(platform.rows) * tileSize / 2,
			y = -(platform.columns) * tileSize / 2
		};
		IntRect rect = new IntRect();
		List<string> targetIDS = new List<string>();
		List<Sector.LevelEntity> ents = new List<Sector.LevelEntity>();
		rect.x = x;
		rect.y = y;
		rect.w = width;
		rect.h = height;
		sct.bounds = rect;
		int ID = 0;
		foreach(PlaceableObject oj in objects) {
			if(oj.type != ObjectTypes.Platform) {
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
						break;
					case ObjectTypes.PowerRock:
						ent.assetID = "energy_rock";
						break;
				}
				ent.name = ent.assetID + ent.ID;
				ents.Add(ent);
			} else {
				int[] coordinates = new int[2];
				coordinates[1] = -Mathf.RoundToInt((oj.obj.transform.position.y + offset.y) / 4.19F);
				coordinates[0] =  columns - 1 + Mathf.RoundToInt((oj.obj.transform.position.x + offset.x) / 4.19F);

				Debug.Log(coordinates[0] + " " + coordinates[1]);
				platform.tilemap[coordinates[1] + platform.columns * coordinates[0]] = 0;
			}
		}
		sct.entities = ents.ToArray();
		sct.targets = targetIDS.ToArray();
		sct.name = sctName;
		sct.backgroundColor = new Color(0.5F,0,0);
		string output = JsonUtility.ToJson(sct) + "\n" + JsonUtility.ToJson(platform);
		if(!System.IO.Directory.Exists(Application.dataPath + "\\..\\Sectors\\")) {
			System.IO.Directory.CreateDirectory(Application.dataPath + "\\..\\Sectors\\");
		}
		System.IO.File.WriteAllText(Application.dataPath + "\\..\\Sectors\\" + sct.name, output);
		Debug.Log("JSON written to location: " + Application.dataPath + "\\..\\Sectors\\" + sct.name);
	}
}
