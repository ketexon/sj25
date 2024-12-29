using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent;
    new CinemachineCamera camera;

    Vector3 moveDir = Vector3.zero;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"Player spawned with id {OwnerClientId}");
        if(!IsOwner) {
            Debug.Log($"Disabling player input for {OwnerClientId}");
            GetComponent<PlayerInput>().enabled = false;
        }
        agent.Warp(transform.position);
        enabled = false;
        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);
        GameManager.Instance.GameEndEvent.AddListener(OnGameEnd);
    }

    void Start(){
        camera = CinemachineBrain.GetActiveBrain(0)
            .ActiveVirtualCamera as CinemachineCamera;
    }

    void OnGameStart(){
        if(IsOwner) {
            Debug.Log("Reenabling input.");
            enabled = true;
        }
    }

    void OnGameEnd(){
        if(IsOwner) enabled = false;
    }

    void Update(){
        if (!IsOwner || !IsSpawned) return;
        if(camera == null) {
            camera = CinemachineBrain.GetActiveBrain(0)
                .ActiveVirtualCamera as CinemachineCamera;
                return;
        }

        var forward = camera.transform.forward;
        forward.y = 0;
        var rotation = Quaternion.LookRotation(forward);
        agent.velocity = rotation * moveDir * agent.speed;

        Room.PlayerPosition = transform.position;
    }

    void OnMove(InputValue inputValue){
        if(!IsOwner || !IsSpawned || !enabled) return;
        var value = inputValue.Get<Vector2>();
        moveDir = new Vector3(value.x, 0, value.y);
    }
}
