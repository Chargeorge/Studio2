using UnityEngine;
using System.Collections;

public class CursorOptions : MonoBehaviour {

	public float moveSpeed;

	public float movingRotateSpeed;
	public float restingRotateSpeed;
	public float loadingRotateSpeed;
	private float rotateSpeed;
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
	
	// Use this for initialization
	void Start () {

		lerping = false;
		lerpRate = 0.2f;
		
		goingRight = false;
		goingLeft = false;
		goingUp = false;
		goingDown = false;
		
		optionsScript = options.GetComponent<OptionsManager>();
		rotateSpeed = restingRotateSpeed * -1;
		rotatingLeft = -1;

		float cursorDepth = -8.9f;

		float firstLineHeight = 0.9f;
		float secondLineHeight = -2.3f;
		float thirdLineHeight = -3.5f;

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

		if(lerping){
			Vector3 newPos = transform.position;
			newPos.x = Mathf.Lerp(newPos.x, target.x, lerpRate);
			newPos.y = Mathf.Lerp(newPos.y, target.y, lerpRate);
			transform.position = newPos;
			if(Mathf.Abs(Vector3.Magnitude(newPos-target)) < 0.1f){
				transform.position = target;
				lerping = false;
				goingRight = false;
				goingLeft = false;
				goingUp = false;
				goingDown = false;
			}
		}
		
		float x = Input.GetAxis("HorizontalPlayer1") * moveSpeed * Time.deltaTime;
		float y = Input.GetAxis("VerticalPlayer1") * moveSpeed * Time.deltaTime;
		
		if (!options.GetComponent<OptionsManager>().loadingNewScreen && !lerping && !waiting){
			if(Input.GetAxis("HorizontalPlayer1") > 0.1f){//player wants to go right
				if(OptionsManager.playersSelected){
					lerping = true;
					target = fogPos;
					goingRight = true;
					goingLeft = false;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.fogSelected){
					lerping = true;
					target = waterPos;
					goingRight = true;
					goingLeft = false;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					StartCoroutine("waitMenuTranslate", speedPos);
				}else if(OptionsManager.speedSelected){
					StartCoroutine("waitMenu");
					lerping = true;
					goingRight = true;
					goingLeft = false;
					target = sizePos;
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					goingRight = true;
					goingLeft = false;
					target = tutorialPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					StartCoroutine("waitMenuTranslate", backPos);
				}else if(OptionsManager.backSelected){
					StartCoroutine("waitMenu");
					lerping = true;
					goingRight = true;
					goingLeft = false;
					target = resetPos;
				}else if(OptionsManager.resetSelected){
					lerping = true;
					goingRight = true;
					goingLeft = false;
					target = startPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					//don't do shit cause that's the end of the line, playa.
				}
			}else if(Input.GetAxis("HorizontalPlayer1") < -0.1f){//if the player wants to go left	
				if(OptionsManager.playersSelected){
					//nope, that's not happening
				}else if(OptionsManager.fogSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = playersPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = fogPos;
					StartCoroutine("waitMenu"); 
				}else if(OptionsManager.speedSelected){
					StartCoroutine("waitMenuTranslate", waterPos);
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = speedPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = sizePos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.backSelected){
					StartCoroutine("waitMenuTranslate", tutorialPos);
				}else if(OptionsManager.resetSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = backPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					lerping = true;
					goingRight = false;
					goingLeft = true;
					target = resetPos;
					StartCoroutine("waitMenu");
				}
			}else if(Input.GetAxis("VerticalPlayer1") > 0.1f){//if the player wants to go up	
				if(OptionsManager.speedSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = playersPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = fogPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = waterPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.backSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = speedPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.resetSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = sizePos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.startSelected){
					lerping = true;
					goingDown = false;
					goingUp = true;
					target = tutorialPos;
					StartCoroutine("waitMenu");
				}
			}else if(Input.GetAxis("VerticalPlayer1") < -0.1f){//if the player wants to go down	
				if(OptionsManager.playersSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = speedPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.fogSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = sizePos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.waterSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = tutorialPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.speedSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = backPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.sizeSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = resetPos;
					StartCoroutine("waitMenu");
				}else if(OptionsManager.tutorialSelected){
					lerping = true;
					goingDown = true;
					goingUp = false;
					target = startPos;
					StartCoroutine("waitMenu");
				}
			}
		}
		
		if (!optionsScript.loadingNewScreen) {
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
			transform.RotateAround (transform.position, Vector3.forward, rotateSpeed * Time.deltaTime);
		}
	}

	IEnumerator waitMenuTranslate(Vector3 _target){
		waiting = true;
		yield return new WaitForSeconds(0.3f);
		waiting = false;
		transform.position = _target;
	}

	IEnumerator waitMenu(){
		yield return new WaitForSeconds(0.3f);
		//lerping = false;
	}
}
