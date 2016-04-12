using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Main : MonoBehaviour {

    static Main instance = null;
    public static Main I() {
        if (instance == null)
            instance = Camera.main.GetComponent<Main>();
        return instance;
    }

    public float floorY = 0f;
    [NonSerialized]
    public float gravity = 500f;

	// Use this for initialization
	void Start () {
        var floorGO = GameObject.Find("Floor");
        floorY = floorGO.transform.position.y + ((RectTransform)floorGO.transform).sizeDelta.y * 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
