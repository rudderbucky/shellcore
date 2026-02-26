using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Manual", menuName = "WorldCreator/Manual", order = 2)]
public class ManualDatabase : ScriptableObject
{
    public List<ManualEntry> manualEntries;

    [System.Serializable]
    public class ManualEntry
    {
        public string title;
        public string imageID;
        [TextArea(5, 20)] public string contents;
    }
}