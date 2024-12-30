using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    void Start(){
        canvasGroup.alpha = 1;
        GameManager.Instance.GameReadyEvent.AddListener(OnGameReady);
        GameManager.Instance.NextGameEvent.AddListener(OnNextGame);
    }

    void OnGameReady(){
        canvasGroup.alpha = 0;
    }

    void OnNextGame(){
        canvasGroup.alpha = 1;
    }
}
