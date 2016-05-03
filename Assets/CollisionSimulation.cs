using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollisionSimulation : MonoBehaviour {
    public GameObject massPrefab;

    public float groundRepulsion = 100f;
    public float groundAbsorption = 0.1f;
    public float airFrictionConst = 0.02f;
    public float ballSizeMin = 24f;
    public float ballSizeMax = 64f;
    public Color normalColor;
    public Color collidingColor;
    Canvas canvas;
    List<PhysicsObject> masses;
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
                ball.ApplyForce(8000f * new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
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
        if (dist > 0f) {
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

    void PhysicsSimulate() {
        for (int i = 0; i < Uber.I().objects.Count; ++i) {
            bool colliding = false;
            var a = Uber.I().objects[i];
            for (int j = 0; j < Uber.I().objects.Count; ++j) {
                var b = Uber.I().objects[j];
                if (a == b) continue;
                var aRT = a.GetComponent<RectTransform>();
                var bRT = b.GetComponent<RectTransform>();
                var aR = aRT.sizeDelta.x * 0.75f; // should be half width but not sure why this works better
                var bR = bRT.sizeDelta.x * 0.75f;
                float distSq = (aRT.position - bRT.position).sqrMagnitude;
                float radiiSq = aR * aR + bR * bR;
                colliding |= distSq <= radiiSq;
            }
            a.GetComponent<Image>().color = colliding ? collidingColor : normalColor;
        }
    }
}
