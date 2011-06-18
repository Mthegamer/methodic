// Copyright (c) 2011 Matthew Miner.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Displays a dropdown menu of functions available for the selected game object to access.
/// </summary>
public class Methodic : EditorWindow
{
	/// <summary>
	/// A simple struct for holding method info and its parent component.
	/// </summary>
	public struct Method
	{
		public Component component;
		public MethodInfo method;
		
		/// <summary>
		/// Whether the method accepts any parameters.
		/// </summary>
		public bool hasParameters {
			get { return method.GetParameters().Length > 0; }
		}
	}
	
	/// <summary>
	/// The version of Methodic.
	/// </summary>
	public static readonly System.Version version = new System.Version(0, 1);
	
	static readonly GUIContent popupLabel = new GUIContent("Method");
	static readonly GUIContent invokeLabel = new GUIContent("Invoke", "Execute this method.");
	
	static GameObject target;
	static Method[] methods = {};
	static GUIContent[] methodLabels = {};
	static int selected;
	static Vector2 scrollPos;
	
	/// <summary>
	/// Adds Methodic to Window menu.
	/// </summary>
	[MenuItem ("Window/Methodic %#m")]
	static void Init ()
	{
		// Get existing open window, or make new one if none
		GetWindow<Methodic>();
	}
	
	void OnGUI ()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		
		if (methods.Length == 0) {
			GUI.enabled = false;
		}
		
		EditorGUILayout.BeginHorizontal();
			
			selected = EditorGUILayout.Popup(popupLabel, selected, methodLabels);
			
			if (GUILayout.Button(invokeLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
				var selectedMethod = methods[selected];
			
				if (selectedMethod.hasParameters) {
					MethodicParametersPopup.ShowPopup(selectedMethod);
				} else {
					InvokeMethod(selectedMethod, null);
				}
			}
		
		EditorGUILayout.EndHorizontal();
		
		GUI.enabled = true;
		MethodicPrefs.OnGUI();
		EditorGUILayout.EndScrollView();
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
					var label = new GUIContent("", method.ToString());
					
					if (MethodicPrefs.displayClass) {
						label.text = component.GetType() + ": ";
					}
					
					label.text += method.Name;
					_methods.Add(new Method { component = component, method = method });
					_methodLabels.Add(label);
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
	/// <param name="parameters">The parameters to send the method.</param>
	public static void InvokeMethod (Method toInvoke, object[] parameters)
	{
		try {
			var result = toInvoke.method.Invoke(toInvoke.component, parameters);

			// Display the return value if one is expected
			if (toInvoke.method.ReturnType != typeof(void)) {
				Debug.Log("[Methodic] Result: " + result);
			}
		} catch (System.ArgumentException e) {
			Debug.LogError("[Methodic] Unable to invoke method: " + e.Message);
		}
		
	}
}