using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollisionSimulation : MonoBehaviour {
    public GameObject massPrefab;
    public GameObject segmentPrefab;

    public float groundRepulsion = 100f;
    public float groundAbsorption = 0.1f;
    public float airFrictionConst = 0.02f;
    public float ballSizeMin = 24f;
    public float ballSizeMax = 64f;
    public Color normalColor;
    public Color collidingColor;
    Canvas canvas;
    List<PhysicsObject> masses;
    List<Segment> segments;
    public int numBalls = 10;
    public PhysicsObject.NumericalIntegrationMethod integrationMethod = PhysicsObject.NumericalIntegrationMethod.RK4;
    bool startVelocityApplied = false;

    void Awake() {
        // get the canvas
        canvas = GameObject.FindObjectOfType<Canvas>().gameObject.GetComponent<Canvas>();

        // create some balls
        masses = new List<PhysicsObject>();
        for (int i = 0; i < numBalls; ++i) {
            var mass = Instantiate(massPrefab, new Vector3(Random.Range(-300f, 300f), Random.Range(-300f, 300f)), Quaternion.identity) as GameObject;
            mass.transform.SetParent(canvas.transform);
            mass.transform.localScale = new Vector3(1f, 1f, 1f);
            var obj = mass.GetComponent<PhysicsObject>();
            obj.mass = 1f;
            obj.integrationMethod = integrationMethod;
            var r = Random.Range(ballSizeMin, ballSizeMax);
            ((RectTransform)obj.transform).sizeDelta = new Vector2(r, r);
            masses.Add(obj);
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
            foreach (var ball in masses) {
                float a = Random.Range(0f, Mathf.PI * 2f);
                ball.ApplyForce(1000f * new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
            }
        }

        foreach (var ball in masses) {
            // gravity
            //obj.ApplyForce(new Vector3(0f, Uber.I().gravity * obj.mass, 0f));

            // air friction
            //ball.ApplyForce(-ball.v * airFrictionConst);

        }
    }

    void PhysicsSimulate(float deltaTime) {
        for (int i = 0; i < masses.Count; ++i) {
            //bool colliding = false;
            var a = masses[i];

            // check screen edges
            foreach (var segment in segments)
            {
                var p = a.transform.position;
                // early out if the ball's not heading toward the wall at all
                var vDotN = Vector3.Dot(a.v, segment.normal);
                if (vDotN < 0) {
                    // find the time to collide
                    var r = a.GetComponent<RectTransform>().sizeDelta.x * 0.5f;
                    // t = (r - D - n . p) / (n . v)
                    var t = (r - segment.D - Vector3.Dot(segment.normal, p)) / vDotN;
                    // check if the collision will be this frame
                    if (t < deltaTime) {
                        // advance that sphere to the collision point
                        a.PhysicsSimulate(t);
                        // bounce off the wall
                        a.v = a.v - 2f * vDotN * segment.normal;
                        // advance the sphere the rest of the time
                        a.PhysicsSimulate(deltaTime - t);
                    } else {
                        // advance it the full time
                        a.PhysicsSimulate(deltaTime);
                    }
                } else {
                    // advance it the full time
                    a.PhysicsSimulate(deltaTime);
                }
            }

            //for (int j = 0; j < masses.Count; ++j) {
            //    var b = masses[j];
            //    if (a == b) continue;
            //    var aRT = a.GetComponent<RectTransform>();
            //    var bRT = b.GetComponent<RectTransform>();
            //    var aR = aRT.sizeDelta.x * 0.75f; // should be half width but not sure why this works better
            //    var bR = bRT.sizeDelta.x * 0.75f;
            //    float distSq = (aRT.position - bRT.position).sqrMagnitude;
            //    float radiiSq = aR * aR + bR * bR;
            //    colliding |= distSq <= radiiSq;
            //}
            //a.GetComponent<Image>().color = colliding ? collidingColor : normalColor;
        }
    }


}
