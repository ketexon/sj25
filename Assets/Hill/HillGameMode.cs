using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HillGameMode : GameMode
{
    [SerializeField] Transform spawnPointContainer;

    int playersFinished = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);

        var playerId = NetworkManager.Singleton.LocalClientId;
        // don't spawn player if their snowball has 0 mass
        if(GameManager.Instance.PlayerMasses[(int)playerId] > 0){
            SpawnPlayer();
        }
        else {
            ReadyUp();
            playersFinished++;
        }
        HillSnowball.FracturedEvent.AddListener(PlayerFinished);
    }

    private void OnGameStart()
    {
        GameManager.Instance.GameStartEvent.RemoveListener(OnGameStart);
    }

    protected override (Vector3, Quaternion) GetPlayerSpawnPosRot(int i)
    {
        var spawnPoint = spawnPointContainer.GetChild(i);
        return (spawnPoint.position, spawnPoint.rotation);
    }

    public void PlayerFinished(){
        if(IsServer){
            Debug.Log("Player finished");
            playersFinished++;
            TryEndEarly();
        }
    }

    void TryEndEarly(){
        if(playersFinished == NetworkManager.Singleton.ConnectedClients.Count){
            GameManager.Instance.EndGame();
        }
    }
}
