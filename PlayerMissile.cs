using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMissile : MonoBehaviour {

	float _speed = 8f;
	public GameObject[] P_kill = new GameObject[5];

	public AudioClip Sound_KillEnemy;
	public AudioClip Sound_BossKill;


	void Start () {
		for(int i=0; i<5; i++)
		{
			P_kill[i] = GameObject.Instantiate(Resources.Load("Enemy"+ (i+1).ToString() + "DieEffect")) as GameObject;
		}
	}
	
	void Update () {

		Rotate_P_Missile ();
		Move_P_Missile ();
		for(int i=0; i<5; i++)
		{
			P_kill[i].transform.position = gameObject.transform.position;
		}

	}

	void Rotate_P_Missile ()
	{
		gameObject.transform.Rotate (new Vector3 (450f * Time.deltaTime, 0f, 1000f * Time.deltaTime));
	}

	void Move_P_Missile()
	{
		gameObject.transform.position += new Vector3(0f, _speed * Time.deltaTime, 0f);
			
		if(gameObject.transform.position.y >= 5.5f)
		{
			GameObject.Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider obj)
	{
		if(obj.tag != "Player")
		{
			int monster_code = 0;
			if(obj.name == "Monster_1(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position);
				monster_code = 1;  P_kill[0].GetComponent<ParticleEmitter>().Emit();  }
			else if (obj.name == "Monster_2(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position);
				monster_code = 2;  P_kill[1].GetComponent<ParticleEmitter>().Emit();  }
			else if (obj.name == "Monster_3(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position);
				monster_code = 3;  P_kill[2].GetComponent<ParticleEmitter>().Emit();  }
			else if (obj.name == "Monster_4(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position);
				monster_code = 4;  P_kill[3].GetComponent<ParticleEmitter>().Emit();  }
			else if (obj.name == "Monster_5(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position);
				monster_code = 5;  P_kill[4].GetComponent<ParticleEmitter>().Emit();  }
			else if (obj.name == "Boss(Clone)")
			{  AudioSource.PlayClipAtPoint (Sound_BossKill, transform.position);
				monster_code = 6;  P_kill[4].GetComponent<ParticleEmitter>().Emit();  }
			else
			{ AudioSource.PlayClipAtPoint (Sound_KillEnemy, transform.position); }

			Camera.main.SendMessage("GetScore", monster_code);

			GameObject.Destroy (obj.gameObject);
			GameObject.Destroy (this.gameObject);
		}
	}
}
