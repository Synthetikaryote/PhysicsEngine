using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsObject : MonoBehaviour {

    public Vector3 force = Vector3.zero;
    public Vector3 v = Vector3.zero;
    public float mass = 10f;

    public enum NumericalIntegrationMethod {
        EulerExplicit,
        EulerSemiImplicit,
        Verlet,
        RK4
    };
    public NumericalIntegrationMethod integrationMethod = NumericalIntegrationMethod.EulerExplicit;

	// Use this for initialization
	void Start() {
        Uber.I().PhysicsInit += PhysicsInit;
        Uber.I().PhysicsSolve += PhysicsSolve;
        Uber.I().PhysicsSimulate += PhysicsSimulate;
    }

    public virtual void ApplyForce(Vector3 force) {
        this.force += force;
    }

    public virtual void PhysicsInit() {
        force = Vector3.zero;
    }

    public virtual void PhysicsSolve() {
        
    }

    public virtual void PhysicsSimulate() {
        switch(integrationMethod) {
            case NumericalIntegrationMethod.EulerExplicit:
                v += (force / mass) * Time.deltaTime;
                transform.position += v * Time.deltaTime;
                break;
            case NumericalIntegrationMethod.EulerSemiImplicit:
                break;
            case NumericalIntegrationMethod.Verlet:
                break;
            case NumericalIntegrationMethod.RK4:
                break;
        }
    }
}
