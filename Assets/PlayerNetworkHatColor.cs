using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHatColor : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
		GetComponent<PlayerHatColor>().SetPlayerIndex((int) OwnerClientId);
    }
}
