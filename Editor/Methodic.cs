// Copyright (c) 2011 Matthew Miner

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Displays a dropdown menu of functions available for the selected game object to access.
/// </summary>
public class Methodic : EditorWindow
{
	struct Method
	{
		public Component component;
		public MethodInfo method;
	}
	
	static readonly GUIContent popupLabel = new GUIContent("Method");
	static readonly GUIContent invokeLabel = new GUIContent("Invoke", "Execute this method.");
	
	static GameObject target;
	static Method[] methods = {};
	static GUIContent[] methodLabels = {};
	static int selected;
	
	/// <summary>
	/// Adds Methodic to Window menu.
	/// </summary>
	[MenuItem ("Window/Methodic")]
	static void Init ()
	{
		// Get existing open window, or make new one if none
		EditorWindow.GetWindow<Methodic>();
	}
	
	void OnGUI ()
	{
		if (methods.Length == 0) {
			GUI.enabled = false;
		}
		
		EditorGUILayout.BeginHorizontal();
			
			selected = EditorGUILayout.Popup(popupLabel, selected, methodLabels);
			
			if (GUILayout.Button(invokeLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
				InvokeMethod(methods[selected]);
			}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		GUI.enabled = true;
		MethodicPrefs.OnGUI();
	}
	
	void OnSelectionChange ()
	{
		DiscoverMethods();
	}
	
	/// <summary>
	/// Discovers the selected game object's methods and refreshes the GUI.
	/// </summary>
	public static void DiscoverMethods ()
	{
		target = Selection.activeGameObject;
		selected = 0;
		var _methods = new List<Method>();
		var _methodLabels = new List<GUIContent>();
		
		if (target != null) {
			// Discover methods in attached components
			foreach (var component in target.GetComponents<MonoBehaviour>()) {
				var type = component.GetType();
				var allMethods = type.GetMethods(MethodicPrefs.flags);
				
				foreach (var method in allMethods) {
					_methods.Add(new Method { component = component, method = method });
					_methodLabels.Add(new GUIContent(method.Name, method.ToString()));
				}
			}
		}
		
		methods = _methods.ToArray();
		methodLabels = _methodLabels.ToArray();
		EditorWindow.GetWindow<Methodic>().Repaint();
	}
	
	/// <summary>
	/// Executes the specified method.
	/// </summary>
	/// <param name="toInvoke">The method to execute.</param>
	static void InvokeMethod (Method toInvoke)
	{
		var result = toInvoke.method.Invoke(toInvoke.component, null); // null = parameters

		// Display the return value if one is expected
		if (toInvoke.method.ReturnType != typeof(void)) {
			Debug.Log("[Methodic] Result: " + result);
		}
	}
}