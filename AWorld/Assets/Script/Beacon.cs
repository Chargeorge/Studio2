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
	public float percActionComplete = 0;
	public float percInfluenceComplete= 0;	//Countdown till another influence is popped
	public float percRotateComplete = 0;
	public float percUpgradeComplete = 0;
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

	public AudioClip beaconInfluenceProgress;
	public AudioClip beaconBuilding;
	public AudioClip beaconBuilt;
	public AudioClip beaconUpgrading;
	public AudioClip beaconUpgraded;
	public AudioClip beaconRotating;

	private Material matBasic;
	private Material matUpgraded;

	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
		sRef= GameObject.Find ("Settings").GetComponent<Settings>();
		matBasic = (Material) Resources.Load ("Sprites/Materials/Beacon");
		matUpgraded = (Material) Resources.Load ("Sprites/Materials/Beacon_Upgraded");
		
	}
	
	// Update is called once per frame
	void Update () {
		int brdX; int brdY;
		brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
	 	brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
		
		setVisualDirection();	//Why is this happening every frame?

		if(Input.GetKeyUp(KeyCode.Space)){
			audio.Stop();
		}

		//influence work
		// Find list of all influencible tiles
		//Get closest tile in terms of distance and  begin influening it
		//Add Influence till tile is at full
		// move on to the next tile
		//repeat till all tiles are full or influence is expended
		
		//This solution isn't "Correct" AKA, it doesn't resolve perfectly every frame, but over the aggregrate it should be correct
		// We can move to fixed update to get closer to correct
		if((_currentState == BeaconState.Basic || _currentState == BeaconState.BuildingAdvanced || _currentState == BeaconState.Advanced) && controllingTeam != null){

			//find nearest convertable block
			//FIND The first convertable tile, list is ordered by distance
			//TODO setup for bases so multiple tiles can influence at once
			foreach (List<InfluencePatternHolder> list in _patternList) {
				float influenceThisFrame = sRef.vpsBeaconBaseInfluence * Time.deltaTime;				
				bool waterFound = false;
				list.ForEach(delegate (InfluencePatternHolder p){
					if (!waterFound) {
						gm.PlaySFX(beaconInfluenceProgress, 0.1f);
						if(influenceThisFrame > 0f){
							int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
							int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
							GameObject tile;
							
							try { tile = gm.tiles[x, y]; }
								catch { return; }
							if(tile != null && tile.GetComponent<BaseTile>().currentType == TileTypeEnum.water) { 
								waterFound = true;
								return;
							}
							if(tile != null){
							BaseTile Bt =  tile.GetComponent<BaseTile>();
								if(Bt.controllingTeam == null){
									Bt.startInfluence(influenceThisFrame, controllingTeam);
									influenceThisFrame = 0;  //Assume a null controlled Tile will eat all influence.
									//Debug.Log("influencing Null tile");
								}
								else if(Bt.controllingTeam.teamNumber != controllingTeam.teamNumber){
									influenceThisFrame = Bt.subTractInfluence(influenceThisFrame * p.coefInfluenceFraction, controllingTeam);
									//Debug.Log("Removing other influence");
									if(influenceThisFrame > 0){
										influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
										//Debug.Log("Adding next frames influence");
									}
								}
								else if(Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled < 100f){
									influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
									//Debug.Log ("Adding my influence");
								}
		//						|| Bt.percControlled < 100f
							}
							
						}
					}
				});
			 }

//			 
//			 else{
//				gm.PlaySFX(beaconInfluencing, 0.8f);
//			 //TODO: Handle situations where other tiles are influencing.  
//				Debug.Log("Trying to influence at rate " + patternConverting.vpsInfluence );
//				if(tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam)){
//					tileBeingConverted = null;
//					patternConverting= null;
//				}
//			 }
		}
		
		UpdateInfluencePatterns();	//Probably shouldn't call this every frame, but just doing this for now
		
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
	public void startBuilding(GameObject tileLocation, GameObject player, float valInit){
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = player.GetComponent<Player>().facing;
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.BuildingBasic;
		if (controllingTeam == null) {
			controllingTeam = player.GetComponent<Player>().team;
			this.setTeam();
		}	
		audio.PlayOneShot(beaconBuilding, 0.9f);
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

	}

	public void buildNeutral(GameObject tileLocation){
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = (DirectionEnum)Random.Range(1,5);
		this.dirRotatingToward = facing;
		this._currentState	= BeaconState.Basic;
		this.setTeam(null);
		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().beacon = this.gameObject;

		_currentState = BeaconState.Basic;
		audio.Stop();
		_patternList = createBasicInfluenceList(getAngleForDir(facing));
		
	}

	public void startUpgrading(){
		audio.Stop();
		this._currentState = BeaconState.BuildingAdvanced;
		audio.PlayOneShot(beaconUpgrading, 1.0f);
	}
	
	public void startRotating () {
		//Add SFX here
	}
	
	public void setTeam(){
		Color32 controllingTeamColor = controllingTeam.beaconColor;		
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = 0;
//		controllingTeamColor.r += 30;
//		controllingTeamColor.g += 30;
//		controllingTeamColor.b += 30;
		renderer.material.color = controllingTeamColor;
		
	}

	public void setTeam(TeamInfo teamIn){
	if(teamIn != null){controllingTeam = teamIn;
		Color32 controllingTeamColor = controllingTeam.beaconColor;		
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = (byte)(renderer.material.color.a * 255);
		
//		controllingTeamColor.r += 30;
//		controllingTeamColor.g += 30;
//		controllingTeamColor.b += 30;
		renderer.material.color = controllingTeamColor;		}
		else{
			controllingTeam = null;
			renderer.material.color = Color.grey;
		}	}
	
	/// <summary>
	/// Adds progress to the current building action
	/// </summary>
	/// <param name="rate">Rate.</param>
	public void addBuildingProgress(float rate){
		percActionComplete += rate*Time.deltaTime;
						
		Color32 beaconColor = renderer.material.color;
		float newColor =  (255f * (percActionComplete/100f)) ;
		newColor = (newColor >= 255) ? 254 : newColor;		
		beaconColor.a = (byte)newColor;
		renderer.material.color = beaconColor;		

		if(percActionComplete >= 100){
			gm.PlaySFX(beaconBuilt, 1.0f);
		}

	}
	
	public void addInfluenceProgress(float rate){
		percInfluenceComplete += rate*Time.deltaTime;

		//Debug.Log(percControlled);
	}
	
	public void addRotateProgress (float rate) {
		percRotateComplete += rate*Time.deltaTime;
	}
	
	public void addUpgradeProgress (float rate) {
		percUpgradeComplete += rate*Time.deltaTime;
	}
	
	#region creating_influence_lists
	//This is gonna be mad long so I added a region - minimize at your pleasure - should probably use for loops, but whatevs, it's all manual now 
	
	public List<List<InfluencePatternHolder>> createBasicInfluenceList(float degreeRotated){
		
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Not sure if sorting the lists is necessary?

		forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f, degreeRotated));
		forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), .5f, degreeRotated));
		forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .33f, degreeRotated));
		forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .25f, degreeRotated));

		//Onixtal: Influence in non-facing directions at 25% strength
		if ( (controllingTeam != null) && gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal)) {

			List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
			rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 90f));
			rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), .5f*sRef.coefOnixtal, degreeRotated + 90f));
			rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .33f*sRef.coefOnixtal, degreeRotated + 90f));
			rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .25f*sRef.coefOnixtal, degreeRotated + 90f));
			list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
			
			List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
			backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 180f));
			backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), .5f*sRef.coefOnixtal, degreeRotated + 180f));
			backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .33f*sRef.coefOnixtal, degreeRotated + 180f));
			backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .25f*sRef.coefOnixtal, degreeRotated + 180f));
			list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
			
			List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
			leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 270f));
			leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), .5f*sRef.coefOnixtal, degreeRotated + 270f));
			leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .33f*sRef.coefOnixtal, degreeRotated + 270f));
			leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .25f*sRef.coefOnixtal, degreeRotated + 270f));
			list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		}
		
		//Tepwante: Influence beam is 3 tiles wide instead of 1 (currently not stacking with Onixtal)
		if (controllingTeam !=null &&  gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
			List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
			forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,1), 1f*sRef.coefTepwante, degreeRotated));
			forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,2), .5f*sRef.coefTepwante, degreeRotated));
			forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,3), .33f*sRef.coefTepwante, degreeRotated));
			forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,4), .25f*sRef.coefTepwante, degreeRotated));
			list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		

			List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
			forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,1), 1f*sRef.coefTepwante, degreeRotated));
			forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,2), .5f*sRef.coefTepwante, degreeRotated));
			forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,3), .33f*sRef.coefTepwante, degreeRotated));
			forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,4), .25f*sRef.coefTepwante, degreeRotated));
			list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
		}
				
		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
//		return forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList();
		return list;
	}	
	
	public List<List<InfluencePatternHolder>> createAdvancedInfluenceList(float degreeRotated){
	
		List<List<InfluencePatternHolder>> list = new List<List<InfluencePatternHolder>>();		
		List<InfluencePatternHolder> forwardInfluenceList = new List<InfluencePatternHolder>();	//Every influence list will definitely have this, regardless of altars
		
		//Munalwa: Upgraded altars give 3x bonus instead of 2x
		if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Munalwa)) {
//			for (float i = 1f; i <= 12f; i++) {	//This might work as a better way to write this - haven't tested it though, staying with manual code for now
//				forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,i), 1/(Mathf.Ceil(i/3)), degreeRotated));
//			}
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), 1f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .5f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .5f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .33f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .33f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,9), .33f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,10), .25f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,11), .25f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,12), .25f, degreeRotated));
		}
		
		//Onixtal: Influence in non-facing directions at 25% strength
		if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal)) {
			//Has Munalwa, so 3x bonus for upgraded beacons
			if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Munalwa)) {
				
				List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), 1f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .5f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .5f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .33f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .33f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,9), .33f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,10), .25f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,11), .25f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,12), .25f*sRef.coefOnixtal, degreeRotated + 90f));
				list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
				
				List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), 1f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .5f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .5f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .33f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .33f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,9), .33f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,10), .25f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,11), .25f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,12), .25f*sRef.coefOnixtal, degreeRotated + 180f));
				list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
				
				List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), 1f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .5f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .5f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .33f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .33f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,9), .33f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,10), .25f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,11), .25f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,12), .25f*sRef.coefOnixtal, degreeRotated + 270f));
				list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
			}
			else {	//No Munalwa
			
				List<InfluencePatternHolder> rightInfluenceList = new List<InfluencePatternHolder>();
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .5f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .33f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .33f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .25f*sRef.coefOnixtal, degreeRotated + 90f));
				rightInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .25f*sRef.coefOnixtal, degreeRotated + 90f));
				list.Add (rightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
				
				List<InfluencePatternHolder> backwardInfluenceList = new List<InfluencePatternHolder>();
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .5f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .33f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .33f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .25f*sRef.coefOnixtal, degreeRotated + 180f));
				backwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .25f*sRef.coefOnixtal, degreeRotated + 180f));
				list.Add (backwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
				
				List<InfluencePatternHolder> leftInfluenceList = new List<InfluencePatternHolder>();
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .5f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .33f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .33f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .25f*sRef.coefOnixtal, degreeRotated + 270f));
				leftInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .25f*sRef.coefOnixtal, degreeRotated + 270f));
				list.Add (leftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
				
			}
		}
		
		//Tepwante: Influence beam is 3 tiles wide instead of 1 (currently not stacking with Onixtal, but stacking with Munalwa)
		if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante)) {
			//Munalwa: Upgraded altars give 3x bonus instead of 2x
			if (gm.getCapturedAltars(controllingTeam).Contains (AltarType.Munalwa)) {
				List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,1), 1f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,2), 1f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,3), 1f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,4), .5f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,5), .5f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,6), .5f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,7), .33f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,8), .33f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,9), .33f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,10), .25f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,11), .25f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,12), .25f*sRef.coefTepwante, degreeRotated));
				list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				
				List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,1), 1f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,2), 1f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,3), 1f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,4), .5f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,5), .5f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,6), .5f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,7), .33f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,8), .33f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,9), .33f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,10), .25f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,11), .25f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,12), .25f*sRef.coefTepwante, degreeRotated));
				list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				
			}
			//No Munalwa
			else {
				List<InfluencePatternHolder> mainInfluenceList = new List<InfluencePatternHolder>();
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .5f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .33f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .33f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .25f, degreeRotated));
				mainInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .25f, degreeRotated));
				list.Add (mainInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		


				List<InfluencePatternHolder> forwardLeftInfluenceList = new List<InfluencePatternHolder>();
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,1), 1f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,2), 1f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,3), .5f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,4), .5f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,5), .33f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,6), .33f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,7), .25f*sRef.coefTepwante, degreeRotated));
				forwardLeftInfluenceList.Add(new InfluencePatternHolder(new Vector2(-1,8), .25f*sRef.coefTepwante, degreeRotated));
				list.Add (forwardLeftInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());		
				
				List<InfluencePatternHolder> forwardRightInfluenceList = new List<InfluencePatternHolder>();
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,1), 1f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,2), 1f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,3), .5f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,4), .5f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,5), .33f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,6), .33f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,7), .25f*sRef.coefTepwante, degreeRotated));
				forwardRightInfluenceList.Add(new InfluencePatternHolder(new Vector2(1,8), .25f*sRef.coefTepwante, degreeRotated));
				list.Add (forwardRightInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());				
			}
		}
		
		//No altars that affect beacons are owned
		if (!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Munalwa) && 
			!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Onixtal) && 
			!gm.getCapturedAltars(controllingTeam).Contains (AltarType.Tepwante))
		{
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,1), 1f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,2), 1f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,3), .5f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,4), .5f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,5), .33f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,6), .33f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,7), .25f, degreeRotated));
			forwardInfluenceList.Add(new InfluencePatternHolder(new Vector2(0,8), .25f, degreeRotated));
		}
		
		list.Insert (0, forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList());
		//		return forwardInfluenceList.OrderBy(o=>o.relCoordRotated.magnitude).ToList();
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
		transform.RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
	}
	
	public void setVisualDirection(){
		transform.localEulerAngles = new Vector3(0,0,-1*getAngleForDir(facing));
		
		if (dirRotatingToward != facing && percRotateComplete > 0f && percRotateComplete < 100f) { //Rotating
			
			Vector3 angleRotatingToward = Vector3.zero;	//The angle we're roughly rotating toward
			float percRotatingToward = 0f;				//How far toward the angle above we're going to get just before 100% rotation
						
			if (Mathf.Abs ((getAngleForDir (facing)-getAngleForDir(dirRotatingToward)) % 360f) == 180f) {	//Rotating 180 degrees
				if (facing == DirectionEnum.North || facing == DirectionEnum.South)  
					angleRotatingToward = new Vector3 (-90f, 0f, 0f);
				else
					angleRotatingToward = new Vector3 (0, -90f, 0f); 
					
				percRotatingToward = 80f;
				transform.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			//I know there's a mathy way to do this but holy motherfucking shit fuck I cannot figure it out so fuck it
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.South) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.North)) 
			{
				angleRotatingToward = new Vector3 (0,0,90f);
				percRotatingToward = 50f;
				transform.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
			}
			
			else if ((facing == DirectionEnum.North && dirRotatingToward == DirectionEnum.East) ||
			     (facing == DirectionEnum.West && dirRotatingToward == DirectionEnum.North) ||
			     (facing == DirectionEnum.South && dirRotatingToward == DirectionEnum.West) ||
			     (facing == DirectionEnum.East && dirRotatingToward == DirectionEnum.South)) 
			{
				angleRotatingToward = new Vector3 (0,0,-90f);
				percRotatingToward = 50f;
				transform.localEulerAngles += angleRotatingToward * percRotatingToward/100f * percRotateComplete/100f;
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
		UpdateInfluencePatterns();
	
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
		
		audio.Stop();
		
		if(percActionComplete >= 100f){
			percActionComplete = 100f;
			
			
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
	}
	
	
	public void AbortBuild () {
		audio.Stop ();
		selfDestructing = true;
		timeStoppedBuilding = Time.time;
		Invoke ("CheckSelfDestruct", sRef.selfDestructDelay);
	//	GameObject.Destroy (this.gameObject);
	}
	
	public void CheckSelfDestruct () {
		if (selfDestructing && timeStoppedBuilding <= Time.time - sRef.selfDestructDelay) {
			GameObject.Destroy (this.gameObject);
		}
	}
	
	public void Upgrade () {

		losingUpgradeProgress = false;

		//This block moved from old finishAction() function, now Build() 
		audio.Stop();
		audio.PlayOneShot(beaconUpgraded, 1.0f);
		_currentState = BeaconState.Advanced;
		_patternList = createAdvancedInfluenceList(getAngleForDir(facing));
		
		_currentState = BeaconState.Advanced;
		renderer.material = matUpgraded;
		
		//hax
		setTeam ();
		Color32 beaconColor = renderer.material.color;
		beaconColor.a = 255;
		renderer.material.color = beaconColor;
	}
	
	//Player stopped in the middle of an upgrade
	public void AbortUpgrade () {
		audio.Stop();
		losingUpgradeProgress = true;
		timeStoppedUpgrading = Time.time;
		Invoke ("CheckLoseUpgradeProgress", sRef.loseUpgradeProgressDelay);
//		percUpgradeComplete = 0f;
//		_currentState = BeaconState.Basic;
		Debug.Log ("Aborting upgrade...");
		
	}
	
	public void CheckLoseUpgradeProgress () {
		if (losingUpgradeProgress && timeStoppedUpgrading <= Time.time - sRef.loseUpgradeProgressDelay) {
			_currentState = BeaconState.Basic;
			percUpgradeComplete = 0f;
			Debug.Log ("Lost upgrade progress");
		}
		
	}
	
}
