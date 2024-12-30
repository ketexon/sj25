using UnityEngine;

[CreateAssetMenu(fileName = "Scrap", menuName = "Scrap")]
public class Scrap : ScriptableObject {
	[SerializeField] public string Name;
	[SerializeField] public GameObject Prefab;
}