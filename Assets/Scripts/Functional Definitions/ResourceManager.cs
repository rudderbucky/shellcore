using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{


    [System.Serializable]
    public struct Resource
    {
        public string ID;
        public Object obj;
    }

    public List<Resource> builtInResources;

    Dictionary<string, Object> resources;
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        resources = new Dictionary<string, Object>();

        //Add built in resources to dictionaries

        for(int i = 0; i < builtInResources.Count; i++)
        {
            if(builtInResources[i].obj is PartBlueprint)
            {
                continue;
            }
            else
            {
                resources.Add(builtInResources[i].ID, builtInResources[i].obj);
            }
        }

        for (int i = 0; i < builtInResources.Count; i++)
            if (builtInResources[i].obj is PartBlueprint)
                resources.Add(builtInResources[i].ID, ShellPart.BuildPart(builtInResources[i].obj as PartBlueprint));

        if (File.Exists("ResourceData.txt"))
        {
            string[] lines = File.ReadAllLines("ResourceData.txt");
            int mode = -1;

            //get names
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.ToLower().StartsWith("sprites:"))
                    mode = 0;
                else if (line.ToLower().StartsWith("parts:"))
                    mode = 1;
                else if (line.ToLower().StartsWith("blueprints:"))
                    mode = 2;
                else
                {
                    string[] separated = line.Split(':');
                    switch (mode)
                    {
                        case 0:
                            //load sprite
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(File.ReadAllBytes(separated[1]));
                            resources.Add(separated[0], Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                            break;
                        case 1:
                            //load part
                            string partData = File.ReadAllText(separated[1]);
                            resources.Add(separated[0], ShellPart.BuildPart(JsonUtility.FromJson<PartBlueprint>(partData)));
                            break;
                        case 2:
                            //load entity
                            string entityData = File.ReadAllText(separated[1]);
                            resources.Add(separated[0], JsonUtility.FromJson<EntityBlueprint>(entityData));
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public static T GetAsset<T>(string ID) where T : Object
    {
        return Instance.getAsset<T>(ID);
    }

    public T getAsset<T>(string ID) where T : Object
    {
        if (ID == "")
            return null;

        if (resources.ContainsKey(ID))
            if (resources[ID] is T)
                return resources[ID] as T;
            else
                Debug.LogWarning("Trying to get " + ID + " (" + resources[ID].GetType() + ") as " + typeof(T).FullName);
        else
            Debug.LogWarning("Resource ID " + ID + " not found");
        return null;
    }
}
