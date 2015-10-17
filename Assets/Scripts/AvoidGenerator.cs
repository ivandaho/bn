using UnityEngine;
using System.Collections;

public class AvoidGenerator : MonoBehaviour {

    public GameObject obj;
    public float interval = 1f; // interval is 1 second
    public float spd;
    
    float timeToNext;
    float startTime;

	// Use this for initialization
	void Start () {
        startTime = Time.time + interval; //startTime is for countdown
        // starttime = runtime + interval
	
	}
	
	// Update is called once per frame
	void Update () {
        timeToNext = startTime - Time.time;
        if (timeToNext < 0) {
            Instantiate(obj, transform.position, Quaternion.identity);

            startTime = Time.time + interval;
            System.Random ran = new System.Random();
            int newInterval = ran.Next(0,150);
            interval = (float)newInterval/100f;
        }
	}
}
