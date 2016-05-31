using UnityEngine;
using System.Collections;

public class Segment : MonoBehaviour {

    public Vector3 normal;

    public float D
    {
        get {
            return -(normal.x * transform.position.x +
                    normal.y * transform.position.y +
                    normal.z * transform.position.z);
        }
    }

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
	
	}
}
