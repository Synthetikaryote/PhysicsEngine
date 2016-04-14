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
