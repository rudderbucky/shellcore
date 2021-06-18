﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extra data stored for world logic
public class WorldData : ScriptableObject
{
    [System.Serializable]
    public class CharacterData {
        public string ID;
        public string name;
        public string blueprintJSON;
        public int faction;

        public PartyData partyData;
    }

    [System.Serializable]
    public class PartyData {
        public string attackDialogue;
        public string defendDialogue;
        public string collectDialogue;
        public string buildDialogue;
        public string followDialogue;
    }

    [System.Serializable]
    public class PartIndexData {
        public EntityBlueprint.PartInfo part;
        public List<string> origins;
    }

    // Characters are special entities linked to the storyline of which there can only be one
    // spawned at a time in the entire world. Party members are an example though
    // not all characters are party members. Characters may be spawned independently of sector entities.
    public CharacterData[] defaultCharacters;

    // initial spawn point for the player if the world is loaded for the first time.
    public Vector2 initialSpawn;
    public string defaultBlueprintJSON;
    public int[] defaultSlots = new int[4];
    public bool hasAccel;
    public string author;
    public string description;
    public PartIndexData[] partIndexDataArray;
}
