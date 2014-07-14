using UnityEngine;
using System.Collections;
public class ThreeLevelTimer : MonoBehaviour {
	private GameObject _InnerTimer;
	public GameObject InnerTimer{
		get{
			if(_InnerTimer == null){
				_InnerTimer = transform.FindChild ("InnerTimer").gameObject;
			}
			return _InnerTimer;
		}
	}
	private GameObject _MiddelTimer;
	public GameObject MiddleTimer{
		get{
			if(_MiddelTimer == null){
				_MiddelTimer = transform.FindChild("MiddleTimer").gameObject;
			}
			return _MiddelTimer;
		}
	}
	private GameObject _OuterTimer;
	public GameObject OuterTimer{
		get{
			if(_OuterTimer == null){
				_OuterTimer =transform.FindChild ("OuterTimer").gameObject;
			}
			return _OuterTimer;
		}
	}
	
	public void setTimers(float? inner, float? middle, float? outer){
		if(inner.HasValue){
			InnerTimer.renderer.material.SetFloat("_Cutoff",inner.Value);
		}	
		if(middle.HasValue){
			MiddleTimer.renderer.material.SetFloat("_Cutoff",middle.Value);
		}
		if(outer.HasValue){
			OuterTimer.renderer.material.SetFloat("_Cutoff",outer.Value);
		}
		
	}
	
	public void setTimersBase100(float? inner, float? middle, float? outer){
		if(inner.HasValue){
			inner = inner.Value / 100f;
			inner = 1.0005f-inner.Value;
		}	
		if(middle.HasValue){
			middle = middle.Value /100f;
			middle = 1.0005f-middle.Value;
		}
		if(outer.HasValue){
			outer = outer.Value/ 100f;
			outer = 1.0005f-outer.Value;
		}
	

		
		setTimers(inner,middle,outer);	
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//setTimers(Mathf.Sin (Time.time), Mathf.Cos (Time.time), Mathf.Tan(Time.time));
		
	}
	
	public void turnAllOff(){
		foreach(Renderer t in transform.GetComponentsInChildren<Renderer>()){
			t.enabled = false;
		}
	}
	
	public void turnAllOn(){
		foreach(Renderer t in transform.GetComponentsInChildren<Renderer>()){
			t.enabled = true;
		}
	}
	
	public void setColor(Color32 col){
		InnerTimer.renderer.material.color = col;
		MiddleTimer.renderer.material.color = col;
		OuterTimer.renderer.material.color = col;
	}
	
	public void setColor(Color32 innerColor, Color32 middleColor, Color32 outerColor){
		InnerTimer.renderer.material.color = innerColor;
		MiddleTimer.renderer.material.color = middleColor;
		OuterTimer.renderer.material.color = outerColor;
	}
}
