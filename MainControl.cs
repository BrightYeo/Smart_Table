using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Leap;

public class MainControl : MonoBehaviour {

	int[] goods_code = new int[24] { 100, 101, 102, 103, 104,
									200, 201, 202, 203, 204,
									300, 301, 302, 303, 304, 305,
									400, 401, 402, 403,
									500, 501,
									600, 601 };
	string[] goods_name = new string[24] { "토스트", "와플", "베이글", "치즈케익", "생크림케익",
										"페퍼민트", "자스민", "캐모마일", "로즈마리", "라벤더",
										"에스프레소", "아메리카노", "카페라떼", "바닐라라떼", "카푸치노", "카라멜 마끼아또",
										"바나나주스", "파인애플주스", "레몬에이드", "토마토주스",
										"아메리카노+머핀", "카푸치노+애플파이",
										"머그컵", "탄산수"};
	// for order list
	string[] goods_name2 = new string[24] { "토스트", "와플", "베이글", "치즈케익", "생크림케익",
										"페퍼민트", "자스민", "캐모마일", "로즈마리", "라벤더",
										"에스프레소", "아메리카노", "카페라떼", "바닐라라떼", "카푸치노", "카라멜\n마끼아또",
										"바나나주스", "파인애플\n주스", "레몬에이드", "토마토주스",
										"아메리카노\n+ 머핀", "카푸치노\n+ 애플파이",
										"머그컵", "탄산수"};
	int[] goods_price = new int[24] { 2000, 1500, 1500, 2000, 2000,
									3000, 3000, 3000, 3000, 3000,
									2500, 3000, 4000, 4000, 4000, 4000,
									2500, 2500, 2000, 2500,
									5500, 6000,
									8000, 1000};

	public GameObject Board_cover;

	public GameObject Menu;
	public GameObject Menu2;
	public GameObject Menu3;
	public GameObject Menu4;
	public GameObject Menu5;
	public GameObject Menu6;

	public GameObject Ribbon;
	private GameObject[] SubMenus;
	public GameObject[][] SubMenu_prefab = new GameObject[6][];

	public GameObject[] SubMenu1;
	public GameObject[] SubMenu2;
	public GameObject[] SubMenu3;
	public GameObject[] SubMenu4;
	public GameObject[] SubMenu5;
	public GameObject[] SubMenu6;

	public GameObject Alarm_1;
	public GameObject Alarm_2;
	public GameObject Btn_OK, Btn_NO;

	public List<GameObject> Order_List = new List<GameObject> ();
	public List<GameObject> Order_List_Text = new List<GameObject> ();
	public List<GameObject> Order_BG = new List<GameObject> ();

	public GameObject SubMenu_Count;
	public Texture[] Count_Number;

	public GameObject Order_Focus;
	public GameObject Order_Paper;

	public TextMesh txt_TotalPrice;

	// ask to move scene
	public GameObject Alpha_Cover;
	public GameObject text_ask;
	public GameObject btn_yes_1;
	public GameObject btn_yes_2;
	public GameObject btn_no_1;
	public GameObject btn_no_2;
	private bool b_move_yes_no = true;

	public struct Menu_Set
	{
		public int type;
		public int count;
	}
	public struct Order_Set
	{
		public int menuCount;
		public Menu_Set[] menuList;
	} Order_Set order_set;

	public List<Menu_Set> l_MenuList = new List<Menu_Set>();

	bool doFixedUpdate = false;

	// Select mode. 0: None , 1 2 3: mode number , 4: order
	int mode = 1;
	int selected_menu = 3; // current menu showing
	int selected_submenu = 1; // current submenu showing
	int updated_selected_submenu = 0;
	int submenu_count = 0;
	int selected_submenu_count;
	int selected_order = 1;
	int order_focus_num = 1;
	bool b_menu4_OK_NO = true; // true = yes

	bool isSelected_Count = false;
	bool b_menu_move_lock = false;

	// Socket Client (to Order Server)
	private Socket sockClient;
	private byte[] setByte = new byte[1024];
	private const int cPort = 6000;
	IPAddress serverIP2;
	IPEndPoint serverEndPoint2;

	// thread
	private int leap_code = 0;
	private bool b_menu4_maketext = false;

	// leap motion
	Frame newframe = new Frame();
	Controller leap_Controller;
	private int loop_count = 0;
	private int loop_result = 0;


	// Audio
	public AudioClip BookPage;
	public AudioClip Move_Scene;
	public AudioClip Select_Yes_No;
	public AudioClip Selected_Yes_No;
	public AudioClip UpDown_Move;
	public AudioClip Selected_Count;


	void Start ()
	{
		leap_Controller = new Leap.Controller();
		
		leap_Controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
		leap_Controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);

		try
		{
			ConnectToServer ();
		} 
		catch(Exception ex)
		{ 
			Debug.Log("Connect Error(to Order server)"); 
		}


		// set default menu text color
		Menu.renderer.material.color = Color.black;
		Menu2.renderer.material.color = Color.black;
		Menu3.renderer.material.color = Color.white;
		Menu4.renderer.material.color = Color.black;
		Menu5.renderer.material.color = Color.black;
		Menu6.renderer.material.color = Color.black;

		// allocate
		SubMenu_prefab[0] = new GameObject[5];
		SubMenu_prefab[1] = new GameObject[5];
		SubMenu_prefab[2] = new GameObject[6];
		SubMenu_prefab[3] = new GameObject[4];
		SubMenu_prefab[4] = new GameObject[2];
		SubMenu_prefab[5] = new GameObject[2];

		SubMenu_prefab[0] = SubMenu1;
		SubMenu_prefab[1] = SubMenu2;
		SubMenu_prefab[2] = SubMenu3;
		SubMenu_prefab[3] = SubMenu4;
		SubMenu_prefab[4] = SubMenu5;
		SubMenu_prefab[5] = SubMenu6;

		ShowCount (0, false);

		doFixedUpdate = true;
		move_num = 8;
	}

	private void ConnectToServer()
	{
		IPAddress serverIP2 = IPAddress.Parse("127.0.0.1");

		//IPAddress serverIP2 = IPAddress.Parse("192.168.0.2");
		IPEndPoint serverEndPoint2 = new IPEndPoint(serverIP2, cPort);

		sockClient = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		sockClient.Connect (serverEndPoint2);
		if(sockClient.Connected)
			Debug.Log("Connected to Order Server");
	}

	private void SendData(string msg)
	{
		try
		{
			int getValueLength = 0;
			setByte = Encoding.UTF7.GetBytes (msg);
			sockClient.Send (setByte, 0, setByte.Length, SocketFlags.None);
			Debug.Log ("Message was sent. : " + msg);
			setByte = new byte[1024];
		}
		catch(Exception ex) {	}
		finally
		{
			mode = 1;

			l_MenuList.Clear();
			for(int i=0; i<Order_List.Count; i++)
			{
				Destroy(Order_List[i]);
				Destroy(Order_List_Text[i]);
			}
			Order_List.Clear();
			Order_List_Text.Clear();
		}

		// 대기화면으로 갈 지 게임으로 갈 지 선택하는 메뉴 나와야 할듯.

	}

	private int old_tick = Environment.TickCount;
	private int new_tick = Environment.TickCount;

	void Update () 
	{
		if(!doFixedUpdate)
		{
			//GameObject.Find("prefab_cube").transform.Rotate(new Vector3(0,1,0), Time.deltaTime*100);  // 333333333

			if(leap_Controller.Devices[0].IsValid == true)
			{
				Frame oldframe = newframe == null ? Frame.Invalid : newframe;
				newframe = leap_Controller.Frame ();
				
				if(oldframe.Id != newframe.Id)
				{
					GetUnfoldFinger();
					GestureEvents(newframe, oldframe);
				}
			}

			new_tick = Environment.TickCount;
			if(leap_code != 0)
			{
				if(old_tick <= new_tick - 500)
				{
					Control (leap_code);
					old_tick = new_tick;
				}
				leap_code = 0;
			}
			GetKey ();

		// cover on/off
			ShowMode ();
		//SetOrderPosition ();
			//int _price = ;
			//string txt_price = _price.ToString();

			txt_TotalPrice.text = SumTotalPrice().ToString("###,##0") + "원";
		}
	}
	
	void ShowSubMenu(int num)
	{
		// num = Menu number
		switch(num)
		{
		case 1:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}
			
			SubMenus = new GameObject[SubMenu_prefab[0].Length];
			selected_menu = 1;
			break;
		case 2:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}
			
			SubMenus = new GameObject[SubMenu_prefab[1].Length];
			selected_menu = 2;
			break;
		case 3:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}

			SubMenus = new GameObject[SubMenu_prefab[2].Length];
			selected_menu = 3;
			break;
		case 4:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}
			
			SubMenus = new GameObject[SubMenu_prefab[3].Length];
			selected_menu = 4;
			break;
		case 5:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}
			
			SubMenus = new GameObject[SubMenu_prefab[4].Length];
			selected_menu = 5;
			break;
		case 6:

			for (int i=0; i<10; i++) // most longer of sub-menus
			{
				try {
					Destroy (SubMenus[i]); 
				} catch(Exception e) {
					//Debug.Log(e.Message);
				}
			}
			
			SubMenus = new GameObject[SubMenu_prefab[5].Length];
			selected_menu = 6;
			break;
		default:
			break;
		}

		float x, z;
		x = 0.4f;
		z = 0f;
		float scale_x, scale_y;
		scale_x = 5f;
		scale_y = 6.45f;

		// init Submenus Position, Scale
		for(int i=0; i<SubMenus.Length; i++)
		{
			SubMenus[i] = Instantiate(SubMenu_prefab[selected_menu-1][i], new Vector3(x, -1f, z), Quaternion.identity) as GameObject;
			SubMenus[i].transform.localScale = new Vector3(scale_x, scale_y, 1f);
			if(i < 1)
			{
				x += 2.1f;
			}
			if(i == 0)
			{
				scale_x -= 1.5f;
				scale_y -= 1.935f;
			}
			if(i == 1)
			{
				scale_x = 0f;
				scale_y = 0f;
			}
			z += 1f;
		}
	}

	private void GetUnfoldFinger()
	{
		float gap_thumb_z = newframe.Hands[0].Arm.Direction.z - newframe.Hands[0].Fingers[0].Direction.z;
		float gap_index_z = newframe.Hands[0].Arm.Direction.z - newframe.Hands[0].Fingers[1].Direction.z;
		float gap_middle_z = newframe.Hands[0].Arm.Direction.z - newframe.Hands[0].Fingers[2].Direction.z;
		float gap_ring_z = newframe.Hands[0].Arm.Direction.z - newframe.Hands[0].Fingers[3].Direction.z;
		float gap_pinky_z = newframe.Hands[0].Arm.Direction.z - newframe.Hands[0].Fingers[4].Direction.z;
		
		float gap_thumb_y = newframe.Hands[0].Arm.Direction.y - newframe.Hands[0].Fingers[0].Direction.y;
		float gap_index_y = newframe.Hands[0].Arm.Direction.y - newframe.Hands[0].Fingers[1].Direction.y;
		float gap_middle_y = newframe.Hands[0].Arm.Direction.y - newframe.Hands[0].Fingers[2].Direction.y;
		float gap_ring_y = newframe.Hands[0].Arm.Direction.y - newframe.Hands[0].Fingers[3].Direction.y;
		float gap_pinky_y = newframe.Hands[0].Arm.Direction.y - newframe.Hands[0].Fingers[4].Direction.y;
		
		int result = 0;
		
		
		if(gap_index_z <= 0.1f && gap_index_z >= -0.1f)
		{
			result = 7;
			if(gap_middle_z <= 0.15f && gap_middle_z >= -0.15f)
			{
				result = 8;
				if(gap_ring_z <= 0.2f && gap_ring_z >= -0.2f)
				{
					result = 9;
					if(gap_pinky_z <= 0.15f && gap_pinky_z >= -0.15f)
					{
						result = 0;
					}
				}
			}
		}
		
		if(loop_result != result)
		{
			loop_count = 0;
		}
		else
		{
			loop_count++;
		}
		loop_result = result;
		//Debug.Log (loop_count + ", " + loop_result);
		
		if(loop_count >= 40)
		{
			if(result != 0)
			{
				switch(result)
				{
				case 7:
					//debugGUIText.text = "Finger 1";
					break;
				case 8:
					//debugGUIText.text = "Finger 2";
					break;
				case 9:
					//debugGUIText.text = "Finger 3";
					break;
				default:
					break;
				}
				//SendData(result);
				leap_code = result;
				//Debug.Log (result);
			}
			
			loop_count = 0;
		}
		
		//Debug.Log(gap_thumb_z +", "+ gap_index_z + ", " + gap_middle_z + ", " + gap_ring_z + ", " + gap_pinky_z);
		
		
	}
	
	private void GestureEvents(Frame newFrame, Frame oldFrame) 
	{
		if(newFrame.Hands.Count == 1)
		{
			int dir; // 1:left, 2:right, 3:up, 4:down
			int keytap_count = 0;
			bool set_swipe = false;
			Vector3 swipe_start = new Vector3();
			
			
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
					keytap_count++;
					if(keytap_count >= 3)
					{
						keytap_count = 0;
						leap_code = 5;
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
							
							if(arr[0] == left)
							{
								dir = 1;
								leap_code = 1;
								//Debug.Log("Left");
							}
							else if(arr[0] == right)
							{
								dir = 2;
								leap_code = 2;
								//Debug.Log("Right");
							}
							else if(arr[0] == up)
							{
								dir = 3;
								leap_code = 3;
								//Debug.Log("Up");
							}
							else if(arr[0] == down)
							{
								dir = 4;
								leap_code = 4;
								//Debug.Log("Down");
							}
							else
								dir = -1; //invalid
							
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
		else if(newFrame.Hands.Count == 2)
		{
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
					keytap_count++;
					if(keytap_count >= 6)
					{
						keytap_count = 0;
						leap_code = 6;
					}
					
					break;
				default:
					break;
				}
			}
		}
	}

	// for FixedUpdate
	int move_num = 0;
	float g_temp_x;
	float g_temp_y;

	Vector3[] target_Pos;
	Vector3[] target_Scale;
	bool[] fixed_count;

	int temp_focus;

	int warning_count = 0;
	int warning_num;

	void FixedUpdate()
	{
		if(doFixedUpdate)
		{
			switch(move_num)
			{
			case 0:
				doFixedUpdate = false;
				break;

			case 1: // menu move down
				if(g_temp_y != Menu.transform.position.y)
				{
					Vector3 targetVec = new Vector3(Menu.transform.position.x, g_temp_y, -27f);
					Menu.transform.position = Vector3.Lerp(Menu.transform.position, targetVec, Time.deltaTime * 5f);
					if( (g_temp_y - Menu.transform.position.y) < 0.04f )
					{
						Menu.transform.position = new Vector3(Menu.transform.position.x, g_temp_y, -27f);
					}
				}
				else
				{
					move_num = 0;
					doFixedUpdate = false;
				}
				break;

			case 2: // menu move up
				if(g_temp_y != Menu.transform.position.y)
				{
					Vector3 targetVec = new Vector3(Menu.transform.position.x, g_temp_y, -27f);
					Menu.transform.position = Vector3.Lerp(Menu.transform.position, targetVec, Time.deltaTime * 5f);
					if( (Menu.transform.position.y - g_temp_y) < 0.04f )
					{
						Menu.transform.position = new Vector3(Menu.transform.position.x, g_temp_y, -27f);
					}
				}
				else
				{
					move_num = 0;
					doFixedUpdate = false;
				}
				break;

			case 3: // submenu move right
				for(int i=0; i<SubMenus.Length; i++)
				{
					if(i == selected_submenu-1)
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						 	                                             Time.deltaTime * 5f);
							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
						                                                 Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];

						}
						else
							fixed_count[i] = true;
					}
					else if(((selected_submenu-1) - i) == 1) // 1 more small
					{
						if(SubMenus[i].transform.position.z != target_Pos[i].z || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						    	                                          Time.deltaTime * 5f);
							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                        Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];

						}
						else
						{
							fixed_count[i] = true;
							SubMenus[i].SetActive(false);
						}
					}
					else if(i - (selected_submenu-1) == 1) // 1 more big
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						                                              	Time.deltaTime * 5f);
							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
						                                                 	Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];
						}
						else
							fixed_count[i] = true;
					}
					else if(((selected_submenu-1) - i) > 1) // over 2 more smaller
					{
						if(SubMenus[i].transform.position != target_Pos[i])
						{
							//Debug.Log("aaa : " + SubMenus[i].name);
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						                                              	Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
						}
						else
							fixed_count[i] = true;
					}
					else if(i - (selected_submenu-1) > 1) // over 2 more bigger
					{
						//Debug.Log("start : " + SubMenus[i].transform.position + "   ,  target : " + target_Pos[i]);
						if(SubMenus[i].transform.position.z != target_Pos[i].z || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						                                              	Time.deltaTime * 5f);
							if(i == selected_submenu+1)
							{
								SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                        Time.deltaTime * 5f);
							}

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];

							if(i == selected_submenu+1)
								SubMenus[i].SetActive(true);
						}
						else
							fixed_count[i] = true;
					}
				}

				int temp = 0;
				for(int i=0; i<fixed_count.Length; i++)
				{
					if(fixed_count[i]) temp++;
				}
				if(temp == fixed_count.Length)
				{
					move_num = 0;
					doFixedUpdate = false;

					//update
					selected_submenu = updated_selected_submenu;
				}

				break;

			case 4: // submenu move left
				for(int i=0; i<SubMenus.Length; i++)
				{
					if(i == selected_submenu-1)
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
							                                              Time.deltaTime * 5f);

							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                 Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];
						}
						else
							fixed_count[i] = true;
					}
					else if(((selected_submenu-1) - i) == 1) // 1 more small
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
							                                              Time.deltaTime * 5f);
							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                 Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];
						}
						else
							fixed_count[i] = true;
					}
					else if(i - (selected_submenu-1) == 1) // 1 more big
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						    	                                          Time.deltaTime * 5f);
							SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                        Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];

						}
						else
						{
							fixed_count[i] = true;
							SubMenus[i].SetActive(false);
						}
					}
					else if(i < selected_submenu-1) // smaller more than 1
					{
						if(SubMenus[i].transform.position != target_Pos[i] || SubMenus[i].transform.localScale != target_Scale[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						    	                                          Time.deltaTime * 5f);
							if(i == selected_submenu-3)
							{
								SubMenus[i].transform.localScale = Vector3.Lerp (SubMenus[i].transform.localScale, target_Scale[i],
							                                                        Time.deltaTime * 5f);
							}

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
							if(Vector3.Distance(SubMenus[i].transform.localScale, target_Scale[i]) < 0.03)
								SubMenus[i].transform.localScale = target_Scale[i];

							if(i == selected_submenu-3)
								SubMenus[i].SetActive(true);
						}
						else
							fixed_count[i] = true;
					}
					else if(i > selected_submenu-1) // bigger more than 1
					{
						if(SubMenus[i].transform.position != target_Pos[i])
						{
							SubMenus[i].transform.position = Vector3.Lerp(SubMenus[i].transform.position, target_Pos[i],
						    	                                          Time.deltaTime * 5f);

							if(Vector3.Distance(SubMenus[i].transform.position, target_Pos[i]) < 0.03)
								SubMenus[i].transform.position = target_Pos[i];
						}
						else
							fixed_count[i] = true;
					}
				}

				int temp2 = 0;
				for(int i=0; i<fixed_count.Length; i++)
				{
					if(fixed_count[i]) temp2++;
				}
				if(temp2 == fixed_count.Length)
				{
					move_num = 0;
					doFixedUpdate = false;
					
					//update
					selected_submenu = updated_selected_submenu;
				}

				break;

			case 5:
				SetOrderFocus(temp_focus);
				break;
			case 6: // Warning message count
			{
				if(warning_count <= 70)
				{
					warning_count++;
				}
				else
				{
					Alarm_1.SetActive(false);
					Alarm_2.SetActive(false);
					warning_count = 0;
					move_num = 0;
					doFixedUpdate = false;
				}
				break;
			}
			case 7: // 어둡게 zzzz

				if(Alpha_Cover.renderer.material.color.a < 2.0f)
				{
					Alpha_Cover.renderer.material.color += new Color(0f, 0f, 0f, Time.deltaTime * 2f); 
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
					mode = 5;

					b_menu_move_lock = true;
				}

				break;

			case 8: // 밝게

				text_ask.SetActive(false);
				btn_yes_1.SetActive(false);
				btn_yes_2.SetActive(false);
				btn_no_1.SetActive(false);
				btn_no_2.SetActive(false);
				
				if(Alpha_Cover.renderer.material.color.a > 0.01f)
				{
					Alpha_Cover.renderer.material.color -= new Color(0f, 0f, 0f, Time.deltaTime * 2f); 
				}
				else
				{
					doFixedUpdate = false;
					move_num = 0;
				}

				break;

			default:
				break;
			}
		}
	}

	void ShowMode()
	{
		if(mode == 1)
		{
			//Board_cover[0].SetActive(false);
			//Board_cover[1].SetActive(true);
			//Board_cover[2].SetActive(true);

			Show_Bright_Menu(1);

			Order_Focus.SetActive (false);
			Order_Paper.SetActive(false);
		}
		else if(mode == 2)
		{
			//Board_cover[0].SetActive(true);
			//Board_cover[1].SetActive(false);
			//Board_cover[2].SetActive(true);

			Show_Bright_Menu(2);

			Order_Focus.SetActive (false);
			Order_Paper.SetActive(false);

		}
		else if(mode == 3)
		{
			//Board_cover[0].SetActive(true);
			//Board_cover[1].SetActive(true);
			//Board_cover[2].SetActive(false);

			Show_Bright_Menu(3);

			Order_Focus.SetActive (true);
			Order_Paper.SetActive(false);

		}
		else if(mode == 4)
		{
			//Board_cover[0].SetActive(true);
			//Board_cover[1].SetActive(true);
			//Board_cover[2].SetActive(true);

			Show_Bright_Menu(0);

			Order_Focus.SetActive (false);
			Order_Paper.SetActive(true);

			// Show selected goods and prices  llllll
			if(b_menu4_maketext == false)
			{
				CreateOrderPaperText();
				b_menu4_maketext = true;
			}

			// active Order paper OK_NO Btn
			if(b_menu4_OK_NO)
			{
				Btn_OK.SetActive(true);
				Btn_NO.SetActive(false);
			}
			else
			{
				Btn_OK.SetActive(false);
				Btn_NO.SetActive(true);
			}

		}
	}

	void Control(int code)
	{
		if(!b_menu_move_lock)
		{
			switch(code)
			{
			case 7:
				mode = 1;
				//selected_submenu = 1;

				Show_Bright_Menu(1);

				order_set.menuCount = 0;
				order_set.menuList = null;
				AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				break;

			case 8:
				if(SubMenus == null)
				{
					// Warning
					Alarm_1.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else
				{
					mode = 2;
					//selected_submenu = 1;

					Show_Bright_Menu(2);

					order_set.menuCount = 0;
					order_set.menuList = null;
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				}
				break;

			case 9:
				if(mode == 1 && SubMenus == null)
				{
					// Warning (please select menu)
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else if((mode == 1 || mode == 2) && l_MenuList.Count == 0)
				{
					// Warning (orderlist is empty)
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else
				{
					mode = 3;

					Show_Bright_Menu(3);

					order_set.menuCount = 0;
					order_set.menuList = null;
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				}
				break;

			case 10:  // didn't make yet

				mode = 4;
				b_menu_move_lock = true;
				
				// should apply for all struct which go for menu 4
				// and if go back to previous menu, it should be NULL
				order_set.menuCount = l_MenuList.Count;
				order_set.menuList = new Menu_Set[l_MenuList.Count];
				l_MenuList.CopyTo(order_set.menuList);
				//Debug.Log("length : "+order_set.menuList.Length);
				break;

			default:
				break;
			}
		}
		ShowMode ();


		if(mode == 1)
		{
			switch(code)
			{
			case 3:
				if(selected_menu < 6)
				{
					selected_menu++;
					// 1.25 up or down
					g_temp_y = Menu.transform.position.y + 1.25f; // 1.25 = distance to move
					doFixedUpdate = true;
					move_num = 1;

					ChangeMenuColor();
					AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
				}
				break;
			case 4:
				if(selected_menu > 1)
				{
					selected_menu--;
					// 1.25 up or down
					g_temp_y = Menu.transform.position.y - 1.25f; // 1.25 = distance to move
					doFixedUpdate = true;
					move_num = 2;

					ChangeMenuColor();
					AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
				}
				break;
			case 5:
				mode = 2;
				selected_submenu = 1;
				ShowSubMenu (selected_menu);
				AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				break;
			case 6:
				if(l_MenuList.Count == 0)
				{
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;

					Debug.Log ("There is no list of order");
					return;
				}
				Show_Bright_Menu(0);
				b_menu4_maketext = false;
				mode = 4;

				b_menu_move_lock = true;
				
				// should apply for all struct which go for menu 4
				// and if go back to previous menu, it should be NULL
				order_set.menuCount = l_MenuList.Count;
				order_set.menuList = new Menu_Set[l_MenuList.Count];
				l_MenuList.CopyTo(order_set.menuList);
				break;
			default:
				break;
			}
		}
		else if(mode == 2)
		{
			switch(code)
			{
			case 1:
				if(!isSelected_Count)
				{
					if(selected_submenu < SubMenus.Length)
					{
						AudioSource.PlayClipAtPoint(BookPage, transform.position);
						SelectSubMenu(selected_submenu + 1);
					}
				}
				else
				{
					// change count
					if(selected_submenu_count > 0)
					{
						AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
						SelectSubMenuCount(selected_submenu_count - 1);
					}
				}
				break;
			case 2:
				if(!isSelected_Count)
				{
					if(selected_submenu > 1)
					{
						AudioSource.PlayClipAtPoint(BookPage, transform.position);
						SelectSubMenu(selected_submenu - 1);
					}
				}
				else
				{
					// change count
					if(selected_submenu_count < 9)
					{
						AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
						SelectSubMenuCount(selected_submenu_count + 1);
					}
				}
				break;
			case 5:
				if(!isSelected_Count)
				{
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
					b_menu_move_lock = true;
					ShowCount(0, true);
					isSelected_Count = true;
				}
				else
				{
					b_menu_move_lock = false;
					// choose done
					ShowCount(0, false);
					isSelected_Count = false;
					AudioSource.PlayClipAtPoint(Selected_Count, transform.position);
					AddToOrder( (selected_menu * 100) + selected_submenu-1, selected_submenu_count );
				}
				break;
			case 6:
				if(l_MenuList.Count == 0)
				{
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;

					Debug.Log ("There is no list of order");
					return;
				}
				Show_Bright_Menu(0);
				b_menu4_maketext = false;
				mode = 4;

				b_menu_move_lock = true;
				
				// should apply for all struct which go for menu 4
				// and if go back to previous menu, it should be NULL
				order_set.menuCount = l_MenuList.Count;
				order_set.menuList = new Menu_Set[l_MenuList.Count];
				l_MenuList.CopyTo(order_set.menuList);
				break;
			default:
				break;
			}
		}
		else if(mode == 3)
		{
			switch(code)
			{
			case 1:
				if(isSelected_Count && selected_submenu_count > 0)
				{
					AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
					SelectSubMenuCount(selected_submenu_count - 1);
				}
				break;
			case 2:
				if(isSelected_Count && selected_submenu_count < 9)
				{
					AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
					SelectSubMenuCount(selected_submenu_count + 1);
				}
				break;
			case 3:
				if(!isSelected_Count)
				{
					AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
					SetOrderPosition(2);

					int pos = order_focus_num - 1;
					if (pos > 0 && pos < 5 && pos <= Order_List.Count)
					{
						move_num = 5;
						doFixedUpdate = true;
						temp_focus = pos;
					}
					//SetOrderFocus(order_focus_num - 1);
				}
				break;
			case 4:
				if(!isSelected_Count)
				{
					AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
					SetOrderPosition(1);

					int pos = order_focus_num + 1;
					if (pos > 0 && pos < 5 && pos <= Order_List.Count)
					{
						move_num = 5;
						doFixedUpdate = true;
						temp_focus = pos;
					}
					//SetOrderFocus(order_focus_num + 1);
				}
				break;
			case 5:
				if(!isSelected_Count)
				{
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
					b_menu_move_lock = true;
					selected_submenu_count = l_MenuList[selected_order-1].count;
					SelectSubMenuCount(selected_submenu_count);
					ShowCount(order_focus_num, true);
					isSelected_Count = true;
				}
				else
				{
					AudioSource.PlayClipAtPoint(Selected_Count, transform.position);
					b_menu_move_lock = false;
					// choose done
					ShowCount(order_focus_num, false);
					isSelected_Count = false;
					
					ModifyOrder(selected_order, selected_submenu_count);
					
					selected_submenu_count = 0;
					SelectSubMenuCount (0);
				}
				break;
			case 6:
				if(l_MenuList.Count == 0)
				{
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;

					Debug.Log ("There is no list of order");
					return;
				}
				Show_Bright_Menu(0);
				b_menu4_maketext = false;
				mode = 4;

				b_menu_move_lock = true;
				
				// should apply for all struct which go for menu 4
				// and if go back to previous menu, it should be NULL
				order_set.menuCount = l_MenuList.Count;
				order_set.menuList = new Menu_Set[l_MenuList.Count];
				l_MenuList.CopyTo(order_set.menuList);
				break;
			default:
				break;
			}
		}
		else if(mode == 4)
		{
			string msg = order_set.menuCount.ToString();
			msg+= ",";
			for(int i = 0; i < order_set.menuCount; i++)
			{
				msg += order_set.menuList[i].type.ToString() + " ";
				msg += order_set.menuList[i].count.ToString();
				if(i != order_set.menuCount-1)
				{
					msg += ",";
				}
			}
			switch(code)
			{
			case 1:
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				b_menu4_OK_NO = true;
				break;
			case 2:
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				b_menu4_OK_NO = false;
				break;
			case 5:
				//order done

				b_menu_move_lock = false;

				if(b_menu4_OK_NO)
				{
					SendData(msg);
					RemoveOrderTexts();

					doFixedUpdate = true;
					move_num = 7;
					//Application.LoadLevel("GameScene");
				}
				else
				{
					RemoveOrderTexts();
					mode = 3;
				}
				
				order_set.menuCount = 0;
				order_set.menuList = null;
				break;
			case 6:
				//order done

				b_menu_move_lock = false;

				if(b_menu4_OK_NO)
				{
					SendData(msg);
					RemoveOrderTexts();

					doFixedUpdate = true;
					move_num = 7;
					//Application.LoadLevel("GameScene");
				}
				else
				{
					RemoveOrderTexts();
					mode = 3;
				}
				
				order_set.menuCount = 0;
				order_set.menuList = null;
				break;
			}

		}
		else if(mode == 5)
		{
			switch(code)
			{
			case 1:
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				btn_yes_1.SetActive(true);
				btn_no_1.SetActive(false);
				b_move_yes_no = true;
				break;
			case 2:
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				btn_yes_1.SetActive(false);
				btn_no_1.SetActive(true);
				b_move_yes_no = false;
				break;
			case 5:
				b_menu_move_lock = false;
				
				if(b_move_yes_no)
				{
					AudioSource.PlayClipAtPoint(Selected_Yes_No, transform.position);
					Application.LoadLevel("GameScene");
				}
				else
				{
					mode = 1;
					doFixedUpdate = true;
					move_num = 8;
				}
				break;
			case 6:
				
				b_menu_move_lock = false;
				
				if(b_move_yes_no)
				{
					AudioSource.PlayClipAtPoint(Selected_Yes_No, transform.position);
					Application.LoadLevel("GameScene");
				}
				else
				{
					mode = 1;
					doFixedUpdate = true;
					move_num = 8;
				}

				break;
			}
		}
	}

	private bool[] b_show_menu = new bool[3] {false, false, false};

	// if num is -1 then turn off all
	void Show_Bright_Menu(int num)
	{
		num = num - 1;

		// turn off
		for(int i=0; i<3; i++)
		{
			if(b_show_menu[i] == true)
			{
				foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Menu" + (i+1).ToString() ))
				{
					obj.transform.position += new Vector3(0f, 0f, 20f);
				}
				b_show_menu[i] = false;
			}
		}

		// turn on selected one
		if(num != -1)
		{
			if(b_show_menu[num] == false)
			{
				foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Menu" + (num+1).ToString() ))
				{
					obj.transform.position -= new Vector3(0f, 0f, 20f);
				}
				b_show_menu[num] = true;
			}
		}


	}

	// 1 left, 2 right, 3 up, 4 down, 5 tap, 6 double tap, 7 finger_1, 8 finger_2, 9 finger_3
	void GetKey()
	{
		if(!b_menu_move_lock)
		{
			if(Input.GetKeyDown(KeyCode.A))
			{
				mode = 1;
				//selected_submenu = 1;

				Show_Bright_Menu(1);

				order_set.menuCount = 0;
				order_set.menuList = null;
				AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
			}
			else if(Input.GetKeyDown(KeyCode.B))
			{
				if(SubMenus == null)
				{
					// Warning
					Alarm_1.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else
				{
					Show_Bright_Menu(2);

					mode = 2;
					//selected_submenu = 1;

					order_set.menuCount = 0;
					order_set.menuList = null;
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				}
			}
			else if(Input.GetKeyDown(KeyCode.C))
			{
				if(mode == 1 && SubMenus == null)
				{
					// Warning (please select menu)
					Alarm_1.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else if((mode == 1 || mode == 2) && Order_List.Count == 0)
				{
					// Warning (orderlist is empty)
					Alarm_2.SetActive(true);
					move_num = 6;
					doFixedUpdate = true;
				}
				else
				{
					Show_Bright_Menu(3);

					mode = 3;

					order_set.menuCount = 0;
					order_set.menuList = null;
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				}
			}
			else if(Input.GetKeyDown(KeyCode.D))
			{
				Show_Bright_Menu(0);

				mode = 4;
				b_menu_move_lock = true;

				// should apply for all struct which go for menu 4
				// and if go back to previous menu, it should be NULL
				order_set.menuCount = l_MenuList.Count;
				order_set.menuList = new Menu_Set[l_MenuList.Count];
				l_MenuList.CopyTo(order_set.menuList);
				//Debug.Log("length : "+order_set.menuList.Length);
			}
			else if(Input.GetKeyDown(KeyCode.K))
			{

			}
			else if(Input.GetKeyDown(KeyCode.L))
			{

			}
		}
		ShowMode ();


		if(mode == 1)
		{
				if(Input.GetKeyDown(KeyCode.UpArrow))
				{
					if(selected_menu < 6)
					{
						AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
						selected_menu++;
						// 1.25 up or down	
						g_temp_y = Menu.transform.position.y + 1.25f; // 1.25 = distance to move
						doFixedUpdate = true;
						move_num = 1;
						
						ChangeMenuColor();
					}

				}
				else if(Input.GetKeyDown(KeyCode.DownArrow))
				{
					if(selected_menu > 1)
					{
						AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
						selected_menu--;
						// 1.25 up or down
						g_temp_y = Menu.transform.position.y - 1.25f; // 1.25 = distance to move
						doFixedUpdate = true;
						move_num = 2;

						ChangeMenuColor();
					}
				}
				else if(Input.GetKeyDown(KeyCode.Space)) // go to next
				{
					mode = 2;
					selected_submenu = 1;
					ShowSubMenu (selected_menu);
					AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
				}
				else if(Input.GetKeyDown(KeyCode.Return)) // order
				{
					if(l_MenuList.Count == 0)
					{
						Alarm_2.SetActive(true);
						move_num = 6;
						doFixedUpdate = true;

						Debug.Log ("There is no list of order");
						return;
					}
					b_menu4_maketext = false;
					mode = 4;

					b_menu_move_lock = true;

					// should apply for all struct which go for menu 4
					// and if go back to previous menu, it should be NULL
					order_set.menuCount = l_MenuList.Count;
					order_set.menuList = new Menu_Set[l_MenuList.Count];
					l_MenuList.CopyTo(order_set.menuList);
				}
		}
		else if(mode == 2)
		{
				
				if(Input.GetKeyDown(KeyCode.LeftArrow))
				{
					if(!isSelected_Count)
					{
						if(selected_submenu < SubMenus.Length)
						{
							SelectSubMenu(selected_submenu + 1);
							AudioSource.PlayClipAtPoint(BookPage, transform.position);
						}
					}
					else
					{
						// change count
						if(selected_submenu_count > 0)
						{
							AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
							SelectSubMenuCount(selected_submenu_count - 1);
						}
					}
				}
				else if(Input.GetKeyDown(KeyCode.RightArrow)) //choice sub-menu or count
				{
					if(!isSelected_Count)
					{
						if(selected_submenu > 1)
						{
							SelectSubMenu(selected_submenu - 1);
							AudioSource.PlayClipAtPoint(BookPage, transform.position);
						}
					}
					else
					{
						// change count
						if(selected_submenu_count < 9)
						{
							AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
							SelectSubMenuCount(selected_submenu_count + 1);
						}	
					}
				}
				else if(Input.GetKeyDown(KeyCode.Space))
				{
					if(!isSelected_Count)
					{
						AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
						b_menu_move_lock = true;
						ShowCount(0, true);
						isSelected_Count = true;
					}
					else
					{
						AudioSource.PlayClipAtPoint(Selected_Count, transform.position);
						b_menu_move_lock = false;
						// choose done
						ShowCount(0, false);
						isSelected_Count = false;

						AddToOrder( (selected_menu * 100) + selected_submenu-1, selected_submenu_count );
					}
				}
				else if(Input.GetKeyDown(KeyCode.Return))
				{
					if(l_MenuList.Count == 0)
					{
						Alarm_2.SetActive(true);
						move_num = 6;
						doFixedUpdate = true;

						Debug.Log ("There is no list of order");
						return;
					}
					b_menu4_maketext = false;
					mode = 4;

					b_menu_move_lock = true;

					// should apply for all struct which go for menu 4
					// and if go back to previous menu, it should be NULL
					order_set.menuCount = l_MenuList.Count;
					order_set.menuList = new Menu_Set[l_MenuList.Count];
					l_MenuList.CopyTo(order_set.menuList);
				}
		}
		else if(mode == 3)
		{
				if(Input.GetKeyDown(KeyCode.DownArrow))
				{
					if(!isSelected_Count)
					{
						AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
						SetOrderPosition(1);
						
						int pos = order_focus_num + 1;
						if (pos > 0 && pos < 5 && pos <= Order_List.Count)
						{
							move_num = 5;
							doFixedUpdate = true;
							temp_focus = pos;
						}
						//SetOrderFocus(order_focus_num + 1);
					}
				}
				else if(Input.GetKeyDown(KeyCode.UpArrow))
				{
					if(!isSelected_Count)
					{
						AudioSource.PlayClipAtPoint(UpDown_Move, transform.position);
						SetOrderPosition(2);
						
						int pos = order_focus_num - 1;
						if (pos > 0 && pos < 5 && pos <= Order_List.Count)
						{
							move_num = 5;
							doFixedUpdate = true;
							temp_focus = pos;
						}
						//SetOrderFocus(order_focus_num - 1);
					}
				}
				else if(Input.GetKeyDown(KeyCode.LeftArrow))
				{
					if(isSelected_Count && selected_submenu_count > 0)
					{
						AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
						SelectSubMenuCount(selected_submenu_count - 1);
					}
				}
				else if(Input.GetKeyDown(KeyCode.RightArrow))
				{
					if(isSelected_Count && selected_submenu_count < 9)
					{
						AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
						SelectSubMenuCount(selected_submenu_count + 1);
					}
				}
				else if(Input.GetKeyDown(KeyCode.Space)) // change count
				{
					if(!isSelected_Count)
					{
						AudioSource.PlayClipAtPoint(Move_Scene, transform.position);
						b_menu_move_lock = true;
						selected_submenu_count = l_MenuList[selected_order-1].count;
						SelectSubMenuCount(selected_submenu_count);
						ShowCount(order_focus_num, true);
						isSelected_Count = true;
					}
					else
					{
						AudioSource.PlayClipAtPoint(Selected_Count, transform.position);
						b_menu_move_lock = false;
						// choose done
						ShowCount(order_focus_num, false);
						isSelected_Count = false;

						ModifyOrder(selected_order, selected_submenu_count);

						selected_submenu_count = 0;
						SelectSubMenuCount (0);
					}
				}
				else if(Input.GetKeyDown(KeyCode.Return))
				{
					if(l_MenuList.Count == 0)
					{
						Alarm_2.SetActive(true);
						move_num = 6;
						doFixedUpdate = true;

						Debug.Log ("There is no list of order");
						return;
					}
					b_menu4_maketext = false;
					mode = 4;

					b_menu_move_lock = true;

					// should apply for all struct which go for menu 4
					// and if go back to previous menu, it should be NULL
					order_set.menuCount = l_MenuList.Count;
					order_set.menuList = new Menu_Set[l_MenuList.Count];
					l_MenuList.CopyTo(order_set.menuList);
				}
		}
		else if(mode == 4)
		{
				string msg = order_set.menuCount.ToString();
				msg+= ",";
				for(int i = 0; i < order_set.menuCount; i++)
				{
					msg += order_set.menuList[i].type.ToString() + " ";
					msg += order_set.menuList[i].count.ToString();
					if(i != order_set.menuCount-1)
					{
						msg += ",";
					}
				}
				if(Input.GetKeyDown(KeyCode.LeftArrow)) //choice yes or no
				{
					AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
					b_menu4_OK_NO = true;
				}
				else if(Input.GetKeyDown(KeyCode.RightArrow))
				{
					AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
					b_menu4_OK_NO = false;
				}
				else if(Input.GetKeyDown(KeyCode.Space)) // OK
				{
					b_menu_move_lock = false;

					//order done
					if(b_menu4_OK_NO)
					{
						AudioSource.PlayClipAtPoint(Selected_Yes_No, transform.position);
						SendData(msg);
						RemoveOrderTexts();

						doFixedUpdate = true;
						move_num = 7;

						//Application.LoadLevel("GameScene");
					}
					else
					{
						RemoveOrderTexts();
						mode = 3;
					}
					
					order_set.menuCount = 0;
					order_set.menuList = null;

					
				}
				else if(Input.GetKeyDown(KeyCode.Return)) // OK
				{
					//order done
					b_menu_move_lock = false;
					
					if(b_menu4_OK_NO)
					{
						AudioSource.PlayClipAtPoint(Selected_Yes_No, transform.position);
						SendData(msg);
						RemoveOrderTexts();

						doFixedUpdate = true;
						move_num = 7;
					
						// Application.LoadLevel("GameScene");
					}
					else
					{
						RemoveOrderTexts();
						mode = 3;
					}

					order_set.menuCount = 0;
					order_set.menuList = null;
				}
		}
		else if(mode == 5)
		{
			if(Input.GetKeyDown(KeyCode.LeftArrow)) //choice yes or no
			{
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				btn_yes_1.SetActive(true);
				btn_no_1.SetActive(false);
				b_move_yes_no = true;
			}
			else if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				AudioSource.PlayClipAtPoint(Select_Yes_No, transform.position);
				btn_yes_1.SetActive(false);
				btn_no_1.SetActive(true);
				b_move_yes_no = false;
			}
			else if(Input.GetKeyDown(KeyCode.Space))
			{
				b_menu_move_lock = false;
				
				if(b_move_yes_no)
				{
					AudioSource.PlayClipAtPoint(Selected_Yes_No, transform.position);
					Application.LoadLevel("GameScene");
				}
				else
				{
					mode = 1;
					doFixedUpdate = true;
					move_num = 8;
				}
			}
			else if(Input.GetKeyDown(KeyCode.Return))	
			{
				b_menu_move_lock = false;
				
				if(b_move_yes_no)
				{
					Application.LoadLevel("GameScene");
				}
				else
				{
					mode = 1;
					doFixedUpdate = true;
					move_num = 8;
				}
			}	

		}
	}
	
	void SelectSubMenu(int num)
	{
		// X - 1 : -4.6   2 : -2.8     3 : 0.3    4 : 3.4     5 : 5.2
		//          -> +1.8     -> +3.1     -> +1.8      -> +3.1
		// Z -      2         1            0          1           2
		//Scale    2 2       3 3          5 5        3 3         2 2

		bool dir = true; //right
		if(num < selected_submenu)
			dir = false; //left

		target_Pos = new Vector3[SubMenus.Length];
		target_Scale = new Vector3[SubMenus.Length];

		if(dir)
		{
			for(int i=0; i<SubMenus.Length; i++)
			{
				if(i == selected_submenu-1)
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x - 2.1f, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
					target_Scale[i] = new Vector3(SubMenus[i].transform.localScale.x - 1.5f,
					                              SubMenus[i].transform.localScale.y - 1.935f,
					                              SubMenus[i].transform.localScale.z);
				}
				else if(((selected_submenu-1) - i) == 1) // 1 more small
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
					target_Scale[i] = new Vector3(0f,0f,0f);
				}
				else if(i - (selected_submenu-1) == 1) // 1 more big
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x - 2.1f, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z - 1f);
					target_Scale[i] = new Vector3(SubMenus[i].transform.localScale.x + 1.5f,
					                              SubMenus[i].transform.localScale.y + 1.935f,
					                              SubMenus[i].transform.localScale.z);
				}
				else if(((selected_submenu-1) - i) > 1) // over 2 more smaller
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
				}
				else if(i - (selected_submenu-1) > 1) // over 2 more bigger
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z - 1f);
					if(i == selected_submenu+1)
						target_Scale[i] = new Vector3(3.5f, 4.515f, 1f);
					else
						target_Scale[i] = SubMenus[i].transform.localScale;
				}
			}
			move_num = 3;
		}
		else
		{
			for(int i=0; i<SubMenus.Length; i++)
			{
				if(i == selected_submenu-1)
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x + 2.1f, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
					target_Scale[i] = new Vector3(SubMenus[i].transform.localScale.x - 1.5f,
					                              SubMenus[i].transform.localScale.y - 1.935f,
					                              SubMenus[i].transform.localScale.z);
				}
				else if(((selected_submenu-1) - i) == 1) // 1 more small
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x + 2.1f, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z - 1f);
					target_Scale[i] = new Vector3(SubMenus[i].transform.localScale.x + 1.5f,
					                              SubMenus[i].transform.localScale.y + 1.935f,
					                              SubMenus[i].transform.localScale.z);
				}
				else if(i - (selected_submenu-1) == 1) // 1 more big
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
					target_Scale[i] = new Vector3(0f,0f,0f);
				}
				else if(((selected_submenu-1) - i) > 1) // over 2 more smaller
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z - 1f);
					if(i == selected_submenu-3)
						target_Scale[i] = new Vector3(3.5f, 4.515f, 1f);
					else
						target_Scale[i] = SubMenus[i].transform.localScale;
				}
				else if(i - (selected_submenu-1) > 1) // over 2 more bigger
				{
					target_Pos[i] = new Vector3(SubMenus[i].transform.position.x, 
					                            SubMenus[i].transform.position.y,
					                            SubMenus[i].transform.position.z + 1f);
				}
				else
				{
					Debug.Log("SelectSubMenu() Error!!!");
				}
			}
			move_num = 4;
		}
		doFixedUpdate = true;
		fixed_count = new bool[SubMenus.Length];
		for(int i=0; i<fixed_count.Length; i++)
			fixed_count[i] = false;

		updated_selected_submenu = num;
	}

	void SelectSubMenuCount(int num)
	{
		SubMenu_Count.renderer.material.mainTexture = Count_Number [num];
		selected_submenu_count = num;
	}

	private void CreateObject(string name, string path)
	{
		GameObject Creature = Instantiate(Resources.Load(path)) as GameObject;
		Creature.name = name;
	}

	int SumTotalPrice()
	{
		int price = 0;

		for(int i = 0; i < l_MenuList.Count; i++)
		{
			for(int j = 0; j < 24; j++)
			{
				if(l_MenuList[i].type == goods_code[j])
				{
					price += l_MenuList[i].count * goods_price[j];
					break;
				}
			}
		}

		return price;
	}

	void AddToOrder(int type, int count)
	{
		if(selected_submenu_count != 0)
		{
			// make new object from prefab
			GameObject new_order = null;
			Order_List.Add(new_order);
			string name = SubMenu_prefab[selected_menu-1][selected_submenu-1].name + "_";

			GameObject txt_order = Instantiate(Resources.Load("OrderText")) as GameObject;
			Order_List_Text.Add(txt_order);

			//give name
			for(int i=0; i<24; i++)
			{
				if(type == goods_code[i])
					txt_order.GetComponent<TextMesh>().text = goods_name2[i];
			}

			Order_List[Order_List.Count-1] = Instantiate(Resources.Load(name), new Vector3(0.3f, -3.5f, -5f), Quaternion.identity) as GameObject;
			Order_List[Order_List.Count-1].transform.localScale = new Vector3(0.9f, 0.9f, 1f);

			//set frames
			/*
			GameObject new_bg = Instantiate(Resources.Load("OrderList")) as GameObject;
			if(Order_List.Count <= 3)
			{
				new_bg.transform.position = new Vector3(6.97f, 3.83f-(Order_List.Count * 2.1f), -4.9f);
			}
			*/
			// set orderlist
			if(Order_List.Count == 1)
			{
				Order_List [Order_List.Count - 1].transform.position = new Vector3 (6.15f, 1.35f, -5f);
				Order_List_Text [Order_List.Count - 1].transform.position = new Vector3 (7.65f, 1.35f, -5f);
			}
			else
			{
				Order_List [Order_List.Count - 1].transform.position = new Vector3 (6.15f, 
	                                           						  Order_List[Order_List.Count-2].transform.position.y - 1.45f, 
				                                                                    -5f);
				Order_List_Text [Order_List.Count - 1].transform.position = new Vector3 (7.65f, 
				                                                                    Order_List[Order_List.Count-2].transform.position.y - 1.45f, 
				                                                                    -5f);
			}

			Menu_Set menuset1;

			menuset1.type = type;
			menuset1.count = count;

			//add to list
			l_MenuList.Add(menuset1);
		}
		else
		{
		}

		selected_submenu_count = 0;
		SelectSubMenuCount (0);
	}

	void ModifyOrder(int sequence, int count)
	{
		if(count == 0)
		{
			//how about show message like this "are you sure?"

			//delete current order
			l_MenuList.RemoveAt(sequence-1);
			Destroy(Order_List[sequence-1]);
			Order_List.RemoveAt(sequence-1);
			//delete text
			Destroy(Order_List_Text[sequence-1]);
			Order_List_Text.RemoveAt(sequence-1);

			if( (sequence) > Order_List.Count )
			{
				int pos = order_focus_num - 1;
				if (pos > 0 && pos < 5 && pos <= Order_List.Count)
				{
					move_num = 5;
					doFixedUpdate = true;
					temp_focus = pos;
				}
				selected_order--;
			}
			SetOrderPosition(0); // line up
		}
		else
		{
			Menu_Set temp;
			temp.type = l_MenuList [sequence - 1].type;
			temp.count = count;
			l_MenuList[sequence-1] = temp;
		}

	}

	void SetOrderPosition (int dir)
	{
		switch(dir)
		{
		case 0:  // line up
			if(Order_List.Count == 0)
			{
				//go to mode 2
				mode = 2;
			}
			else
			{
				Order_List[0].transform.position = new Vector3(6.15f, 1.35f, Order_List[0].transform.position.z);
				Order_List_Text[0].transform.position = new Vector3(7.65f, 1.35f, Order_List_Text[0].transform.position.z);

				for(int i = 1; i < Order_List.Count; i++) // from second object
				{
					Order_List[i].transform.position = new Vector3(Order_List[i-1].transform.position.x,
					                                               Order_List[i-1].transform.position.y - 1.45f,
					                                               Order_List[i-1].transform.position.z);
					Order_List_Text[i].transform.position = new Vector3(Order_List_Text[i-1].transform.position.x,
					                                                    Order_List_Text[i-1].transform.position.y - 1.45f,
					                                                    Order_List_Text[i-1].transform.position.z);
				}
			}
			break;
		case 1:  // go next
		{
			//make outside. (global)
			Vector3[] targetPos = new Vector3[Order_List.Count];
			//굳이 배열 안만들고 그냥 하나로 해도 될듯..
			for(int i=0; i<targetPos.Length; i++)
			{
				targetPos[i] = new Vector3(Order_List[i].transform.position.x,
				                           Order_List[i].transform.position.y + 1.45f,
				                           Order_List[i].transform.position.z);
			}

			if(selected_order != Order_List.Count)
			{
				selected_order++;

				if(order_focus_num == 4)
				{
					for(int i = 0; i < Order_List.Count; i++)
					{
						Order_List[i].transform.position = new Vector3(Order_List[i].transform.position.x,
						                                               Order_List[i].transform.position.y + 1.45f,
						                                               Order_List[i].transform.position.z);
						Order_List_Text[i].transform.position = new Vector3(Order_List_Text[i].transform.position.x,
						                                                    Order_List_Text[i].transform.position.y + 1.45f,
						                                                    Order_List_Text[i].transform.position.z);
					}
				}
			}
			break;
		}

		case 2:  // go previous
		{

			if(selected_order != 1)
			{
				selected_order--;

				if(order_focus_num == 1)
				{
					for(int i = 0; i < Order_List.Count; i++)
					{
						Order_List[i].transform.position = new Vector3(Order_List[i].transform.position.x,
						                                               Order_List[i].transform.position.y - 1.45f,
						                                               Order_List[i].transform.position.z);
						Order_List_Text[i].transform.position = new Vector3(Order_List_Text[i].transform.position.x,
						                                                    Order_List_Text[i].transform.position.y - 1.45f,
						                                                    Order_List_Text[i].transform.position.z);
					}
				}
			}
			break;
		}
		}
	}

	void SetOrderFocus(int position)
	{
		/*switch(position)
		{
		case 1:
			Order_Focus.transform.position = new Vector3(7.25f, 1.8f, -5f);
			break;
		case 2:
			Order_Focus.transform.position = new Vector3(7.25f, 0.6f, -5f);
			break;
		case 3:
			Order_Focus.transform.position = new Vector3(7.25f, -0.7f, -5f);
			break;
		case 4:
			Order_Focus.transform.position = new Vector3(7.25f, -1.9f, -5f);
			break;
		}*/

		Vector3 targetPos;
		if(position == 1)
			targetPos = new Vector3(7.08f, 1.35f, -24f);
		else if(position == 2)
			targetPos = new Vector3(7.08f, -0.1f, -24f);
		else if(position == 3)
			targetPos = new Vector3(7.08f, -1.55f, -24f);
		else
			targetPos = new Vector3(7.08f, -3f, -24f);

		switch(position)
		{
		case 1:
			Order_Focus.transform.position = Vector3.Lerp(Order_Focus.transform.position, targetPos, 
			                                              Time.deltaTime * 10f);
			break;
		case 2:
			Order_Focus.transform.position = Vector3.Lerp(Order_Focus.transform.position, targetPos, 
			                                              Time.deltaTime * 10f);
			break;
		case 3:
			Order_Focus.transform.position = Vector3.Lerp(Order_Focus.transform.position, targetPos, 
			                                              Time.deltaTime * 10f);
			break;
		case 4:
			Order_Focus.transform.position = Vector3.Lerp(Order_Focus.transform.position, targetPos, 
			                                              Time.deltaTime * 10f);
			break;
		}

		if(Vector3.Distance(Order_Focus.transform.position, targetPos) < 0.01f)
		{
			Order_Focus.transform.position = targetPos;
			order_focus_num = position;
			doFixedUpdate = false;
			move_num = 0;
		}
	}

	void ShowCount(int position, bool _switch)
	{
		//position - 0: main , 1: order1 , 2: order2 , 3:order3 . . . .
		
		switch(position)
		{
		case 0:
			SubMenu_Count.transform.position = new Vector3(0.3f, -4.15f, -30f);
			SubMenu_Count.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
			break;
		case 1:
			SubMenu_Count.transform.position = new Vector3(7.1f, 0.71f, -30f);
			SubMenu_Count.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			break;
		case 2:
			SubMenu_Count.transform.position = new Vector3(7.1f, -0.73f, -30f);
			SubMenu_Count.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			break;
		case 3:
			SubMenu_Count.transform.position = new Vector3(7.1f, -2.25f, -30f);
			SubMenu_Count.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			break;
		case 4:
			SubMenu_Count.transform.position = new Vector3(7.1f, -3.66f, -30f);
			SubMenu_Count.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			break;
		default:
			break;
		}
		
		//Plus.SetActive (_switch);
		//Minus.SetActive (_switch);
		SubMenu_Count.SetActive (_switch);
	}


	public static int byteArrayDefrag(byte[] sData)
	{
		int endLength = 0;
		
		for(int i = 0; i < sData.Length; i++)
		{
			if((byte)sData[i] != (byte)0)
			{
				endLength = i;
			}
		}
		
		return endLength; 
	}

	void ChangeMenuColor()
	{
		switch(selected_menu)
		{
		case 1:
			Menu.renderer.material.color = Color.white;
			Menu2.renderer.material.color = Color.black;
			Menu3.renderer.material.color = Color.black;
			Menu4.renderer.material.color = Color.black;
			Menu5.renderer.material.color = Color.black;
			Menu6.renderer.material.color = Color.black;
			
			break;
		case 2:
			Menu.renderer.material.color = Color.black;
			Menu2.renderer.material.color = Color.white;
			Menu3.renderer.material.color = Color.black;
			Menu4.renderer.material.color = Color.black;
			Menu5.renderer.material.color = Color.black;
			Menu6.renderer.material.color = Color.black;
			
			break;
		case 3:
			Menu.renderer.material.color = Color.black;
			Menu2.renderer.material.color = Color.black;
			Menu3.renderer.material.color = Color.white;
			Menu4.renderer.material.color = Color.black;
			Menu5.renderer.material.color = Color.black;
			Menu6.renderer.material.color = Color.black;
			
			break;
		case 4:
			Menu.renderer.material.color = Color.black;
			Menu2.renderer.material.color = Color.black;
			Menu3.renderer.material.color = Color.black;
			Menu4.renderer.material.color = Color.white;
			Menu5.renderer.material.color = Color.black;
			Menu6.renderer.material.color = Color.black;
			
			break;
		case 5:
			Menu.renderer.material.color = Color.black;
			Menu2.renderer.material.color = Color.black;
			Menu3.renderer.material.color = Color.black;
			Menu4.renderer.material.color = Color.black;
			Menu5.renderer.material.color = Color.white;
			Menu6.renderer.material.color = Color.black;
			
			break;
		case 6:
			Menu.renderer.material.color = Color.black;
			Menu2.renderer.material.color = Color.black;
			Menu3.renderer.material.color = Color.black;
			Menu4.renderer.material.color = Color.black;
			Menu5.renderer.material.color = Color.black;
			Menu6.renderer.material.color = Color.white;
			break;
		default:
			break;
		}
	}

	void RemoveOrderTexts()
	{
		/*
		if(GameObject.Find("txt_Order_Title_1") != null)
		{
			Destroy (GameObject.Find ("txt_Order_Title_1"));
			Destroy (GameObject.Find ("txt_Order_Title_2"));

			try
			{
				for(int i=0; i<40; i++)
					Destroy(GameObject.Find("txt_Order_" + (i+1).ToString() ));
			}
			catch(Exception e)
			{	}
		}
		*/
		if(l_txt_List.Count != 0)
		{
			for(int i=0; i < l_txt_List.Count; i++)
				Destroy(l_txt_List[i]);
			l_txt_List.Clear();
		}
	}

	private List<GameObject> l_txt_List = new List<GameObject>();

	void CreateOrderPaperText()
	{
		// oooooo
		GameObject txt_title = Instantiate(Resources.Load("OrderText"), new Vector3(0.53f, 2.82f, -15.5f), Quaternion.identity) as GameObject;
		txt_title.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
		txt_title.GetComponent<TextMesh>().text = "주문서";
		txt_title.name = "txt_Order_Title_1";
		l_txt_List.Add (txt_title);
		GameObject txt_title2 = Instantiate(Resources.Load("OrderText"), new Vector3(0.53f, 2.08f, -15.5f), Quaternion.identity) as GameObject;
		txt_title2.transform.localScale = new Vector3(0.25f, 0.25f, 0.3f);
		txt_title2.GetComponent<TextMesh>().text = "   품명          수량             금액";
		txt_title2.name = "txt_Order_Title_2";
		l_txt_List.Add (txt_title2);

		float y_pos = 1.3f;
		for(int i=0; i<order_set.menuCount; i++)
		{
			GameObject txt_Orders = Instantiate(Resources.Load("OrderText"), new Vector3(-1.1f, y_pos, -15.5f), Quaternion.identity) as GameObject;
			txt_Orders.transform.localScale = new Vector3(0.22f, 0.22f, 0.3f);
			txt_Orders.name = "txt_Order_" + (i + 1).ToString();
			GameObject txt_Orders2 = Instantiate(Resources.Load("OrderText"), new Vector3(0.56f, y_pos, -15.5f), Quaternion.identity) as GameObject;
			txt_Orders2.transform.localScale = new Vector3(0.22f, 0.22f, 0.3f);
			txt_Orders2.name = "txt_Order_" + (i + 1).ToString();
			GameObject txt_Orders3 = Instantiate(Resources.Load("OrderText"), new Vector3(2.5f, y_pos, -15.5f), Quaternion.identity) as GameObject;
			txt_Orders3.transform.localScale = new Vector3(0.22f, 0.22f, 0.3f);
			txt_Orders3.name = "txt_Order_" + (i + 1).ToString();

			l_txt_List.Add(txt_Orders);
			l_txt_List.Add(txt_Orders2);
			l_txt_List.Add(txt_Orders3);

			string nameStr = "";
			int price = 0;

			for(int j=0; j<24; j++)
			{
				if(goods_code[j] == order_set.menuList[i].type)
				{
					nameStr = goods_name[j];
					price = goods_price[j];
					break;
				}
			}

			// get price
			price = price * order_set.menuList[i].count;
			// change text
			txt_Orders.GetComponent<TextMesh>().text = nameStr;
			txt_Orders2.GetComponent<TextMesh>().text = order_set.menuList[i].count.ToString();
			txt_Orders3.GetComponent<TextMesh>().text = price.ToString();

			y_pos -= 0.5f;
		}
	}

	void OnApplicationQuit()
	{
		try
		{
			Debug.Log("Quiting........");
			sockClient.Close();
		}
		catch(Exception e)
		{ Debug.Log ("Quit Error : " + e.Message); }
	}

}








