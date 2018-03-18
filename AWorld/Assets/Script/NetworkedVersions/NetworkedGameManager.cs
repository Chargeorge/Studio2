using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;

using BeardedManStudios.Forge.Networking;
using UnityEngine;

public class NetworkedGameManager : NetGameManaegerBehavior {


    public override void RequestGeneratedMap(RpcArgs args)
    {
        
        if(networkObject.IsServer){
            
        }
    }

    public override void SetGeneratedMap(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
