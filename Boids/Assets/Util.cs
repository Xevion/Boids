using UnityEditor;
using UnityEngine;

public class Util {
    public static Vector2 RotateBy(Vector2 v, float a) {
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
    }

    public static Vector2 LimitVelocity(Vector2 v, float max) {
        if (v.magnitude > max) {
            v = (v / v.magnitude) * max;
        }

        return v;
    }

    public static Vector2 AbsVector(Vector2 vector) {
        return new Vector2(vector.x, vector.y);
    }
    
    public class ReadOnlyAttribute : PropertyAttribute {
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label) {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}