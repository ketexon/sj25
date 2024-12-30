using Unity.Netcode;
using UnityEngine;

public class HillEndTrigger : MonoBehaviour
{
    [SerializeField] HillGameMode gameMode;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerSnowball"))
        {
            var snowball = other.GetComponent<HillSnowball>();
            if(snowball.IsOwner){
                gameMode.PlayerFinished();
                snowball.Finished = true;
            }
        }
    }
}
