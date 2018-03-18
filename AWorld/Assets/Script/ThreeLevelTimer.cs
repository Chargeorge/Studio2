using UnityEngine;
using System.Collections;
public class ThreeLevelTimer : MonoBehaviour {
	private GameObject _InnerTimer;
	public GameObject InnerTimer{
		get{
			if(_InnerTimer == null){
				_InnerTimer = transform.Find ("InnerTimer").gameObject;
			}
			return _InnerTimer;
		}
	}
	private GameObject _MiddelTimer;
	public GameObject MiddleTimer{
		get{
			if(_MiddelTimer == null){
				_MiddelTimer = transform.Find("MiddleTimer").gameObject;
			}
			return _MiddelTimer;
		}
	}
	private GameObject _OuterTimer;
	public GameObject OuterTimer{
		get{
			if(_OuterTimer == null){
				_OuterTimer =transform.Find ("OuterTimer").gameObject;
			}
			return _OuterTimer;
		}
	}
	
	public void setTimers(float? inner, float? middle, float? outer){
		if(inner.HasValue){
			InnerTimer.GetComponent<Renderer>().material.SetFloat("_Cutoff",inner.Value);
		}	
		if(middle.HasValue){
			MiddleTimer.GetComponent<Renderer>().material.SetFloat("_Cutoff",middle.Value);
		}
		if(outer.HasValue){
			OuterTimer.GetComponent<Renderer>().material.SetFloat("_Cutoff",outer.Value);
		}
		
	}
	
	public void setTimersBase100(float? inner, float? middle, float? outer){
		if(inner.HasValue){
			inner = inner.Value / 100f;
			inner = 1f-inner.Value;
		}	
		if(middle.HasValue){
			middle = middle.Value /100f;
			middle = 1f-middle.Value;
		}
		if(outer.HasValue){
			outer = outer.Value/ 100f;
			outer = 1f-outer.Value;
		}
	

		
		setTimers(inner,middle,outer);	
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		setTimers(Mathf.Sin (Time.time), Mathf.Cos (Time.time), Mathf.Tan(Time.time));
		
	}
	
	void turnAllOff(){
		foreach(Renderer t in transform.GetComponentsInChildren<Renderer>()){
			t.enabled = false;
		}
	}
	
	void turnAllOn(){
		foreach(Renderer t in transform.GetComponentsInChildren<Renderer>()){
			t.enabled = true;
		}
	}
	
	void setColor(Color32 col){
		InnerTimer.GetComponent<Renderer>().material.color = col;
		MiddleTimer.GetComponent<Renderer>().material.color = col;
		OuterTimer.GetComponent<Renderer>().material.color = col;
	}
}
