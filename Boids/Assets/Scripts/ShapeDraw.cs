using UnityEngine;

/// <summary>
/// A simple static utility class that assists with drawing shapes using the <c>LineRenderer</c> class.
/// </summary>
public class ShapeDraw {
    /// <summary>
    /// Draw a Arc aimed straight up with a certain angle width and radius.
    /// Use <see cref="RotateLineRenderer"/> to point the Arc at a certain direction.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to draw the Arc upon.</param>
    /// <param name="angle">Angle of the Arc</param>
    /// <param name="radius">Radius of the Arc</param>
    /// <param name="vertexCount">Number of vertexes to be used in the arc, clamp minimum 3 </param>
    /// <seealso cref="RotateLineRenderer"/>
    public static void DrawArc(LineRenderer lineRenderer, float angle, float radius, int vertexCount) {
        
    }

    /// <summary>
    /// Draw a Circle with a specific radius and number of vertexes (detail level)
    /// </summary>
    /// <param name="lineRenderer"></param>
    public static void DrawCircle(LineRenderer lineRenderer) {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lineRenderer"></param>
    public static void RotateLineRenderer(LineRenderer lineRenderer) {
        
    }
}