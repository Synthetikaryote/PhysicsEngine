using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsObject : MonoBehaviour {

    public Vector3 force = Vector3.zero;
    public Vector3 v = Vector3.zero;
    public float mass = 10f;
    public List<Vector3> forces;

    public enum NumericalIntegrationMethod {
        EulerExplicit,
        EulerSemiImplicit,
        Verlet,
        RK4
    };
    public NumericalIntegrationMethod integrationMethod = NumericalIntegrationMethod.EulerExplicit;

	// Use this for initialization
	void Start() {

	}

    public virtual void PhysicsInit() {
        forces.Clear();
    }

    public virtual void ApplyForce(Vector3 force) {
        forces.Add(force);
    }

    public virtual void PhysicsSolve() {
        
    }

    public virtual void PhysicsUpdate() {
        switch(integrationMethod) {
            case NumericalIntegrationMethod.EulerExplicit:
                {

                }
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
