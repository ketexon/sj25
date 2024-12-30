using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    void Start()
    {
        if(NetworkManager.Singleton.IsServer){
            var noPrefab = prefab.GetComponent<NetworkObject>();
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                noPrefab,
                NetworkManager.Singleton.LocalClientId
            );
        }
    }
}
