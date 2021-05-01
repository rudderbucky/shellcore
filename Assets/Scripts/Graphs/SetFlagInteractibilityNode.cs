using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

public enum FlagInteractibility
{
    None,
    Warp
}

namespace NodeEditorFramework.Standard
{
    // sets interactibility for flags, if they are interactible also allows you to do specialized tasks with them (e.g., warping)
    [Node(false, "Actions/Set Flag Interactibility")]
    public class SetFlagInteractibilityNode : Node
    {
        public override string GetName { get { return "SetFlagInteractibilityNode"; } }
        public override string Title { get { return "Set Flag Interactibility"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string flagName;
        public FlagInteractibility interactibility;
        public string sectorName;
        public string entityID;
        PopupMenu typePopup;

        public readonly string[] intStrings = new string[]
        {
            "None",
            "Warp"
        };


        public override void NodeGUI()
        {
            // Params: Flag name, Flag interactibility enum
            // If warp: grab entity ID and sector through entity selection
            flagName = GUILayout.TextField(flagName);
            

            GUILayout.Label("Interactibility type:");
            if (GUILayout.Button(intStrings[(int)interactibility]))
            {
                typePopup = new PopupMenu();
                typePopup.SetupGUI();
                for (int i = 0; i < intStrings.Length; i++)
                {
                    typePopup.AddItem(new GUIContent(intStrings[i]), false, SelectType, i);
                }
                typePopup.Show(GUIScaleUtility.GUIToScreenSpace(GUILayoutUtility.GetLastRect().max));
            }
            if(interactibility == FlagInteractibility.Warp)
            {
                GUILayout.Label("Sector name: ");
                sectorName = GUILayout.TextField(sectorName);
                GUILayout.Label("Entity ID: ");
                entityID = GUILayout.TextField(entityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select Warp Entity", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SelectEntity;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }   
            

        }

        // type select for node interactibility dropdown
        void SelectType(object data)
        {
            int index = (int)data;
            interactibility = (FlagInteractibility)index;
        }

        public override int Traverse()
        {
            switch(interactibility)
            {
                case FlagInteractibility.Warp:
                    foreach(var flag in AIData.flags)
                    {
                        if(flag.name == flagName)
                        {
                            Debug.Log("Set flag interactibility");
                            flag.sectorName = sectorName;
                            flag.entityID = entityID;
                            flag.interactibility = interactibility;
                            break;
                        }
                    }
                    
                    break;
            }
            return 0;
        }

        public void SelectEntity(string entityID)
        {
            this.entityID = entityID;
            // search for entity sector for autofilling
            foreach(var ent in WorldCreatorCursor.instance.placedItems)
            {
                if(ent.ID == entityID)
                {
                    var sectorWrapper = WorldCreatorCursor.instance.GetWrapperByPos(ent);
                    this.sectorName = sectorWrapper.sector.sectorName;
                }
            }
            WorldCreatorCursor.selectEntity -= SelectEntity;
        }
    }
}
