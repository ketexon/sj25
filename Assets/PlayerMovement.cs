using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;

    Vector2 value = Vector2.zero;

    void Update(){
        agent.velocity = new Vector3(value.x, 0, value.y)
            * agent.speed;
    }

    void OnMove(InputValue inputValue){
        value = inputValue.Get<Vector2>();
    }
}
