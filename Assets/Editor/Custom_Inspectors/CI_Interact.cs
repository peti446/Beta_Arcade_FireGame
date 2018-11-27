using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interact))]
public class CI_Interact : Editor {
  public Object source;

  private void OnSceneGUI()
  {
    EditorGUILayout.ObjectField(source, typeof(Object), true);
  }
}
