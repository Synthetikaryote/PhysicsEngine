using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopeSimulation : MonoBehaviour {
    public Vector3 ropeConnectionP = Vector3.zero;
    public Vector3 ropeConnectionV = Vector3.zero;

    public float groundRepulsion = 0.1f;
    public float groundFriction = 0.1f;
    public float groundAbsorption = 0.1f;
    public float springLength = 10f;
    public float springConst = 0.1f;
    public float springFriction = 0.1f;

    void Awake() {
        // create the rope
        int segments = 4;
        // masses
        var masses = new List<PhysicsObject>();
        var massPrefab = Resources.Load("Mass") as GameObject;
        for (int i = 0; i < segments; ++i) {
            var mass = Instantiate(massPrefab);
            var obj = mass.GetComponent<PhysicsObject>();
            obj.mass = 10f;
            obj.transform.position = new Vector3(i * springLength, 0f, 0f);
            masses.Add(obj);
        }
        // springs to connect the masses
        var springPrefab = Resources.Load("Spring") as GameObject;
        for (int i = 0; i < segments - 1; ++i) {
            var springGO = Instantiate(springPrefab);
            var spring = springGO.GetComponent<Spring>();
            spring.mass1 = masses[i];
            spring.mass2 = masses[i + 1];
            spring.springConst = springConst;
            spring.springLength = springLength;
            spring.friction = springFriction;
        }
    }

	// Use this for initialization
	void Start () {
	    
	}
	
	void FixedUpdate () {
        foreach(var obj in Uber.I().objects) {
            ropeConnectionP += ropeConnectionV * Time.deltaTime;
            if (ropeConnectionP.y < Uber.I().floorY) {
                ropeConnectionP.y = Uber.I().floorY;
                ropeConnectionV.y = 0f;
            }
            Uber.I().objects[0].transform.position = ropeConnectionP;
            Uber.I().objects[0].v = ropeConnectionV;
        }
        Uber.I().PhysicsUpdate();
	}
}