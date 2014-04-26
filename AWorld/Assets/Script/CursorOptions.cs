using UnityEngine;
using System.Collections;

public class CursorOptions : MonoBehaviour {

	public float moveSpeed;

	public float movingRotateSpeed;
	public float restingRotateSpeed;
	private float rotateSpeed;
	private float rotatingLeft;	//-1 if rotating right, 1 if rotating left

	public GameObject options;
	
	// Use this for initialization
	void Start () {
		
		OptionsManager optionsScript = options.GetComponent<OptionsManager>();
		rotateSpeed = restingRotateSpeed * -1;
		rotatingLeft = -1;
		
		//transform.position.x = startPos;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 pos = transform.position;
		
		float x = Input.GetAxis("HorizontalPlayer1") * moveSpeed * Time.deltaTime;
		float y = Input.GetAxis("VerticalPlayer1") * moveSpeed * Time.deltaTime;
		
		if (!options.GetComponent<OptionsManager>().launching) pos.x += x;
		//pos.y += y;
		
		if(pos.x < 2.85f && pos.y == 0.9f){ //if the cursor is at the left edge of the top line, make it stay there
			pos.x = 2.85f;
		} else if(pos.x > 12.0f && pos.y == 0.9f){
			pos.x = 4.85f;
			pos.y = -2.6f;
		}

		if(pos.y == -2.6f && pos.x > 9.7f){ // if the cursor is at the right edge of the bottom line, make it go to BACK
			pos.x = 13f;
			pos.y = -3.42f;
		} else if(pos.x < 4.4f && pos.y == -2.6f){ //if the cursor is at the left edge of the bottom line, make it go to the right edge of the top line
			pos.x = 11.75f;
			pos.y = 0.9f;
		}

		if(pos.y == -3.42f && pos.x > 14.5f){ // if the cursor is at the right edge of BACK, make it stay there
			pos.x = 14.5f;
		}
		if(pos.y == -3.42f && pos.x < 12.7f){
			pos.y = -2.6f;
			pos.x = 9.6f;
		}
		
		//transform.position = new Vector3(-0.16f, Mathf.Clamp(Time.time, 0.26F, -1.2F), -5.4f);
		transform.position = pos;			
		
		if (x > 0) {
			rotateSpeed = -1 * movingRotateSpeed;
			rotatingLeft = -1;
		}
		else if (x < 0) { 
			rotateSpeed = movingRotateSpeed;
			rotatingLeft = 1;
		}
		else {
			rotateSpeed = restingRotateSpeed * rotatingLeft;
		}
		
		transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * Time.deltaTime);
		
	}
}
