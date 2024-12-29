using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnowballCore : NetworkBehaviour
{
    public NetworkVariable<int> PlayerIndex;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        var scrap = GameManager.Instance.GetPlayerScrap(PlayerIndex.Value);
        SetScrap(scrap);
    }

    public void SetScrap(List<Scrap> scrap){
        // spawn each of the scrap's prefabs
        foreach(Scrap s in scrap){
            var scrapGO = Instantiate(s.Prefab);
            scrapGO.transform.SetParent(transform);
            scrapGO.transform.localPosition = Vector3.zero;
        }
    }
}
