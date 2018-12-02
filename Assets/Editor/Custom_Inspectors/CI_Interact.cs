using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// the script that we're customising is the Interact script - 
/// this means that the interact script will now have custom GUI properties
/// for easier debugging.
/// </summary>
[CustomEditor(typeof(Interact))]
public class CI_Interact : Editor {

  public Character source;


  private void OnEnable()
  {
    
  }

  /// <summary>
  /// custom inpsector for an specific object class
  /// overriding is necessary in order for it to work.
  /// </summary>
  public override void OnInspectorGUI()
  {
    source = (Character)EditorGUILayout.ObjectField(source, typeof(Character), true);
    
    //TODO Kony: add security check that the source object is on scene 

    if (GUILayout.Button("client side interact") && source != null)
    {
      ((Interact)(target)).ClientInteract(source);
    }

    else if (GUILayout.Button("server side interact") && source != null)
    {
      ((Interact)(target)).ServerInteract(source);
    }
  }
}
