using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.IO
{
	/// <summary>
	/// Base class of an arbitrary Import/Export format based directly on the NodeCanvas
	/// </summary>
	public abstract class ImportExportFormat
	{
		/// <summary>
		/// Identifier for this format, must be unique (e.g. 'XML')
		/// </summary>
		public abstract string FormatIdentifier { get; }

		/// <summary>
		/// Optional format description (e.g. 'Legacy', shown as 'XML (Legacy)')
		/// </summary>
		public virtual string FormatDescription { get { return ""; } }

		/// <summary>
		/// Optional extension for this format if saved as a file, e.g. 'xml', default equals FormatIdentifier
		/// </summary>
		public virtual string FormatExtension { get { return FormatIdentifier; } }

		/// <summary>
		/// Returns whether the location selection needs to be performed through a GUI (e.g. for a custom database access)
		/// If true, the Import-/ExportLocationArgsGUI functions are called, else Import-/ExportLocationArgsSelection
		/// </summary>
		public virtual bool RequiresLocationGUI { get {

				return true; // At runtime, use GUI to select a file in a fixed folder

			}
		}

		/// <summary>
		/// Folder for runtime IO operations relative to the game folder.
		/// </summary>
		public static string RuntimeIOPath;

		private string fileSelection = "";
		private Rect fileSelectionMenuRect;
		private List<FileInfo> files;
		private int limit = 25;
		private int currentMin = 0;
		private float width = 0;

		/// <summary>
		/// Called only if RequiresLocationGUI is true.
		/// Displays GUI filling in locationArgs with the information necessary to locate the import operation.
		/// Override along with RequiresLocationGUI for custom database access.
		/// Return true or false to perform or cancel the import operation.
		/// </summary>
		public virtual bool? ImportLocationArgsGUI (ref object[] locationArgs)
		{

			GUILayout.Label("Import canvas " + FormatIdentifier);
			GUILayout.BeginHorizontal();
			//GUILayout.Label(RuntimeIOPath, GUILayout.ExpandWidth(true));
			if (GUILayout.Button(string.IsNullOrEmpty(fileSelection)? "Select..." : fileSelection, GUILayout.ExpandWidth(true)))
			{
                // Find save files
                if (RuntimeIOPath == null)
                    return false;

				Debug.Log(RuntimeIOPath);
				DirectoryInfo dir = Directory.CreateDirectory(RuntimeIOPath);
				FileInfo[] taskdata = dir.GetFiles("*.taskdata");
				FileInfo[] dialoguedata = dir.GetFiles("*.dialoguedata");
				FileInfo[] sectordata = dir.GetFiles("*.sectordata");
                currentMin = 0;
				files = new List<FileInfo>();
				files.AddRange(taskdata);
				files.AddRange(dialoguedata);
				files.AddRange(sectordata);
                // Fill save file selection menu
                GenericMenu fileSelectionMenu = new GenericMenu(false);
				for(int i = currentMin; i < Mathf.Min(currentMin + limit, files.Count); i++)
				{
					int x = i;
					fileSelectionMenu.AddItem(new GUIContent(files[i].Name), false, () => 
					{
						if(files[x].Name.Contains("taskdata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Mission;
						if(files[x].Name.Contains("dialoguedata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Dialogue;
						if(files[x].Name.Contains("sectordata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Sector;
                        fileSelection = Path.GetFileName(files[x].Name);
						NodeEditorGUI.Init();
					});
				}
				fileSelectionMenu.DropDown(fileSelectionMenuRect);
				fileSelectionMenuRect.height = 500;
				width = fileSelectionMenuRect.width;
			}
			if (Event.current.type == EventType.Repaint)
			{
				Rect popupPos = GUILayoutUtility.GetLastRect();
				fileSelectionMenuRect = new Rect(popupPos.x + 2, popupPos.yMax + 2, popupPos.width - 4, 500);
			}
			else if(files != null && Event.current.delta != Vector2.zero && Event.current.isScrollWheel)
			{
				if(Event.current.delta.y > 0)
					currentMin = Mathf.Min(currentMin + 3, files.Count - (Mathf.Min(files.Count, limit)));
				else
					currentMin = Mathf.Max(0, currentMin - 3);
				GenericMenu fileSelectionMenu = new GenericMenu(false);
				for(int i = currentMin; i < Mathf.Min(currentMin + limit, files.Count); i++)
				{
					int x = i;
					fileSelectionMenu.AddItem(new GUIContent(files[i].Name), false, () => 
					{
						if(files[x].Name.Contains("taskdata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Mission;
						if(files[x].Name.Contains("dialoguedata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Dialogue;
						if(files[x].Name.Contains("sectordata")) NodeEditorGUI.state = NodeEditorGUI.NodeEditorState.Sector;
                        fileSelection = Path.GetFileName(files[x].Name);
						NodeEditorGUI.Init();
					});
				}
				fileSelectionMenu.DropDown(fileSelectionMenuRect, width);
				fileSelectionMenuRect.height = 500;
				width = fileSelectionMenuRect.width;
			}
			GUILayout.EndHorizontal();

			// Finish operation buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Cancel"))
			{
				GUILayout.EndHorizontal();
				return false;
			}
			if (GUILayout.Button("Import"))
			{
				if (string.IsNullOrEmpty(fileSelection) || !File.Exists(Path.Combine(RuntimeIOPath, fileSelection)))
				{
					Debug.Log(Path.Combine(RuntimeIOPath, fileSelection));
					GUILayout.EndHorizontal();
					return false;
				}
				fileSelection = Path.GetFileName(fileSelection);
				locationArgs = new object[] { Path.Combine(RuntimeIOPath, fileSelection) };

					if (RuntimeIOPath == null || RuntimeIOPath == "")
                    RuntimeIOPath = Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");
				GUILayout.EndHorizontal();
				return true;
			}
			GUILayout.EndHorizontal();

			return null;
		}

		/// <summary>
		/// Called only if RequiresLocationGUI is false.
		/// Returns the information necessary to locate the import operation.
		/// By default it lets the user select a path as string[1].
		/// </summary>
		public virtual bool ImportLocationArgsSelection (out object[] locationArgs)
		{
			string path = null;
#if UNITY_EDITOR
			path = UnityEditor.EditorUtility.OpenFilePanel(
					"Import " + FormatIdentifier + (!string.IsNullOrEmpty (FormatDescription)? (" (" + FormatDescription + ")") : ""), 
					"Assets", FormatExtension.ToLower ());
#endif
			locationArgs = new object[] { path };
			return !string.IsNullOrEmpty (path);
		}

		public static string GetCanvasExtension()
		{
			string ext = ".taskdata";
            switch (NodeEditorGUI.state)
            {
                case NodeEditorGUI.NodeEditorState.Mission:
                    ext = ".taskdata";
                    break;
                case NodeEditorGUI.NodeEditorState.Dialogue:
                    ext = ".dialoguedata";
                    break;
                case NodeEditorGUI.NodeEditorState.Sector:
                    ext = ".sectordata";
                    break;
            }
			return ext;
		}

		public static object[] GetExportLocation(string fileName)
		{
			//if (IOFormat.ExportLocationArgsSelection(canvasCache.nodeCanvas.saveName, out IOLocationArgs))
			//			ImportExportManager.ExportCanvas(canvasCache.nodeCanvas, IOFormat, IOLocationArgs);
			

			return new object[] { Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder", fileName, GetCanvasExtension())};
		}

		/// <summary>
		/// Called only if RequiresLocationGUI is true.
		/// Displays GUI filling in locationArgs with the information necessary to locate the export operation.
		/// Override along with RequiresLocationGUI for custom database access.
		/// Return true or false to perform or cancel the export operation.
		/// </summary>
		public virtual bool? ExportLocationArgsGUI (string canvasName, ref object[] locationArgs)
		{
			GUILayout.Label("Export canvas ");

			// File save field
			GUILayout.BeginHorizontal();
            string ext = ".taskdata";
            switch (NodeEditorGUI.state)
            {
                case NodeEditorGUI.NodeEditorState.Mission:
                    ext = ".taskdata";
                    break;
                case NodeEditorGUI.NodeEditorState.Dialogue:
                    ext = ".dialoguedata";
                    break;
                case NodeEditorGUI.NodeEditorState.Sector:
                    ext = ".sectordata";
                    break;
            }
			// GUILayout.Label(RuntimeIOPath, GUILayout.ExpandWidth(false));
			fileSelection = GUILayout.TextField(Path.GetFileNameWithoutExtension(fileSelection), GUILayout.ExpandWidth(true));
			GUILayout.Label(ext, GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal();

			// Finish operation buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Cancel")) 
			{
				GUILayout.EndHorizontal();
				return false;

			}
			if (GUILayout.Button("Export"))
			{
				if (string.IsNullOrEmpty(fileSelection))
				{
					GUILayout.EndHorizontal();
					return false;
				}
				fileSelection = Path.GetFileNameWithoutExtension(fileSelection);

                if (RuntimeIOPath == null || RuntimeIOPath == "")
                    RuntimeIOPath = Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");

                locationArgs = new object[] { Path.Combine(RuntimeIOPath, fileSelection + ext)};
                Debug.Log(Path.Combine(RuntimeIOPath, fileSelection + ext));
				GUILayout.EndHorizontal();
				return true;
			}
			GUILayout.EndHorizontal();

			return null;
		}

		/// <summary>
		/// Called only if RequiresLocationGUI is false.
		/// Returns the information necessary to locate the export operation.
		/// By default it lets the user select a path as string[1].
		/// </summary>
		public virtual bool ExportLocationArgsSelection (string canvasName, out object[] locationArgs)
		{
			string path = null;
#if UNITY_EDITOR
			path = UnityEditor.EditorUtility.SaveFilePanel(
				"Export " + FormatIdentifier + (!string.IsNullOrEmpty (FormatDescription)? (" (" + FormatDescription + ")") : ""), 
				"Assets", canvasName, FormatExtension.ToLower ());
#endif
			locationArgs = new object[] { path };
			return !string.IsNullOrEmpty (path);
		}

		/// <summary>
		/// Imports the canvas at the location specified in the args (usually string[1] containing the path) and returns it's simplified canvas data
		/// </summary>
		public abstract NodeCanvas Import (params object[] locationArgs);

		/// <summary>
		/// Exports the given simplified canvas data to the location specified in the args (usually string[1] containing the path)
		/// </summary>
		public abstract void Export (NodeCanvas canvas, params object[] locationArgs);
	}

	/// <summary>
	/// Base class of an arbitrary Import/Export format based on a simple structural data best for most formats
	/// </summary>
	public abstract class StructuredImportExportFormat : ImportExportFormat
	{
		public override NodeCanvas Import (params object[] locationArgs) 
		{
			CanvasData data = ImportData (locationArgs);
			if (data == null)
				return null;
			return ImportExportManager.ConvertToNodeCanvas (data);
		}

		public override void Export (NodeCanvas canvas, params object[] locationArgs)
		{
			CanvasData data = ImportExportManager.ConvertToCanvasData (canvas);
			ExportData (data, locationArgs);
		}

		/// <summary>
		/// Imports the canvas at the location specified in the args (usually string[1] containing the path) and returns it's simplified canvas data
		/// </summary>
		public abstract CanvasData ImportData (params object[] locationArgs);

		/// <summary>
		/// Exports the given simplified canvas data to the location specified in the args (usually string[1] containing the path)
		/// </summary>
		public abstract void ExportData (CanvasData data, params object[] locationArgs);
	}
}