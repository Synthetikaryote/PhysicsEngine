using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class OrientedBoundingBox : MonoBehaviour {
    public Vector2 R = Vector2.zero;
    public Vector2 S = Vector2.zero;
    public Vector2 center = Vector2.zero;
    public float halfSizeR = 0f;
    public float halfSizeS = 0f;
    List<Image> segments;

    public void SetupWithPoints(List<Vector3> points, bool visualize = false) {
        // find the midpoint
        var n = points.Count;
        var m = points.Aggregate((v1, v2) => { return v1 + v2; }) / n;
        // find the covariant matrix in 2d
        // | a c |
        // | c b |
        float a = 0f, b = 0f, c = 0f;
        foreach(var v in points) {
            var dx = v.x - m.x;
            var dy = v.y - m.y;
            a += dx * dx;
            b += dy * dy;
            c += dx * dy;
        }
        a /= n; b /= n; c /= n;

        // find the biggest lambda
        // | a-L c |  where L is lambda
        // | c b-L |
        // (a - L) * (b - L) - c * c = 0
        // a* b -aL - bL + L ^ 2 - c ^ 2 = 0
        // L ^ 2 - aL - bL + a * b - c ^ 2 = 0
        // L ^ 2 - (a + b)L + a * b - c ^ 2 = 0
        // using the quadratic formula,
        // L = (-B + -sqrt(B ^ 2 - 4AC)) / 2A, where A = 1, B = -(a + b), C = a * b - c ^ 2
        // L = (-a - b +/- sqrt((a + b) ^ 2 - 4 * (a * b - c ^ 2))) / 2
        var apb = a + b;
        var sqrt4AC = Mathf.Sqrt(apb * apb - 4 * (a * b - c * c));
        var L1 = (apb + sqrt4AC) * 0.5f;
        var L2 = (apb - sqrt4AC) * 0.5f;
        var L = Mathf.Abs(L1) > Mathf.Abs(L2) ? L1 : L2;

        // find R and S
        R = new Vector2(-c / (a - L), 1f);
        R.Normalize();
        S = new Vector2(-R.y, R.x);

        // find the distances along each OBB axis (defined by R and S)
        float minR, maxR, minS, maxS;
        minR = minS = float.MaxValue;
        maxR = maxS = float.MinValue;
        foreach(var p in points) {
            var projR = Vector2.Dot(p, R);
            minR = Mathf.Min(projR, minR);
            maxR = Mathf.Max(projR, maxR);
            var projS = Vector2.Dot(p, S);
            minS = Mathf.Min(projS, minS);
            maxS = Mathf.Max(projS, maxS);
        }
        halfSizeR = (maxR - minR) * 0.5f;
        halfSizeS = (maxS - minS) * 0.5f;

        // find the OBB center point
        var midR = (minR + maxR) * 0.5f;
        var midS = (minS + maxS) * 0.5f;
        var center = R * midR + S * midS;

        // find the corner points for the visualization
        var cornerPoints = new List<Vector3>(4);
        cornerPoints.Add(center - halfSizeR * R - halfSizeS * S);
        cornerPoints.Add(center - halfSizeR * R + halfSizeS * S);
        cornerPoints.Add(center + halfSizeR * R + halfSizeS * S);
        cornerPoints.Add(center + halfSizeR * R - halfSizeS * S);

        // create a visualization
        if (visualize) {
            segments = new List<Image>();
            for (int i = 0; i < cornerPoints.Count; ++i) {
                var p1 = cornerPoints[i];
                var p2 = cornerPoints[(i + 1) % cornerPoints.Count];
                var go = new GameObject("segment" + i);
                var segment = go.AddComponent<Image>();
                go.transform.SetParent(transform);
                segment.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg + 90f);
                segment.transform.localPosition = (p1 + p2) * 0.5f;
                segment.transform.localScale = Vector3.one;
                float length = (p1 - p2).magnitude + 5f;
                var rt = (RectTransform)segment.transform;
                rt.sizeDelta = new Vector2(1f, length);
                segments.Add(segment);
            }
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}