using UnityEngine;
using System.Collections;

public class Cursor2 : MonoBehaviour {

	public float speed;
	public GameObject menu;
	
	// Use this for initialization
	void Start () {
		
		OptionsManager optionsScript = menu.GetComponent<OptionsManager>();
		
		//transform.position.x = startPos;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 pos = transform.position;
		
		float x = Input.GetAxis("HorizontalPlayer1") * speed * Time.deltaTime;
		float y = Input.GetAxis("VerticalPlayer1") * speed * Time.deltaTime;
		
		pos.x += x;
		//pos.y += y;
		
		if(pos.x < 2.85f && pos.y == 0.9f){ //if the cursor is at the left edge of the top line, make it go to BACK
			pos.x = 14.5f;
			pos.y = -3.42f;
		} else if(pos.x > 12.0f && pos.y == 0.9f){
			pos.x = 4.85f;
			pos.y = -2.6f;
		}

		if(pos.y == -2.6f && pos.x > 9.7f){ // if the cursor is at the right edge of the bottom line, make it go to BACK
			pos.x = 13.8f;
			pos.y = -3.42f;
		} else if(pos.x < 4.4f && pos.y == -2.6f){ //if the cursor is at the left edge of the bottom line, make it go to the right edge of the top line
			pos.x = 11.75f;
			pos.y = 0.9f;
		}

		if(pos.y == -3.42f && pos.x > 15.3f){ // if the cursor is at the right edge of BACK, make it go back to the left edge of the top line
			pos.y = 0.9f;
			pos.x = 2.9f;
		} else if(pos.y == -3.42f && pos.x < 13.7f){
			pos.y = -2.6f;
			pos.x = 9.6f;
		}
		
		//transform.position = new Vector3(-0.16f, Mathf.Clamp(Time.time, 0.26F, -1.2F), -5.4f);
		transform.position = pos;
		
	}
}
