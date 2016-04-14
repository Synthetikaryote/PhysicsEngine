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
    public float gravity = 500f;
    [NonSerialized]
    public float airFriction = 0.01f;
    public List<PhysicsObject> objects;

    // Use this for initialization
    void Start () {
        var floorGO = GameObject.Find("Floor");
        if (floorGO)
            floorY = floorGO.transform.position.y + ((RectTransform)floorGO.transform).sizeDelta.y * 0.5f;
        objects = new List <PhysicsObject>(GameObject.FindObjectsOfType<PhysicsObject>());
    }

    void FixedUpdate() {
    }

    public void PhysicsUpdate() {
        objects.ForEach(o => o.PhysicsInit());
        objects.ForEach(o => o.PhysicsSolve());
        objects.ForEach(o => o.PhysicsUpdate());
    }
}
