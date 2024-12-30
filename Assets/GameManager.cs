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

	public NetworkList<float> PlayerRadii = new(new float[4] {
		0, 0, 0, 0
	});

	public NetworkList<float> PlayerMasses = new(new float[4] {
		0, 0, 0, 0
	});

	NetworkVariable<int> nPlayersReady = new(
		writePerm: NetworkVariableWritePermission.Server
	);

	public UnityEvent GameReadyEvent = new();
	public UnityEvent<int> CountdownEvent = new();
	public UnityEvent GameStartEvent = new();
	public UnityEvent GameEndEvent = new();
	public UnityEvent NextGameEvent = new();

	[System.NonSerialized] public bool GameReady = false;

	int gameDuration = 0;

	public NetworkVariable<int> TimeRemaining = new(
        writePerm: NetworkVariableWritePermission.Server
    );

	public NetworkList<int> PlayerScrapIndices => GetPlayerScrapIndices((int)NetworkManager.Singleton.LocalClientId);

	Coroutine countDownCoro = null;

	public NetworkList<int> GetPlayerScrapIndices(int playerIndex){
		return playerIndex switch {
			0 => Player1Scrap,
			1 => Player2Scrap,
			2 => Player3Scrap,
			3 => Player4Scrap,
			_ => null
		};
	}

	public List<Scrap> GetPlayerScrap(int playerIndex){
		var networkList = playerIndex switch {
			0 => Player1Scrap,
			1 => Player2Scrap,
			2 => Player3Scrap,
			3 => Player4Scrap,
			_ => null
		};
		if(networkList == null) return null;

		var scrapList = new List<Scrap>();
		foreach(var scrapIndex in networkList){
			scrapList.Add(indexScrapMap[scrapIndex]);
		}
		return scrapList;
	}

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

	[Rpc(SendTo.Server, RequireOwnership = false)]
	public void SetRadiusServerRpc(float radius, RpcParams rpcParams = default) {
		var playerIndex = rpcParams.Receive.SenderClientId;
		PlayerRadii[(int) playerIndex] = radius;
	}

	[Rpc(SendTo.Server, RequireOwnership = false)]
	public void SetMassServerRpc(float mass, RpcParams rpcParams = default) {
		var playerIndex = rpcParams.Receive.SenderClientId;
		PlayerMasses[(int) playerIndex] = mass;
	}

	public void CollectScrap(Scrap scrap){
		var scrapIndex = scrapIndexMap[scrap];
		CollectScrapServerRpc(scrapIndex);
	}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
		NewGame();
    }

	public void NewGame(){
		Debug.Log($"Waiting for {LobbyManager.Instance.NPlayers} players to connect...");
		if(IsServer){
			nPlayersReady.Value = 0;
		}
		nPlayersReady.OnValueChanged += OnNPlayersReadyChanged;
	}

	public void OnPlayerReady(){
		nPlayersReady.Value++;
	}

	void OnNPlayersReadyChanged(int _, int newValue){
		Debug.Log($"New player connected");
		if(newValue == LobbyManager.Instance.NPlayers){
			nPlayersReady.OnValueChanged -= OnNPlayersReadyChanged;
			GameReady = true;
			GameReadyEvent.Invoke();
		}
	}

	public void StartTimer(int nSeconds){
		TimeRemaining.OnValueChanged += OnTimeRemainingChanged;
		timerText.text = nSeconds.ToString();
		if(!IsServer) return;
		TimeRemaining.Value = nSeconds;
		IEnumerator Coro(){
			while(TimeRemaining.Value > 0){
				yield return new WaitForSeconds(1);
				TimeRemaining.Value--;
			}
			EndGame();
		}
		StartCoroutine(Coro());
	}

	void OnTimeRemainingChanged(int _, int time){
		if(TimeRemaining.Value <= 0){
			TimeRemaining.OnValueChanged -= OnTimeRemainingChanged;
			timerText.text = "";
			centerText.text = "TIME'S UP!";
			centerAnimator.SetTrigger("show");
			EndGame();
		}
		else {
			timerText.text = TimeRemaining.Value.ToString();
		}
	}

	public void OnTimesUpFinished(){
		NextGameEvent.Invoke();
	}

	public void StartGame(int duration){
		gameDuration = duration;

		IEnumerator Coro(){
			yield return new WaitForSeconds(3);
			int i = 3;
			while(i > 0){
				Debug.Log($"COUNTDOWN: {i}");
				CountdownEvent.Invoke(i);

				centerText.text = i.ToString();
				centerAnimator.SetTrigger("countdown");

				yield return new WaitForSeconds(1);
				i--;
			}

			if(IsServer){
				EndCountdownClientRpc();
			}
		}
		countDownCoro = StartCoroutine(Coro());
	}

	[Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
	void EndCountdownClientRpc(){
		if(countDownCoro != null){
			StopCoroutine(countDownCoro);
			countDownCoro = null;
		}

		CountdownEvent.Invoke(0);
		centerText.text = "GO!";
		centerAnimator.SetTrigger("countdown");
		GameStartEvent.Invoke();
		Debug.Log("GAME START");
		StartTimer(gameDuration);
	}

	public void EndGame(){
		GameEndEvent.Invoke();
	}
}