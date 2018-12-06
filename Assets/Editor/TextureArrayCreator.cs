using UnityEditor;
using UnityEngine;
	

public class TextureArrayCreator : EditorWindow
{
	[SerializeField]
	private Texture2D[] m_textures = { };
	private bool m_textureError = false;
	private string m_savingPath = string.Empty;
	private string m_errorStr = string.Empty;

	[MenuItem("Custom tools/Texture Array Generator")]
	public static void CreateWindow()
	{
		GetWindow<TextureArrayCreator>("Texture Array Generator");
	}

	private void OnGUI()
	{
		//To use an array in the editor we need to conver it into a serilized property
		ScriptableObject target = this;
		SerializedObject so = new SerializedObject(target);
		SerializedProperty textures = so.FindProperty("m_textures");

		EditorGUILayout.PropertyField(textures, new GUIContent("Textures to convert:"), true);
		//Apply any chanes
		so.ApplyModifiedProperties();

		//Ask the user where to save the texture array
		EditorGUILayout.LabelField("Where to save: ", m_savingPath);
		if (GUILayout.Button("Change path"))
		{
			m_savingPath = EditorUtility.SaveFilePanelInProject("Where do you want to save the texture array?", "textur2Darray", "asset", "Please enter a name for the texture array");
		}
		 
		//If we have a file where to save it enable the generate button
		GUI.enabled = m_savingPath != string.Empty;
		if(GUILayout.Button("Generate Texture Array"))
		{
			//Reset the error flag
			m_textureError = false;
			//Data for all textures, it needs to be the same for all textures, so let us check
			int width = 0;
			int height = 0;
			TextureFormat format = 0;
			foreach(Texture2D t in m_textures)
			{
				Debug.Log(t.format);
				//If we do not have info get it
				if (width == 0 || height == 0)
				{
					width = t.width;
					height = t.height;
					format = t.format;
					//Once we have the data lets compare it to make sure that all have same format
				} else if(t.height != height || t.width != width || format != t.format)
				{
					//A texture has a different format so return
					m_textureError = true;
					if(format != t.format)
					{
						m_errorStr = "The textures needs to be the same format";
					} else
					{
						m_errorStr = "The textures needs to be the same size";
					}

					break;
				}
			}

			//If we do not have an error, save the texture array to a file
			if (!m_textureError)
			{
				Texture2DArray textureArray = new Texture2DArray(width, height, m_textures.Length, format, false);
				int i = 0;
				foreach(Texture2D t in m_textures)
				{
					//textureArray.SetPixels(t.GetPixels(0), i, 0);
					Graphics.CopyTexture(t, 0, 0, textureArray, i, 0);
					++i;
				}
				AssetDatabase.CreateAsset(textureArray, m_savingPath);
			}
		}
		GUI.enabled = true;

		//If an error produces display it
		if(m_textureError)
		{
			GUIStyle s = new GUIStyle();
			s.normal.textColor = Color.red;
			EditorGUILayout.LabelField(m_errorStr, s);
		}
	}
}
