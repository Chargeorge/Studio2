using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Beacon : MonoBehaviour {

	private List<List<InfluencePatternHolder>> _patternList;
	public DirectionEnum? facing;
	public DirectionEnum? dirRotatingToward;	//Used to set visual direction while rotating
	public TeamInfo controllingTeam;
	private BeaconState _currentState;
	public int PlayerNumber;
	public float percBuildComplete = 0;
	public float percRotateComplete = 0;
	public float percUpgradeComplete = 0;
	public float percSmaller = 100;
	public float timeStoppedBuilding;
	public bool selfDestructing = false;
	public float timeStoppedUpgrading;
	public bool losingUpgradeProgress = false;
	public GameManager gm;	
	public GameObject tileBeingConverted;
	private InfluencePatternHolder patternConverting;
	public Settings sRef;
	public BeaconState currentState{
		get{
			return _currentState;
		}
	}

	bool buildButtonDown;

	public AudioClip beaconBuilding;
	public AudioClip beaconBuilt;
	public AudioClip beaconUpgrading;
	public AudioClip beaconUpgraded;
	public AudioClip beaconRotating;
	public AudioClip beaconRotated;

	private Material matBasic;
	private Material matUpgraded;
	private Material arrowUpgraded;
	public Color32 neutralColor;
	public Color32 neutralColorB;


	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
		sRef= GameObject.Find ("Settings").GetComponent<Settings>();
		matBasic = (Material) Resources.Load ("Sprites/Materials/Base");
		matUpgraded = (Material) Resources.Load ("Sprites/Materials/BaseUpg");
		arrowUpgraded = (Material) Resources.Load ("Sprites/Materials/ArrowUpgraded");

	}
	
	// Update is called once per frame
	void Update () {
		int brdX; int brdY;
		if (transform.parent != null) { //Hax
			brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
		 	brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
			
			setVisualDirection();	//Why is this happening every frame?

			buildButtonDown = getPlayerBuild();

			if(!buildButtonDown){
				audio.Stop();
			}
			if((_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced || _currentState == BeaconState.Advanced) && controllingTeam != null){

				//find nearest convertable block
				//FIND The first convertable tile, list is ordered by distance
				//TODO setup for bases so multiple tiles can influence at once
				foreach (List<InfluencePatternHolder> list in _patternList) {
					float influenceThisFrame = sRef.vpsBeaconBaseInfluence * Time.deltaTime;				
					bool waterFound = false;	
					list.ForEach(delegate (InfluencePatternHolder p){
						if (!waterFound) {

							if(influenceThisFrame > 0f){
								int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
								int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
								GameObject tile;
								
								try { tile = gm.tiles[x, y]; }
									catch { return; }
								if (tile != null && 
									tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water && 
									!gm.getCapturedAltars (controllingTeam).Contains (AltarType.Thotzeti)) 
								{
									waterFound = true;
									return;
								}
								if(tile != null && tile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
								BaseTile Bt =  tile.GetComponent<BaseTile>();
									if(Bt.controllingTeam == null){
										Bt.startInfluence(influenceThisFrame, controllingTeam);
										Bt.jigglingFromBeacon = true;
										influenceThisFrame = 0;  //Assume a null controlled Tile will eat all influence.
										//Debug.Log("influencing Null tile");
									}
									else if(Bt.controllingTeam.teamNumber != controllingTeam.teamNumber){
										Bt.jigglingFromBeacon = true;
										influenceThisFrame = Bt.subTractInfluence(influenceThisFrame * p.coefInfluenceFraction, controllingTeam);
										//Debug.Log("Removing other influence");
										if(influenceThisFrame > 0){
											influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
											//Debug.Log("Adding next frames influence");
										}
									}
									else if(Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled < 100f){
										Bt.jigglingFromBeacon = true;
										influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
										//Debug.Log ("Adding my influence");
									}
									else if (Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled >= 100f) {
										Bt.jigglingFromBeacon = false;
									}
			//						|| Bt.percControlled < 100f
								}
								
							}
						}
					});
				 }

	//			 
	//			 else{
	//				
	//			 //TODO: Handle situations where other tiles are influencing.  
	//				Debug.Log("Trying to influence at rate " + patternConverting.vpsInfluence );
	//				if(tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam)){
	//					tileBeingConverted = null;
	//					patternConverting= null;
	//				}
	//			 }
			}
		

		//With no powers this probably isn't necessary?

		//UpdateInfluencePatterns();	//Probably shouldn't call this every frame, but just doing this for now
		
		/**
		//Check self-destruction and losing upgrade progress - probably shouldn't call this every frame either, but I'm a rebel and I do what I want when I want 
		if (timeToSelfDestruct ()) { 
			subtractBuildingProgress (sRef.vpsBaseBuild);
		}
		
		if (timeToLoseUpgradeProgress ()) {
			subtractUpgradeProgress (sRef.vpsBaseUpgrade);
		}
		*/
		}
		
	}
	
	//START BUILDING:
	//SET the parent tile
	// Set the facing
	// ser State to building
	/// <summary>
	/// Puts it in the building state and starts the building.  
		/// </summary>
	/// <param name="tileLocation">Tile location.</param>
	/// <param name="player">Player.</param>
	/// <param name="valInit">Value init.</param>
	/// 
	private bool getPlayerBuild(){
		int num = PlayerNumber;
		if(num != 0){
			return Input.GetButton("BuildPlayer"+num);	
		}
		else return false;
	}
		
	public void startBuilding(GameObject tileLocation, GameObject player, float valInit){
		this.gameObject.transform.parent = tileLocation.transform;



		this.facing = player.GetComponentInChildren<Player>().facing;
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.BuildingBasic;
		if (controllingTeam == null) {
			controllingTeam = player.GetComponentInChildren<Player>().team;
			this.setTeam();
		}
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

		this.transform.FindChild("Base").localPosition = new Vector3(0f,0f,-.1f);
		//Debug.Log(this.transform.FindChild("Base").localPosition);
		
		audio.Stop ()	;
		audio.PlayOneShot(beaconBuilding, .09f);
		
		
		
	}
	

	public void buildNeutral(GameObject tileLocation){
		audio.Stop();
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = (DirectionEnum)Random.Range(1,5);
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.Basic;
		this.setTeam(null);
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

		_currentState = BeaconState.Basic;

		_patternList = createBasicInfluenceList(getAngleForDir(facing));

		
	}

	public void startUpgrading(){

		this._currentState = BeaconState.BuildingAdvanced;
		audio.PlayOneShot(beaconUpgrading, 1.0f);
	}
	
	public void startRotating (DirectionEnum? dir) {
		audio.Stop ();
		dirRotatingToward = dir;
		audio.PlayOneShot(beaconRotating, 0.7f);
	}
	
	public void setTeam(){
		Color32 controllingTeamColor = controllingTeam.beaconColor;	
		Color32 platformColor = controllingTeam.tileColor;
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = 0;

		transform.FindChild("Arrow").renderer.material.color = controllingTeamColor;
		transform.FindChild("Base").renderer.material.color = controllingTeamColor;
		transform.FindChild("Anim").renderer.material.color = controllingTeamColor;
		transform.FindChild("Platform").renderer.material.color = platformColor;
	}

	public void setTeam(TeamInfo teamIn){
	if(teamIn != null){controllingTeam = teamIn;
		Color32 controllingTeamColor = controllingTeam.beaconColor;	
			Color32 platformColor = controllingTeam.tileColor;
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = (byte)(transform.FindChild("Arrow").renderer.material.color.a * 255);
		controllingTeamColor.a = (byte)(transform.FindChild("Base").renderer.material.color.a * 255);
			platformColor.a = (byte)(transform.FindChild("Platform").renderer.material.color.a * 255);


			transform.FindChild("Arrow").renderer.material.color = controllingTeamColor;	
			transform.FindChild("Base").renderer.material.color = controllingTeamColor;
			transform.FindChild("Anim").renderer.material.color = controllingTeamColor;
			transform.FindChild("Platform").renderer.material.color = platformColor;
		}
		else{

			neutralColor = new Color32 (100, 100, 100, 255);
			neutralColorB = new Color32 (200, 200, 200, 255);

			controllingTeam = null;
			transform.FindChild("Arrow").renderer.material.color = neutralColor;
			transform.FindChild("Base").renderer.material.color = neutralColor;
			transform.FindChild("Platform").renderer.material.color = neutralColorB;
		}	}
	
	/// <summary>
	/// Adds progress to the current building action
	/// </summary>
	/// <param name="rate">Rate.</param>
	public void addBuildingProgress(float rate){
		percBuildComplete += rate*Time.deltaTime;
						
		Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
		Color32 platformColor = controllingTeam.tileColor;
		float newColor =  (255f * (percBuildComplete/100f)) ;
		newColor = (newColor >= 255) ? 254 : newColor;		
		beaconColor.a = (byte)newColor;

		//successful attempt at rising platform
		Transform beaconbase = transform.FindChild ("Base");
		Transform platform = transform.FindChild ("Platform");
		Vector3 platformPos = platform.localPosition;
		Vector3 beaconPos = beaconbase.localPosition;
		float newPos = (0.09f * (percBuildComplete/100f));
		newPos = (newPos >= 0.09f) ? 0.089f : newPos;
		platformPos.y = newPos;
		beaconPos.y = newPos;
		platform.localPosition = platformPos;
		beaconbase.localPosition = beaconPos;

		updateBuildAnim ();

//		platform.Translate(Vector3.up * platformPos);

		transform.FindChild("Arrow").renderer.material.color = beaconColor;		
		transform.FindChild("Platform").renderer.material.color = platformColor;
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 0;
		transform.FindChild("Base").renderer.material.color = baseColor;	
	}
	
	public void subtractBuildingProgress(float rate) {
	
		percBuildComplete -= rate*Time.deltaTime;
		
		updateBuildAnim ();
		
		if (percBuildComplete <= 0f) {
			GameObject.Destroy (this.gameObject);
		}
		
		else {
			Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
			Color32 platformColor = controllingTeam.tileColor;
			float newColor =  (255f * (percBuildComplete/100f)) ;
			newColor = (newColor >= 255) ? 254 : newColor;		
			beaconColor.a = (byte)newColor;
			transform.FindChild("Arrow").renderer.material.color = beaconColor; 
			transform.FindChild("Platform").renderer.material.color = platformColor;

			Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
			baseColor.a = 0;
			transform.FindChild("Base").renderer.material.color = baseColor;		
		}
	}
		
	public void addRotateProgress (float rate) {
		percRotateComplete += rate*Time.deltaTime;
	}
	
	public void addUpgradeProgress (float rate) {
		percUpgradeComplete += rate*Time.deltaTime;
		updateUpgradeAnim ();
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 255;
		transform.FindChild("Base").renderer.material.color = baseColor;	
		
//		percSmaller -= rate*Time.deltaTime;

//		Vector3 newScale = animTrans.localScale;
//		float newScale =  (0.5f * (percUpgradeComplete/100f)) ;
//		newScale = (newScale >= 0.5f) ? 0.49f : newScale;		

//		newScale.x = percSmaller  / 100f;
//		newScale.y = percSmaller  / 100f;
//		newScale.x = 100f/percSmaller;
//		newScale.y = 100f/percSmaller;
//		animTrans.localScale = newScale;

		//We need some visual representation for this	
	}
	
	
	public void subtractUpgradeProgress (float rate) {
		percUpgradeComplete -= rate*Time.deltaTime;
		updateUpgradeAnim ();		

//		percSmaller += rate*Time.deltaTime;		
//		Vector3 newScale = animTrans.localScale;
//		newScale.x = 100f/percSmaller;
//		newScale.y = 100f/percSmaller;
//		animTrans.localScale = newScale;

		if (percUpgradeComplete <= 0f) {
			percUpgradeComplete = 0f;
			//audio.Stop();
			_currentState = BeaconState.Basic;
			Color32 animColor = transform.FindChild("Anim").renderer.material.color;
			animColor.a = (byte)0f;
			transform.FindChild("Anim").renderer.material.color = animColor;
		}
		
		//We need some visual representation for this
	}
	#region creating_influence_lists
	//This is gonna be mad long so I added a region - minimize at your pleasure - should probably use for loops, but whatevs, it's all manual now 
	
	public List<List<InfluencePatternHolder>> createBasicInfluenceList(float degreeRotated){
		
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Not sure if sorting the lists is necessary?
		
		if (controllingTeam != null) {
		
			bool onixtal = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal);
			bool yaxchay = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay);
			bool tepwante = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante);
			int baseRange = sRef.beaconBasicRange;
			
			if (!yaxchay && !onixtal && !tepwante) {
				for (int i = 1; i <= sRef.beaconBasicRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}			
			}
			
			//Yaxchay: x2 range, x2 power
			else if (yaxchay) {
				
				//Deal with straight-forward pattern first since others don't affect this
				for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				list.Add (forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				
				//Yaxchay + Onixtal - build right, backward, and left patterns
				if (onixtal) {
					
					List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
						backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
						leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
					}
					
					list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					
				}
				
				//Yaxchay + Tepwante - build forward triple-beam patterns
				if (tepwante) {
					
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				}
				
				//Yaxchay + Tepwante + Onixtal - build triple-beam patterns for left, backward, and right
				if (onixtal && tepwante) {
					
					//Backward triple-beam patterns
					List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
						backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
					}
					
					list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					
					//Left and right patterns 
					//lololololololol					
					List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 2; i <= baseRange*sRef.coefYaxchay; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
						rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
						rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
						leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
						leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
					}
					
					list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				}
			}
			
			//No Yaxchay
			else {
				
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				
				//Onixtal: Influence in non-facing directions at 25% strength
				if (onixtal) {
					
					if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal) && !gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
						
						List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
							backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
							leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
						}
						
						list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						
					}
					
					//Onixtal + Tepwante - build triple-beam patterns for left, backward, and right 
					if (tepwante) {
						//Backward triple-beam patterns
						List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
							backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
						}
						
						list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						
						//Left and right patterns 
						List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 2; i <= sRef.beaconBasicRange; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
							rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
							rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
							leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
							leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
						}
						
						list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());					
					}
				}
				
				//Tepwante: Triple beam - assuming no Yaxchay and no Onixtal by now
				else if (tepwante) {
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());			
				}
			}
		}

		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		return list;
	}	
	
	public List<List<InfluencePatternHolder>> createAdvancedInfluenceList(float degreeRotated){
	
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Not sure if sorting the lists is necessary?
		
		if (controllingTeam != null) {
		
			bool onixtal = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal);
			bool yaxchay = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay);
			bool tepwante = gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante);
			int baseRange = sRef.beaconAdvancedRange;
			
			if (!yaxchay && !onixtal && !tepwante) {
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}			
			}
			
			//Yaxchay: x2 range, x2 power
			else if (yaxchay) {
				
				//Deal with straight-forward pattern first since others don't affect this
				for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
				list.Add (forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				
				//Yaxchay + Onixtal - build right, backward, and left patterns
				if (onixtal) {
					
					List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
						backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
						leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
					}
					
					list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					
				}
				
				//Yaxchay + Tepwante - build forward triple-beam patterns
				if (tepwante) {
					
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
				}
				
				//Yaxchay + Tepwante + Onixtal - build triple-beam patterns for left, backward, and right
				if (onixtal && tepwante) {
					
					//Backward triple-beam patterns
					List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange*sRef.coefYaxchay; i++) {
						backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
						backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
					}
					
					list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					
					//Left and right patterns 
					//lololololololol					
					List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 2; i <= baseRange*sRef.coefYaxchay; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
						rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
						rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
						leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
						leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
					}
					
					list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
					list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				}
			}
			
			//No Yaxchay
			else {
			
				for (int i = 1; i <= baseRange; i++) { 
					forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2 (0,i), getBaseBeaconStrength (i), degreeRotated));
				}
		
				//Onixtal: Influence in non-facing directions at 25% strength
				if (onixtal) {
				
					if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal) && !gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
						
						List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							rightInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 90f));
							backwardInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 180f));
							leftInfluenceList.Add (new InfluencePatternHolder (new Vector2 (0,i), getBaseBeaconStrength (i)*sRef.coefOnixtal, degreeRotated + 270f));
						}
						
						list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						
					}
					
					//Onixtal + Tepwante - build triple-beam patterns for left, backward, and right 
					if (tepwante) {
						//Backward triple-beam patterns
						List<InfluencePatternHolder> backwardLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> backwardRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 1; i <= baseRange; i++) {
							backwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));
							backwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 180f));	
						}
						
						list.Add (backwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (backwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						
						//Left and right patterns 
						List<InfluencePatternHolder> rightLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> rightRightInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftLeftInfluenceList = new List<InfluencePatternHolder>();
						List<InfluencePatternHolder> leftRightInfluenceList = new List<InfluencePatternHolder>();
						
						for (int i = 2; i <= baseRange; i++) { //Start at 2 for left + right so you don't overlap at corners with forward + backward triple-beam patterns
							rightLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));
							rightRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 90f));	
							leftLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));
							leftRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante*sRef.coefOnixtal, degreeRotated + 270f));	
						}
						
						list.Add (rightRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (rightLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());	
						list.Add (leftRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
						list.Add (leftLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());					
					}
				}
			
			//Tepwante: Triple beam - assuming no Yaxchay and no Onixtal by now
				else if (tepwante) {
					List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
					List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
					
					for (int i = 1; i <= baseRange; i++) { 
						forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
						forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,i), getBaseBeaconStrength (i)*sRef.coefTepwante, degreeRotated));
					}
					
					list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
					list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());			
				}
			}
		}
		
		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		return list;
	}
	#endregion
	
	void OnMouseOver() {
		GameObject.Find("GameManager").GetComponent<GameManager>().debugBeacon = this;
	}
	
	private float getAngleForDir(DirectionEnum? N){
		switch (N){
		case DirectionEnum.North:
			return 0f;
			
		case DirectionEnum.East:
			return 90f;
			
		case DirectionEnum.South:
			return 180f;
			
		case DirectionEnum.West:
			return 270f;
			
		}
		return 0;
	}
	
	public void setDirection(DirectionEnum? N){
		
		float rotAngle = getAngleForDir(N);
		float currentRotAngle = getAngleForDir(facing);
		
		facing = N;
		dirRotatingToward = N;
		transform.FindChild ("Arrow").RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.FindChild ("Arrow").RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
	}
	
	public void setVisualDirection(){
		Transform arrow = transform.FindChild ("Arrow");
		arrow.localEulerAngles = new Vector3(0,0,-1*getAngleForDir(facing));
		
		if (dirRotatingToward != facing && percRotateComplete > 0f && percRotateComplete < 100f) { //Rotating
			
			Vector3 angleRotatingToward = Vector3.zero;	//The angle we're roughly rotating toward
			float percRotatingToward = 0f;				//How far toward the angle above we're going to get just before 100% rotation
						
			if (Mathf.Abs ((getAngleForDir (facing)-getAngleForDir(dirRotatingToward)) % 360f) == 180f) {	//Rotating 180 degrees
				if (facing == DirectionEnum.North || facing == DirectionEnum.South)  
					angleRotatingToward = new Vector3 (-90f, 0f, 0f);
				else
					angleRotatingToward = new Vector3 (0, -90f, 0f); 
					
				percRotatingToward = 80f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			//I know there's a mathy way to do this but holy motherfucking shit fuck I cannot figure it out so fuck it
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.South) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.North)) 
			{
				angleRotatingToward = new Vector3 (0,0,90f);
				percRotatingToward = 50f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.North) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.South)) 
			{
				angleRotatingToward = new Vector3 (0,0,-90f);
				percRotatingToward = 50f;
				arrow.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			else{
				Debug.LogWarning ("Something's wrong in Beacon.setVisualDirection");
			}

				
		}
	}
	
	public void Rotate (DirectionEnum? N) {
		//audio.PlayOneShot(beaconRotating, 1.0f);
		setDirection (N);
		setVisualDirection ();
		ClearJiggle ();
		UpdateInfluencePatterns();

		if(percRotateComplete < 100f && !buildButtonDown){
			audio.Stop();
		}

		if(percRotateComplete >= 100f){
			audio.Stop();
			audio.PlayOneShot(beaconRotated, 1.0f);	
		}
	
	}
	
	//Stops a-jiggling what ought not be a-jiggling - use before UpdateInfluencePatterns
	public void ClearJiggle () {
		
		int brdX; int brdY;
		if (transform.parent != null) { //Hax
			brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
			brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
			
			if((_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced || _currentState == BeaconState.Advanced) && controllingTeam != null){
				
				foreach (List<InfluencePatternHolder> list in _patternList) {
					bool waterFound = false;	
					list.ForEach(delegate (InfluencePatternHolder p){
						if (!waterFound) {
							
							if(true == true){
								int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
								int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
								GameObject tile;
								
								try { tile = gm.tiles[x, y]; }
								catch { return; }
								if (tile != null && 
								    tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water && 
								    !gm.getCapturedAltars (controllingTeam).Contains (AltarType.Thotzeti)) 
								{
									waterFound = true;
									return;
								}
								if(tile != null && tile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
									BaseTile Bt =  tile.GetComponent<BaseTile>();
									Bt.jigglingFromBeacon = false;
								}								
							}
						}
					});
				}
			}
		}
	}
	
	//Creates new influence patterns, for example when a new altar is captured or when the beacon is rotated.
	public void UpdateInfluencePatterns () {
		if (_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced) {
			_patternList = createBasicInfluenceList (getAngleForDir (facing));
		}
		if (_currentState == BeaconState.Advanced) {
			_patternList = createAdvancedInfluenceList (getAngleForDir(facing));
		}		
	}
	
	/// <summary>
	/// Finishes the current building action.  USE ONLY FOR BUILDING, INFLUENCE HANDLED ELSEWHERE
	/// </summary>
	public void Build (){
		
		//Used to be finishAction() - refactored upgrade stuff into Upgrade() - seems cleaner this way
		
		///TODO: add end semaphore stuff her
		selfDestructing = false;
		
		if(percBuildComplete >= 100f){
			percBuildComplete = 100f;
			audio.Stop();
			audio.PlayOneShot(beaconBuilt, 1.0f);
			
			_currentState = BeaconState.Basic;
			_patternList = createBasicInfluenceList(getAngleForDir(facing));
			
			for(int x = 0; x<  GameManager.GameManagerInstance.tiles.GetLength(0); x++){
				for(int y =0 ; y< GameManager.GameManagerInstance.tiles.GetLength(1); y++){
					if(GameManager.GameManagerInstance.tiles[x,y].GetComponent<BaseTile>().currentType != TileTypeEnum.water){
						GameManager.GameManagerInstance.tiles[x,y].GetComponent<BaseTile>().tooCloseToBeacon();
					}
				}
			}
		}
		
		Color32 baseColor = transform.FindChild ("Arrow").renderer.material.color;
		baseColor.a = 255;
		transform.FindChild("Base").renderer.material.color = baseColor;	
		
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = 0;
		transform.FindChild("Anim").renderer.material.color = animColor;
		
//		transform.FindChild("Point light").GetComponent<Light>().enabled = true;
		
	}
	
	
	public void AbortBuild () {
		audio.Stop ();
		selfDestructing = true;
		timeStoppedBuilding = Time.time;
		Invoke ("CheckSelfDestruct", sRef.selfDestructDelay);
	}
	
	public void CheckSelfDestruct () {
		if (timeToSelfDestruct ()) {
	//		GameObject.Destroy (this.gameObject);	//Destroys beacon immediately
			StartCoroutine (selfDestruct());
		}
	}
	
	private IEnumerator selfDestruct () {
		while (selfDestructing) {
			subtractBuildingProgress (sRef.vpsBaseBuild);
			yield return new WaitForEndOfFrame ();
			
		}
	}
	
	private bool timeToSelfDestruct () {
		return selfDestructing && timeStoppedBuilding <= Time.time - sRef.selfDestructDelay;
	}
	
	public void Upgrade () {

		losingUpgradeProgress = false;

		//This block moved from old finishAction() function, now Build() 
		audio.Stop();
		audio.PlayOneShot(beaconUpgraded, 1.0f);
		_currentState = BeaconState.Advanced;
		_patternList = createAdvancedInfluenceList(getAngleForDir(facing));
		
		_currentState = BeaconState.Advanced;
		transform.FindChild("Base").renderer.material = matUpgraded;
		transform.FindChild("Arrow").renderer.material = arrowUpgraded;
		
		//hax
		setTeam ();
		Color32 platformColor = transform.FindChild("Base").renderer.material.color;
		Color32 beaconColor = transform.FindChild("Arrow").renderer.material.color;
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		platformColor.a = 255;
		beaconColor.a = 255;
		animColor.a = 0;
		transform.FindChild("Arrow").renderer.material.color = beaconColor;
		transform.FindChild("Base").renderer.material.color = platformColor;
		transform.FindChild("Anim").renderer.material.color = animColor;
	}
	
	//Player stopped in the middle of an upgrade
	public void AbortUpgrade () {
		audio.Stop();
		losingUpgradeProgress = true;
		timeStoppedUpgrading = Time.time;
		Invoke ("CheckLoseUpgradeProgress", sRef.loseUpgradeProgressDelay);		
	}
	
	public void CheckLoseUpgradeProgress () {
		if (timeToLoseUpgradeProgress ()) {
//			_currentState = BeaconState.Basic;
//			percUpgradeComplete = 0f;
			StartCoroutine(loseUpgradeProgress());
		}
	}
	
	private bool timeToLoseUpgradeProgress () {
		return losingUpgradeProgress && timeStoppedUpgrading <= Time.time - sRef.loseUpgradeProgressDelay;
	}
	
	private IEnumerator loseUpgradeProgress () {
		while (losingUpgradeProgress) {
			subtractUpgradeProgress (sRef.vpsBaseUpgrade);
			yield return new WaitForEndOfFrame ();
		}
	}
	
	private void updateBuildAnim () {
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = (byte) (255f * ((sRef.buildCircleFinishAlpha - sRef.buildCircleStartAlpha) * percBuildComplete/100f + sRef.buildCircleStartAlpha));
		transform.FindChild("Anim").renderer.material.color = animColor;
		
		Transform animTrans = transform.FindChild("Anim");
		float newScale = sRef.buildCircleStartScale - (sRef.buildCircleStartScale - sRef.buildCircleFinishScale) * percBuildComplete/100f;
		animTrans.localScale = new Vector3 (newScale, newScale, animTrans.localScale.z);
	}
	
	private void updateUpgradeAnim () {
		Color32 animColor = transform.FindChild("Anim").renderer.material.color;
		animColor.a = (byte) (255f * ((sRef.upgradeCircleFinishAlpha - sRef.upgradeCircleStartAlpha) * percUpgradeComplete/100f + sRef.upgradeCircleStartAlpha));
		transform.FindChild("Anim").renderer.material.color = animColor;
		
		Transform animTrans = transform.FindChild("Anim");
		float newScale = sRef.upgradeCircleStartScale - (sRef.upgradeCircleStartScale - sRef.upgradeCircleFinishScale) * percUpgradeComplete/100f;
		animTrans.localScale = new Vector3 (newScale, newScale, animTrans.localScale.z);
	}
	
	private float getBaseBeaconStrength (int distance) {

		float strength = -1;
		
		//Basic, no Yaxchay
		if (!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && (currentState == BeaconState.Basic || currentState == BeaconState.BuildingAdvanced)) { 
			if (distance == 1) strength = 1f;
			else if (distance == 2) strength = 0.5f;
			else if (distance == 3) strength = 0.33f;
			else if (distance == 4) strength = 0.25f;				
		}
		
		//Basic, Yaxchay
		else if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && (currentState == BeaconState.Basic || currentState == BeaconState.BuildingAdvanced)) { 
			if (distance >= 1 && distance <= 2) strength = 1f;
			if (distance >= 3 && distance <= 4) strength = 0.5f;
			if (distance >= 5 && distance <= 6) strength = 0.33f;
			if (distance >= 7 && distance <= 8) strength = 0.25f;
		}
		
		//Advanced, no Yaxchay - same as basic + Yaxchay given current bonuses, but that may change
		else if (!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && currentState == BeaconState.Advanced) { 
			if (distance >= 1 && distance <= 2) strength = 1f;
			if (distance >= 3 && distance <= 4) strength = 0.5f;
			if (distance >= 5 && distance <= 6) strength = 0.33f;
			if (distance >= 7 && distance <= 8) strength = 0.25f;
		}
		
		//Advanced, Yaxchay
		else if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Yaxchay) && currentState == BeaconState.Advanced) { 
			if (distance >= 1 && distance <= 4) strength = 1f;
			if (distance >= 5 && distance <= 8) strength = 0.5f;
			if (distance >= 9 && distance <= 12) strength = 0.33f;
			if (distance >= 13 && distance <= 16) strength = 0.25f;
		}		
		
		if (strength == -1) {
			Debug.LogWarning ("Error when determining base beacon strength");
		}
		
		return strength;
	}
}
