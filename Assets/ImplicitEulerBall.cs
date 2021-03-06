﻿using UnityEngine;
using System.Collections;

public class ImplicitEulerBall : PhysicsObject {

    Vector3 v = Vector3.zero;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        v.y -= Uber.I().gravity * Time.deltaTime;
        var p = transform.position;
        p.y += v.y * Time.deltaTime;
        float bottomY = p.y - ((RectTransform)transform).sizeDelta.y * 0.5f;
        if (bottomY < Uber.I().floorY) {
            p.y -= bottomY - Uber.I().floorY;
            v.y = -v.y;
        }
        transform.position = p;
    }
}
