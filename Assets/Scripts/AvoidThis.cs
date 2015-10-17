using UnityEngine;
using System.Collections;

public class AvoidThis : MonoBehaviour {

    public int speed = 10;
    public float maxDist = 25;
    float initialY;

	// Use this for initialization
	void Start () {
        initialY = transform.position.y;
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(transform.position.x,
                transform.position.y - speed * Time.deltaTime*2,
                transform.position.z);
        /*
        if(transform.position.y < initialY - 25) {
            transform.position = new Vector3(transform.position.x,
                initialY, transform.position.z);
        }
    */
	}
    
    void LateUpdate() {
        if (transform.position.y <initialY - maxDist) {
            Destroy  (this.gameObject);
        }
    }
}
