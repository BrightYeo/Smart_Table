using UnityEngine;
using System.Collections;

public class EnemyMissile : MonoBehaviour {

	float _speed = 4f;

	public AudioClip Sound_GetCoffee;
	public AudioClip Sound_Destroy_player;

	void Start () {
		_speed = GameMain.e_missile_speed;
	}
	

	void Update () {
		if(GameMain.b_start)
		{
			Move_E_Missile ();
			Rotate_E_Missile ();
		}
	}

	void Rotate_E_Missile ()
	{
		gameObject.transform.Rotate (new Vector3 (0f, 0f, 500f * Time.deltaTime));
	}

	void Move_E_Missile()
	{
		gameObject.transform.position -= new Vector3(0f, _speed * Time.deltaTime, 0f);
		
		if(gameObject.transform.position.y <= -5.5f)
		{
			GameObject.Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider obj)
	{
		if(obj.tag == "Player")
		{
			if(gameObject.name == ("Monster_1(Clone)_Missile(Clone)"))
			{
				AudioSource.PlayClipAtPoint (Sound_GetCoffee, transform.position);
				GameObject.Destroy (this.gameObject);
				Camera.main.SendMessage("AddCoffeeCount", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				AudioSource.PlayClipAtPoint (Sound_Destroy_player, transform.position);
				GameObject.Destroy (this.gameObject);
				Camera.main.SendMessage("PlayerDeath", SendMessageOptions.DontRequireReceiver);
			}
		}
		else if(obj.tag == "PlayerMissile")
		{

		}
	}
}
