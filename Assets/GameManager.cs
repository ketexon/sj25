using System.Collections;
using System.Collections.Generic;
using Kutie.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : NetworkBehaviour {
	public static GameManager Instance;
	public static UnityEvent InstanceReadyEvent = new();


	[SerializeField]
	ScrapPool allScrap;
	[SerializeField] TMPro.TMP_Text timerText;
	[SerializeField] TMPro.TMP_Text centerText;
    [SerializeField] Animator centerAnimator;

	Dictionary<int, Scrap> indexScrapMap = new();
	Dictionary<Scrap, int> scrapIndexMap = new();

	public NetworkList<int> Player1Scrap = new();
	public NetworkList<int> Player2Scrap = new();
	public NetworkList<int> Player3Scrap = new();
	public NetworkList<int> Player4Scrap = new();

	public UnityEvent GameReady = new();
	public UnityEvent<int> CountdownEvent = new();
	public UnityEvent GameStartEvent = new();
	public UnityEvent GameEndEvent = new();

	public NetworkVariable<int> TimeRemaining = new(
        writePerm: NetworkVariableWritePermission.Server
    );

	public NetworkList<int> PlayerScrap => NetworkManager.Singleton.LocalClientId switch {
		0 => Player1Scrap,
		1 => Player2Scrap,
		2 => Player3Scrap,
		3 => Player4Scrap,
		_ => null
	};

	void Awake() {
		if(Instance){
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instance = this;
		InstanceReadyEvent.Invoke();

		foreach(var (i, scrap) in allScrap.Items.ZipIndex()){
			indexScrapMap[i] = scrap;
			scrapIndexMap[scrap] = i;
		}
	}

	[Rpc(SendTo.Server, RequireOwnership = false)]
	void CollectScrapServerRpc(int scrapIndex, RpcParams rpcParams = default){
		var playerIndex = rpcParams.Receive.SenderClientId;
		var networkList = playerIndex switch {
			0 => Player1Scrap,
			1 => Player2Scrap,
			2 => Player3Scrap,
			3 => Player4Scrap,
			_ => null
		};
		if(networkList == null) {
			Debug.LogWarning($"Player {playerIndex} is not in the game");
			return;
		}
		networkList.Add(scrapIndex);

		Debug.Log($"Player {playerIndex} collected scrap {indexScrapMap[scrapIndex].Name}");
	}

	public void CollectScrap(Scrap scrap){
		var scrapIndex = scrapIndexMap[scrap];
		CollectScrapServerRpc(scrapIndex);
	}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

		Debug.Log($"Waiting for {LobbyManager.Instance.NPlayers} players to connect...");
		// wait for all players in lobby to connect, then start the game
		if(NetworkManager.Singleton.ConnectedClientsList.Count == LobbyManager.Instance.NPlayers){
			GameReady.Invoke();
		}
		else {
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		}
    }

	void OnClientConnected(ulong _){
		Debug.Log($"New player connected");
		if(NetworkManager.Singleton.ConnectedClientsList.Count == LobbyManager.Instance.NPlayers){
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
			GameReady.Invoke();
		}
	}

	public void StartTimer(int nSeconds){
		if(!IsServer) return;
		TimeRemaining.Value = nSeconds;
		IEnumerator Coro(){
			while(TimeRemaining.Value > 0){
				timerText.text = TimeRemaining.Value.ToString();
				yield return new WaitForSeconds(1);
				TimeRemaining.Value--;
			}
			timerText.text = "";
			centerText.text = "TIME'S UP!";
			centerAnimator.SetTrigger("show");
			EndGame();
		}
		StartCoroutine(Coro());
	}

	public void StartGame(int duration){
		if(!IsServer) return;
		IEnumerator Coro(){
			int i = 3;
			while(i > 0){
				Debug.Log($"COUNTDOWN: {i}");
				CountdownEvent.Invoke(i);

				centerText.text = i.ToString();
				centerAnimator.SetTrigger("countdown");

				yield return new WaitForSeconds(1);
				i--;
			}

			CountdownEvent.Invoke(0);
			centerText.text = "GO!";
			centerAnimator.SetTrigger("countdown");

			GameStartEvent.Invoke();
			Debug.Log("GAME START");
			StartTimer(duration);
		}
		StartCoroutine(Coro());
	}

	public void EndGame(){
		GameEndEvent.Invoke();
	}
}