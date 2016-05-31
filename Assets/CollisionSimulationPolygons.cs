using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollisionSimulationPolygons : MonoBehaviour {
    public GameObject polyPrefab;

    public float groundRepulsion = 1000f;
    public float groundAbsorption = 0.1f;
    public float airFrictionConst = 0.02f;
    public float polyPointsMin = 3;
    public float polyPointsMax = 7;
    public Color normalColor;
    public Color collidingColor;
    Canvas canvas;
    List<Polygon> polys;
    public int numPolys = 10;
    public PhysicsObject.NumericalIntegrationMethod integrationMethod = PhysicsObject.NumericalIntegrationMethod.RK4;
    bool startVelocityApplied = false;

    void Awake() {
        // get the canvas
        canvas = GameObject.FindObjectOfType<Canvas>().gameObject.GetComponent<Canvas>();

        // create some polygons
        polys = new List<Polygon>();
        for (int i = 0; i < numPolys; ++i) {
            var go = Instantiate(polyPrefab, new Vector3(Random.Range(-300f, 300f), Random.Range(-300f, 300f)), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            var poly = go.GetComponent<Polygon>();
            var points = new List<Vector3>();
            int numPoints = Mathf.RoundToInt(Random.Range(polyPointsMin, polyPointsMax));
            float r = 25f;
            float slice = Mathf.PI * 2f / numPoints;
            for (int j = 0; j < numPoints; ++j) {
                float a = Random.Range(j * slice, (j + 1) * slice);
                points.Add(new Vector3(r * Mathf.Cos(a), r * Mathf.Sin(a), 0f));
            }
            poly.points = points;
            poly.mass = 1f;
            poly.integrationMethod = integrationMethod;
            ((RectTransform)poly.transform).sizeDelta = new Vector2(r, r);
            polys.Add(poly);
            poly.CreateOBB();
        }
    }

    void Start () {
        Uber.I().PhysicsSolve += PhysicsSolve;
        Uber.I().PhysicsSimulate += PhysicsSimulate;
    }
	
	// Update is called once per frame
	void Update () {
	}

    void PhysicsSolve() {
        if (!startVelocityApplied) {
            startVelocityApplied = true;
            foreach (var poly in polys) {
                float a = Random.Range(0f, Mathf.PI * 2f);
                poly.ApplyForce(8000f * new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
            }
        }

        foreach (var obj in Uber.I().objects) {
            // gravity
            //obj.ApplyForce(new Vector3(0f, Uber.I().gravity * obj.mass, 0f));

            // air friction
            obj.ApplyForce(-obj.v * airFrictionConst);

            // check screen edges
            var p = obj.transform.position;
            var r = ((RectTransform)(obj.transform)).sizeDelta.x * 0.5f;
            ApplyBound(obj, 0f - p.y + r, true, false); // bottom
            ApplyBound(obj, 0f - p.x + r, false, false); // left
            ApplyBound(obj, p.y - Screen.height + r, true, true); // top
            ApplyBound(obj, p.x - Screen.width + r, false, true); // bottom
        }
    }

    void ApplyBound(PhysicsObject obj, float dist, bool isY, bool reverseRepulsion) {
        if (dist >= 0f) {
            var v = obj.v;
            if (isY)
                v.y = 0f;
            else
                v.x = 0f;
            obj.ApplyForce(-v * groundAbsorption);
            var repulForce = new Vector3(isY ? 0f : groundRepulsion, isY ? groundRepulsion : 0f, 0f) * dist;
            repulForce *= reverseRepulsion ? -1f : 1f;
            obj.ApplyForce(repulForce);
        }
    }

    void PhysicsSimulate(float deltaTime) {
        for (int i = 0; i < polys.Count; ++i) {
            bool colliding = false;
            var a = polys[i];
            for (int j = 0; j < polys.Count; ++j) {
                var b = polys[j];
                if (a == b) continue;
                var aRT = a.GetComponent<RectTransform>();
                var bRT = b.GetComponent<RectTransform>();
                var aR = aRT.sizeDelta.x * 0.75f; // should be half width but not sure why this works better
                var bR = bRT.sizeDelta.x * 0.75f;
                float distSq = (aRT.position - bRT.position).sqrMagnitude;
                float radiiSq = aR * aR + bR * bR;
                colliding |= distSq <= radiiSq;
            }
            foreach (var segment in a.transform.GetComponentsInChildren<Image>())
                segment.color = colliding ? collidingColor : normalColor;
            a.PhysicsSimulate(deltaTime);
        }
    }
}
