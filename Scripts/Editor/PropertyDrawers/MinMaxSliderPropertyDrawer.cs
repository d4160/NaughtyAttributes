using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaughtyAttributes.Editor
{
	[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
	public class MinMaxSliderPropertyDrawer : PropertyDrawerBase
	{
		protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
		{
			return (property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int)
				? GetPropertyHeight(property)
				: GetPropertyHeight(property) + GetHelpBoxHeight();
		}

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rect, label, property);

			MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)attribute;

			if (property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int)
			{
				EditorGUI.BeginProperty(rect, label, property);

				float indentLength = NaughtyEditorGUI.GetIndentLength(rect);
				float labelWidth = EditorGUIUtility.labelWidth + NaughtyEditorGUI.HorizontalSpacing;
				float floatFieldWidth = EditorGUIUtility.fieldWidth;
				float sliderWidth = rect.width - labelWidth - 2.0f * floatFieldWidth;
				float sliderPadding = 5.0f;

                Rect labelRect = new Rect(
					rect.x,
					rect.y,
					labelWidth,
					rect.height);

				Rect sliderRect = new Rect(
					rect.x + labelWidth + floatFieldWidth + sliderPadding - indentLength,
					rect.y,
					sliderWidth - 2.0f * sliderPadding + indentLength,
					rect.height);

				Rect minFloatFieldRect = new Rect(
					rect.x + labelWidth - indentLength,
					rect.y,
					floatFieldWidth + indentLength,
					rect.height);

				Rect maxFloatFieldRect = new Rect(
					rect.x + labelWidth + floatFieldWidth + sliderWidth - indentLength,
					rect.y,
					floatFieldWidth + indentLength,
					rect.height);

				// Draw the label
				EditorGUI.LabelField(labelRect, label.text);

				// Draw the slider
				EditorGUI.BeginChangeCheck();

                Vector2 sliderValue = property.propertyType == SerializedPropertyType.Vector2 ? property.vector2Value : property.vector2IntValue;
                EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y,
                    minMaxSliderAttribute.MinValue, minMaxSliderAttribute.MaxValue);

                sliderValue.x = EditorGUI.FloatField(minFloatFieldRect, sliderValue.x);
                sliderValue.x = Mathf.Clamp(sliderValue.x, minMaxSliderAttribute.MinValue,
                    Mathf.Min(minMaxSliderAttribute.MaxValue, sliderValue.y));

                sliderValue.y = EditorGUI.FloatField(maxFloatFieldRect, sliderValue.y);
                sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minMaxSliderAttribute.MinValue, sliderValue.x),
                    minMaxSliderAttribute.MaxValue);

                if (EditorGUI.EndChangeCheck())
                {
					if(property.propertyType == SerializedPropertyType.Vector2)
                        property.vector2Value = sliderValue;
					else
                        property.vector2IntValue = new Vector2Int((int)sliderValue.x, (int)sliderValue.y);
				}

				EditorGUI.EndProperty();
			}
            else
			{
				string message = minMaxSliderAttribute.GetType().Name + " can be used only on Vector2 fields";
				DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
			}

			EditorGUI.EndProperty();
		}
	}
}