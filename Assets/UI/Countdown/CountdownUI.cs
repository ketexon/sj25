using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] Animator animator;

    void Start()
    {
        GameManager.InstanceReadyEvent.AddListener(OnGameManagerInstanceReady);
        if(GameManager.Instance) OnGameManagerInstanceReady();
    }

    void OnGameManagerInstanceReady(){
        GameManager.InstanceReadyEvent.RemoveListener(OnGameManagerInstanceReady);
        GameManager.Instance.CountdownEvent.AddListener(OnCountdown);
    }

    void OnCountdown(int countdown){
        if(countdown == 0) {
            text.text = "GO!";
        }
        else {
            text.text = countdown.ToString();
        }
        animator.SetTrigger("countdown");
    }
}
