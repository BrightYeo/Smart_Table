using UnityEngine;
using System.Collections;

public class BossControl : MonoBehaviour {

	private float speed = 2f;

	void Start () {
	
	}
	

	void Update () {
		MoveBoss();
	}

	void MoveBoss()
	{
		if(gameObject.transform.position.x <= 6.0f)
			gameObject.transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
		else
			Destroy (gameObject);
	}
}
