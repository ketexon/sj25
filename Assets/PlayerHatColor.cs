using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerHatColor : MonoBehaviour {
	[SerializeField] List<Color> colors;
	[SerializeField] new Renderer renderer;
	[SerializeField] int materialIndex;

    public void SetPlayerIndex(int index)
    {
		renderer.materials[materialIndex].color = colors[
			index % colors.Count
		];
    }
}