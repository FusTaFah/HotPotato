using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChefBehaviour : NetworkBehaviour
{
    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams serverRpcParams = default)
    {

    }

    public void OnMove()
    {
        
    }



    void Update()
    {
        
    }
}
