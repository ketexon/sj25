using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] float inactiveAlpha = 0.5f;
    [SerializeField] TMP_Text inviteCodeText;
    [SerializeField] Button startGameButton;
    [SerializeField] List<CanvasGroup> playerCanvasGroups;
    [SerializeField] List<TMP_Text> playerUsernames;

    void Start()
    {
        inviteCodeText.text = GameManager.Instance.Lobby.LobbyCode;
        if(GameManager.Instance.IsLobbyHost){
            startGameButton.onClick.AddListener(StartGame);
        }
        else {
            startGameButton.gameObject.SetActive(false);
        }

        UpdatePlayerList();

        GameManager.Instance.PlayerJoinedEvent.AddListener(UpdatePlayerList);
        GameManager.Instance.PlayerLeftEvent.AddListener(UpdatePlayerList);
        GameManager.Instance.PlayerDataChangedEvent.AddListener(UpdatePlayerList);
    }

    void UpdatePlayerList(){
        var usernames = GameManager.Instance.GetPlayerUsernames();
        for(int i = 0; i < playerCanvasGroups.Count; i++){
            if(i < usernames.Count){
                playerCanvasGroups[i].alpha = 1;
                playerUsernames[i].text = usernames[i];
            }
            else {
                playerCanvasGroups[i].alpha = inactiveAlpha;
                playerUsernames[i].text = "";
            }
        }
    }

    async void StartGame()
    {
        await GameManager.Instance.StartHost();
        GameManager.Instance.StartGame();
    }
}
