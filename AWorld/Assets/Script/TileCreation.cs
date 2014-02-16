using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileCreation : MonoBehaviour {
	private Dictionary<int, Material> textureResources;
	private GameObject[,] tilesGameBoard;
	public GameObject prfbTile;
	public Material matTest;
	private GameManager gm;
	public GameObject manager;
	public int boardX, boardY;

	// Use this for initialization
	void Start () {
		//Eventually procedural, for now random. Wee
		textureResources =new Dictionary<int, Material>();
		textureResources.Add((int)TileTypeEnum.regular, (Material)Resources.Load("Sprites/Materials/Regular"));
		textureResources.Add((int)TileTypeEnum.water, (Material)Resources.Load("Sprites/Materials/Water"));
		gm = manager.GetComponent<GameManager>();
		tilesGameBoard = new GameObject[boardX ,boardY];


		for(int x= 0;x < boardX; x++ ){
			for(int y=0; y< boardY; y++ ){
				Debug.Log(string.Format("In board Creation x:{0} y:{1}", x, y));
				tilesGameBoard[x,y] = (GameObject)Instantiate(prfbTile, new Vector3(x,y,0), Quaternion.identity);
				tilesGameBoard[x,y].GetComponent<BaseTile>().IsHover = false;
				tilesGameBoard[x,y].GetComponent<BaseTile>().MoveCost =1;	
				tilesGameBoard[x,y].GetComponent<BaseTile>().brdXPos = x;
				tilesGameBoard[x,y].GetComponent<BaseTile>().brdYPos = y;
				
				tilesGameBoard[x,y].renderer.material = textureResources[1];
				//tilesGameBoard[x,y].renderer.material= matTest;
				//tilesGameBoard[x,y].renderer.material =(Material)Resources.Load("Sprites/Materials/River");
			}
		}
		
		
		//Build the Graph
		for(int x= 0;x < boardX; x++ ){
			for(int y=0; y< boardY; y++ ){
				if((y - 1 )>= 0){
					tilesGameBoard[x,y].GetComponent<BaseTile>().South  = tilesGameBoard[x,y-1].GetComponent<BaseTile>();
				}
				if((y+ 1 )< boardY){
					tilesGameBoard[x,y].GetComponent<BaseTile>().North  = tilesGameBoard[x,y+1].GetComponent<BaseTile>();
				}
				if((x - 1 )>= 0){
					tilesGameBoard[x,y].GetComponent<BaseTile>().West  = tilesGameBoard[x-1,y].GetComponent<BaseTile>();
				}
				if((x + 1 )<  boardX){
					tilesGameBoard[x,y].GetComponent<BaseTile>().East  = tilesGameBoard[x+1,y].GetComponent<BaseTile>();
				}
			}
		}

		//Perlin Pass to generate terrrain

		perlinPass (TileTypeEnum.water, 5000);
		/*
		Character test = HeroFactory.CreateHero(HeroType.warrior, gm);
		Character enemyTest = HeroFactory.CreateEnemy(EnemyType.dragon, gm);
		tilesGameBoard[test.xPos, test.yPos].GetComponent<BaseTile>().Hero = test;
		tilesGameBoard[enemyTest.xPos, enemyTest.yPos].GetComponent<BaseTile>().Hero = enemyTest;
		*/
		gm.tiles = tilesGameBoard;
	}
	
	// Update is called once per frame
	void Update () {
		
		}

	public  void perlinPass(TileTypeEnum tte, int threshold	){
		float RandomChange  = Random.value;
		for(int x= 0;x < boardX; x++ ){
			for(int y=0; y< boardY; y++ ){
				float xVal  = (x+RandomChange)*2.5f;
				float yVal = (y+RandomChange)*2.5f;
				float perlinVal = Mathf.PerlinNoise(xVal,yVal)*10;
				
				//if(perlinVal*perlinVal > 60 && perlinVal*perlinVal < 80){
				if(perlinVal*perlinVal*perlinVal*perlinVal > threshold){
					Debug.Log(perlinVal);
					BaseTile.createTile(tte, tilesGameBoard[x,y]);
					
				}
				//tilesGameBoard[x,y].renderer.material= matTest;
				//tilesGameBoard[x,y].renderer.material =(Material)Resources.Load("Sprites/Materials/River");
			}
		}
	}
}
