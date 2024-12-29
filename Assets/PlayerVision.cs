using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class PlayerVision : MonoBehaviour
{
    [SerializeField] LayerMask pickupLayerMask;

    [SerializeField] public UnityEvent<ScrapPickup> ScrapPickupVisibleEvent;
    [SerializeField] public UnityEvent<ScrapPickup> ScrapPickupInvisibleEvent;

    [System.NonSerialized] public List<ScrapPickup> ScrapPickups = new();

    void OnTriggerEnter(Collider other) {
        var scrapPickup = other.GetComponent<ScrapPickup>();
        ScrapPickups.Add(scrapPickup);
        ScrapPickupVisibleEvent.Invoke(scrapPickup);
    }

    void OnTriggerExit(Collider other) {
        var scrapPickup = other.GetComponent<ScrapPickup>();
        ScrapPickups.Remove(scrapPickup);
        ScrapPickupInvisibleEvent.Invoke(scrapPickup);
    }
}
