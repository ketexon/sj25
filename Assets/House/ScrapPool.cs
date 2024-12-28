using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScrapPool", menuName = "Scrap Pool")]
public class ScrapPool : ScriptableObject {
	[SerializeField] public List<Scrap> Items;
}