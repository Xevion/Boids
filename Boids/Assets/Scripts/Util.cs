using UnityEngine;

public class Util {
    public static Vector2 RotateBy(Vector2 v, float a) {
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2(rx, v.x * sa + v.y * ca);
    }

    // Returns a velocity (Vector2) at a random angle with a specific overall magnitude
    public static Vector2 GetRandomVelocity(float magnitude) {
        var vector = new Vector2(magnitude, magnitude);
        return RotateBy(vector, Random.Range(0, 180));
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

    public static float Vector2ToAngle(Vector2 velocity) {
        float result = Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x);
        return (result < 0) ? (360f + result) : result;
    }

    public static float AngleBetween(Vector2 from, Vector2 to) {
        Vector2 diff = to - from;
        float result = Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x);
        return (result < 0) ? (360f + result) : result;
    }

    public static float AngleDifference(float angle1, float angle2) {
        return Mathf.Abs((angle1 > 180 ? 360 - angle1 : angle1) - (angle2 > 180 ? 360 - angle2 : angle2));
    }

    public static float AddAngle(float angle, float add) {
        float result = angle + add;
        if (result > 360) return result - 360;
        if (result < 0) return 360 + result;
        return result;
    }
}