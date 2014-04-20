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
		
		if(pos.x < 2.85f){
			pos.x = 2.85f;
		} else if(pos.x > 11.8f){
			pos.x = 11.8f;
		}
		
		//transform.position = new Vector3(-0.16f, Mathf.Clamp(Time.time, 0.26F, -1.2F), -5.4f);
		transform.position = pos;
		
	}
}
