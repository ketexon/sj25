using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : NetworkBehaviour {
	[SerializeField] PlayerVision vision;
	[SerializeField] float pickupAngle = 45;

	ScrapPickup pickup = null;

	void Start(){
		if(!IsOwner) {
			vision.gameObject.SetActive(false);
			return;
		}
	}

	void Update(){
		if(!IsOwner) return;

		foreach(var scrap in vision.ScrapPickups){
			if(!scrap) continue;
			var angle = Vector3.Angle(
				transform.forward,
				scrap.transform.position - transform.position
			);
			if(angle < pickupAngle){
				pickup = scrap;
				break;
			}
		}
	}

	void OnUse(InputValue inputValue){
		if(inputValue.isPressed && pickup != null){
			var scrap = pickup.Scrap;
			pickup.PickupServerRpc();
			pickup = null;
			GameManager.Instance.CollectScrap(scrap);
		}
	}
}