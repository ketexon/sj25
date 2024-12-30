using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class HillSnowball : NetworkBehaviour
{
    public static UnityEvent FracturedEvent = new();

    [SerializeField] float baseBreakingImpulse = 10.0f;
    [SerializeField] GameObject fracturedSnowballPrefab;
    [SerializeField] float startVelocity = 2.0f;
    [SerializeField] Transform center;

    [System.NonSerialized] public float Radius = 0;
    [System.NonSerialized] public float Mass = 0;

    float breakingImpulse = 0.0f;
    [System.NonSerialized] public bool Finished = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CalculateParams();

        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);
        GameManager.Instance.GameEndEvent.AddListener(OnGameEnd);

        HillCameraManager.Instance.Targets.Add(center);
        if(IsOwner){
            HillCameraManager.Instance.PrimaryTarget = center;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        HillCameraManager.Instance.Targets.Remove(transform);
        if(IsOwner){
            HillCameraManager.Instance.PrimaryTarget = null;
        }
    }

    void OnGameStart(){
        GameManager.Instance.GameStartEvent.RemoveListener(OnGameStart);
        if(IsServer){
            var rb = GetComponent<Rigidbody>();
            rb.linearVelocity = transform.forward * startVelocity;
        }
    }

    void CalculateParams(){
        breakingImpulse = baseBreakingImpulse;
        var playerScrap = GameManager.Instance.GetPlayerScrap((int)OwnerClientId);
        foreach(var scrap in playerScrap){
            breakingImpulse += scrap.BreakingImpulse;
        }

        Radius = GameManager.Instance.PlayerRadii[(int)OwnerClientId];
        Mass = GameManager.Instance.PlayerMasses[(int)OwnerClientId];

        if(IsServer){
            transform.localScale = Vector3.one * Radius;
            var rb = GetComponent<Rigidbody>();
            rb.mass = Mass;

            Debug.Log($"Snowball {OwnerClientId}: r: {Radius}, m: {Mass}, I: {breakingImpulse}");
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(IsServer && !Finished){
            var impulse = collision.impulse.magnitude;
            Debug.Log($"Impulse {OwnerClientId}: {impulse}");
            if(impulse > breakingImpulse){
                GameManager.Instance.PlayerDead[(int)OwnerClientId] = true;
                FractureClientRpc();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = true)]
    void FractureClientRpc(){
        if(IsServer){
            GetComponent<NetworkObject>().Despawn(true);
        }
        var fracturedGO = Instantiate(
            fracturedSnowballPrefab,
            transform.position,
            transform.rotation
        );
        fracturedGO.transform.localScale = transform.localScale;
        FracturedEvent.Invoke();
    }

    void OnGameEnd(){
        GameManager.Instance.GameEndEvent.RemoveListener(OnGameEnd);
        Finished = true;
    }
}
