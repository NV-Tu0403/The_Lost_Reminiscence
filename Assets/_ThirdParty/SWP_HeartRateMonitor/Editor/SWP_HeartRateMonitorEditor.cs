// --------------ABOUT AND COPYRIGHT----------------------
//  Copyright © 2013 SketchWork Productions Limited
//        support@sketchworkproductions.com
// -------------------------------------------------------

using UnityEngine;
using System.Collections;
using _ThirdParty.SWP_HeartRateMonitor.Scripts;
using UnityEditor;

[CustomEditor(typeof(SwpHeartRateMonitor))]
public class SWP_HeartRateMonitorEditor : Editor
{
	static public bool ShowHeader = true;
	static public bool ShowTitles = true;
	static public bool ShowQuickDebugControls = true;

	public override void OnInspectorGUI()
	{
		SwpHeartRateMonitor _HeartRateMonitorScript = (SwpHeartRateMonitor)target;  
		
		#region GLOBAL STATIC CONTROLS
		if (SWP_HeartRateMonitorEditor.ShowHeader)
			GetHeader();
		
		if (SWP_HeartRateMonitorEditor.ShowTitles)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			EditorGUILayout.LabelField("Heart Rate/Beat Globals");
			EditorGUILayout.EndHorizontal();
		}
		
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
		SWP_HeartRateMonitorEditor.ShowHeader = EditorGUILayout.ToggleLeft("Show Editor Header", SWP_HeartRateMonitorEditor.ShowHeader);
		SWP_HeartRateMonitorEditor.ShowTitles = EditorGUILayout.ToggleLeft("Show Editor Titles", SWP_HeartRateMonitorEditor.ShowTitles);
		SWP_HeartRateMonitorEditor.ShowQuickDebugControls = EditorGUILayout.ToggleLeft("Show Debug Controls", SWP_HeartRateMonitorEditor.ShowQuickDebugControls);
		#else
		EditorGUILayout.BeginVertical();
		SWP_HeartRateMonitorEditor.ShowHeader = EditorGUILayout.Toggle("Show Editor Header", SWP_HeartRateMonitorEditor.ShowHeader);
		SWP_HeartRateMonitorEditor.ShowTitles = EditorGUILayout.Toggle("Show Editor Titles", SWP_HeartRateMonitorEditor.ShowTitles);
		SWP_HeartRateMonitorEditor.ShowQuickDebugControls = EditorGUILayout.Toggle("Show Debug Controls", SWP_HeartRateMonitorEditor.ShowQuickDebugControls);
		#endif
		EditorGUILayout.EndVertical();
		#endregion

		#region SOUND CONTROLS
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		_HeartRateMonitorScript.EnableSound = EditorGUILayout.ToggleLeft("Enable Sound", _HeartRateMonitorScript.EnableSound);
		#else
		_HeartRateMonitorScript.enableSound = EditorGUILayout.Toggle("Enable Sound", _HeartRateMonitorScript.enableSound);
		#endif
		EditorGUILayout.EndHorizontal();
		
		if (_HeartRateMonitorScript.enableSound)
		{
			#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
			EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
			#else
			EditorGUILayout.BeginVertical();
			#endif
			_HeartRateMonitorScript.soundVolume = EditorGUILayout.Slider("Sound Volume", _HeartRateMonitorScript.soundVolume, 0f, 1f);
			_HeartRateMonitorScript.heart1Sound = (AudioClip) EditorGUILayout.ObjectField("Heart 1 Sound", _HeartRateMonitorScript.heart1Sound, typeof(AudioClip), false);
			_HeartRateMonitorScript.heart2Sound = (AudioClip) EditorGUILayout.ObjectField("Heart 2 Sound", _HeartRateMonitorScript.heart2Sound, typeof(AudioClip), false);
			_HeartRateMonitorScript.flatlineSound = (AudioClip) EditorGUILayout.ObjectField("Flatline Sound", _HeartRateMonitorScript.flatlineSound, typeof(AudioClip), false);
			EditorGUILayout.EndVertical();
		}
		#endregion

		#region MAIN CONTROLLER SETTINGS
		if (SWP_HeartRateMonitorEditor.ShowTitles)  
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			EditorGUILayout.LabelField("Heart Rate/Beat Controls");
			EditorGUILayout.EndHorizontal();
		}
		
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
		#else
		EditorGUILayout.BeginVertical();
		#endif

		_HeartRateMonitorScript.beatsPerMinute = EditorGUILayout.IntSlider("Beats Per Minute", _HeartRateMonitorScript.beatsPerMinute, 0, 240);		
		
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		_HeartRateMonitorScript.FlatLine = EditorGUILayout.ToggleLeft("Flatline", _HeartRateMonitorScript.FlatLine);
		EditorGUILayout.Separator();
		_HeartRateMonitorScript.ShowBlip = EditorGUILayout.ToggleLeft("Show Leading Blip", _HeartRateMonitorScript.ShowBlip);
		#else
		_HeartRateMonitorScript.flatLine = EditorGUILayout.Toggle("Flatline", _HeartRateMonitorScript.flatLine);
		EditorGUILayout.Separator();
		_HeartRateMonitorScript.showBlip = EditorGUILayout.Toggle("Show Leading Blip", _HeartRateMonitorScript.showBlip);
		#endif

		_HeartRateMonitorScript.blip = (GameObject) EditorGUILayout.ObjectField("Blip Prefab", _HeartRateMonitorScript.blip, typeof(GameObject), false);
		_HeartRateMonitorScript.blipSize = EditorGUILayout.Slider("Blip Size", _HeartRateMonitorScript.blipSize, 0.1f, 10f);
		_HeartRateMonitorScript.blipTrailStartSize = EditorGUILayout.Slider("Blip Trail Start Size", _HeartRateMonitorScript.blipTrailStartSize, 0.1f, 10f);
		_HeartRateMonitorScript.blipTrailEndSize = EditorGUILayout.Slider("Blip Trail End Size", _HeartRateMonitorScript.blipTrailEndSize, 0f, 10f);
		EditorGUILayout.Separator();
		_HeartRateMonitorScript.blipMonitorWidth = EditorGUILayout.FloatField("Blip Width", _HeartRateMonitorScript.blipMonitorWidth);
		_HeartRateMonitorScript.blipMonitorHeightModifier = EditorGUILayout.FloatField("Blip Height Modifier", _HeartRateMonitorScript.blipMonitorHeightModifier);

		_HeartRateMonitorScript.mainMaterial = (Material) EditorGUILayout.ObjectField("Main Material", _HeartRateMonitorScript.mainMaterial, typeof(Material), false);
		
		EditorGUILayout.EndVertical();
		#endregion
		
		#region VISUAL CONTROLS
		if (SWP_HeartRateMonitorEditor.ShowTitles)  
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			EditorGUILayout.LabelField("Visual Controls");
			EditorGUILayout.EndHorizontal();
		}
		
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
		#else
		EditorGUILayout.BeginVertical();
		#endif   
		
		_HeartRateMonitorScript.normalColour = EditorGUILayout.ColorField("Normal Colour", _HeartRateMonitorScript.normalColour);
		_HeartRateMonitorScript.mediumColour = EditorGUILayout.ColorField("Medium Colour", _HeartRateMonitorScript.mediumColour);
		_HeartRateMonitorScript.badColour = EditorGUILayout.ColorField("Bad Colour", _HeartRateMonitorScript.badColour);
		_HeartRateMonitorScript.flatlineColour = EditorGUILayout.ColorField("Flatline Colour", _HeartRateMonitorScript.flatlineColour);
				
		EditorGUILayout.EndVertical();	
		#endregion
		
		#region DEBUG SECTION CONTROLS
		if (SWP_HeartRateMonitorEditor.ShowQuickDebugControls)
		{ 
			if (SWP_HeartRateMonitorEditor.ShowTitles)  
			{
				EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
				EditorGUILayout.LabelField("Quick Debug Controls (" + (_HeartRateMonitorScript.flatLine ? "FLATLINE" : (_HeartRateMonitorScript.beatsPerMinute + "BPM")) + ")");
				EditorGUILayout.EndHorizontal();
			}
			
			#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
			EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
			#else
			EditorGUILayout.BeginVertical();
			#endif   
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Normal") && Application.isPlaying)
				_HeartRateMonitorScript.flatLine = false;
			
			if (GUILayout.Button("Flatline") && Application.isPlaying)
				_HeartRateMonitorScript.flatLine = true;
			
			EditorGUILayout.EndHorizontal();		
			
			EditorGUILayout.BeginHorizontal(); 
			
			if (GUILayout.Button("-10 BPM") && Application.isPlaying)
				_HeartRateMonitorScript.beatsPerMinute -= 10;
			
			if (GUILayout.Button("+10 BPM") && Application.isPlaying)
				_HeartRateMonitorScript.beatsPerMinute += 10;
			
			EditorGUILayout.EndHorizontal();		
		    			
			EditorGUILayout.BeginHorizontal(); 
			
			EditorGUILayout.LabelField("Test Colours:", GUILayout.MaxWidth(90));
			
			if (GUILayout.Button("Normal") && Application.isPlaying)
				_HeartRateMonitorScript.SetHeartRateColour(_HeartRateMonitorScript.normalColour);
			
			if (GUILayout.Button("Medium") && Application.isPlaying)
				_HeartRateMonitorScript.SetHeartRateColour(_HeartRateMonitorScript.mediumColour);

			if (GUILayout.Button("Bad") && Application.isPlaying)
				_HeartRateMonitorScript.SetHeartRateColour(_HeartRateMonitorScript.badColour);
			
			if (GUILayout.Button("Flatline") && Application.isPlaying)
				_HeartRateMonitorScript.SetHeartRateColour(_HeartRateMonitorScript.flatlineColour);
			
			EditorGUILayout.EndHorizontal();		
			
			EditorGUILayout.EndVertical();	
		}
		#endregion
		
		if (GUI.changed)
			EditorUtility.SetDirty(_HeartRateMonitorScript);
		
		this.Repaint();
	}

	void GetHeader()
	{
		Texture thisTextureHeader = (Texture) AssetDatabase.LoadAssetAtPath("Assets/SWP_HeartRateMonitor/Editor/Textures/SketchWorkHeader.png", typeof(Texture));
		
		if (thisTextureHeader != null)
		{
			Rect thisRect = GUILayoutUtility.GetRect(0f, 0f);
			thisRect.width = thisTextureHeader.width;
			thisRect.height = thisTextureHeader.height;
			GUILayout.Space(thisRect.height);
			GUI.DrawTexture(thisRect, thisTextureHeader);
		}
	}
}
