using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

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
    }

    // Use this for initialization
    void Start () {
        
        for (int i = 0; i < 10; i++){
            if (networkObject.IsServer)
            {
                NetworkManager.Instance.InstantiateNetTile(0, Vector3.zero + new Vector3(i, 0, 0), Quaternion.identity);

            }
        

        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
