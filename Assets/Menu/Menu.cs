using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField usernameField;
    [SerializeField] Button hostButton;
    [SerializeField] TMPro.TMP_InputField inviteCodeField;
    [SerializeField] Button joinButton;

    void Start()
    {
        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinLobby);
    }

    async void HostGame()
    {
        if(usernameField.text == ""){
            Debug.Log("Username field is empty");
            return;
        }
        SetUsername();

        Debug.Log("Trying to host game...");
        if(!LobbyManager.Instance.IsSignedIn){
            Debug.Log("Not signed in, cannot host game");
            return;
        }

        var success = await LobbyManager.Instance.TryCreateLobbyAsync();
        if(success){
            SceneManager.LoadScene("Lobby");
        }
    }

    async void JoinLobby(){
        if(usernameField.text == ""){
            Debug.Log("Username field is empty");
            return;
        }
        SetUsername();
        Debug.Log("Trying to join lobby...");
        var code = inviteCodeField.text;
        var success = await LobbyManager.Instance.TryJoinLobbyAsync(code);
        if(success){
            SceneManager.LoadScene("Lobby");
        }
    }

    void SetUsername(){
        LobbyManager.Instance.Username = usernameField.text;
    }
}
