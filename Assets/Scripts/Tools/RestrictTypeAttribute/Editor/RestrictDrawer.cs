using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.RestrictTypeAttribute.Editor
{
	[CustomPropertyDrawer(typeof(RestrictAttribute))]
	public class RestrictDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label);

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedObject so = property.serializedObject;
			if (!so.hasModifiedProperties) so.Update();
			
			Type restrictedType = ((RestrictAttribute) attribute).Type;
			object oldObj = GetObject(property);
			
			EditorGUI.BeginChangeCheck();
			if (!MatchesRestrictions(oldObj, restrictedType)) SetObject(property, null);
			EditorGUI.PropertyField(position, property, label);
			if (!EditorGUI.EndChangeCheck()) return;

			object obj = GetObject(property);

			if (!MatchesRestrictions(obj, restrictedType)) SetObject(property, oldObj);
			so.ApplyModifiedProperties();
		}

		private static object GetObject(SerializedProperty property) =>
			property.propertyType switch
			{
				SerializedPropertyType.ObjectReference => property.objectReferenceValue,
				SerializedPropertyType.ManagedReference => property.managedReferenceValue,
				_ => null
			};

		private static void SetObject(SerializedProperty property, object obj)
		{
			if (property.propertyType is SerializedPropertyType.ObjectReference) property.objectReferenceValue = (Object) obj;
			else if (property.propertyType is SerializedPropertyType.ManagedReference) property.managedReferenceValue = obj;
		}

		private static bool MatchesRestrictions(object to, Type restrictedType)
		{
			if (restrictedType == null) return true;
			if (to == null) return true;
			Type type = to.GetType();
			if (type == restrictedType) return true;
			if (restrictedType.IsInterface) return type.GetInterfaces().Contains(restrictedType);
			
			Type baseType = type.BaseType;
			while (baseType != null)
			{
				if (baseType == restrictedType) return true;
				baseType = baseType.BaseType;
			}

			return false;
		}
	}
}