using UnityEngine;
using System.Collections;

public class RK4Ball : MonoBehaviour {
    Vector3 v = Vector3.zero;
    Vector3 p;
    const float oneSixth = 1f / 6f;

	// Use this for initialization
	void Start() {
        p = transform.position;
	}

    class Derivative {
        public float p = 0f;
        public float v = 0f;
        public Derivative() { }
        public Derivative(float p, float v) {
            this.p = p;
            this.v = v;
        }

    };
	
	void FixedUpdate() {
        // https://en.wikipedia.org/wiki/Runge%E2%80%93Kutta_methods
        float dt = Time.deltaTime;
        float hdt = dt * 0.5f;

        p = transform.position;
        var a = evaluate(0.0f, new Derivative());
        var b = evaluate(hdt, a);
        var c = evaluate(hdt, b);
        var d = evaluate(dt, c);

        float dpdt = oneSixth * (a.p + 2f * (b.p + c.p) + d.p);
        float dvdt = oneSixth * (a.v + 2f * (b.v + c.v) + d.v);

        p.y = p.y + dpdt * dt;
        v.y = v.y + dvdt * dt;

        float adjustedFloorY = Uber.I().floorY + ((RectTransform)transform).sizeDelta.y * 0.5f;
        if (p.y < adjustedFloorY) {
            p.y -= p.y - adjustedFloorY;
            v.y = -v.y;
        }

        transform.position = p;
	}

    Derivative evaluate(float dt, Derivative d) {
        return new Derivative(v.y + d.v * dt, -Uber.I().gravity);
    }
}
