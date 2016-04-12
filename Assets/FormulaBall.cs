using UnityEngine;
using System.Collections;

public class FormulaBall : PhysicsObject {
    Vector3 start;
    float h = 0f;
    float tMod = 0f;

	// Use this for initialization
	void Start () {
        start = transform.position;
        h = start.y - ((RectTransform)transform).sizeDelta.y * 0.5f - Main.I().floorY;
        tMod = Mathf.Sqrt(2f * h / Main.I().gravity) * 2f;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        // h = h0 - 0.5 * gt^2
        // https://www.physicsforums.com/threads/bouncing-ball-equation.403229/
        float t = (Mathf.Repeat(Time.time - tMod * 0.5f, tMod) - tMod * 0.5f);
        float s = -h + h - 0.5f * Main.I().gravity * t * t;
        transform.position = new Vector3(start.x, start.y + s, start.z);
	}
}
