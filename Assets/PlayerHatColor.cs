using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerHatColor : NetworkBehaviour {
	[SerializeField] List<Color> colors;
	[SerializeField] new Renderer renderer;
	[SerializeField] int materialIndex;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
		renderer.materials[materialIndex].color = colors[
			(int) OwnerClientId % colors.Count
		];
    }
}