using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour {

	public GameObject Eye;

	// kind of monster
	string missile_Name;

	int shoot_time = 0;

	int old_tick_count = 0;
	int old_tick_count2 = 0;
	int new_tick_count = 0;

	int m_term_1 = 0; //missile term (random between term_1 and term_2)
	int m_term_2 = 0;

	bool _start = false;
	bool _canShoot = false;
	bool _eye_dir = false; // left

	// Use this for initialization
	void Start () {
		m_term_1 = GameMain.e_missile_term[0];
		m_term_2 = GameMain.e_missile_term[1];
	}

	void Enemy_Init()
	{
		missile_Name = gameObject.name + "_Missile";
		
		shoot_time = CalculateRandomValue ();
		
		old_tick_count = System.Environment.TickCount;
		new_tick_count = old_tick_count;
	}

	// only bottom line can shoot 
	void RayCasting()
	{
		RaycastHit hit;
		if(Physics.Raycast(transform.position, Vector3.down, out hit, 20f))
		{
			if(hit.transform.tag == "temp" || hit.transform.tag == "Player")
				_canShoot = true;
			else if(hit.transform.tag == "Enemy")
				_canShoot = false;
		}
	}
	
	void Update () 
	{
		RayCasting();

		if(GameMain.b_start)
		{
			if(!_start)
			{
				Enemy_Init();
				_start = true;
			}
			else
			{
				//MoveEye();

				new_tick_count = System.Environment.TickCount;
				
				// do each second
				if(old_tick_count <= new_tick_count - shoot_time)
				{
					EnemyShoot();
					old_tick_count = new_tick_count;
				}

				/*
				if(old_tick_count2 <= new_tick_count - (shoot_time*2))
				{
					_eye_dir = !_eye_dir;
					old_tick_count2 = new_tick_count;
				}
				*/
			}
		}
		else
		{
			old_tick_count = System.Environment.TickCount;
			//old_tick_count2 = System.Environment.TickCount;
		}
	}

	void MoveEye()
	{
		if(!_eye_dir) // left
		{
			if(Eye.transform.position.x >= -0.022f)
				Eye.transform.position -= new Vector3(0.1f * Time.deltaTime, 0f, 0f);
		}
		else
		{
			if(Eye.transform.position.x <= 0.022f)
				Eye.transform.position -= new Vector3(0.1f * Time.deltaTime, 0f, 0f);
		}
	}

	int CalculateRandomValue()
	{
		int temp_1 = (int)gameObject.transform.position.x * 1000 + (int)gameObject.transform.position.y * 1000 +
						System.Environment.TickCount;

		System.Random rnd = new System.Random (temp_1);

		// 딱 한 놈씩 랜덤한 타임에 쏘는게 좋을 것 같음..
		int value = rnd.Next (m_term_1, m_term_2);

		return value;
	}


	void EnemyShoot()
	{
		if(_canShoot)
			Instantiate (Resources.Load (missile_Name), gameObject.transform.position, Quaternion.identity);
	}

	void OnTriggerEnter(Collider obj)
	{
		if(obj.tag == "Player")
		{
			GameObject.Destroy (this.gameObject);
			Camera.main.SendMessage("PlayerDeath", SendMessageOptions.DontRequireReceiver);
		}
	}
}
