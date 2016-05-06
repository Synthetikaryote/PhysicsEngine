using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Polygon : PhysicsObject {
    public List<Vector3> points;

    List<Image> segments;

	public override void Start () {
        base.Start();
        segments = new List<Image>();
        for (int i = 0; i < points.Count; ++i) {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            var go = new GameObject("segment" + i);
            var segment = go.AddComponent<Image>();
            go.transform.SetParent(transform);
            segment.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg + 90f);
            segment.transform.localPosition = (p1 + p2) * 0.5f;
            float length = (p1 - p2).magnitude + 5f;
            var rt = (RectTransform)segment.transform;
            rt.sizeDelta = new Vector2(6f, length);
            segments.Add(segment);
        }
    }

    public void CreateOBB() {
        var obb = gameObject.AddComponent<OrientedBoundingBox>();
        obb.SetupWithPoints(points, true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
