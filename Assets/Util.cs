using UnityEngine;

public class Util {
    public static Vector2 RotateBy(Vector2 v, float a) {
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
    }
}