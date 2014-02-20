using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(LinkParams))]
public class TailLinkPropertyDrawer : PropertyDrawer {

    private float propertyHeight = 18f;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return 55f;
    }

    override public void OnGUI (Rect pos, SerializedProperty property, GUIContent label) {
        SerializedProperty minBound = property.FindPropertyRelative ("minBound");
        SerializedProperty scale = property.FindPropertyRelative ("scale");
        SerializedProperty groundPoint = property.FindPropertyRelative ("groundPoint");

        int indent = EditorGUI.indentLevel;

        EditorGUI.indentLevel = 3;

        EditorGUI.PrefixLabel (new Rect (0, pos.y, 50, propertyHeight), label);

        EditorGUI.indentLevel = 4;
        EditorGUI.PropertyField (
            new Rect (0, pos.y + propertyHeight, pos.width - 50, propertyHeight),
            minBound, new GUIContent ("minBound:"));

        EditorGUI.Slider (new Rect (0, pos.y + propertyHeight * 2, pos.width - 50, propertyHeight), scale, 0.1f, 1f, "scale:");

        //EditorGUI.PropertyField (
        //    new Rect (0, pos.y + propertyHeight * 3, pos.width - 50, propertyHeight),
        //    groundPoint, new GUIContent ("groundPt:"));

        EditorGUI.indentLevel = indent;
    }
}
