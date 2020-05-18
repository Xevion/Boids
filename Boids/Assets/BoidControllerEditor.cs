using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoidController))]
public class BoidControllerEditor : Editor {
    public override void OnInspectorGUI() {
        BoidController controller = (BoidController) target;
        
        // Boid Count update
        EditorGUI.BeginChangeCheck();
        controller.boidCount = EditorGUILayout.IntSlider("Boid Count", controller.boidCount, 1, 500);
        if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
            int diff = controller.boidCount - controller.boids.Count;
            Debug.Log($"Difference: {diff}");
            if (diff > 1)
                controller.AddBoids(diff);
            else if (diff < 0)
                controller.RemoveBoids(Mathf.Abs(diff));
        }
        
    }
}