using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class HillSnowball : NetworkBehaviour
{
    [SerializeField] float baseBreakingImpulse = 10.0f;
    [SerializeField] GameObject fracturedSnowballPrefab;
    [SerializeField] float startImpulse = 10.0f;
    [SerializeField] Transform center;

    [System.NonSerialized] public float Radius = 0;
    [System.NonSerialized] public float Mass = 0;

    float breakingImpulse = 0.0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CalculateParams();

        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);

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
            rb.AddForce(
                transform.forward * startImpulse / Mass,
                ForceMode.Impulse
            );
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
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(IsServer){
            var impulse = collision.impulse.magnitude;
            Debug.Log($"Impulse {OwnerClientId}: {impulse}");
            if(impulse > breakingImpulse){
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
    }
}
