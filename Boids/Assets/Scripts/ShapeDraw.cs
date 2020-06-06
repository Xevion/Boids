using UnityEngine;

/// <summary>
/// A simple static utility class that assists with drawing shapes using the <c>LineRenderer</c> class.
/// </summary>
public static class ShapeDraw {
    public static float CircleWidth = 1f;
    public static float ArcWidth = 1f;
    public static int CircleVertexCount = 360;
    public static int ArcVertexCount = 77;
    
    
    /// <summary>
    /// Draw a Arc aimed straight up with a certain angle width and radius.
    /// This Arc is not direct at any specific angle and start from 0 degrees and ends at <c>angle</c> degrees.
    /// You should rotate the <c>LineRenderer</c> to direct it at a specific point.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to draw the Arc upon.</param>
    /// <param name="angle">Angle (width) of the Arc</param>
    /// <param name="radius">Radius of the Arc</param>
    public static void DrawArc(LineRenderer lineRenderer, float angle, float radius) {
        // Setup LineRenderer properties
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = ArcWidth;
        lineRenderer.endWidth = ArcWidth;
        lineRenderer.positionCount = ArcVertexCount + 1 + 2;

        // Calculate points for circle
        var pointCount = ArcVertexCount + 1;
        var points = new Vector3[pointCount + 2];
        
        // Generate all points
        for (int i = 0; i < pointCount; i++) {
            var rad = Mathf.Deg2Rad * Mathf.Lerp(0, angle, (float) i / pointCount);
            points[i + 1] = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0) * radius;
        }

        points[0] = new Vector3(0, 0, 0);
        points[points.Length - 1] = points[0];

        // Add points to LineRenderer
        lineRenderer.SetPositions(points);
    }

    /// <summary>
    /// Draw a Circle with a specific radius and number of vertexes (detail level)
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to draw the Circle upon</param>
    /// <param name="radius">Radius of the Circle</param>
    public static void DrawCircle(LineRenderer lineRenderer, float radius) {
        // Setup LineRenderer properties
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = CircleWidth;
        lineRenderer.endWidth = CircleWidth;
        lineRenderer.positionCount = CircleVertexCount + 1;

        // Calculate points for circle
        var pointCount = CircleVertexCount + 1;
        var points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++) {
            var rad = Mathf.Deg2Rad * (i * 360f / CircleVertexCount);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0);
        }

        // Add points to LineRenderer
        lineRenderer.SetPositions(points);
    }
}