using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoidController))]
public class BoidControllerEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
        var controller = (BoidController) target;
        bool redraw = false;

        // Boid Count update
        EditorGUI.BeginChangeCheck();
        controller.boidCount = EditorGUILayout.IntSlider("Boid Count", controller.boidCount, 0, 500);
        // Check must be performed or Boids will be added outside of gameplay
        if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
            int diff = controller.boidCount - controller.boids.Count;
            if (diff > 1)
                controller.AddBoids(diff, controller.boids.Count > 15);
            else if (diff < 0)
                controller.RemoveBoids(Mathf.Abs(diff));
        }

        // Basic Boid Controller Attributes
        EditorGUILayout.MinMaxSlider("Speed Limit", ref controller.minSpeed, ref controller.maxSpeed, 0.01f, 25f);
        // controller.minSpeed = EditorGUILayout.Slider("Minimum Speed", controller.minSpeed, 0.01f, 25.0f);
        // controller.maxSpeed = EditorGUILayout.Slider("Maximum Speed", controller.maxSpeed, 0.01f, 25.0f);
        controller.boundaryForce = EditorGUILayout.Slider("Boundary Force", controller.boundaryForce, 0.25f, 500f);
        controller.maxSteerForce = EditorGUILayout.Slider("Max Steer Force", controller.maxSteerForce, 1f, 500f);

        EditorGUI.BeginChangeCheck();
        controller.boidGroupRange = EditorGUILayout.Slider("Group Range", controller.boidGroupRange, 0.01f, 7.5f);
        controller.boidSeparationRange =
            EditorGUILayout.Slider("Separation Range", controller.boidSeparationRange, 0.01f, 5.0f);
        controller.boidFov = EditorGUILayout.Slider("Boid FOV", controller.boidFov, 1f, 360f);
        redraw = redraw || EditorGUI.EndChangeCheck();

        // Boid Bias Attributes
        controller.alignmentBias = EditorGUILayout.Slider("Alignment Bias", controller.alignmentBias, 0.001f, 1.5f);
        controller.cohesionBias = EditorGUILayout.Slider("Cohesion Bias", controller.cohesionBias, 0.001f, 1.5f);
        controller.separationBias =
            EditorGUILayout.Slider("Separation Bias", controller.separationBias, 0.001f, 2.5f);
        controller.boundaryBias = EditorGUILayout.Slider("Boundary Bias", controller.boundaryBias, 0.01f, 1.5f);

        controller.localFlocks = EditorGUILayout.Toggle("Use Groups?", controller.localFlocks);
        controller.edgeWrapping = EditorGUILayout.Toggle("Enable Wrapping?", controller.edgeWrapping);
        controller.enableAlignment = EditorGUILayout.Toggle("Enable Alignment?", controller.enableAlignment);
        controller.enableCohesion = EditorGUILayout.Toggle("Enable Cohesion?", controller.enableCohesion);
        controller.enableSeparation = EditorGUILayout.Toggle("Enable Separation?", controller.enableSeparation);
        
        // Relevant to Focused Boid Rendering
        EditorGUI.BeginChangeCheck();
        controller.enableFovChecks = EditorGUILayout.Toggle("Enable FOV?", controller.enableFovChecks);
        redraw = redraw || EditorGUI.EndChangeCheck();
        
        controller.enableBoundary = EditorGUILayout.Toggle("Enable Boundary?", controller.enableBoundary);
        
        // Focused Boid Rendering Attributes
        EditorGUI.BeginChangeCheck();
        ShapeDraw.CircleVertexCount = EditorGUILayout.IntSlider("Circle Vertex Count", ShapeDraw.CircleVertexCount, 4, 360);
        ShapeDraw.ArcVertexCount = EditorGUILayout.IntSlider("Arc Vertex Count", ShapeDraw.ArcVertexCount + 2, 3, 360) - 2;
        ShapeDraw.CircleWidth = EditorGUILayout.Slider("Circle Line Width", ShapeDraw.CircleWidth, 0.01f, 1f);
        ShapeDraw.ArcWidth = EditorGUILayout.Slider("Arc Line Width", ShapeDraw.ArcWidth, 0.01f, 1f);
        redraw = redraw || EditorGUI.EndChangeCheck();

        // Inspector elements related to Boid Focusing have changed - redraw!
        if (redraw && controller.focusedBoid != null)
            controller.focusedBoid.Draw(true);

        Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0.02f, 1f);
    }
}