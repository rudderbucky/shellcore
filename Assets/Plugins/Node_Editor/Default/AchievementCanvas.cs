using UnityEngine;
using System;

namespace NodeEditorFramework.Standard
{
    [NodeCanvasType("Achievement")]
    public class AchievementCanvas : NodeCanvas
    {
        public override string canvasName { get { return "Achievement Canvas"; } }
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
