using UnityEngine;
using System.Collections;


public class ReadyUp : MonoBehaviour {

	public Player player;
	public bool ready = false;
	public float readyPct = 0f;
	
	
	public GameObject readyCircle;
	public GameObject readyText;
	public GameObject readiedText;
	// Use this for initialization
	void Start () {
		Color32 readyColor = player.team.beaconColor;
	//	readyColor.a = 130;
	//	transform.FindChild("Back").renderer.material.color = readyColor;
		this.renderer.enabled = false;
		readyText.renderer.enabled = true;
		readiedText.renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(!ready){
			readyCircle.renderer.material.SetFloat("_Cutoff",1.001f-(readyPct /100f));
			if(player.getPlayerBuild()){
				readyPct += Settings.SettingsInstance.vpsBaseBuild*2f * Time.deltaTime;
				if(readyPct > 100f){
					readyPct = 100f;
					ready = true;
					readiedText.renderer.enabled = true;
					readyText.renderer.enabled = false;
					readyCircle.renderer.enabled = false;
				}
			}		
			else{
				if(readyPct > 0){
					readyPct -= Settings.SettingsInstance.vpsBaseBuild * Time.deltaTime/2;
				}
				else{
					readyPct = 0;
				}
			}
		}
		
		
	}
	
	public void setPlayer(Player p){
		player  = p;
		readyCircle.renderer.material.color = player.renderer.material.color;
		readyText.GetComponent<TextMesh>().text  = string.Format(readyText.GetComponent<TextMesh>().text, player.PlayerNumber);
		readiedText.GetComponent<TextMesh>().text  = string.Format(readiedText.GetComponent<TextMesh>().text, player.PlayerNumber);
	}
	
}
