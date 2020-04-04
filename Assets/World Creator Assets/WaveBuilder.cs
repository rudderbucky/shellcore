using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveBuilder : GUIWindowScripts
{
    public GameObject wavePrefab;
    private List<WCSiegeWaveHandler> waveHandlers;
    public Transform contents;

    void OnEnable()
    {
        ClearWaves();
    }

    void ClearWaves()
    {
        for(int i = 0; i < contents.childCount; i++)
        {
            Destroy(contents.GetChild(i).gameObject);
        }

        waveHandlers = new List<WCSiegeWaveHandler>();  
    }

    public WCSiegeWaveHandler AddWaveAndReturnHandler()
    {
        var waveHandler = Instantiate(wavePrefab, contents).GetComponent<WCSiegeWaveHandler>();
        waveHandlers.Add(waveHandler);
        return waveHandler;
    }

    public void AddWave()
    {
        AddWaveAndReturnHandler();
    }

    public void ParseWaves(string path)
    {
        WaveSet set = new WaveSet();
        set.waves = new SiegeWave[waveHandlers.Count];

        for(int i = 0; i < set.waves.Length; i++)
        {
            set.waves[i] = waveHandlers[i].Parse();
        }

        CopyToClipboard(JsonUtility.ToJson(set));
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(set));
    }

    public static void CopyToClipboard(string s)
    {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }

    public void ReadWaves(WaveSet waves)
    {
        ClearWaves();

        foreach(var wave in waves.waves)
        {
            var waveHandler = AddWaveAndReturnHandler();
            waveHandler.Initialize(wave.entities);
        }
    }
}
