using UnityEngine;
using System.Collections;

public class VerletBall : PhysicsObject
{

    Vector3 lastPos;

    // Use this for initialization
    void Start() {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 p = transform.position;
        float adjustedFloorY = Main.I().floorY + ((RectTransform)transform).sizeDelta.y * 0.5f;
        if (p.y < adjustedFloorY) {
            float dy = p.y - lastPos.y;
            p.y = adjustedFloorY + (adjustedFloorY - p.y);
            lastPos.y = p.y - dy;
        }
        p.y += (p.y - lastPos.y) - Main.I().gravity * Time.deltaTime * Time.deltaTime;
        lastPos = transform.position;
        transform.position = p;
    }
}
