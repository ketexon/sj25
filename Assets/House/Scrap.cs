using UnityEngine;

[CreateAssetMenu(fileName = "Scrap", menuName = "Scrap")]
public class Scrap : ScriptableObject {
	[SerializeField] public string Name;
	[SerializeField] public GameObject Prefab;
	[SerializeField] public float Length;
	[SerializeField] public float Mass;
	[SerializeField] public float BreakingImpulse;
}