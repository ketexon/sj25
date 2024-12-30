using System.Collections.Generic;
using UnityEngine;

public class HillGameMode : GameMode
{
    [SerializeField] List<Transform> spawnPoints = new();

    protected override (Vector3, Quaternion) GetPlayerSpawnPosRot(int i)
    {
        var spawnPoint = spawnPoints[i];
        return (spawnPoint.position, spawnPoint.rotation);
    }
}
