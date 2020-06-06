using UnityEditor;
using UnityEngine;

// [CustomEditor(typeof(Boid))]
// public class BoidEditor : UnityEditor.Editor {
//     public override void OnInspectorGUI() {
//         base.OnInspectorGUI();
//
//         Boid boid = (Boid) target;
//         GUILayout.Label($"Boid heading at ({boid.velocity.x}, {boid.velocity.y})");
//     }
// }

public class BoidGizmoDrawer {
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    static void DrawGizmo(Boid boid, GizmoType gizmoType) {
        // Simply draw the # of Boids within the perceived flock
        Handles.Label(boid.transform.position, $"{boid.transform.name}/{boid.latestNeighborhoodCount}");
        
        if(boid.isFocused)
            foreach(Boid flockmate in boid.latestNeighborhood)
                Handles.DrawDottedLine(boid.transform.position, flockmate.transform.position, 1f);
    }
}