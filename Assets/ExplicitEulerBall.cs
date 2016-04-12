using UnityEngine;
using System.Collections;

public class ExplicitEulerBall : PhysicsObject {

    Vector3 v = Vector3.zero;

	// Use this for initialization
	void Start() {

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        var p = transform.position;
        p.y += v.y * Time.deltaTime;
        float bottomY = p.y - ((RectTransform)transform).sizeDelta.y * 0.5f;
        if (bottomY < Main.I().floorY) {
            p.y -= bottomY - Main.I().floorY;
            v.y = -v.y;
        }
        transform.position = p;
        v.y -= Main.I().gravity * Time.deltaTime;
	}
}
