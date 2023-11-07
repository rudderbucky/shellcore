using System;
using UnityEditor;
using UnityEngine;

public class Autobuilder
{
    public static string[] scenes = { 
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/SampleScene.unity",
        "Assets/Scenes/WorldCreator.unity"
     };


	[MenuItem("ShellCore Command/Build/All Targets")]
	public static void BuildAll()
	{
		BuildWindows();
        BuildLinux();
		BuildOSX();
        BuildLinuxHeadless();
        Debug.Log("done");
	}



	[MenuItem("ShellCore Command/Build/Windows (release)")]
	public static void BuildWindows()
	{
		WCWorldIO.DeletePlaceholderDirectories();
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
		Debug.Log("Starting Windows Build!");
		BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = "Build/Windows/ShellCore Command.exe",
			    target = BuildTarget.StandaloneWindows,
			    options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Player
            });
	}

    [MenuItem("ShellCore Command/Build/Linux (release)")]
	public static void BuildLinux()
	{
		WCWorldIO.DeletePlaceholderDirectories();
		Debug.Log("Starting Linux Build!");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = 
			    "Build/Linux/ShellCore Command.x86_64",
			    target = BuildTarget.StandaloneLinux64,
			    options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Player
            });
	}

	
    [MenuItem("ShellCore Command/Build/Linux (debug)")]
	public static void BuildLinuxDebug()
	{
		WCWorldIO.DeletePlaceholderDirectories();
		Debug.Log("Starting Linux Build!");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = 
			    "Build/Linux/ShellCore Command.x86_64",
			    target = BuildTarget.StandaloneLinux64,
			    options = BuildOptions.AutoRunPlayer,
                subtarget = (int)StandaloneBuildSubtarget.Player
				
            });
	}

	
    [MenuItem("ShellCore Command/Build/OSX (release)")]
	public static void BuildOSX()
	{
		Debug.Log("Starting OSX Build!");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = 
			    "Build/OSX/ShellCore Command",
			    target = BuildTarget.StandaloneOSX,
			    options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Player
            });
	}

	[MenuItem("ShellCore Command/Build/OSX (debug)")]
	public static void BuildOSXDebug()
	{
		Debug.Log("Starting OSX Build!");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = 
			    "Build/OSX/ShellCore Command",
			    target = BuildTarget.StandaloneOSX,
			    options = BuildOptions.AutoRunPlayer,
                subtarget = (int)StandaloneBuildSubtarget.Player
            });
	}

    [MenuItem("ShellCore Command/Build/Linux (headless)")]
	public static void BuildLinuxHeadless()
	{
		Debug.Log("Starting Linux Build!");
        BuildPipeline.BuildPlayer(new BuildPlayerOptions() 
            {
                scenes = scenes,
			    locationPathName = "Build/Linux-Headless/ShellCore Command.x86_64",
			    target = BuildTarget.StandaloneLinux64,
			    options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server
            });
	}
}
