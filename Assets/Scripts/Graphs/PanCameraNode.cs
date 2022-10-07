using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Cutscenes/Pan Camera")]
    public class PanCameraNode : Node
    {
        public override string GetName
        {
            get { return "PanCameraNode"; }
        }

        public override string Title
        {
            get { return "Pan Camera"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, height); }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string flagName;
        public Vector2 coordinates;
        public bool useCoordinates;
        public bool endPanning;
        public bool asynchronous;
        public float velocityFactor;
        public float height = 100;

        public override void NodeGUI()
        {
            height = 100;
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            if (!(endPanning = GUILayout.Toggle(endPanning, "End Panning")))
            {
                height = 240;
                if (useCoordinates = Utilities.RTEditorGUI.Toggle(useCoordinates, "Use coordinates"))
                {
                    GUILayout.Label("Coordinates:");
                    float x = coordinates.x, y = coordinates.y;
                    GUILayout.BeginHorizontal();
                    x = Utilities.RTEditorGUI.FloatField("X", x);
                    y = Utilities.RTEditorGUI.FloatField("Y", y);
                    coordinates = new Vector2(x, y);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Flag Name:");
                    flagName = GUILayout.TextField(flagName);
                }

                velocityFactor = Utilities.RTEditorGUI.FloatField("Velocity Factor", velocityFactor);
                if (velocityFactor < 0)
                {
                    velocityFactor = Utilities.RTEditorGUI.FloatField("Velocity Factor", 0);
                    Debug.LogWarning("Can't register negative numbers!");
                }
                asynchronous = RTEditorGUI.Toggle(asynchronous, "Asynchronous Mode", GUILayout.MinWidth(400));
            }
        }

        public override int Traverse()
        {
            Vector3 coords = coordinates;
            if (!useCoordinates)
            {
                for (int i = 0; i < AIData.flags.Count; i++)
                {
                    if (AIData.flags[i].name == flagName)
                    {
                        coords = AIData.flags[i].transform.position;
                        break;
                    }
                }
            }

            if (endPanning)
            {
                CameraScript.panning = false;
                CameraScript.instance.Focus(PlayerCore.Instance.transform.position);
                foreach (var rect in RectangleEffectScript.instances)
                {
                    if (rect)
                    {
                        rect.Start();
                    }
                }

                return 0;
            }
            else
            {
                CameraScript.velocityFactor = velocityFactor;
                coords.z = -CameraScript.zLevel;
                CameraScript.panning = true;
                CameraScript.target = coords;
                if (!asynchronous)
                {
                    CameraScript.callback = continueTraversing;
                }

                return asynchronous ? 0 : -1;
            }
        }

        private void continueTraversing()
        {
            CameraScript.callback = null;
            TaskManager.Instance.setNode(output);
        }
    }
}
