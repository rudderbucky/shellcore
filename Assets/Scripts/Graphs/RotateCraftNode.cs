using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Rotate Craft")]
    public class RotateCraftNode : Node
    {
        public override string GetName { get { return "RotateCraftNode"; } }
        public override string Title { get { return "Rotate Craft"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        //public bool action; //TODO: action input
        public bool useIDInput;
        public bool useIDInputTarget;
        public bool asynchronous;
        public string entityID = "";
        public string targetEntityID = "";

        public ConnectionKnob IDInput;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("Name Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (useIDInput)
            {
                if (IDInput == null)
                {
                    if (inputKnobs.Count == 1)
                        IDInput = CreateConnectionKnob(IDInStyle);
                    else
                        IDInput = inputKnobs[1];
                }
                IDInput.DisplayLayout();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Subject to rotation");
            GUILayout.EndHorizontal();
            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use Name Input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInput)
            {
                GUILayout.Label("Entity ID");
                entityID = GUILayout.TextField(entityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetEntityID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target for rotation");
            GUILayout.EndHorizontal();
            useIDInputTarget = RTEditorGUI.Toggle(useIDInputTarget, "Use Name Input (Unfinished, don't use)", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInputTarget)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInputTarget)
            {
                GUILayout.Label("Entity ID");
                targetEntityID = GUILayout.TextField(targetEntityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetTargetID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            asynchronous = RTEditorGUI.Toggle(asynchronous, "Asynchronous Mode", GUILayout.MinWidth(400));

            RTEditorGUI.Seperator();
        }

        void SetEntityID(string ID)
        {
            Debug.Log("selected ID " + ID + "!");

            entityID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        void SetTargetID(string ID)
        {
            Debug.Log("selected ID " + ID + "!");

            targetEntityID = ID;
            WorldCreatorCursor.selectEntity -= SetTargetID;
        }

        AirCraft entity = null;
        Entity target = null;
        public override int Traverse()
        {
            if (useIDInput)
            {
                if (useIDInput && IDInput == null)
                    IDInput = inputKnobs[1];

                if (IDInput.connected())
                {
                    entityID = (IDInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Name Input not connected!");
                }
            }

            Debug.Log("Entity ID: " + entityID);
            Debug.Log("Target ID: " + targetEntityID);

            if(!(target && entity)) // room for improvement but probably unecessary
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i].ID == entityID && AIData.entities[i] is AirCraft)
                    entity = AIData.entities[i] as AirCraft;
                if (AIData.entities[i].ID == targetEntityID && AIData.entities[i] is AirCraft)
                    target = AIData.entities[i];
            }

            if(!(target && entity))
            {
                Debug.LogWarning("Could not find target/entity! " + target + " " + entity);
                return 0;
            }

            Vector2 targetVector = target.transform.position - entity.transform.position; 
            //calculate difference of angles and compare them to find the correct turning direction
            if (!(entity is PlayerCore))
            {
                if(!asynchronous) entity.GetAI().RotateTo(targetVector, continueTraversing);   
                else entity.GetAI().RotateTo(targetVector);
            }
            else
            {
                entity.StartCoroutine(rotatePlayer(targetVector));
            }
            return asynchronous ? 0 : -1;
        }

        private void continueTraversing()
        {
            TaskManager.Instance.setNode(output);
        }

        IEnumerator rotatePlayer(Vector2 targetVector)
        {
            var player = (entity as PlayerCore);
            player.SetIsInteracting(false);

            Vector2 normalizedTarget = targetVector.normalized;
            float delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
            while (delta > 0.0001F)
            {
                player.RotateCraft(targetVector);
                delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
                yield return null;
            }

            player.SetIsInteracting(true);

            if(!asynchronous) continueTraversing();
        }
    }
}