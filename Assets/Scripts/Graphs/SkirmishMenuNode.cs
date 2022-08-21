using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Skirmish Menu")]
    public class SkirmishMenuNode : Node
    {
        public override string GetName
        {
            get { return "SkirmishMenuNode"; }
        }

        public override string Title
        {
            get { return "Skirmish Menu"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200, 50); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Input", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;

        public List<SkirmishOption> skirmishOptions = new List<SkirmishOption>();

        public override void NodeGUI()
        {
            GUILayout.Label("List of options:");
            for (int i = 0; i < skirmishOptions.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    skirmishOptions.RemoveAt(i);
                    i--;
                    if (i == -1)
                    {
                        break;
                    }

                    continue;
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sector name:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                skirmishOptions[i].sectorName = GUILayout.TextField(skirmishOptions[i].sectorName);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Entity ID (for warp point):");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                skirmishOptions[i].entityID = GUILayout.TextField(skirmishOptions[i].entityID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Description:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                skirmishOptions[i].mapDescription = GUILayout.TextArea(skirmishOptions[i].mapDescription);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Credit limit:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                skirmishOptions[i].creditLimit = Mathf.Max(0, Utilities.RTEditorGUI.IntField(skirmishOptions[i].creditLimit));
                if (skirmishOptions[i].creditLimit < 0)
                {
                    skirmishOptions[i].creditLimit = Mathf.Max(0, Utilities.RTEditorGUI.IntField(skirmishOptions[i].creditLimit));
                    Debug.LogWarning("Can't register negative numbers!");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                skirmishOptions[i].clearParty = RTEditorGUI.Toggle(skirmishOptions[i].clearParty, "Clear party");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                skirmishOptions.Add(new SkirmishOption());
            }

            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            SectorManager.instance.skirmishMenu.options = skirmishOptions;
            SectorManager.instance.skirmishMenu.Activate();
            return -1;
        }
    }
}
