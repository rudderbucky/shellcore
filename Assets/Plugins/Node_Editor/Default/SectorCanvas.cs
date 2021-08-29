using UnityEngine;
using System;

namespace NodeEditorFramework.Standard
{
    [NodeCanvasType("Sector")]
    public class SectorCanvas : NodeCanvas
    {
        public override string canvasName { get { return "Sector Canvas"; } }
        //public string sectorName;

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
