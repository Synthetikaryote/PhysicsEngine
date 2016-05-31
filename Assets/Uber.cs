using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Uber : MonoBehaviour {

    static Uber instance = null;
    public static Uber I() {
        if (instance == null)
            instance = Camera.main.GetComponent<Uber>();
        return instance;
    }

    public float floorY = 0f;
    [NonSerialized]
    public float gravity = -300f;
    [NonSerialized]
    public float airFriction = 0.01f;
    public List<PhysicsObject> objects;

    public delegate void PhysicsInitDelegate();
    public PhysicsInitDelegate PhysicsInit;
    public delegate void PhysicsSolveDelegate();
    public PhysicsSolveDelegate PhysicsSolve;
    public delegate void PhysicsSimulateDelegate(float deltaTime);
    public PhysicsSimulateDelegate PhysicsSimulate;

    void Start () {
        var floorGO = GameObject.Find("Floor");
        if (floorGO)
            floorY = floorGO.transform.position.y + ((RectTransform)floorGO.transform).sizeDelta.y * 0.5f;
        objects = new List<PhysicsObject>(FindObjectsOfType<PhysicsObject>());
    }

    void FixedUpdate() {
        PhysicsInit();
        PhysicsSolve();
        PhysicsSimulate(Time.deltaTime);
    }
}
