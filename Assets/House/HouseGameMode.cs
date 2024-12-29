using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class HouseGameMode : GameMode
{
    [SerializeField] List<GameObject> roomPrefabs;
    [SerializeField] Grid grid;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] int length = 10;
    [SerializeField] GameObject scrapPickupPrefab;
    [SerializeField] ScrapPool scrapPool;

    List<Room> rooms = new();

    List<Vector3> spawnPoints;

    List<NetworkObject> spawnedScrap = new();

    void Awake(){
        spawnPoints = new(){
            grid.CellToWorld(new Vector3Int(0, 0, 0)),
            grid.CellToWorld(new Vector3Int(length - 1, 0, length - 1)),
            grid.CellToWorld(new Vector3Int(length - 1, 0, 0)),
            grid.CellToWorld(new Vector3Int(0, 0, length - 1)),
        };
    }

    protected override (Vector3, Quaternion) GetPlayerSpawnPosRot(int i)
    {
        var spawnPos = spawnPoints[i];
        var center = (spawnPoints[0] + spawnPoints[1]) / 2;
        var spawnRot = Quaternion.LookRotation(
            center - spawnPos,
            Vector3.up
        );

        return (spawnPos, spawnRot);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SpawnRooms();
        if(IsServer){
            SpawnScrap();
        }
        SpawnPlayer();
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
                var scrapPickupGO = Instantiate(
                    scrapPickupPrefab,
                    child.position,
                    child.rotation
                );
                var scrapPickup = scrapPickupGO.GetComponent<ScrapPickup>();
                scrapPickup.ScrapIndex.Value = Random.Range(0, scrapPool.Items.Count);
                var scrapPickupNO = scrapPickupGO.GetComponent<NetworkObject>();
                scrapPickupNO.Spawn();

                spawnedScrap.Add(scrapPickupNO);
            }
        }
    }

    protected override void Cleanup()
    {
        base.Cleanup();

        foreach(var scrap in spawnedScrap){
            scrap.Despawn();
        }
    }
}
