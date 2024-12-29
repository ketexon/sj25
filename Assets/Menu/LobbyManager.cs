using System.Collections.Generic;
using System.Threading.Tasks;
using Kutie.Singleton;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum ProtocolType {
	Relay,
	Unity,
}



public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
	const string RELAY_JOIN_CODE_KEY = "relayJoinCode";
	const string CONNECTION_TYPE = "dtls";

	[SerializeField] string startScene = "House";
	[SerializeField] public ProtocolType ProtocolType = ProtocolType.Relay;
	[System.NonSerialized] public Lobby Lobby;
	[System.NonSerialized] public ILobbyEvents LobbyEvents;
	[System.NonSerialized] public string RelayJoinCode;
	[System.NonSerialized] public string Username;
	public int NPlayers => Lobby?.Players.Count ?? 0;

	public UnityEvent PlayerJoinedEvent;
	public UnityEvent PlayerLeftEvent;
	public UnityEvent PlayerDataChangedEvent;

	public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
	public bool IsLobbyHost => Lobby?.HostId == AuthenticationService.Instance.PlayerId;

	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(gameObject);
	}

	async void Start(){
		await UnityServices.InitializeAsync();
		Debug.Log(UnityServices.State);

		await TrySignInAsync();
	}

	public List<string> GetPlayerUsernames(){
		List<string> usernames = new();
		foreach(var player in Lobby.Players){
			if(player.Data?.ContainsKey("username") ?? false){
				usernames.Add(player.Data["username"].Value);
			}
			else {
				usernames.Add("Unknown");
			}
		}
		return usernames;
	}

	public async Task<bool> TrySignInAsync(){
        Debug.Log("Trying to sign in anonymousely...");
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
        return false;
    }

	public async Task<bool> TryCreateLobbyAsync() {
        Debug.Log("Trying to create lobby...");
        try
        {
            var currentPlayer = new Player(AuthenticationService.Instance.PlayerId);
            var partyLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = true,
                Player = currentPlayer,
            };

            Lobby = await LobbyService.Instance.CreateLobbyAsync(
                "new lobby",
                maxPlayers: 4,
                partyLobbyOptions
            );
			await OnJoinLobby();
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
            return false;
        }
    }

	void OnPlayerJoined(List<LobbyPlayerJoined> player){
		PlayerJoinedEvent.Invoke();
	}

	void OnPlayerLeft(List<int> player){
		PlayerLeftEvent.Invoke();
	}

	void OnPlayerDataChanged(
		Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> playerData
	){
		PlayerDataChangedEvent.Invoke();
	}

	async void OnLobbyDataChanged(
		Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> lobbyData
	){
		if(lobbyData?.TryGetValue(RELAY_JOIN_CODE_KEY, out var value) ?? false){
			RelayJoinCode = value.Value.Value;
			if(!IsLobbyHost){
				await StartClient();
				StartGame();
			}
		}
	}

	async Task RegisterLobbyEvents(){
		LobbyEventCallbacks callbacks = new();
		callbacks.PlayerJoined += OnPlayerJoined;
		callbacks.PlayerLeft += OnPlayerLeft;
		callbacks.PlayerDataAdded += OnPlayerDataChanged;
		callbacks.PlayerDataChanged += OnPlayerDataChanged;
		callbacks.PlayerDataRemoved += OnPlayerDataChanged;
		callbacks.DataAdded += OnLobbyDataChanged;
		callbacks.DataChanged += OnLobbyDataChanged;
		callbacks.DataRemoved += OnLobbyDataChanged;

		try {
			LobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(
				Lobby.Id,
				callbacks
			);
		}
		catch(LobbyServiceException ex){
			Debug.LogException(ex);
		}
	}

	async Task SetLobbyUsername(){
		UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
		{
			Data = new() {
				{ "username", new PlayerDataObject(
					visibility: PlayerDataObject.VisibilityOptions.Public,
					value: Username
				)}
			}
		};

		try {
			await LobbyService.Instance.UpdatePlayerAsync(
				Lobby.Id,
				AuthenticationService.Instance.PlayerId,
				updatePlayerOptions
			);
		}
		catch(LobbyServiceException ex){
			Debug.LogException(ex);
		}
	}

	async Task SetLobbyClientId(){
		UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
		{
			Data = new() {
				{ "clientId", new PlayerDataObject(
					visibility: PlayerDataObject.VisibilityOptions.Public,
					value: NetworkManager.Singleton.LocalClientId.ToString()
				)}
			}
		};

		try {
			await LobbyService.Instance.UpdatePlayerAsync(
				Lobby.Id,
				AuthenticationService.Instance.PlayerId,
				updatePlayerOptions
			);
		}
		catch(LobbyServiceException ex){
			Debug.LogException(ex);
		}
	}

	public async Task<bool> TryJoinLobbyAsync(string code){
		Debug.Log("Trying to join lobby...");
		try {
			Lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
			if(Lobby == null){
				Debug.Log("Lobby not found");
				return false;
			}
			await OnJoinLobby();
			return true;
		}
		catch(LobbyServiceException ex){
			Debug.LogException(ex);
		}
		return false;
	}

	public async Task TryLeaveLobbyAsync(){
		if(Lobby != null){
			try {
				await LobbyService.Instance.RemovePlayerAsync(
					Lobby.Id,
					AuthenticationService.Instance.PlayerId
				);
			}
			catch(LobbyServiceException ex){
				Debug.LogException(ex);
			}
		}
	}

	async Task OnJoinLobby(){
		await RegisterLobbyEvents();
		await SetLobbyUsername();
	}

	public async void StartGame(){
		Debug.Log("Starting game...");
		await SetLobbyClientId();
		SceneManager.LoadScene(startScene);
	}

	public async Task<bool> StartHost(){
		if(ProtocolType == ProtocolType.Unity){
			return NetworkManager.Singleton.StartHost();
		}
		else {
			RelayJoinCode = await StartHostWithRelay();
			if(RelayJoinCode != null){
				await SetLobbyRelayJoinCode(RelayJoinCode);
				return true;
			}
			return false;
		}
	}

	public async Task<bool> StartClient(){
		if(ProtocolType == ProtocolType.Unity){
			return NetworkManager.Singleton.StartClient();
		}
		else {
			if(RelayJoinCode == null){
				return false;
			}
			var joinAllocation = await RelayService.Instance.JoinAllocationAsync(RelayJoinCode);
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
				joinAllocation.ToRelayServerData(CONNECTION_TYPE)
			);
			return NetworkManager.Singleton.StartClient();
		}
	}

	async Task<string> StartHostWithRelay(){
		Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
			allocation.ToRelayServerData(CONNECTION_TYPE)
		);
		var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		return NetworkManager.Singleton.StartHost() ? joinCode : null;
	}

	async Task SetLobbyRelayJoinCode(string joinCode){
		UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
		{
			Data = new() {
				{RELAY_JOIN_CODE_KEY, new DataObject(
					visibility: DataObject.VisibilityOptions.Public,
					value: joinCode
				)}
			}
		};

		try {
			await LobbyService.Instance.UpdateLobbyAsync(
				Lobby.Id,
				updateLobbyOptions
			);
		}
		catch(LobbyServiceException ex){
			Debug.LogException(ex);
		}
	}
}