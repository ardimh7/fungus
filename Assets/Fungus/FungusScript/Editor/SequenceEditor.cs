﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rotorz.ReorderableList;

namespace Fungus.Script
{

	[CustomEditor (typeof(Sequence))]
	public class SequenceEditor : Editor 
	{
		public void DrawSequenceGUI(FungusScript fungusScript)
		{
			if (fungusScript.selectedSequence == null)
			{
				return;
			}

			serializedObject.Update();
			
			Sequence sequence = fungusScript.selectedSequence;
			
			EditorGUI.BeginChangeCheck();
			
			string name = EditorGUILayout.TextField(new GUIContent("Name", "Name of sequence displayed in editor window"), sequence.name);
			string desc = EditorGUILayout.TextField(new GUIContent("Description", "Sequence description displayed in editor window"), sequence.description);
			
			EditorGUILayout.Separator();
			
			if (name != sequence.name)
			{
				// The name is the gameobject name, so have to undo seperately
				Undo.RecordObject(sequence.gameObject, "Set Sequence Name");
				sequence.name = name;
			}
			
			if (desc != sequence.description)
			{
				Undo.RecordObject(sequence, "Set Sequence Description");
				sequence.description = desc;
			}

			ReorderableListGUI.Title("Command Sequence");

			SerializedProperty commandListProperty = serializedObject.FindProperty("commandList");
			FungusCommandListAdaptor adaptor = new FungusCommandListAdaptor(commandListProperty, 0);
			ReorderableListControl.DrawControlFromState(adaptor, null, 0);

			if (Application.isPlaying)
			{
				return;
			}
			
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();

			// Build list of categories
			List<string> categories = new List<string>();
			List<System.Type> subTypes = EditorExtensions.FindDerivedTypes(typeof(FungusCommand)).ToList();
			foreach(System.Type type in subTypes)
			{
				object[] attributes = type.GetCustomAttributes(false);
				foreach (object obj in attributes)
				{
					CommandInfoAttribute categoryAttr = obj as CommandInfoAttribute;
					if (categoryAttr != null)
					{
						if (!categories.Contains(categoryAttr.Category))
						{
							categories.Add(categoryAttr.Category);
						}
					}
				}
			}
			categories.Sort();

			GUILayout.Label("New Command");
			GUILayout.FlexibleSpace();
			int selectedCategoryIndex = EditorGUILayout.Popup(fungusScript.selectedCommandCategoryIndex, categories.ToArray());
			
			List<string> commandNames = new List<string>();
			List<System.Type> commandTypes = new List<System.Type>();
			
			string categoryName = categories[selectedCategoryIndex];
			foreach (System.Type type in subTypes)
			{
				CommandInfoAttribute commandInfoAttr = FungusCommandEditor.GetCommandInfo(type);
				if (commandInfoAttr == null)
				{
					continue;
				}

				if (categoryName == commandInfoAttr.Category)
				{
					commandNames.Add(commandInfoAttr.CommandName);
					commandTypes.Add(type);
				}
			}
			
			int selectedCommandIndex = EditorGUILayout.Popup(fungusScript.selectedAddCommandIndex, commandNames.ToArray());
			if (selectedCategoryIndex != fungusScript.selectedCommandCategoryIndex)
			{
				// Default to first item in list if category has changed
				selectedCommandIndex = 0;
			}

			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(fungusScript, "Select Command");
				fungusScript.selectedCommandCategoryIndex = selectedCategoryIndex;
				fungusScript.selectedAddCommandIndex = selectedCommandIndex;
			}
			
			if (selectedCommandIndex >= commandTypes.Count)
			{
				return;
			}
			
			System.Type selectedType = commandTypes[selectedCommandIndex];
			if (fungusScript.selectedSequence == null ||
			    selectedType == null)
			{
				return;
			}

			fungusScript.selectedAddCommandType = selectedType;

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
		
			if (fungusScript.copyCommand != null)
			{
				if (GUILayout.Button("Paste"))
				{
					fungusScript.selectedCommand = FungusCommandEditor.PasteCommand(fungusScript.copyCommand, fungusScript.selectedSequence);
				}
			}

			EditorGUILayout.EndHorizontal();

			CommandInfoAttribute infoAttr = FungusCommandEditor.GetCommandInfo(selectedType);
			if (infoAttr != null)
			{
				GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
				labelStyle.wordWrap = true;
				EditorGUILayout.HelpBox(infoAttr.HelpText, MessageType.Info);
			}

			serializedObject.ApplyModifiedProperties();
		}

		static public Sequence SequenceField(GUIContent label, GUIContent nullLabel, FungusScript fungusScript, Sequence sequence)
		{
			if (fungusScript == null)
			{
				return null;
			}
			
			Sequence result = sequence;
			
			// Build dictionary of child sequences
			List<GUIContent> sequenceNames = new List<GUIContent>();
			
			int selectedIndex = 0;
			sequenceNames.Add(nullLabel);
			Sequence[] sequences = fungusScript.GetComponentsInChildren<Sequence>();
			for (int i = 0; i < sequences.Length; ++i)
			{
				sequenceNames.Add(new GUIContent(sequences[i].name));
				
				if (sequence == sequences[i])
				{
					selectedIndex = i + 1;
				}
			}
			
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, sequenceNames.ToArray());
			if (selectedIndex == 0)
			{
				result = null; // Option 'None'
			}
			else
			{
				result = sequences[selectedIndex - 1];
			}
			
			return result;
		}

		static public Sequence SequenceField(Rect position, GUIContent nullLabel, FungusScript fungusScript, Sequence sequence)
		{
			if (fungusScript == null)
			{
				return null;
			}
			
			Sequence result = sequence;
			
			// Build dictionary of child sequences
			List<GUIContent> sequenceNames = new List<GUIContent>();
			
			int selectedIndex = 0;
			sequenceNames.Add(nullLabel);
			Sequence[] sequences = fungusScript.GetComponentsInChildren<Sequence>();
			for (int i = 0; i < sequences.Length; ++i)
			{
				sequenceNames.Add(new GUIContent(sequences[i].name));
				
				if (sequence == sequences[i])
				{
					selectedIndex = i + 1;
				}
			}
			
			selectedIndex = EditorGUI.Popup(position, selectedIndex, sequenceNames.ToArray());
			if (selectedIndex == 0)
			{
				result = null; // Option 'None'
			}
			else
			{
				result = sequences[selectedIndex - 1];
			}
			
			return result;
		}
	}

}