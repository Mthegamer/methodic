//
// Method.cs
//
// Author: Matthew Miner
//         matthew@matthewminer.com
//         http://www.matthewminer.com/
//
// Copyright (c) 2012
//

using System;
using System.Reflection;
using UnityEngine;

namespace Methodic
{
	/// <summary>
	/// Holds method info and its parent component.
	/// </summary>
	class Method
	{
		readonly Component component;
		readonly MethodInfo method;
		readonly Parameters parameters;

		/// <summary>
		/// The name of the method.
		/// </summary>
		internal string name {
			get { return method.Name; }
		}

		/// <summary>
		/// Creates a new Method instance.
		/// </summary>
		/// <param name="component">The component the scripts are attached to.</param>
		/// <param name="method">The method.</param>
		internal Method (Component component, MethodInfo method)
		{
			this.component = component;
			this.method = method;
			this.parameters = new Parameters(method);
		}

		/// <summary>
		/// If method has parameters, displays fields for them to be modified.
		/// </summary>
		internal void OnGUI ()
		{
			if (method.GetParameters().Length > 0) {
				Util.DrawDivider();
				parameters.OnGUI();
			}
		}

		/// <summary>
		/// Executes the method.
		/// </summary>
		internal void Invoke ()
		{
			try {
				var result = method.Invoke(component, parameters.parametersArray);

				// Display the return value if one is expected
				if (method.ReturnType != typeof(void)) {
					Debug.Log("[Methodic] Result: " + result);
				}
			} catch (ArgumentException e) {
				Debug.LogError("[Methodic] Unable to invoke method: " + e.Message);
			}
		}
	}
}

