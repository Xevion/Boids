using UnityEngine;

public class Util {
    public static Vector2 RotateBy(Vector2 v, float a) {
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
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