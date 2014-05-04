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

	Vector3 startPos;
	Vector3 optionsPos;
	Vector3 quitPos;
	Vector3 target;

	public float lerpRate;
	bool lerping;
	bool goingUpUpAndAway;
	bool goingLowLowLow;

	// Use this for initialization
	void Start () {

		float cursorDepth = -6f;

		menuScript = menu.GetComponent<MainMenu>();

		rotateSpeed = restingRotateSpeed;
		rotatingLeft = -1;
	
		startPos = new Vector3(-2.5f, -0.45f, cursorDepth);
		optionsPos = new Vector3(-2.5f, -1.65f, cursorDepth);
		quitPos = new Vector3(-2.5f, -2.95f, cursorDepth);
		lerping = false;
		lerpRate = 0.2f;
	
	}

	IEnumerator waitMenu(){
		yield return new WaitForSeconds(0.3f);
	}
	
	// Update is called once per frame
	void Update () {

		if(lerping){
			Vector3 newPos = transform.position;
			newPos.y = Mathf.Lerp(transform.position.y, target.y, lerpRate);
			transform.position = newPos;
			if(Mathf.Abs(Vector3.Magnitude(newPos-target)) < 0.01f){
				transform.position = target;
				lerping = false;
				goingUpUpAndAway = false;
				goingLowLowLow = false;
			}

			if(goingUpUpAndAway){
				rotateSpeed = movingRotateSpeed;
				rotatingLeft = 1;
			} else if(goingLowLowLow){
				rotateSpeed = movingRotateSpeed;
				rotatingLeft = -1;
			} else {
				rotateSpeed = restingRotateSpeed;
			}
		}

		float x = Input.GetAxis("HorizontalPlayer1") * moveSpeed * Time.deltaTime;
		float y = Input.GetAxis("VerticalPlayer1") * moveSpeed * Time.deltaTime;
		
		if (!menu.GetComponent<MainMenu>().loadingNewScreen && !lerping){
			if(MainMenu.startSelected){
				if(Input.GetAxis("VerticalPlayer1") < -0.1f){
					lerping = true;
					target = optionsPos;
					goingUpUpAndAway = false;
					goingLowLowLow = true;
					MainMenu.startSelected = false;
					MainMenu.optionsSelected = true;
					StartCoroutine("waitMenu");
				}
			}
			else if(MainMenu.optionsSelected){
				if(Input.GetAxis("VerticalPlayer1") > 0.1f){
						lerping = true;
						target = startPos;
						goingUpUpAndAway = true;
						goingLowLowLow = false;
						MainMenu.optionsSelected = false;
						MainMenu.startSelected = true;
						StartCoroutine("waitMenu");
				}else if(Input.GetAxis("VerticalPlayer1") < -0.1f){
						lerping = true;
						target = quitPos;
						goingUpUpAndAway = false;
						goingLowLowLow = true;
						MainMenu.optionsSelected = false;
						MainMenu.quitSelected = true;
						StartCoroutine("waitMenu");
				}
			}
			else if(MainMenu.quitSelected){
				if(Input.GetAxis("VerticalPlayer1") > 0.1f){
					lerping = true;
					target = optionsPos;
					goingUpUpAndAway = true;
					goingLowLowLow = false;
					MainMenu.quitSelected = false;
					MainMenu.optionsSelected = true;
					StartCoroutine("waitMenu");
				}
			}

		}
		
		if (!menuScript.quitting && !menuScript.loadingNewScreen) {
			/*if (y < 0) {
				rotateSpeed = -1 * movingRotateSpeed;
				rotatingLeft = -1;
			}
			else if (y > 0) { 
				rotateSpeed = movingRotateSpeed;
				rotatingLeft = 1;
			}
			else {
				rotateSpeed = restingRotateSpeed * rotatingLeft;
			}*/
		}
		if (menuScript.loadingNewScreen) {
			transform.RotateAround (transform.position, Vector3.forward, loadingRotateSpeed * rotatingLeft * Time.deltaTime);
			if (transform.renderer.material.color.a > 0f) { 
				Color32 newColor = transform.renderer.material.color;
				newColor.a -= (byte) (0.021f * 255f);
				transform.renderer.material.color = newColor;
			}
		}
		else if (!menuScript.quitting) {	//Freeze rotation if quitting
			transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * rotatingLeft * Time.deltaTime);				
		}		
	}
}
