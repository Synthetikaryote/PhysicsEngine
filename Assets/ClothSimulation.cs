using UnityEngine;
using System;
using System.Collections.Generic;

public class ClothSimulation : MonoBehaviour {
    public GameObject massPrefab;
    public GameObject springPrefab;

    public Vector3 ropeConnectionP = Vector3.zero;
    public Vector3 ropeConnectionV = Vector3.zero;

    public float groundRepulsion = 100f;
    public float groundAbsorption = 0.1f;
    public float connectionMass = 0.1f;
    public float springLength = 50f;
    public float springConst = 0.1f;
    public float springFriction = 0.1f;
    public float airFrictionConst = 0.02f;
    public int rows = 10;
    public int columns = 10;
    public PhysicsObject.NumericalIntegrationMethod integrationMethod = PhysicsObject.NumericalIntegrationMethod.RK4;

    List<PhysicsObject> masses;
    List<Spring> springs;
    Canvas canvas;

    void Awake() {
        // get the canvas
        canvas = GameObject.FindObjectOfType<Canvas>().gameObject.GetComponent<Canvas>();
        // create the cloth
        // masses
        masses = new List<PhysicsObject>();
        for (int r = 0; r < rows; ++r) {
            for (int c = 0; c < columns; ++c) {
                var startPos = ropeConnectionP;
                var mass = Instantiate(massPrefab, startPos + new Vector3(-columns * springLength * 0.5f + c * springLength, -r * springLength, 0f), Quaternion.identity) as GameObject;
                mass.transform.SetParent(canvas.transform);
                mass.transform.localScale = new Vector3(1f, 1f, 1f);
                var obj = mass.GetComponent<PhysicsObject>();
                obj.mass = connectionMass;
                obj.integrationMethod = integrationMethod;
                masses.Add(obj);
            }
        }
        // springs to connect the masses
        springs = new List<Spring>();
        float diagonalSpringLength = springLength * Mathf.Sqrt(2f);
        Action<PhysicsObject, PhysicsObject, float> addSpring = (mass1, mass2, length) => {
            var springGO = Instantiate(springPrefab);
            springGO.transform.SetParent(canvas.transform);
            springGO.transform.localScale = new Vector3(1f, 1f, 1f);
            var spring = springGO.GetComponent<Spring>();
            spring.mass1 = mass1;
            spring.mass2 = mass2;
            spring.springConst = springConst;
            spring.springLength = length;
            spring.friction = springFriction;
            springs.Add(spring);
        };
        for (int r = 0; r < rows; ++r) {
            for (int c = 0; c < columns; ++c) {
                if (c < columns - 1) addSpring(masses[r * columns + c], masses[r * columns + c + 1], springLength);
                if (r < rows - 1) addSpring(masses[r * columns + c], masses[(r + 1) * columns + c], springLength);
                if (c < columns - 1 && r < rows - 1) {
                    addSpring(masses[r * columns + c], masses[(r + 1) * columns + c + 1], diagonalSpringLength);
                    addSpring(masses[r * columns + c + 1], masses[(r + 1) * columns + c], diagonalSpringLength);
                }
            }
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
        for (int c = 0; c < columns; ++c) {
            // all first row
            masses[c].transform.position = new Vector3(canvas.pixelRect.size.x / 2 + ropeConnectionP.x - columns * springLength * 0.5f + c * springLength, canvas.pixelRect.size.y / 2 + ropeConnectionP.y, 0f);
            masses[c].v = Vector3.zero;
        }
    }
}