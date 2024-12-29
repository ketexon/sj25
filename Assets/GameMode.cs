using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameMode : NetworkBehaviour
{
    [SerializeField] int duration = 10;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] string nextScene = "";

    NetworkVariable<int> seed = new(
        writePerm: NetworkVariableWritePermission.Server
    );

    List<NetworkObject> spawnedPlayers = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer){
            seed.Value = Random.Range(0, 1000000);
        }
        Random.InitState(seed.Value);
        if(GameManager.Instance.GameReady){
            OnGameReady();
        }
        else {
            GameManager.Instance.GameReadyEvent.AddListener(OnGameReady);
        }
    }

    void OnGameReady(){
        Debug.Log("Game Ready");
        GameManager.Instance.StartGame(duration);
        GameManager.Instance.NextGameEvent.AddListener(OnNextGame);
    }

    abstract protected (Vector3, Quaternion) GetPlayerSpawnPosRot(int i);

    protected void SpawnPlayer(){
        SpawnPlayerServerRPC();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    void SpawnPlayerServerRPC(RpcParams rpcParams = default){
        var playerId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Spawning player for {playerId}");
        var (spawnPos, spawnRot) = GetPlayerSpawnPosRot((int)playerId);
        Debug.Log($"Player {playerId} will spawn at {spawnPos}");
        var playerGO = Instantiate(
            playerPrefab,
            spawnPos,
            spawnRot
        );
        var playerNO = playerGO.GetComponent<NetworkObject>();
        playerNO.SpawnAsPlayerObject(playerId);
        GameManager.Instance.OnPlayerReady();

        spawnedPlayers.Add(playerNO);
    }

    void OnNextGame(){
        if(IsServer){
            Cleanup();
            if(!string.IsNullOrEmpty(nextScene)){
                GameManager.Instance.NewGame();
                ChangeSceneClientRpc();
                SceneManager.LoadScene(nextScene);
            }
            NetworkObject.Despawn(false);
        }
    }

    virtual protected void Cleanup(){
        foreach(var player in spawnedPlayers){
            player.Despawn(false);
        }
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    void ChangeSceneClientRpc(){
        GameManager.Instance.NewGame();
        SceneManager.LoadScene(nextScene);
    }
}
