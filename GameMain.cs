using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Leap;

public class GameMain : MonoBehaviour {

	public GUIText Text_BestScore;
	public GUIText Text_Score;
	public GUIText Text_Stage;

	// difficulty
	public static int[] e_missile_term = new int[2];
	public static float e_missile_speed = 4; // default 4

	// false when showing intro or messages
	public static bool b_start = false;
	private int g_pause_num = 1;

	// try again
	public static bool b_try_again = false;
	public static bool b_global_signal = false;

	// about intro
	public GameObject Intro;
	public static bool b_pass = false;
	private float intro_speed = 18f;

	// temp time stop
	private int stop_time = 0;

	// get ready and game over
	public GameObject msg_GetReady;
	public GameObject msg_GameOver;
	public GameObject msg_StageClear;
	public GameObject msg_TryAgain;
	private float msg_speed = 28f;

	public GameObject Player;
	public ParticleEmitter P_Coffee_Particle;

	// collect coffee
	public GameObject[] Stored_Coffee = new GameObject[5];
	private int coffee_count = 0;

	// life
	public GameObject[] Life_Object = new GameObject[2];
	private int life_count = 2;

	// stage text object
	public GameObject Stage_Object;

	// texts on board
	public GUIText ScoreText;
	public GUIText BestScoreText;
	private int g_stage = 1;
	private int g_score = 0;
	private int g_bestScore = 0;

	// enemy character
	public GameObject Monsters;
	private List<GameObject> l_enemy = new List<GameObject>();
	private float enemy_Speed = 0.4f;

	// control enemy
	private int enemy_direction = 1;  //1 = right   2 = left   3 = right - down   4 = left - down
	private float g_yPos = 0f;

	// tick count
	private int tick_count = 0;
	private int new_tick_count = 0;

	// best record (and file IO)
	private string path = Directory.GetCurrentDirectory() + "\\record.data";
	private int bestRecord = 0;

	// player textures
	public Texture2D[] player_texture = new Texture2D[5];

	private bool doFixedUpdate = false;
	private int move_num = 0;

	public GameObject AlphaBoard;
	public GameObject text_ask;
	public GameObject btn_yes_1;
	public GameObject btn_yes_2;
	public GameObject btn_no_1;
	public GameObject btn_no_2;

	// Audio
	public AudioClip Boss_Appear;
	public AudioClip GameOver;
	public AudioClip Introduce;
	public AudioClip LevelClear;


	void Start ()
	{
		doFixedUpdate = true;
		move_num = 1;
	}

	private void InitAll()
	{
		// load best record
		ReadRecord();
		
		e_missile_term[0] = 3000;
		e_missile_term[1] = 12000;
		e_missile_speed = 4f;
		
		tick_count = System.Environment.TickCount;
		old_timer = tick_count;
		
		//create player object
		Instantiate(Player, new Vector3(-1.9f, -4.2f, -10f), Quaternion.identity);
		
		Text_Stage.text = "Stage " + g_stage.ToString();
		
		MakeEnemy();
	}

	int new_timer, old_timer;

	public static bool b_main_leap_control = false;
	public static bool b_start_once = false;

	// leap
	Frame newFrame = new Frame();
	Controller leap_Controller;

	void InitLeap()
	{
		b_start_once = true;
		Debug.Log ("GameMain InitLeap()");

		leap_Controller = new Leap.Controller();
		leap_Controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
		leap_Controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);

		doFixedUpdate = true;
		move_num = 2;
	}

	private int yes_no = 0;
	private bool b_intro_sound = false;

	void Update () 
	{
		if(b_main_leap_control)
		{
			if(!b_start_once)
				InitLeap();

			if(leap_Controller.Devices[0].IsValid == true)
			{
				Frame oldFrame = newFrame == null ? Frame.Invalid : newFrame;
				newFrame = leap_Controller.Frame();
				
				if(oldFrame.Id != newFrame.Id)
				{
					bool set_swipe = false;
					Vector3 swipe_start = new Vector3();

					int keytap_count = 0;

					foreach (Gesture g in newFrame.Gestures(oldFrame))
					{
						//SwipeGesture aa=newFrame.Gestures(oldFrame);
						SwipeGesture swipe = new SwipeGesture(g);
						
						// filter invalid events
						if (!g.IsValid || (g.Type == Gesture.GestureType.TYPE_INVALID))
							continue;
						// process valid events based on types
						switch (g.Type) 
						{
						case Gesture.GestureType.TYPE_KEY_TAP:

							Debug.Log("key_tapped");
							keytap_count++;
							if(keytap_count >= 3)
							{
								if(yes_no == 1)
								{
									Application.LoadLevel("OrderScene");
									b_main_leap_control = false;
									b_start_once = false;
								}
								else if(yes_no == 2)
								{
									doFixedUpdate = true;
									b_main_leap_control = false;
									b_start_once = false;
									leap_Controller = null;
									move_num = 3;

									text_ask.SetActive(false);
									btn_yes_1.SetActive(false);
									btn_yes_2.SetActive(false);
									btn_no_1.SetActive(false);
									btn_no_2.SetActive(false);

								}
								keytap_count = 0;
								yes_no = 0;
							}

							break;
							
							// process swipes (continuous events)
						case Gesture.GestureType.TYPE_SWIPE:
							switch (g.State)
							{
							case Gesture.GestureState.STATE_START:
								swipe_start = new Vector3(swipe.StartPosition.x, swipe.StartPosition.y, swipe.StartPosition.z);
								//swipe_start = swipe.StartPosition.ToUnity();
								
								break;
								
							case Gesture.GestureState.STATE_UPDATE: 
								
								//Debug.Log("swipe update");
								break;
								
							case Gesture.GestureState.STATE_STOP:
								set_swipe = true;
								
								
								if(set_swipe)
								{
									//Debug.Log("start : " + swipe.StartPosition.z.ToString() + " - end : "+ swipe.Position.z.ToString());
									float[] arr = new float[4];
									float left, right, up, down;
									arr[0] = left = swipe.StartPosition.x - swipe.Position.x;
									arr[1] = right = swipe.Position.x - swipe.StartPosition.x;
									arr[2] = up = swipe.StartPosition.z - swipe.Position.z;
									arr[3] = down = swipe.Position.z - swipe.StartPosition.z;
									
									Array.Sort(arr);
									Array.Reverse(arr);
									
									if(arr[0] == left)  // yes
									{
										Debug.Log("left");
										btn_yes_1.SetActive(true);
										btn_no_1.SetActive(false);
										yes_no = 1;
									}
									else if(arr[0] == right)  // no
									{
										btn_yes_1.SetActive(false);
										btn_no_1.SetActive(true);
										Debug.Log("right");
										yes_no = 2;
									}
									else
										Debug.Log("swipe error");
									
									set_swipe = false;
								}
								break;
							}
							break;
							
							// invalid gesture (do nothing)
						default:
							break;
						}
					}
				}
			}
		}

		if(!b_start)
		{
			if(!doFixedUpdate)
			{
				new_timer = Environment.TickCount;
				switch(g_pause_num)
				{
				case 1: // when before start game

					if(!b_pass)
					{
						if(Intro.transform.position.x <= -2.0f)
							Intro.transform.position += new Vector3(intro_speed * Time.deltaTime, 0f, 0f);
						else
						{
							if(!b_intro_sound)
							{
								AudioSource.PlayClipAtPoint(Introduce, transform.position);
								b_intro_sound = true;
							}
							if(old_timer <= new_timer - 3000)
							{
								b_pass = true;
								b_intro_sound = false;
							}
						}
					}
					else
					{
						if(Intro.transform.position.x <= 13.0f)
							Intro.transform.position += new Vector3(intro_speed * Time.deltaTime, 0f, 0f);
						else
						{
							b_pass = false;
							g_pause_num = 4; // get ready
							old_timer = Environment.TickCount;
							Intro.transform.position = new Vector3(-13f, 0f, -12f);
						}
					}
					break;
				case 2: // game over
					if(!b_pass)
					{
						if(msg_GameOver.transform.position.x <= -2.0f)
							msg_GameOver.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
						else
						{
							if(old_timer <= new_timer - 1300)
							{
								b_pass = true;
							}
						}
					}
					else
					{
						b_global_signal = true;

						if(!b_try_again)
						{
							msg_TryAgain.SetActive(true);

							if(GameObject.Find("Player1(Clone)") == null)
							{
								GameObject new_one = Instantiate(Resources.Load("Player1"), new Vector3(-10f, -10f, 10f), Quaternion.identity) as GameObject;
								Debug.Log("Created!!!");
							}
						}
						else
						{
							if(GameObject.Find("Player1(Clone)") != null)
								Destroy(GameObject.Find("Player1(Clone)"));

							msg_TryAgain.SetActive(false);

							if(msg_GameOver.transform.position.x <= 13.0f)
							{
								msg_GameOver.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
							}
							else
							{
								b_pass = false;
								old_timer = Environment.TickCount;
								msg_GameOver.transform.position = new Vector3(-13f, 0f, -12f);

								//init all. life, score, etc..
								Init (1, 3);

								g_pause_num = 4;
								b_try_again = false;

								b_global_signal = false;
							}
						}
					}
					break;
				case 3: // when clear stage

					if(!b_pass)
					{
						if(msg_StageClear.transform.position.x <= -2.0f)
							msg_StageClear.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
						else
						{
							if(old_timer <= new_timer - 1000)
							{
								// if 0.5 seconds passed
								b_pass = true;
							}
						}
					}
					else
					{
						if(msg_StageClear.transform.position.x <= 13.0f)
							msg_StageClear.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
						else
						{
							ChangeStageText(g_stage);
							
							// remove and reallocate enemies
							foreach(GameObject g in l_enemy)
							{
								Destroy(g);
							}
							l_enemy.Clear();
							
							MakeEnemy();
							enemy_direction = 1;

							b_pass = false;
							g_pause_num = 4; // get ready
							old_timer = Environment.TickCount;
							msg_StageClear.transform.position = new Vector3(-13f, 0f, -12f);
						}
					}




					break;
				case 4: // get ready
					if(!b_pass)
					{
						if(msg_GetReady.transform.position.x <= -2.0f)
							msg_GetReady.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
						else
						{
							if(old_timer <= new_timer - 1000)
							{
								// if 0.5 seconds passed
								b_pass = true;
							}
						}
					}
					else
					{
						if(msg_GetReady.transform.position.x <= 13f)
							msg_GetReady.transform.position += new Vector3(msg_speed * Time.deltaTime, 0f, 0f);
						else
						{
							b_start = true;
							b_pass = false;
							g_pause_num = 0; // get ready
							msg_GetReady.transform.position = new Vector3(-13f, 0f, -12f);

							if(GameObject.Find("Player1(Clone)") == null)
							{
								// make new player character
								Instantiate(Resources.Load("Player1"), new Vector3(-1.9f, -4.2f, -10f), Quaternion.identity);
							}
						}
					}
					break;

				case 5: // temp time stop
					if(old_timer <= new_timer - stop_time)
					{
						// if some milliseconds passed
						b_start = true;
						g_pause_num = 0;
					}
					else
					{
						
					}
					break;
				default:
					break;
				}
				tick_count = System.Environment.TickCount;
			}
		}
		else
		{
			if(!doFixedUpdate)
			{
				// move right or left enemy
				MoveEnemy();

				UpdateScore();

				new_tick_count = System.Environment.TickCount;
				if(tick_count <= new_tick_count - 20000)
				{
					//every 30 seconds
					MakeBoss();

					tick_count = new_tick_count;
				}
			}
		}
		GetKey ();
	}

	void FixedUpdate()
	{
		if(doFixedUpdate)
		{
			switch(move_num)
			{
			case 1: // make bright screen

				if(AlphaBoard.renderer.material.color.a > 0.01f)
				{
					AlphaBoard.renderer.material.color -= new Color(0f, 0f, 0f, Time.deltaTime * 2f); 
				}
				else
				{
					InitAll();
					doFixedUpdate = false;
					move_num = 0;
				}

				break;
			case 2: // pause (make black screen)

				if(AlphaBoard.renderer.material.color.a < 2.0f)
				{
					AlphaBoard.renderer.material.color += new Color(0f, 0f, 0f, Time.deltaTime * 2f); 
				}
				else
				{
					doFixedUpdate = false;
					move_num = 0;

					text_ask.SetActive(true);
					btn_yes_1.SetActive(false);
					btn_yes_2.SetActive(true);
					btn_no_1.SetActive(false);
					btn_no_2.SetActive(true);
				}

				break;

			case 3: // keep going
				if(AlphaBoard.renderer.material.color.a > 0.01f)
				{
					AlphaBoard.renderer.material.color -= new Color(0f, 0f, 0f, Time.deltaTime * 2f); 
				}
				else
				{
					doFixedUpdate = false;
					move_num = 0;
					b_start = true;
				}
				break;
			default:
				doFixedUpdate = false;
				break;
			}
		}
	}

	void GetKey()
	{
		if(Input.GetKeyDown(KeyCode.A))
		{
			b_pass = true;
		}

		
	}


	void Init(int stage, int type)
	{
		if(type == 1) // when cleared stage
		{
			P_Coffee_Particle.Emit();

			g_stage++;

			b_start = false;
			b_pass = false;
			old_timer = System.Environment.TickCount;
			g_pause_num = 3;

		}
		else if(type == 2) // when death
		{
			Monsters.transform.position = new Vector3(-6.2f, 3.5f, -10f);
			enemy_direction = 1;

			g_pause_num = 4; // message get ready
		}
		else if(type == 3) // try again
		{
			g_stage = 1;
			g_score = 0;
			life_count = 2;

			Life_Object[0].SetActive(true);
			Life_Object[1].SetActive(true);

			ChangeStageText(g_stage);
			
			// remove and reallocate enemies
			foreach(GameObject g in l_enemy)
			{
				Destroy(g);
			}
			l_enemy.Clear();
			
			MakeEnemy();
			enemy_direction = 1;
			
			b_pass = false;
			g_pause_num = 4; // get ready
			old_timer = Environment.TickCount;

			
			
		}
		
		// time stop
		old_timer = System.Environment.TickCount;
		b_start = false;
		
		// init stored coffee
		coffee_count = 0;
		foreach(GameObject g in Stored_Coffee)
			g.SetActive(false);

		tick_count = System.Environment.TickCount;
		Destroy (GameObject.Find ("Boss(Clone)"));

		DestroyMissiles();

	}

	void MakeBoss()
	{
		//Boss_Appear.channels
		AudioSource.PlayClipAtPoint (Boss_Appear, transform.position);
		Instantiate(Resources.Load("Boss"));
	}

	void ChangeStageText(int stage)
	{
		if(g_stage < 7)
		{
			Destroy(GameObject.Find("Stage"));
			GameObject m_stage = Instantiate(Resources.Load("Stage" + stage.ToString())) as GameObject;
			m_stage.name = "Stage";
		}

		switch(g_stage)
		{
		case 1:
			e_missile_term[0] = 3000;
			e_missile_term[1] = 12000;
			e_missile_speed = 4f;
			break;
		case 2:
			e_missile_term[0] = 2500;
			e_missile_term[1] = 11000;
			e_missile_speed = 4.5f;
			break;
		case 3:
			e_missile_term[0] = 2000;
			e_missile_term[1] = 10000;
			e_missile_speed = 5f;
			break;
		case 4:
			e_missile_term[0] = 1500;
			e_missile_term[1] = 9000;
			e_missile_speed = 5.5f;
			break;
		case 5:
			e_missile_term[0] = 1000;
			e_missile_term[1] = 8000;
			e_missile_speed = 6f;
			break;
		case 6:
			e_missile_term[0] = 1000;
			e_missile_term[1] = 7000;
			e_missile_speed = 7f;
			break;
		case 7:
			e_missile_term[0] = 1000;
			e_missile_term[1] = 6000;
			e_missile_speed = 8f;
			break;
		default:
			break;
		}
	}

	void UpdateScore()
	{
		ScoreText.text = g_score.ToString();
		if(g_score > bestRecord)
		{
			BestScoreText.text = g_score.ToString();
		}
	}

	void DestroyMissiles()
	{
		GameObject[] obj = GameObject.FindGameObjectsWithTag("EnemyMissile");

		for(int i=0; i<obj.Length; i++)
		{
			Destroy(obj[i]);
		}
	}


	public void AddCoffeeCount()
	{
		if(coffee_count < 5)
		{
			Stored_Coffee[coffee_count].SetActive(true);
			coffee_count++;
			g_score += 1000;

			if(coffee_count < 5)
				GameObject.Find("Player1(Clone)").renderer.material.mainTexture = player_texture[coffee_count];
		}

		if(coffee_count == 5)
		{
			AudioSource.PlayClipAtPoint (LevelClear, transform.position);
			g_score += 1000;
			GameObject.Find("Player1(Clone)").renderer.material.mainTexture = player_texture[0];
			Init ((g_stage+1), 1);
		}

	}

	void GetScore(int _enemy_num)
	{
		switch(_enemy_num)
		{
		case 1:
			g_score += 500;
			break;
		case 2:
			g_score += 100;
			break;
		case 3:
			g_score += 200;
			break;
		case 4:
			g_score += 100;
			break;
		case 5:
			g_score += 100;
			break;
		case 6:
			g_score += 2000;
			break;
		default:
			break;
		}
	}

	void MakeEnemy()
	{
		for(int i=0; i<l_enemy.Count; i++)
		{
			Destroy(l_enemy[i]);
		}
		l_enemy.Clear();

		float gap = 1.1f;

		float x_pos = -6.2f;
		float y_pos = 3.5f;
		int rnd_value = System.Environment.TickCount; // for random value

		Monsters.transform.position = new Vector3(x_pos, y_pos, -10f);

		for(int i=0; i<4; i++)
		{
			int[] rnd_enemy_num = new int[10] {1,1,2,2,3,3,4,4,5,5};
			rnd_enemy_num = ArrayShuffle(rnd_enemy_num, rnd_value);

			rnd_value += 2971; //for random (no means)

			for(int j=0; j<10; j++)
			{
				GameObject _enemy = Instantiate(Resources.Load("Monster_" + rnd_enemy_num[j].ToString()), new Vector3(x_pos, y_pos, -10f), Quaternion.identity) as GameObject;
				l_enemy.Add(_enemy);
				
				// make to child
				l_enemy[(i*10)+j].transform.parent = Monsters.transform;
								
				x_pos += gap;
				if(j == 9)
				{
					x_pos = -6.2f;	
				}
			}
			y_pos -= gap;
		}
	}

	// make shuffle array
	private int[] ArrayShuffle(int[] arrInt, int rnd_value)
	{
		int maxValue = arrInt.Length;
		int tmpValue;
		int swapValue;
		System.Random rnd = new System.Random (rnd_value);
		
		for (int i = 0; i < maxValue; i++)
		{
			tmpValue = rnd.Next(maxValue - i) + i;
			swapValue = arrInt[i];
			arrInt[i] = arrInt[tmpValue];
			arrInt[tmpValue] = swapValue;
		}
		return arrInt;
	}

	void PlayerDeath()
	{
		if(life_count > 0)
		{
			Destroy(GameObject.Find("Player1(Clone)"));

			Life_Object[life_count-1].SetActive(false);
			life_count--;

			Init (g_stage, 2);
		}
		else
		{
			// Game over
			Destroy(GameObject.Find("Player1(Clone)"));
			AudioSource.PlayClipAtPoint (GameOver, transform.position);

			if(g_score > bestRecord)
			{
				bestRecord = g_score;
				WriteRecord();
			}
			TimeStop(1000);

			b_start = false;
			old_timer = System.Environment.TickCount;
			g_pause_num = 2;
		}
	}

	void TimeStop(int _millisecond)
	{
		stop_time = _millisecond;
		b_start = false;
		old_timer = System.Environment.TickCount;
		g_pause_num = 5;
	}

	void MoveEnemy()
	{
		if(enemy_direction == 1) // right
		{
			if(Monsters.transform.position.x <= -5.5f)
			{
				Monsters.transform.position += new Vector3(enemy_Speed * Time.deltaTime, 0f, 0f);
			}
			else
			{
				enemy_direction = 3;
				g_yPos = Monsters.transform.position.y - 0.3f;
			}
		}
		else if(enemy_direction == 2) // left
		{
			if(Monsters.transform.position.x >= -8.3f)
			{
				Monsters.transform.position -= new Vector3(enemy_Speed * Time.deltaTime, 0f, 0f);
			}
			else
			{
				enemy_direction = 4;
				g_yPos = Monsters.transform.position.y - 0.3f;
			}
		}
		else if(enemy_direction == 3) // right and down
		{
			if(Monsters.transform.position.y >= g_yPos)
			{
				Monsters.transform.position -= new Vector3(0f, enemy_Speed * Time.deltaTime, 0f);
			}
			else
			{
				enemy_direction = 2;
			}
		}
		else if(enemy_direction == 4) // left and down
		{
			if(Monsters.transform.position.y >= g_yPos)
			{
				Monsters.transform.position -= new Vector3(0f, enemy_Speed * Time.deltaTime, 0f);
			}
			else
			{
				enemy_direction = 1;
			}
		}
	}

	void WaitForMilliSecond(int milli_Second)
	{
		int temp1 = System.Environment.TickCount;
		int temp2 = 0;
		do
		{
			temp2 = System.Environment.TickCount;
		} 
		while (temp1 > temp2 - 1000);
	}

	void WriteRecord()
	{
		StreamWriter sw = new StreamWriter (path);
		sw.Write (bestRecord);
		sw.Close ();
	}
	void ReadRecord()
	{
		StreamReader sr = new StreamReader (path);
		bestRecord = int.Parse ( sr.ReadLine () );
		BestScoreText.text = bestRecord.ToString();
		sr.Close ();
	}

}
