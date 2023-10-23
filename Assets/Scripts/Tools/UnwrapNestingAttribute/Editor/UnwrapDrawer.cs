using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools.UnwrapNestingAttribute.Editor
{	
	[CustomPropertyDrawer(typeof(UnwrapAttribute), true)]
	public class UnwrapDrawer : PropertyDrawer
	{
		private static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;
		private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
		private static readonly float FullLine = LineHeight + Spacing;

		private SerializedObject _so;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			UnwrapAttribute unwrap = (UnwrapAttribute) attribute;
			bool full = unwrap.FullUnwrap;
			bool isObject = property.propertyType == SerializedPropertyType.ObjectReference;
			float defaultHeight = LineHeight + Spacing * 3;
			if (!full && !property.isExpanded) return defaultHeight;
			if (isObject && property.objectReferenceValue == null) return defaultHeight;
			float height = !full || isObject ? FullLine + Spacing * 2 : 0;
			foreach (SerializedProperty child in GetChildren(property)) height += EditorGUI.GetPropertyHeight(child);
			return height + Spacing * 4;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UnwrapAttribute unwrap = (UnwrapAttribute) attribute;
			SerializedObject so = property.serializedObject;
			if (so.hasModifiedProperties) so.ApplyModifiedProperties();
			so.Update();

			Rect backgroundRect = position;
			float indentSize = LineHeight * (EditorGUI.indentLevel + 1);
			backgroundRect.width += indentSize + Spacing;
			backgroundRect.x -= indentSize - Spacing;
			GUI.Box(backgroundRect, Texture2D.whiteTexture, new GUIStyle(GUI.skin.window));
			position.y += Spacing;
			Rect rect = position;
			rect.height = LineHeight;
			bool full = unwrap.FullUnwrap;
			bool isObject = property.propertyType == SerializedPropertyType.ObjectReference;
			if (!full) property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, property.displayName);
			else if (isObject) EditorGUI.LabelField(rect, property.displayName);
			if (isObject) EditorGUI.ObjectField(rect, property);

			if (!full || isObject) position.y += FullLine;
			int indent = full ? 0 : 1;

			if (full || property.isExpanded)
				using (new EditorGUI.IndentLevelScope(indent))
					DrawChildren(position, GetChildren(property));
			
			so.ApplyModifiedProperties();
		}

		
		private static void DrawChildren(Rect rect, IEnumerable<SerializedProperty> children)
		{
			foreach (SerializedProperty child in children)
			{
				rect.height = EditorGUI.GetPropertyHeight(child, true);
				EditorGUI.PropertyField(rect, child, true);
				child.serializedObject.ApplyModifiedProperties();
				rect.y += rect.height + Spacing;
			}
		}

		private IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (property.objectReferenceValue == null) yield break;
				_so ??= new SerializedObject(property.objectReferenceValue);
				
				_so.Update();
				SerializedProperty soProperties = _so.GetIterator();
				soProperties.NextVisible(true);
				while (soProperties.NextVisible(false))
					yield return soProperties.Copy();
				_so.ApplyModifiedProperties();
				yield break;
			}
			
			SerializedProperty currentProperty = property.Copy();
			SerializedProperty nextSiblingProperty = property.Copy();
			nextSiblingProperty.NextVisible(false);

			if (!currentProperty.NextVisible(true)) yield break;
			do
			{
				if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty)) break;
				yield return currentProperty;
			}
			while (currentProperty.NextVisible(false));
		}
	}
}