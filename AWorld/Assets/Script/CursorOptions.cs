using UnityEngine;
using System.Collections;
using InControl;
public class CursorOptions : MonoBehaviour {

	public float moveSpeed;

	public float movingRotateSpeed;
	public float restingRotateSpeed;
	public float loadingRotateSpeed;
	private float rotateSpeed;
	private float targetRotateSpeed;
	private float rotatingLeft;	//-1 if rotating right, 1 if rotating left

	public GameObject options;
	private OptionsManager optionsScript;

	Vector3 playersPos;
	Vector3 fogPos;
	Vector3 waterPos;
	Vector3 speedPos;
	Vector3 sizePos;
	Vector3 tutorialPos;

	Vector3 backPos;
	Vector3 resetPos;
	Vector3 startPos;

	Vector3 target;

	bool goingRight;
	bool goingLeft;
	bool goingUp;
	bool goingDown;
	bool waiting;
	bool lerping;
	float lerpRate;
		
	private InputDevice _controller;
	// Use this for initialization
	void Start () {

		lerping = false;
		lerpRate = 0.2f;
		
		goingRight = false;
		goingLeft = false;
		goingUp = false;
		goingDown = false;
		
		optionsScript = options.GetComponent<OptionsManager>();
		rotateSpeed = restingRotateSpeed;
		targetRotateSpeed = rotateSpeed;
		rotatingLeft = -1;

		float cursorDepth = -8.9f;

		float firstLineHeight = 1.0f;
		float secondLineHeight = -2.2f;
		float thirdLineHeight = -3.4f;

		float firstColumn = 2.85f;
		float secondColumn = 7.4f;
		float thirdColumn = 11.9f;

		float firstColumnBottom = 1.1f;
		float secondColumnBottom = 7.4f;
		float thirdColumnBottom = 14f;
		
		playersPos = new Vector3(firstColumn, firstLineHeight, cursorDepth);
		fogPos = new Vector3(secondColumn, firstLineHeight, cursorDepth);
		waterPos = new Vector3(thirdColumn, firstLineHeight, cursorDepth);

		speedPos = new Vector3(firstColumn, secondLineHeight, cursorDepth);
		sizePos = new Vector3(secondColumn, secondLineHeight, cursorDepth);
		tutorialPos = new Vector3(thirdColumn, secondLineHeight, cursorDepth);

		backPos = new Vector3(firstColumnBottom, thirdLineHeight, cursorDepth);
		resetPos = new Vector3(secondColumnBottom, thirdLineHeight, cursorDepth);
		startPos = new Vector3(thirdColumnBottom, thirdLineHeight, cursorDepth);		
	}
	
	// Update is called once per frame
	void Update () {
		_controller = InputManager.ActiveDevice;
		if(lerping){
			Vector3 newPos = transform.position;
			newPos.x = Mathf.Lerp(newPos.x, target.x, lerpRate);
			newPos.y = Mathf.Lerp(newPos.y, target.y, lerpRate);
			transform.position = newPos;
			if(Mathf.Abs(Vector3.Magnitude(newPos-target)) < 0.07f){
				transform.position = target;
				Invoke ("TakeNewPosition", 0.01f);
			}
		}
		
		float x = _controller.LeftStick.X ;
		float y = _controller.LeftStick.Y ;
		
		if (!options.GetComponent<OptionsManager>().loadingNewScreen && !lerping && !waiting){
		
			// Failsafes
			if (OptionsManager.playersSelected) {
				target = playersPos;
			}
			else if (OptionsManager.fogSelected) {
				target = fogPos;
			}
			else if (OptionsManager.waterSelected) {
				target = waterPos;
			}
			else if (OptionsManager.speedSelected) {
				target = speedPos;
			}
			else if (OptionsManager.sizeSelected) {
				target = sizePos;
			}
			else if (OptionsManager.tutorialSelected) {
				target = tutorialPos;
			}
			else if (OptionsManager.backSelected) {
				target = backPos;
			}
			else if (OptionsManager.resetSelected) {
				target = resetPos;
			}
			else if (OptionsManager.startSelected) {
				target = startPos;
			}
			
			if(y > 0.1f){//if the player wants to go up	
				if(OptionsManager.speedSelected){
					lerping = true;
					target = playersPos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					target = fogPos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					target = waterPos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.backSelected){
					lerping = true;
					target = speedPos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.resetSelected){
					lerping = true;
					target = sizePos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					lerping = true;
					target = tutorialPos;
					goingDown = false;
					goingUp = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}
			}else if(y < -0.1f){//if the player wants to go down	
				if(OptionsManager.playersSelected){
					lerping = true;
					target = speedPos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.fogSelected){
					lerping = true;
					target = sizePos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					lerping = true;
					target = tutorialPos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.speedSelected){
					lerping = true;
					target = backPos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					target = resetPos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					target = startPos;
					goingDown = true;
					goingUp = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}
			}
			else if(x > 0.1f){//player wants to go right
				if(OptionsManager.playersSelected){
					lerping = true;
					target = fogPos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.fogSelected){
					lerping = true;
					target = waterPos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					//StartCoroutine("waitMenuTranslate", speedPos);
				}else if(OptionsManager.speedSelected){
					lerping = true;
					target = sizePos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					target = tutorialPos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					//StartCoroutine("waitMenuTranslate", backPos);
				}else if(OptionsManager.backSelected){
					lerping = true;
					target = resetPos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.resetSelected){
					lerping = true;
					target = startPos;
					goingRight = true;
					goingLeft = false;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					//don't do shit cause that's the end of the line, playa.
				}
			}else if(x < -0.1f){//if the player wants to go left	
				if(OptionsManager.playersSelected){
					//nope, that's not happening
				}else if(OptionsManager.fogSelected){
					lerping = true;
					target = playersPos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					lerping = true;
					target = fogPos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu"); 
				}else if(OptionsManager.speedSelected){
					//StartCoroutine("waitMenuTranslate", waterPos);
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					target = speedPos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					target = sizePos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.backSelected){
					//StartCoroutine("waitMenuTranslate", tutorialPos);
				}else if(OptionsManager.resetSelected){
					lerping = true;
					target = backPos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					lerping = true;
					target = resetPos;
					goingRight = false;
					goingLeft = true;
					Invoke ("SwitchOption", 0.04f);
					StartCoroutine("waitMenu");
				}
			}
		}
		
		
		if (!optionsScript.loadingNewScreen && !goingRight && !goingLeft && !goingUp && !goingDown) {
			if (x == 0.0f && y == 0.0f) {
				targetRotateSpeed = restingRotateSpeed;
			}
			else if (Mathf.Abs (x) > Mathf.Abs (y)) {
				targetRotateSpeed = movingRotateSpeed * Mathf.Abs (x);
			}
			else {
				targetRotateSpeed = movingRotateSpeed * Mathf.Abs (y);
			}

			if (x > 0.0f || y > 0.0f) {
				rotatingLeft = -1;
			}
			else if (x < 0.0f || y < 0.0f) {
				rotatingLeft = 1;
			}
		}
		
		if (Mathf.Abs (targetRotateSpeed - rotateSpeed) <= 1) {
			rotateSpeed = targetRotateSpeed;
		}
		else {
			rotateSpeed = Mathf.Lerp (rotateSpeed, targetRotateSpeed, 0.1f);
		}

		if (Mathf.Abs (rotateSpeed) < restingRotateSpeed) {
			rotateSpeed = Mathf.Sign (rotateSpeed) * restingRotateSpeed;
		}
							
		if (optionsScript.loadingNewScreen) {
			transform.RotateAround (transform.position, Vector3.forward, loadingRotateSpeed * rotatingLeft * Time.deltaTime);
			if (transform.renderer.material.color.a > 0f) { 
				Color32 newColor = transform.renderer.material.color;
				newColor.a -= (byte) (0.021f * 255f);
				transform.renderer.material.color = newColor;
			}
			
		}
		else {
			transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * rotatingLeft * Time.deltaTime);
		}
	}

	IEnumerator waitMenuTranslate(Vector3 _target){
		waiting = true;
		yield return new WaitForSeconds(0.3f);
		waiting = false;
		transform.position = _target;
	}

	IEnumerator waitMenu(){
		if (goingRight) rotatingLeft = -1;
		else if (goingLeft) rotatingLeft = 1;
		if (goingUp) rotatingLeft = -1;
		else if (goingDown) rotatingLeft = 1;
		
		rotateSpeed = movingRotateSpeed;
		targetRotateSpeed = movingRotateSpeed;
		
		yield return new WaitForSeconds(0.3f);
		//lerping = false;
	}
	
	void SwitchOption(){
		OptionsManager.playersSelected = false;
		OptionsManager.fogSelected = false;
		OptionsManager.waterSelected = false;
		OptionsManager.speedSelected = false;
		OptionsManager.sizeSelected = false;
		OptionsManager.tutorialSelected = false;
		OptionsManager.backSelected = false;
		OptionsManager.resetSelected = false;
		OptionsManager.startSelected = false;
		
		if (target == playersPos) 	 	OptionsManager.playersSelected = true;
		else if (target == fogPos) 	 	OptionsManager.fogSelected = true;
		else if (target == waterPos) 	OptionsManager.waterSelected = true;
		else if(target == speedPos)		OptionsManager.speedSelected = true;
		else if(target == sizePos)   	OptionsManager.sizeSelected = true;
		else if(target == tutorialPos)	OptionsManager.tutorialSelected = true;
		else if(target == backPos)		OptionsManager.backSelected = true;
		else if(target == resetPos)		OptionsManager.resetSelected = true;
		else if(target == startPos)		OptionsManager.startSelected = true;
	}
	
	//Sets movement check bools to false - ensures that the thing is selected before you start moving again
	void TakeNewPosition () {
		goingRight = false;
		goingLeft = false;
		goingUp = false;
		goingDown = false;
		lerping = false;
	}
}
