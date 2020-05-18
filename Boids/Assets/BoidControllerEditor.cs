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
        // Check must be performed or Boids will be added outside of Gameplay
        if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
            int diff = controller.boidCount - controller.boids.Count;
            if (diff > 1)
                controller.AddBoids(diff);
            else if (diff < 0)
                controller.RemoveBoids(Mathf.Abs(diff));
        }

        // Basic Boid Controller Attributes
        controller.boidGroupRange = EditorGUILayout.Slider("Group Range", controller.boidGroupRange, 0.01f, 25.0f);
        controller.boidStartVelocity = EditorGUILayout.Slider("Start Velocity", controller.boidStartVelocity, 0.01f, 5.0f);
        controller.boidVelocityLimit = EditorGUILayout.Slider("Max Velocity", controller.boidVelocityLimit, 0.01f, 7.5f);
        controller.boidSeparationRange = EditorGUILayout.Slider("Separation Range", controller.boidSeparationRange, 0.01f, 25.0f);
        
        // Boid Bias Attributes
        controller.alignmentBias = EditorGUILayout.Slider("Alignment Bias", controller.alignmentBias, 0.001f, 1.5f);
        controller.cohesionBias = EditorGUILayout.Slider("Cohesion Bias", controller.cohesionBias, 0.001f, 1.5f);
        controller.separationBias = EditorGUILayout.Slider("Separation Bias", controller.separationBias, 0.001f, 1.5f);

        controller.localFlocks = EditorGUILayout.Toggle("Use Groups?", controller.localFlocks);
    }
}