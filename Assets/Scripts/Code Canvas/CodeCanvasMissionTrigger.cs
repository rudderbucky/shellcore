using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CodeCanvasSequence;
using static CodeTraverser;

public class CodeCanvasMissionTrigger : MonoBehaviour
{
    public struct MissionTrigger
    {
        public string missionName;
        public List<string> prerequisites;
        public Sequence sequence;
    }
    private void ParseMissionTrigger(int lineIndex, int charIndex,
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes, out FileCoord coord)
    {
        
    }
}
