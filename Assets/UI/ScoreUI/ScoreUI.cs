using System.Collections.Generic;
using Kutie.Singleton;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public enum ScoreMode {
    Scrap,
    Radius,
}

public class ScoreUI : SingletonMonoBehaviour<ScoreUI>
{
    ScoreMode _scoreMode;
    public ScoreMode ScoreMode {
        get => _scoreMode;
        set {
            if(_scoreMode != value){
                _scoreMode = value;
                UpdateUI();
            }
        }
    }
    [SerializeField] List<CanvasGroup> canvasGroups;
    [SerializeField] List<TMPro.TMP_Text> scoreTexts;

    void Start(){
        if(GameManager.Instance){
            OnGameManagerReady();
        }
        GameManager.InstanceReadyEvent.AddListener(OnGameManagerReady);
    }

    void OnGameManagerReady(){
        GameManager.InstanceReadyEvent.RemoveListener(OnGameManagerReady);

        for(int i = 0; i < canvasGroups.Count; i++){
            canvasGroups[i].alpha = i < LobbyManager.Instance.NPlayers
                ? 1
                : 0;
        }

        GameManager.Instance.Player1Scrap.OnListChanged += OnScrapChanged;
        GameManager.Instance.Player2Scrap.OnListChanged += OnScrapChanged;
        GameManager.Instance.Player3Scrap.OnListChanged += OnScrapChanged;
        GameManager.Instance.Player4Scrap.OnListChanged += OnScrapChanged;
        GameManager.Instance.PlayerRadii.OnListChanged += OnRadiusChanged;

        UpdateUI();
    }

    void OnDestroy() {
        if(GameManager.Instance){
            if(ScoreMode == ScoreMode.Scrap){
                GameManager.Instance.Player1Scrap.OnListChanged -= OnScrapChanged;
            } else if(ScoreMode == ScoreMode.Radius){
                GameManager.Instance.PlayerRadii.OnListChanged -= OnRadiusChanged;
            }
        }
    }

    void OnScrapChanged(NetworkListEvent<int> _){
        if(ScoreMode == ScoreMode.Scrap){
            UpdateUI();
        }
    }

    void OnRadiusChanged(NetworkListEvent<float> _){
        if(ScoreMode == ScoreMode.Radius){
            UpdateUI();
        }
    }

    void UpdateUI(){
        for(int i = 0; i < LobbyManager.Instance.NPlayers; i++){
            if(ScoreMode == ScoreMode.Scrap){
                scoreTexts[i].text = GameManager.Instance.GetPlayerScrapIndices(i).Count.ToString();
            } else if(ScoreMode == ScoreMode.Radius){
                scoreTexts[i].text = GameManager.Instance.PlayerRadii[i].ToString("F2");
            }
        }
    }
}
