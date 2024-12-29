using Unity.Netcode;
using UnityEngine;

public class ScrapPickup : NetworkBehaviour
{
	[SerializeField] public Scrap Scrap;

	[ServerRpc(RequireOwnership = false)]
	public void PickupServerRpc(){
		NetworkObject.Despawn(true);
	}
}
