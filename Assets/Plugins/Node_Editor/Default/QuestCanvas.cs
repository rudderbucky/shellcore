using UnityEngine;
using System;

namespace NodeEditorFramework.Standard
{
    [NodeCanvasType("Quest")]
    public class QuestCanvas : NodeCanvas
    {
        public override string canvasName { get { return "Quest Canvas"; } }

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
