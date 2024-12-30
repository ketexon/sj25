using System.Collections.Generic;
using UnityEngine;

public class Scores : MonoBehaviour
{
    [SerializeField] List<GameObject> snowmen;
    [SerializeField] List<TMPro.TMP_Text> scoreTexts;

    struct Score {
        public int PlayerIndex;
        public float Radius;
        public bool Died;
    }

    List<Score> scores = new();

    void Start(){
        GameManager.Instance.ReadyUpServerRpc();

        for(int i = 0; i < snowmen.Count; i++){
            snowmen[i].SetActive(
                i < LobbyManager.Instance.NPlayers
            );
            scoreTexts[i].gameObject.SetActive(
                i < LobbyManager.Instance.NPlayers
            );
        }
        CalculateScores();
        // set snowmen hat colors
        for(int i = 0; i < scores.Count; i++){
            var playerId = scores[i].PlayerIndex;
            var snowman = snowmen[i].GetComponent<PlayerHatColor>();
            snowman.SetPlayerIndex(playerId);
            var dead = scores[i].Died;
            var playerName = GameManager.Instance.PlayerNames[playerId].ToString();
            scoreTexts[i].text = $"{(dead ? "<s>" : "")}{playerName}{(dead ? "</s>" : "")}\n";
            scoreTexts[i].text += $"<size=75%>Radius: {scores[i].Radius:F2}</size>";
        }
    }

    void CalculateScores(){
        // the player with the largest radius
        // who didn't die wins
        scores = new();
        for(int i = 0; i < LobbyManager.Instance.NPlayers; i++){
            var radius = GameManager.Instance.PlayerRadii[i];
            var dead = GameManager.Instance.PlayerDead[i];
            var score = new Score {
                PlayerIndex = i,
                Radius = radius,
                Died = dead
            };
            scores.Add(score);
        }

        scores.Sort((a, b) => {
            if(a.Died && !b.Died){
                return 1;
            }
            if(!a.Died && b.Died){
                return -1;
            }
            return b.Radius.CompareTo(a.Radius);
        });
    }
}
