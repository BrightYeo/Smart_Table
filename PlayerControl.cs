using UnityEngine;
using System.Collections;
using System;
using Leap;

public class PlayerControl : MonoBehaviour {
	private GameObject temp;
	// leap motion
	Frame newFrame = new Frame();
	Controller leap_Controller;

	public GameObject P_Particle;
	public GameObject P_Particle2;

	bool b_pinch_on = false;

	public AudioClip Sound_Shoot;
	public AudioClip Sound_Die;

	void Start () 
	{
		leap_Controller = new Leap.Controller();
		leap_Controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);

		P_Particle = Instantiate(Resources.Load("PlayerShootEffect")) as GameObject;
		P_Particle2 = Instantiate(Resources.Load("PlayerDieEffect")) as GameObject;

		temp = Instantiate(Resources.Load("Debug")) as GameObject;
		temp.SetActive(false);

	}
	bool _canShoot = false;


	void Update () 
	{
		if(GameMain.b_start)
		{
			LeapControl();

			GetKey ();
			P_Particle.transform.position = gameObject.transform.position;
			P_Particle2.transform.position = gameObject.transform.position;
		}

		if(!GameMain.b_start && GameMain.b_global_signal)
		{
			LeapControl();
			GetKey ();
			P_Particle.transform.position = gameObject.transform.position;
			P_Particle2.transform.position = gameObject.transform.position;
		}

	}

	int new_tick;
	int old_tick = Environment.TickCount;

	void LeapControl()
	{
		if(leap_Controller.Devices[0].IsValid == true)
		{
			Frame oldFrame = newFrame == null ? Frame.Invalid : newFrame;
			newFrame = leap_Controller.Frame();

			if(oldFrame.Id != newFrame.Id)
			{
				new_tick = Environment.TickCount;
				GestureEvent(newFrame, oldFrame);
			}

			if(GameMain.b_start)
			{
				float temp_1 = newFrame.Hands[0].Arm.Direction.x * 10000;
				int x_dir = (int)temp_1;
				temp_1 = (((float)x_dir/10000) * 13f) + 2.0f;

				temp.GetComponent<GUIText>().text = temp_1.ToString();

				if(temp_1 >= -8.2f)
				{
					if(temp_1<= 4.6f)
					{	
						gameObject.transform.position = new Vector3(temp_1, -4.2f, -10f);
					}
				}
				
				float pinch = newFrame.Hands[0].PinchStrength;
				if(pinch >= 1f)
				{
					if(!b_pinch_on)
					{
						b_pinch_on = true;
						PlayerShoot();
					}
				}
				else
				{
					b_pinch_on = false;
				}
			}
		}
	}
	
	void GestureEvent(Frame _newFrame, Frame _oldFrame)
	{
		//if(old_tick <= new_tick - 500)
		//{
			if(_newFrame.Hands.Count == 1)
			{
				int keytap_count = 0;

				foreach(Gesture g in _newFrame.Gestures(_oldFrame))
				{
					switch(g.Type)
					{
					case Gesture.GestureType.TYPE_KEY_TAP:

						keytap_count++;
						if(keytap_count >= 3)
						{
							Debug.Log("Player KeyTap()");
							keytap_count = 0;
							if(GameMain.b_global_signal)
							{
								GameMain.b_try_again = true;
							}
							else
							{
								PlayerShoot();
							}
						}

						break;
					default:
						break;
					}
				}
			}
			else if(_newFrame.Hands.Count == 2)
			{
				int keytap_count = 0;
				foreach (Gesture g in _newFrame.Gestures(_oldFrame))
				{
					Debug.Log("hand 2");
					//SwipeGesture aa=newFrame.Gestures(oldFrame);
					SwipeGesture swipe = new SwipeGesture(g);
					
					// filter invalid events
					if (!g.IsValid || (g.Type == Gesture.GestureType.TYPE_INVALID))
						continue;
					// process valid events based on types
					switch (g.Type) 
					{
					case Gesture.GestureType.TYPE_KEY_TAP: // double tap
						keytap_count++;
						if(keytap_count >= 6)
						{
							Debug.Log("Player double tapped()");
							keytap_count = 0;
							GameMain.b_main_leap_control = true;
							GameMain.b_start = false;
						}
						
						break;
					default:
						break;
					}
				}
			}
			old_tick = new_tick;
		//}
	}

	void GetKey()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if(GameMain.b_global_signal)
			{
				GameMain.b_try_again = true;
			}
			else
			{
				PlayerShoot();
			}
		}
		else if(Input.GetKey(KeyCode.LeftArrow)) //player move
		{
			if(gameObject.transform.position.x >= -8.2f)
			{
				gameObject.transform.position -= new Vector3(5f * Time.deltaTime, 0f, 0f);
			}
		}
		else if(Input.GetKey(KeyCode.RightArrow)) //player move
		{
			if(gameObject.transform.position.x <= 4.6f)
			{
				gameObject.transform.position += new Vector3(5f * Time.deltaTime, 0f, 0f);
			}
		}
	}

	void PlayerShoot()
	{
		if(GameObject.Find("Player_Missile(Clone)") == null)
		{
			AudioSource.PlayClipAtPoint (Sound_Shoot, transform.position);
			Instantiate (Resources.Load ("Player_Missile"), gameObject.transform.position, Quaternion.identity);
			P_Particle.GetComponent<ParticleEmitter>().Emit();
		}
	}

	void OnDestroy()
	{
		try {
			P_Particle2.GetComponent<ParticleEmitter>().Emit(); }
		catch(Exception e){ }
	}
}
