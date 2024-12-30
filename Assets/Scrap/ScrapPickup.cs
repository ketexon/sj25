using Unity.Netcode;
using UnityEngine;

public class ScrapPickup : NetworkBehaviour
{
	[SerializeField] ScrapPool scrapPool;

	public Scrap Scrap => scrapPool.Items[ScrapIndex.Value];

	public NetworkVariable<int> ScrapIndex;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

		Instantiate(Scrap.Prefab, transform);
    }

    [ServerRpc(RequireOwnership = false)]
	public void PickupServerRpc(){
		NetworkObject.Despawn(true);
	}
}
