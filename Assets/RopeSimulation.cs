using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RopeSimulation : MonoBehaviour {
    public GameObject massPrefab;
    public GameObject springPrefab;

    public Vector3 ropeConnectionP = Vector3.zero;
    public Vector3 ropeConnectionV = Vector3.zero;

    public float groundRepulsion = 100f;
    public float groundFriction = 0.1f;
    public float groundAbsorption = 0.1f;
    public float connectionMass = 0.1f;
    public float springLength = 50f;
    public float springConst = 0.1f;
    public float springFriction = 0.1f;
    public float airFrictionConst = 0.02f;
    public int segments = 10;

    PhysicsObject ropeTop;
    Canvas canvas;

    List<Spring> springs;

    void Awake() {
        // get the canvas
        canvas = GameObject.FindObjectOfType<Canvas>().gameObject.GetComponent<Canvas>();
        // create the rope
        // masses
        var masses = new List<PhysicsObject>();
        for (int i = 0; i < segments; ++i) {
            var startPos = ropeConnectionP;
            var mass = Instantiate(massPrefab, startPos + new Vector3(0f, -i * springLength, 0f), Quaternion.identity) as GameObject;
            mass.transform.SetParent(canvas.transform);
            mass.transform.localScale = new Vector3(1f, 1f, 1f);
            var obj = mass.GetComponent<PhysicsObject>();
            obj.mass = connectionMass;
            masses.Add(obj);
            if (i == 0) {
                ropeTop = obj;
            }
        }
        // springs to connect the masses
        springs = new List<Spring>();
        for (int i = 0; i < segments - 1; ++i) {
            var springGO = Instantiate(springPrefab);
            springGO.transform.SetParent(canvas.transform);
            springGO.transform.localScale = new Vector3(1f, 1f, 1f);
            var spring = springGO.GetComponent<Spring>();
            spring.mass1 = masses[i];
            spring.mass2 = masses[i + 1];
            spring.springConst = springConst;
            spring.springLength = springLength;
            spring.friction = springFriction;
            springs.Add(spring);
        }
    }

	// Use this for initialization
	void Start () {
        Uber.I().PhysicsSolve += PhysicsSolve;
        Uber.I().PhysicsSimulate += PhysicsSimulate;
    }

    void PhysicsSolve() {
        foreach (var obj in Uber.I().objects) {
            // gravity
            obj.ApplyForce(new Vector3(0f, Uber.I().gravity * obj.mass, 0f));

            // air friction
            obj.ApplyForce(-obj.v * airFrictionConst);

            if (obj.transform.position.y < Uber.I().floorY) {
                // apply ground absorption considering only the x-z
                var v = obj.v;
                v.y = 0f;
                obj.ApplyForce(-v * groundAbsorption);

                // apply ground repulsion considering only the y
                var repulForce = new Vector3(0f, groundRepulsion, 0) * (Uber.I().floorY - obj.transform.position.y);
                obj.ApplyForce(repulForce);
            }
        }
    }

    void PhysicsSimulate() {
        ropeTop.transform.position = new Vector3(canvas.pixelRect.size.x / 2 + ropeConnectionP.x, canvas.pixelRect.size.y / 2 + ropeConnectionP.y, 0f);
        ropeTop.v = Vector3.zero;
    }
}