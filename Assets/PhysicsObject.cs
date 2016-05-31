using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsObject : MonoBehaviour {
    const float oneSixth = 1f / 6f;

    public Vector3 force = Vector3.zero;
    public Vector3 v = Vector3.zero;
    Vector3 p = Vector3.zero;
    public float mass = 10f;

    public enum NumericalIntegrationMethod {
        EulerExplicit,
        EulerSemiImplicit,
        Verlet,
        RK4
    };
    public NumericalIntegrationMethod integrationMethod = NumericalIntegrationMethod.RK4;

	public virtual void Start() {
        Uber.I().PhysicsInit += PhysicsInit;
        Uber.I().PhysicsSolve += PhysicsSolve;
        //Uber.I().PhysicsSimulate += PhysicsSimulate;

        p = transform.position;
        v = Vector3.zero;
    }

    public virtual void ApplyForce(Vector3 force) {
        this.force += force;
    }

    public virtual void PhysicsInit() {
        force = Vector3.zero;
    }

    public virtual void PhysicsSolve() {
        
    }

    public virtual void PhysicsSimulate(float deltaTime) {
        switch(integrationMethod) {
            case NumericalIntegrationMethod.EulerExplicit: {
                    v += (force / mass) * deltaTime;
                    transform.position += v * deltaTime;
                }
                break;

            case NumericalIntegrationMethod.EulerSemiImplicit: {
                    v += (force / mass) * deltaTime;
                    p = transform.position;
                    p += v * deltaTime;
                    float bottomY = p.y - ((RectTransform)transform).sizeDelta.y * 0.5f;
                    if (bottomY < Uber.I().floorY) {
                        p.y -= bottomY - Uber.I().floorY;
                        v.y = -v.y;
                    }
                    transform.position = p;
                }
                break;

            case NumericalIntegrationMethod.Verlet: {
                    // use p as the position one frame ago
                    Vector3 newP = transform.position;
                    newP += (newP - p) + (force / mass) * deltaTime * deltaTime;
                    p = transform.position;
                    transform.position = newP;
                }
                break;

            case NumericalIntegrationMethod.RK4: {
                    // https://en.wikipedia.org/wiki/Runge%E2%80%93Kutta_methods
                    float dt = Time.deltaTime;
                    float hdt = dt * 0.5f;

                    p = transform.position;
                    var a = RK4Evaluate(0.0f, new RK4Derivative());
                    var b = RK4Evaluate(hdt, a);
                    var c = RK4Evaluate(hdt, b);
                    var d = RK4Evaluate(dt, c);

                    Vector3 dpdt = oneSixth * (a.p + 2f * (b.p + c.p) + d.p);
                    Vector3 dvdt = oneSixth * (a.v + 2f * (b.v + c.v) + d.v);

                    p = p + dpdt * dt;
                    v = v + dvdt * dt;
                    transform.position = p;
                }
                break;
        }
    }

    class RK4Derivative {
        public Vector3 p = Vector3.zero;
        public Vector3 v = Vector3.zero;
        public RK4Derivative() { }
        public RK4Derivative(Vector3 p, Vector3 v) {
            this.p = p;
            this.v = v;
        }
    };
    RK4Derivative RK4Evaluate(float dt, RK4Derivative d) {
        return new RK4Derivative(v + d.v * dt, (force / mass));
    }
}
