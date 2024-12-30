using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class HouseGenerator : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<GameObject> roomPrefabs;
    [SerializeField] Grid grid;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] int length = 10;
    [SerializeField] ScrapPool scrapPool;
    [System.NonSerialized] public Vector3 SpawnPoint;

    List<Room> rooms = new();

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
        SpawnRooms();
        if(IsServer){
            SpawnScrap();
        }
        SpawnPlayer();

        GameManager.Instance.GameReady.AddListener(OnGameReady);
    }

    void OnGameReady(){
        GameManager.Instance.StartGame(10);
    }

    void SpawnRooms(){
        Debug.Log("SPAWNING ROOMS");
        for(var x = 0; x < length; x++){
            for(var z = 0; z < length; z++){
                var roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
                var roomGO = Instantiate(roomPrefab);
                var room = roomGO.GetComponent<Room>();
                roomGO.transform.SetParent(grid.transform);
                roomGO.transform.position = grid.CellToWorld(new Vector3Int(x, 0, z));

                room.EnableDoors(
                    positiveX: x < length - 1,
                    negativeX: x > 0,
                    positiveZ: z < length - 1,
                    negativeZ: z > 0
                );

                rooms.Add(room);
            }
        }

        surface.AddData();
        surface.BuildNavMesh();
    }

    void SpawnScrap(){
        foreach(var room in rooms){
            foreach(Transform child in room.ScrapSpawnPointContainer){
                var scrap = scrapPool.Items[Random.Range(0, scrapPool.Items.Count)];
                var scrapNO = scrap.Prefab.GetComponent<NetworkObject>();
                var scrapGO = NetworkManager.SpawnManager.InstantiateAndSpawn(
                    scrapNO,
                    OwnerClientId,
                    position: child.position,
                    rotation: child.rotation
                );
            }
        }
    }

    void SpawnPlayer(){
        SpawnPlayerServerRPC();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    void SpawnPlayerServerRPC(RpcParams rpcParams = default){
        var playerId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Spawning player for {playerId}");
        List<Vector3> spawnPoints = new(){
            grid.CellToWorld(new Vector3Int(0, 0, 0)),
            grid.CellToWorld(new Vector3Int(length - 1, 0, length - 1)),
            grid.CellToWorld(new Vector3Int(length - 1, 0, 0)),
            grid.CellToWorld(new Vector3Int(0, 0, length - 1)),
        };
        Vector3 center = (spawnPoints[0] + spawnPoints[1]) / 2;
        var spawnPoint = spawnPoints[(int)playerId % spawnPoints.Count];
        Debug.Log($"Player {playerId} will spawn at {spawnPoint}");
        Quaternion spawnRot = Quaternion.LookRotation(
            center - spawnPoint,
            Vector3.up
        );
        var playerNOPrefab = playerPrefab.GetComponent<NetworkObject>();
        var playerNO = NetworkManager.SpawnManager.InstantiateAndSpawn(
            playerNOPrefab,
            playerId,
            position: spawnPoint,
            rotation: spawnRot
        );
    }
}
