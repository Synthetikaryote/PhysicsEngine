using UnityEngine;
using System.Collections;

public class Spring : PhysicsObject {

    public PhysicsObject mass1, mass2;
    public float springConst;
    public float springLength;
    public float friction;

    public Spring(PhysicsObject mass1, PhysicsObject mass2, float k, float l, float f) {
        this.mass1 = mass1;
        this.mass2 = mass2;
        this.springConst = k;
        this.springLength = l;
        this.friction = f;
    }

    void Update() {
        var p1 = mass1.transform.position;
        var p2 = mass2.transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg + 90f);
        transform.position = (p1 + p2) * 0.5f;
        var rt = (RectTransform)transform;
        float length = (p1 - p2).magnitude;
        float stretchFactor = Mathf.Min(1f, springLength / length);
        rt.sizeDelta = new Vector2(2f + 4f * stretchFactor, length);
    }

    public override void PhysicsSolve() {
        var delta = mass1.transform.position - mass2.transform.position;
        float currentLength = delta.magnitude;
        var force = Vector3.zero;
        if (currentLength != 0f)
            force += delta / currentLength * (currentLength - springLength) * -springConst;
        force += -(mass1.v - mass2.v) * friction;

        mass1.ApplyForce(force);
        mass2.ApplyForce(-force);
    }
}
