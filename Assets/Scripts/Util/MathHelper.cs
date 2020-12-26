using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MathHelper {

    // Returns Vector3 where each component is random value between min and max
    public static Vector3 RandomVector3(float min, float max) {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }

    public static Vector3 RandomVector2(float min, float max) {
        return new Vector2(Random.Range(min, max), Random.Range(min, max));
    }

    public static Vector3 AbsVector3(Vector3 v) {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    // Get average point of array of vectors
    public static Vector3 AverageVector3(params Vector3[] vectors) {
        Vector3 result = Vector3.zero;

        foreach(Vector3 v in vectors) {
            result += v;
        }
        result *= 1f / vectors.Length;
        return result;
    }

    // Get cumulative length of array of vectors
    public static float TotalVectorLengths(params Vector3[] vectors) {
        float length = 0;
        for(int i = 0; i < vectors.Length; i++) {
            if (i == 0) continue;
            length += Vector3.Distance(vectors[i], vectors[i - 1]);
        }
        return length;
    }

    public static List<Vector3> BezierCurve(Vector3 p0, Vector3 p1, Vector3 control) {
        List<Vector3> curve = new List<Vector3>();

        int steps = 10;

        for(int i = 0; i < steps; i++) {
            float t = (float) i / (steps - 1);
            Vector3 newPoint = Vector3.Lerp(Vector3.Lerp(p0, control, t), Vector3.Lerp(control, p1, t), t);
            curve.Add(newPoint);
        }

        return curve;
    }
}
