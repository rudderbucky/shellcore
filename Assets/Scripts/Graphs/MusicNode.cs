using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Set Music")]
    public class MusicNode : Node
    {
        public override string GetName
        {
            get { return "MusicNode"; }
        }

        public override string Title
        {
            get { return "Set Music"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, height); }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string musicID = "";

        public bool defaultMusic = false;
        //public bool action = false; //TODO: action input

        float height = 80f;

        public override void NodeGUI()
        {
            height = 80f;
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            defaultMusic = GUILayout.Toggle(defaultMusic, "Default music");
            if (!defaultMusic)
            {
                height = 150f;
                GUILayout.Label("Music ID:");
                musicID = GUILayout.TextArea(musicID);
            }
        }

        public override int Traverse()
        {
            if (defaultMusic)
            {
                AudioManager.musicOverrideID = null;
                if (!SectorManager.instance.current.hasMusic || SectorManager.instance.current.musicID == "")
                {
                    Debug.Log("Jazz music stops.");
                    AudioManager.StopMusic();
                }
                else
                {
                    AudioManager.PlayMusic(SectorManager.instance.current.musicID);
                }
            }
            else
            {
                AudioManager.musicOverrideID = musicID;
                AudioManager.PlayMusic(musicID);
            }

            return 0;
        }
    }
}
