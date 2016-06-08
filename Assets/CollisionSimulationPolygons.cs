using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollisionSimulationPolygons : MonoBehaviour {
    public GameObject polyPrefab;
    public GameObject segmentPrefab;
    public GameObject debugDotPrefab;

    public float groundRepulsion = 1000f;
    public float groundAbsorption = 0.1f;
    public float airFrictionConst = 0.02f;
    public float polyPointsMin = 3;
    public float polyPointsMax = 7;
    public float polyRadius = 25f;
    public Color normalColor;
    public Color collidingColor;
    Canvas canvas;
    List<Polygon> polys;
    List<Segment> segments;
    public int numPolys = 10;
    public PhysicsObject.NumericalIntegrationMethod integrationMethod = PhysicsObject.NumericalIntegrationMethod.RK4;
    bool startVelocityApplied = false;

    List<GameObject> debugDots = null;
    int debugDotsPerPoly = 8;

    void Awake() {
        // get the canvas
        canvas = GameObject.FindObjectOfType<Canvas>().gameObject.GetComponent<Canvas>();
        debugDots = new List<GameObject>();
        for (int i = 0; i < numPolys * debugDotsPerPoly; ++i) {
            var debugDot = Instantiate(debugDotPrefab);
            debugDot.transform.SetParent(canvas.transform);
            debugDot.transform.localScale = Vector3.one * 0.2f;
            debugDots.Add(debugDot);
        }

        // create some polygons
        polys = new List<Polygon>();
        for (int i = 0; i < numPolys; ++i) {
            var go = Instantiate(polyPrefab, new Vector3(Random.Range(-300f, 300f), Random.Range(-300f, 300f)), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            var poly = go.GetComponent<Polygon>();
            var points = new List<Vector3>();
            int numPoints = Mathf.RoundToInt(Random.Range(polyPointsMin, polyPointsMax));
            float r = polyRadius;
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

        // create the screen edges
        var leftWallObj = Instantiate(segmentPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
        var leftWall = leftWallObj.GetComponent<Segment>();
        leftWall.normal = new Vector3(1f, 0f, 0f);
        var rightWallObj = Instantiate(segmentPrefab, new Vector3(Screen.width, 0f, 0f), Quaternion.identity) as GameObject;
        var rightWall = rightWallObj.GetComponent<Segment>();
        rightWall.normal = new Vector3(-1f, 0f, 0f);
        var topWallObj = Instantiate(segmentPrefab, new Vector3(0f, Screen.height, 0f), Quaternion.identity) as GameObject;
        var topWall = topWallObj.GetComponent<Segment>();
        topWall.normal = new Vector3(0f, -1f, 0f);
        var bottomWallObj = Instantiate(segmentPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
        var bottomWall = bottomWallObj.GetComponent<Segment>();
        bottomWall.normal = new Vector3(0f, 1f, 0f);
        segments = new List<Segment> { leftWall, rightWall, topWall, bottomWall };
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
            // check screen edges
            var a = polys[i];
            var aOBB = a.GetComponent<OrientedBoundingBox>();
            var aP = a.transform.position + (Vector3)aOBB.center;
            foreach (var segment in segments) {
                // find the effective length
                var r = aOBB.halfSizeR * Mathf.Abs(Vector3.Dot(aOBB.R, segment.normal)) + aOBB.halfSizeS * Mathf.Abs(Vector3.Dot(aOBB.S, segment.normal));
                // early out if the ball's not heading toward the wall at all
                var vDotN = Vector3.Dot(a.v, segment.normal);
                if (vDotN < 0) {
                    // find the time to collide
                    // t = (r - D - n . p) / (n . v)
                    var t = (r - segment.D - Vector3.Dot(segment.normal, aP)) / vDotN;
                    // check if the collision will be this frame
                    if (t <= a.frameTime) {
                        // advance that sphere to the collision point
                        a.PhysicsSimulate(t);

                        // I could find which corner here in order to incorporate rotation, but that's not added yet
                        // without rotation, I don't need to know which corner hits the wall

                        // bounce off the wall
                        a.v = a.v - 2f * vDotN * segment.normal;
                    }
                }
            }

            for (int l = 0; l < debugDotsPerPoly; ++l)
                debugDots[i * debugDotsPerPoly + l].SetActive(false);

            // check against each other polygon
            for (int j = i + 1; j < polys.Count; ++j) {
                var b = polys[j];
                var bOBB = b.GetComponent<OrientedBoundingBox>();
                var bP = b.transform.position + (Vector3)bOBB.center;

                float shortestT = -1f;
                var collisionPoint = Vector3.zero;
                bool aOnB = true;
                // do this both directions: a on b and b on a
                for (int k = 0; k < 2; ++k) {
                    // now compare c to d.  c will be a the first time and b the second time.  d will be the opposite
                    var first = k == 0;
                    var cP = first ? aP : bP;
                    var dP = first ? bP : aP;
                    var cOBB = first ? aOBB : bOBB;
                    var dOBB = first ? bOBB : aOBB;
                    var cV = first ? a.v : b.v;
                    var dV = first ? b.v : a.v;
                    var deltaP = cP - dP;
                    var deltaV = cV - dV;

                    // skip if they're not heading toward each other
                    //var pDotV = Vector3.Dot(deltaP, deltaV);
                    //if (pDotV > 0)
                    //    continue;
                    // find the time to intersect and collision point using this algorithm
                    // http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
                    // go through each of c's points
                    foreach (var p in new Vector3[] {
                        deltaP + (Vector3)cOBB.R * cOBB.halfSizeR + (Vector3)cOBB.S * cOBB.halfSizeS,
                        deltaP + (Vector3)cOBB.R * cOBB.halfSizeR - (Vector3)cOBB.S * cOBB.halfSizeS,
                        deltaP - (Vector3)cOBB.R * cOBB.halfSizeR + (Vector3)cOBB.S * cOBB.halfSizeS,
                        deltaP - (Vector3)cOBB.R * cOBB.halfSizeR - (Vector3)cOBB.S * cOBB.halfSizeS
                    }) {
                        var r = Mathf.Min(a.frameTime, b.frameTime) * deltaV;

                        // go through each of d's sides
                        foreach (var side in new Vector3[][] {
                            new Vector3[] { dOBB.R * dOBB.halfSizeR + dOBB.S * dOBB.halfSizeS, 2f * dOBB.halfSizeS * -dOBB.S },
                            new Vector3[] { dOBB.R * dOBB.halfSizeR - dOBB.S * dOBB.halfSizeS, 2f * dOBB.halfSizeR * -dOBB.R },
                            new Vector3[] { -dOBB.R * dOBB.halfSizeR + dOBB.S * dOBB.halfSizeS, 2f * dOBB.halfSizeR * dOBB.R },
                            new Vector3[] { -dOBB.R * dOBB.halfSizeR - dOBB.S * dOBB.halfSizeS, 2f * dOBB.halfSizeS * dOBB.S }
                        }) {
                            var q = side[0];
                            var s = side[1];

                            if (first) {

                            }
                            // t = (q − p) × s / (r × s)
                            // u = (q − p) × r / (r × s)
                            var qmp = q - p;
                            var rxs = r.x * s.y - r.y * s.x;
                            // if rxs is 0, a's velocity is in the same direction as b's side.  skip it
                            if (Mathf.Approximately(rxs, 0f))
                                continue;
                            var rxsInv = 1f / rxs;
                            var t = (qmp.x * s.y - qmp.y * s.x) * rxsInv;
                            var u = (qmp.x * r.y - qmp.y * r.x) * rxsInv;
                            // check if the collision point is outside b's side
                            if (u < 0 || u > 1f)
                                continue;
                            // check if the collision time is outside this frame
                            if (t < 0 || t > 1f)
                                continue;
                            // keep track of the earliest collision
                            if (shortestT < 0 || t < shortestT) {
                                shortestT = t * Mathf.Min(a.frameTime, b.frameTime);
                                collisionPoint = dP + q + u * s;
                                aOnB = first;
                            }
                        }
                    }
                }
                // if no collision was found, skip
                if (shortestT < 0f)
                    continue;

                // advance both polygons to the collision point
                a.PhysicsSimulate(shortestT);
                b.PhysicsSimulate(shortestT);
                // bounce both off each other
                var n = (aOnB ? aP - bP : bP - aP).normalized;
                // calculate impact momentum
                // momentum = (2 * (aV . n - bV . n)) / (a.mass + b.mass)
                var momentum = (2f * (Vector3.Dot(a.v, n) - Vector3.Dot(b.v, n))) / (a.mass + b.mass);
                // change each's velocity according to that momentum
                a.v += -momentum * b.mass * n;
                b.v += momentum * a.mass * n;
            }

            a.PhysicsSimulate(a.frameTime);

            //bool colliding = false;
            //for (int j = 0; j < polys.Count; ++j) {
            //    var b = polys[j];
            //    if (a == b) continue;
            //    var aRT = a.GetComponent<RectTransform>();
            //    var bRT = b.GetComponent<RectTransform>();
            //    var aR = aRT.sizeDelta.x * 0.75f; // should be half width but not sure why this works better
            //    var bR = bRT.sizeDelta.x * 0.75f;
            //    float distSq = (aRT.position - bRT.position).sqrMagnitude;
            //    float radiiSq = aR * aR + bR * bR;
            //    colliding |= distSq <= radiiSq;
            //}
            //foreach (var segment in a.transform.GetComponentsInChildren<Image>())
            //    segment.color = colliding ? collidingColor : normalColor;
            //a.PhysicsSimulate(deltaTime);
        }
    }
}
