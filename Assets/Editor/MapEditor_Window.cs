using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapEditor_Window : EditorWindow {

	[MenuItem("Custom tools/Map Tile Controller")]
	public static void CreateWindow()
	{
		GetWindow<MapEditor_Window>("Map Tile Controller");
	}
}
