using UnityEngine;
using System.Collections;

public class Ground  {

	// Use this for initialization
	private Ground _North;
	private Ground _South;
	private Ground _East;
	private Ground _West;
	
	private int _MoveCost;
	private float _DamageModifier;
	
	private BaseTile _BT; 
	

	
	public BaseTile BT {
		get {
			return this._BT;
		}
		set {
			_BT = value;
		}
	}

	public Ground East {
		get {
			return this._East;
		}
		set {
			_East = value;
		}
	}

	public Ground North {
		get {
			return this._North;
		}
		set {
			_North = value;
		}
	}

	public Ground South {
		get {
			return this._South;
		}
		set {
			_South = value;
		}
	}

	public Ground West {
		get {
			return this._West;
		}
		set {
			_West = value;
		}
	}
	
	public Ground(BaseTile BT){
		this.BT = BT;
	}

	public float DamageModifier {
		get {
			return this._DamageModifier;
		}
		set {
			_DamageModifier = value;
		}
	}

	public int MoveCost {
		get {
			return this._MoveCost;
		}
		set {
			_MoveCost = value;
		}
	}
}
