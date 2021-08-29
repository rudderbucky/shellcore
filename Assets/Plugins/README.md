# Node Editor Framework for Unity

#### A flexible and modular Node Editor Framework for creating node based displays and editors

<br>

<p align="center">
  <img alt="Node Editor Image" src="http://i.imgur.com/HcXhhGf.png" width="80%"/>
  <br><br>
  <b>
    <a href="https://nodeeditor.seneral.dev/Examples.html">WebGL Demo</a> - 
    <a href="http://forum.unity3d.com/threads/simple-node-editor.189230/#post-2134738">Forum Thread</a> -
    <a href="https://nodeeditor.seneral.dev/index.html">Documentation</a>
  </b>
</p>

### Features
- Fully-featured and extensible interface
- Extensive controls including zooming/panning
- Super extensible with runtime-fetching of custom nodes, connections, canvas, traversal routines and controls
- Advanced save- and cache system, including saving to Scene, Asset and XML
- Complete runtime support (see WebGL demo)
- Clean, structured and commented code!

### Examples
There are several great examples depending on how you are planning to use the framework. You can find them as a subbranch of this repo in Examples/... and need to be copied into a project already containing the Node Editor Framework (**most use the develop branch!**):

1. The [Texture Composer](https://github.com/Seneral/Node_Editor_Framework/tree/Examples/Texture_Composer), as seen in the title screen, is a very simple setup of a few texture nodes built upon the default calculation canvas in the framework. Start here to get a basic idea on how to create simple extensions of the framework with custom functionality.
2. A great, but complex example is the [Dialogue System](https://github.com/Seneral/Node_Editor_Framework/tree/Examples/Dialogue-System), developed and maintained by [ChicK00o](https://github.com/ChicK00o) and [atrblizzard](https://github.com/atrblizzard). Making excellent use of the framework's modularity to extend the frameworks capability and behaviour to get a basic dialogue system, including the editing and runtime execution (with an example scene), up and running. Check it out if you want to get an idea of a bigger setup expanding on the Node Editor Framework with custom rules.
3. Another set of nodes is the 'Expression Node' example. These are a bit different as they use reflection to inject any type of variable into the framework, to convert or execute code on. It's main purpose is to show complex modifications of the Node Knobs and general extended use of the framework.
4. A small example of extending the editor controls can be seen in the included [Node Group](https://github.com/Seneral/Node_Editor/blob/develop/Node_Editor/Framework/Core/NodeGroup.cs). It contains custom controls to handle without modifying any framework code.

### Contributing
If you want to contribute to this framework or have improved this framework internally to suit your needs better, please consider creating a Pull Request with your changes when they could help the framework become better. The [issues section](https://github.com/Seneral/Node_Editor_Framework/issues) serves as a feature discussion forum and I encourage you to check it out to get an idea of the future plans for the framework. You can also PM the main developer [Seneral](http://forum.unity3d.com/members/seneral.638015/) directly if you wish so.

### Credits
The project was started as a part of the thread ["Simple Node Editor"](http://forum.unity3d.com/threads/simple-node-editor.189230/#post-2134738) in may 2015 by [Seneral](http://forum.unity3d.com/members/seneral.638015/). Big thanks to [Baste Nesse Buanes](http://forum.unity3d.com/members/baste.185905/) who helped to set up this repository initially, without his help the framework could not have expanded to the point where it is now! Also, thanks to [Vexe](http://forum.unity3d.com/members/vexe.280515/), who has greatly helped with the reflection related stuff which is used for the zooming functionality of the editor. And of course thanks to everyone contributing or simply motivating the developers by sharing their work with the Node Editor Framework! You can find the full list of contributors [here](https://github.com/Seneral/Node_Editor_Framework/graphs/contributors).

### Licensing
The license used is the MIT License - see license.md
