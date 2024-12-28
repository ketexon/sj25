using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Kutie.Extensions;
using Kutie.Inspector;
using Kutie;

public class Room : MonoBehaviour
{
    public static Vector3 PlayerPosition = new Vector3(0, 0, 0);

    [SerializeField] float wallHiddenOpacity = 0.5f;
    [SerializeField] Transform topCorner;
    [SerializeField] Transform bottomCorner;

    [Header("Walls")]
    [Tooltip("0: positive x, 1: negative x, 2: positive z, 3: negative z")]
    [SerializeField] List<GameObject> wallsWithDoors;
    [SerializeField] List<GameObject> wallsWithoutDoors;
    List<Renderer> wallsWithDoorsRenderers = new();
    List<Renderer> wallsWithoutDoorsRenderers = new();

    [Header("Scrap")]
    [SerializeField] public Transform ScrapSpawnPointContainer;

    Color defaultWallColor;

    [SerializeField] SpringParameters springParameters;
    List<SpringFloat> doorAlphas;

    void Awake() {
        foreach (var wall in wallsWithDoors) {
            wallsWithDoorsRenderers.Add(wall.GetComponent<Renderer>());
        }
        foreach (var wall in wallsWithoutDoors) {
            wallsWithoutDoorsRenderers.Add(wall.GetComponent<Renderer>());
        }
        defaultWallColor = wallsWithoutDoorsRenderers[0].material.color;
        doorAlphas = new() {
            new(1, springParameters),
            new(1, springParameters),
            new(1, springParameters),
            new(1, springParameters)
        };
    }

    public void EnableDoors(
        bool positiveX,
        bool negativeX,
        bool positiveZ,
        bool negativeZ
    ){
        wallsWithDoors[0].SetActive(positiveX);
        wallsWithDoors[1].SetActive(negativeX);
        wallsWithDoors[2].SetActive(positiveZ);
        wallsWithDoors[3].SetActive(negativeZ);

        wallsWithoutDoors[0].SetActive(!positiveX);
        wallsWithoutDoors[1].SetActive(!negativeX);
        wallsWithoutDoors[2].SetActive(!positiveZ);
        wallsWithoutDoors[3].SetActive(!negativeZ);
    }

    void Update(){
        bool positiveZShown = PlayerPosition.z < topCorner.position.z;
        bool negativeZShown = PlayerPosition.z < bottomCorner.position.z;
        bool positionXShown = PlayerPosition.x < topCorner.position.x;
        bool negativeXShown = PlayerPosition.x < bottomCorner.position.x;

        List<bool> shown = new() {
            positionXShown,
            negativeXShown,
            positiveZShown,
            negativeZShown
        };
        if(transform.position == Vector3.zero){
            Debug.Log($"SHOWN: 0: {shown[0]} {doorAlphas[0].CurrentValue} 1: {shown[1]} {doorAlphas[1].CurrentValue} 2: {shown[2]} {doorAlphas[2].CurrentValue} 3: {shown[3]} {doorAlphas[3].CurrentValue}");
        }

        for(int i = 0; i < 4; ++i){
            Color col = defaultWallColor.WithA(
                doorAlphas[i].CurrentValue
            );
            wallsWithoutDoorsRenderers[i].material.color = col;
            wallsWithDoorsRenderers[i].material.color = col;

            doorAlphas[i].TargetValue = shown[i] ? 1 : wallHiddenOpacity;
            doorAlphas[i].Update(Time.deltaTime);
        }
    }
}
