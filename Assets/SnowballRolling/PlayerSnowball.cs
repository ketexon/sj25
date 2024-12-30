using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerSnowball : NetworkBehaviour
{
    [SerializeField] GameObject snowballPrefab;
    [SerializeField] Transform snowballSocket;
    [SerializeField] NavMeshAgent agent;

    Snowball snowball;

    float agentDefaultSpeed = 3.5f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"Player {OwnerClientId} spawned");

        agentDefaultSpeed = agent.speed;
        // spawn snowball
        var snowballGO = Instantiate(
            snowballPrefab,
            snowballSocket
        );
        snowball = snowballGO.GetComponent<Snowball>();
        snowball.Scrap = GameManager.Instance.GetPlayerScrap((int)OwnerClientId);
        if(IsOwner){
            GameManager.Instance.SetRadiusServerRpc(snowball.Radius);
            snowball.SnowCollectedEvent.AddListener(OnSnowCollected);
        }
    }

    void OnSnowCollected(){
        GameManager.Instance.SetRadiusServerRpc(snowball.Radius);
    }

    void Update(){
        if(!IsSpawned) return;
        agent.speed = agentDefaultSpeed / (1 + snowball.Mass);
    }
}
