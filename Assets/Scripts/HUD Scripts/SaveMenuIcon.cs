using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveMenuIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SaveMenuHandler handler;
    public PlayerSave save;
    public string path;
    public Text saveName;
    public Text episodeNumber;
    public Text version;
    public Text timePlayed;
    public int index;
    public Image shellImage;
    public Image coreImage;

    float origPos;

    void Start()
    {
        origPos = saveName.rectTransform.anchoredPosition.x;
        EntityBlueprint print = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(save.currentPlayerBlueprint, print);
        shellImage.sprite = ResourceManager.GetAsset<Sprite>(print.coreShellSpriteID);
        shellImage.rectTransform.sizeDelta = shellImage.sprite.bounds.size * 50;
        coreImage.material = ResourceManager.GetAsset<Material>("material_color_swap");
        shellImage.color = coreImage.color = FactionManager.GetFactionColor(0);
        shellImage.rectTransform.anchoredPosition = new Vector2(shellImage.rectTransform.anchoredPosition.x,
            (shellImage.rectTransform.sizeDelta.y - (shellImage.sprite.pivot).y) * 0.5f);

        saveName.text = save.name;
        episodeNumber.text = $"Episode: {Mathf.Max(1, save.episode)}";
        version.text = "Version: " + save.version;
        if (save.version.Contains("Prototype") || save.version.Contains("Alpha 0.0.0"))
        {
            version.color = Color.red;
            version.text += " - Unsupported Version!";
        }

        if (SaveMenuHandler.migrationVersions.Exists((v) => save.version == v))
        {
            version.color = 0.5F * Color.green + Color.red;
            version.text += " - Click save to attempt migration";
        }

        timePlayed.text = "Time Played: " + (((int)save.timePlayed / 60 >= 10) ? (int)save.timePlayed / 60 + "" : "0" + (int)save.timePlayed / 60) + ":" + (((int)save.timePlayed % 60 >= 10) ? (int)save.timePlayed % 60 + "" : "0" + (int)save.timePlayed % 60);
    }

    public void LoadSave()
    {
        if (save.resourcePath != "" && !save.resourcePath.Contains("main"))
        {
            SectorManager.customPath = save.resourcePath;
        }

        if (!SaveMenuHandler.migrationVersions.Exists((v) => save.version == v))
        {
            LoadSaveByPath(path, true);
        }
        else
        {
            handler.PromptMigrate(index);
        }
    }

    public void BackupSave()
    {
        handler.BackupSave(save);
    }

    public void DeleteSave()
    {
        handler.PromptDelete(index);
    }

    public static void LoadSaveByPath(string path, bool nullifyTestJsonPath)
    {
        string current = Application.persistentDataPath + "\\CurrentSavePath";
        if (!File.Exists(current))
        {
            File.WriteAllText(current, path);
        }
        else
        {
            File.Delete(current);
            File.WriteAllText(current, path);
        }

        MainMenu.StartGame(nullifyTestJsonPath);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine(MoveOut());
        StartCoroutine(MoveIn());
    }

    IEnumerator MoveOut()
    {
        int counter = 0;
        while (counter < 50 && saveName.rectTransform.anchoredPosition.x < origPos + 50)
        {
            saveName.rectTransform.anchoredPosition += new Vector2(5, 0);
            counter += 5;
            yield return new WaitForFixedUpdate();
        }

        if (saveName.rectTransform.anchoredPosition.x > origPos + 50)
        {
            saveName.rectTransform.anchoredPosition = new Vector2(origPos + 50, saveName.rectTransform.anchoredPosition.y);
        }
    }

    IEnumerator MoveIn()
    {
        int counter = 0;
        while (counter < 50 && saveName.rectTransform.anchoredPosition.x > origPos)
        {
            saveName.rectTransform.anchoredPosition -= new Vector2(5, 0);
            counter += 5;
            yield return new WaitForFixedUpdate();
        }

        saveName.rectTransform.anchoredPosition = new Vector2(origPos, saveName.rectTransform.anchoredPosition.y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCoroutine(MoveIn());
        StartCoroutine(MoveOut());
    }
}
