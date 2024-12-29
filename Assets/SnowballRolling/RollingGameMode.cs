using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RollingGameMode : GameMode
{
    [SerializeField] SpawnSnow spawnSnow;
    [SerializeField] Transform spawnPointsContainer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SpawnPlayer();
    }

    protected override (Vector3, Quaternion) GetPlayerSpawnPosRot(int i)
    {
        var spawnPoint = spawnPointsContainer.GetChild(i);
        return (spawnPoint.position, spawnPoint.rotation);
    }
}
