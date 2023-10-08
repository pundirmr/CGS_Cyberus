# unity-utilities

Utilities for the Unity game engine. Include this as a submodule in your Unity project. 
The project must include the Universal Render Pipeline, the Burst Compiler and Unity's new Mathematics library.

These files are encompassed in its own Assembly, if you're unable to use the Utilities ensure that the 
assembly you are working in has a reference to the `unity-utilities` Assembly Definition file. The default 
assembly used by Unity (`Assembly-CSharp`) should automatically hold a reference to it.
 