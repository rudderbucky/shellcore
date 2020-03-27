using UnityEngine;
using System;

namespace NodeEditorFramework.Standard
{
    [NodeCanvasType("Dialogue")]
    public class DialogueCanvas : NodeCanvas
    {
        public override string canvasName { get { return "Dialogue Canvas"; } }
        public string missionName;

        protected override void OnCreate()
        {
            
        }

        public void OnEnable()
        {
            // Register to other callbacks, f.E.:
            //NodeEditorCallbacks.OnDeleteNode += OnDeleteNode;
        }

        protected override void ValidateSelf()
        {

        }

        public override bool CanAddNode(string nodeID)
        {
            return true;
        }
    }
}
