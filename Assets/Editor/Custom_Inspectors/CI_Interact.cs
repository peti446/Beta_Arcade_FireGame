using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interact))]
public class CI_Interact : Editor {

  public Character source;


  private void OnEnable()
  {
    
  }

  public override void OnInspectorGUI()
  {
    source = (Character)EditorGUILayout.ObjectField(source, typeof(Character), true);
    //TODO Kony: add security check that the source object is on scene 
    if (GUILayout.Button("aaa") && source != null)
    {
      ((Interact)(target)).ClientInteract(source);
    }
  }
}
