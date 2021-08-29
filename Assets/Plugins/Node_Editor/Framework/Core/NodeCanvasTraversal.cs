using System;

namespace NodeEditorFramework
{
	[Serializable]
	public abstract class NodeCanvasTraversal
	{
		public NodeCanvas nodeCanvas;
        public Node currentNode = null;

        public NodeCanvasTraversal (NodeCanvas canvas)
		{
			nodeCanvas = canvas;
		}

		public virtual void OnLoadCanvas () { }
		public virtual void OnSaveCanvas () { }

		public abstract void SetNode (Node node);
		public virtual void OnChange (Node node) {}
	}
}

