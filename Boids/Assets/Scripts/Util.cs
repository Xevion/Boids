using UnityEngine;

public class Util {
    public static Vector2 RotateBy(Vector2 v, float a) {
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
    }
    
    // Returns a velocity (Vector2) at a random angle with a specific overall magnitude
    public static Vector2 GetRandomVelocity(float magnitude) {
        var vector = new Vector2(magnitude, magnitude);
        return Util.RotateBy(vector, Random.Range(0, 180));
    }

    public static Vector2 MaxVelocity(Vector2 v, float max) {
        if (v.sqrMagnitude > max * max)
            v = (v / v.magnitude) * max;
        return v;
    }

    public static Vector2 MinVelocity(Vector2 v, float min) {
        if (v.sqrMagnitude > min * min)
            v = (v / v.magnitude) * min;
        return v;
    }

    public static Vector2 AbsVector(Vector2 vector) {
        return new Vector2(vector.x, vector.y);
    }
}