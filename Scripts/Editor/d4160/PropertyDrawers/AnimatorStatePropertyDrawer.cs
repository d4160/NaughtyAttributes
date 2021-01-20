using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
	[CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
	public class AnimatorStatePropertyDrawer : PropertyDrawerBase
	{
		private const string InvalidAnimatorControllerWarningMessage = "Target animator controller is null";
		private const string InvalidLayerWarningMessage = "Layer is invalid";
		private const string InvalidTypeWarningMessage = "{0} must be an int or a string";

		protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
		{
			AnimatorStateAttribute animatorStateAttribute = PropertyUtility.GetAttribute<AnimatorStateAttribute>(property);
			object target = PropertyUtility.GetTargetObjectWithProperty(property);
			
			AnimatorController animator = GetAnimatorController(property, animatorStateAttribute.AnimatorName, target);
			bool validAnimatorController = animator;
			int layer = validAnimatorController ? GetAnimatorLayer(property, animatorStateAttribute.Layer, target) : -1;
			bool validLayer = layer >= 0 && layer < animator.layers.Length;
			bool validPropertyType = property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.String;

			return (validAnimatorController && validPropertyType && validLayer)
				? GetPropertyHeight(property)
				: GetPropertyHeight(property) + GetHelpBoxHeight();
		}

		protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
		{
			AnimatorStateAttribute animatorStateAttribute = PropertyUtility.GetAttribute<AnimatorStateAttribute>(property);
			object target = PropertyUtility.GetTargetObjectWithProperty(property);
			
			AnimatorController animatorController = GetAnimatorController(property, animatorStateAttribute.AnimatorName, target);
			if (!animatorController)
			{
				DrawDefaultPropertyAndHelpBox(rect, property, InvalidAnimatorControllerWarningMessage, MessageType.Warning);
				return;
			}

			int layer = GetAnimatorLayer(property, animatorStateAttribute.Layer, target);

			if (layer <= -1 || layer >= animatorController.layers.Length)
			{
				DrawDefaultPropertyAndHelpBox(rect, property, InvalidLayerWarningMessage, MessageType.Warning);
				return;
			}

			AnimatorStateMachine machine = animatorController.layers[layer].stateMachine;
			int statesCount = machine.states.Length;
			List<ChildAnimatorState> animatorStates = new List<ChildAnimatorState>(statesCount);
			for (int i = 0; i < statesCount; i++)
			{
				ChildAnimatorState state = machine.states[i];
				animatorStates.Add(state);
			}

			FieldInfo durationFieldInfo = GetDurationField(property, animatorStateAttribute.Duration, target);

			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer:
					DrawPropertyForInt(rect, property, label, animatorStates, durationFieldInfo, target);
					break;
				case SerializedPropertyType.String:
					DrawPropertyForString(rect, property, label, animatorStates, durationFieldInfo, target);
					break;
				default:
					DrawDefaultPropertyAndHelpBox(rect, property, string.Format(InvalidTypeWarningMessage, property.name), MessageType.Warning);
					break;
			}
		}

		private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, List<ChildAnimatorState> states, FieldInfo durationFieldInfo, object target)
		{
			int paramNameHash = property.intValue;
			int index = 0;

			for (int i = 0; i < states.Count; i++)
			{
				if (paramNameHash == states[i].state.nameHash)
				{
					index = i + 1; // +1 because the first option is reserved for (None)
					break;
				}
			}

			string[] displayOptions = GetDisplayOptions(states);

			int newIndex = EditorGUI.Popup(rect, label.text, index, displayOptions);
			if (newIndex == 0)
			{
				property.intValue = 0;
				
				if(durationFieldInfo != null)
					durationFieldInfo.SetValue(target, -1);
			}
			else
			{
				property.intValue = states[newIndex - 1].state.nameHash;
				Motion motion = states[newIndex - 1].state.motion;
				if (durationFieldInfo != null)
				{
					if(motion)
						durationFieldInfo.SetValue(target, motion.averageDuration);
					else
						durationFieldInfo.SetValue(target, -1);
				}

			}
		}

		private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, List<ChildAnimatorState> states, FieldInfo durationFieldInfo, object target)
		{
			string paramName = property.stringValue;
			int index = 0;

			for (int i = 0; i < states.Count; i++)
			{
				if (paramName == states[i].state.name)
				{
					index = i + 1; // +1 because the first option is reserved for (None)
					break;
				}
			}

			string[] displayOptions = GetDisplayOptions(states);

			int newIndex = EditorGUI.Popup(rect, label.text, index, displayOptions);
			if (newIndex == 0)
			{
				property.stringValue = null;
			}
			else
			{
				property.stringValue = states[newIndex - 1].state.name;
			}
		}

		private static string[] GetDisplayOptions(List<ChildAnimatorState> states)
		{
			string[] displayOptions = new string[states.Count + 1];
			displayOptions[0] = "(None)";

			for (int i = 0; i < states.Count; i++)
			{
				displayOptions[i + 1] = states[i].state.name;
			}

			return displayOptions;
		}

		private static AnimatorController GetAnimatorController(SerializedProperty property, string animatorName, object target)
		{
			FieldInfo animatorFieldInfo = ReflectionUtility.GetField(target, animatorName);
			if (animatorFieldInfo != null &&
				animatorFieldInfo.FieldType == typeof(Animator))
			{
				Animator animator = animatorFieldInfo.GetValue(target) as Animator;
				if (animator != null)
				{
					AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
					return animatorController;
				}
			}

			PropertyInfo animatorPropertyInfo = ReflectionUtility.GetProperty(target, animatorName);
			if (animatorPropertyInfo != null &&
				animatorPropertyInfo.PropertyType == typeof(Animator))
			{
				Animator animator = animatorPropertyInfo.GetValue(target) as Animator;
				if (animator != null)
				{
					AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
					return animatorController;
				}
			}

			MethodInfo animatorGetterMethodInfo = ReflectionUtility.GetMethod(target, animatorName);
			if (animatorGetterMethodInfo != null &&
				animatorGetterMethodInfo.ReturnType == typeof(Animator) &&
				animatorGetterMethodInfo.GetParameters().Length == 0)
			{
				Animator animator = animatorGetterMethodInfo.Invoke(target, null) as Animator;
				if (animator != null)
				{
					AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
					return animatorController;
				}
			}

			return null;
		}
		
		private static int GetAnimatorLayer(SerializedProperty property, string layerName, object target)
		{
			if (string.IsNullOrEmpty(layerName))
				return 0;
			
			FieldInfo layerFieldInfo = ReflectionUtility.GetField(target, layerName);
			if (layerFieldInfo != null &&
			    layerFieldInfo.FieldType == typeof(int))
			{
				return (int)layerFieldInfo.GetValue(target);
			}

			PropertyInfo layerPropertyInfo = ReflectionUtility.GetProperty(target, layerName);
			if (layerPropertyInfo != null &&
			    layerPropertyInfo.PropertyType == typeof(int))
			{
				return (int)layerPropertyInfo.GetValue(target);
			}

			MethodInfo layerGetterMethodInfo = ReflectionUtility.GetMethod(target, layerName);
			if (layerGetterMethodInfo != null &&
			    layerGetterMethodInfo.ReturnType == typeof(int) &&
			    layerGetterMethodInfo.GetParameters().Length == 0)
			{
				return (int)layerGetterMethodInfo.Invoke(target, null);
			}

			return -1;
		}

		private FieldInfo GetDurationField(SerializedProperty property, string durationName, object target)
		{
			if (string.IsNullOrEmpty(durationName))
				return null;
			
			FieldInfo durationFieldInfo = ReflectionUtility.GetField(target, durationName);
			if (durationFieldInfo != null &&
			    durationFieldInfo.FieldType == typeof(float))
			{
				return durationFieldInfo;
			}

			return null;
		}
	}
}