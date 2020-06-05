using UnityEditor;
using UnityEngine;

// [CustomEditor(typeof(Boid))]
// public class BoidEditor : UnityEditor.Editor {
//     public override void OnInspectorGUI() {
//         base.OnInspectorGUI();
//     }
// }

public class BoidGizmoDrawer {
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    static void DrawGizmo(Boid boid, GizmoType gizmoType) {
        // Simply draw the # of Boids within the perceived flock
        
        Handles.Label(boid.transform.position, $"{boid.latestNeighborhoodCount}");
    }
}