using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class HouseGenerator : NetworkBehaviour
{
    [SerializeField] List<GameObject> roomPrefabs;
    [SerializeField] Grid grid;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] int length = 10;
    [SerializeField] ScrapPool scrapPool;

    List<Room> rooms = new();

    NetworkVariable<int> seed = new(
        writePerm: NetworkVariableWritePermission.Server
    );

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
    }

    void SpawnRooms(){
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
}
