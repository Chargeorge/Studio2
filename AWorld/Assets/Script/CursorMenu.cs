using UnityEngine;
using System.Collections;

public class CursorMenu : MonoBehaviour {

	public float moveSpeed;

	public float movingRotateSpeed;
	public float restingRotateSpeed;
	public float loadingRotateSpeed;
	private float rotateSpeed;
	private float rotatingLeft;	//-1 if rotating right, 1 if rotating left

	public GameObject menu;
	private MainMenu menuScript;

	// Use this for initialization
	void Start () {

		menuScript = menu.GetComponent<MainMenu>();
		
		rotateSpeed = restingRotateSpeed * -1;
		rotatingLeft = -1;
		
		//transform.position.x = startPos;
	
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 pos = transform.position;

		float x = Input.GetAxis("HorizontalPlayer1") * moveSpeed * Time.deltaTime;
		float y = Input.GetAxis("VerticalPlayer1") * moveSpeed * Time.deltaTime;

		//pos.x += x;
		if (!menu.GetComponent<MainMenu>().loadingNewScreen) pos.y += y;

		if(pos.y > -0.4f){
			pos.y = -0.4f;
		} else if(pos.y < -3.0f){
			pos.y = -3.0f;
		}

		//transform.position = new Vector3(-0.16f, Mathf.Clamp(Time.time, 0.26F, -1.2F), -5.4f);
		transform.position = pos;
		
		if (!menuScript.quitting && !menuScript.loadingNewScreen) {
			if (y < 0) {
				rotateSpeed = -1 * movingRotateSpeed;
				rotatingLeft = -1;
			}
			else if (y > 0) { 
				rotateSpeed = movingRotateSpeed;
				rotatingLeft = 1;
			}
			else {
				rotateSpeed = restingRotateSpeed * rotatingLeft;
			}
		}
		if (menuScript.loadingNewScreen) {
			transform.RotateAround (transform.position, Vector3.forward, loadingRotateSpeed * rotatingLeft * Time.deltaTime);
		}
		else if (!menuScript.quitting) {	//Freeze rotation if quitting
			transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * Time.deltaTime);
		}		
	}
}
