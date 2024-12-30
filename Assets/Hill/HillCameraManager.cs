using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class HillCameraManager : MonoBehaviour
{
    public static HillCameraManager Instance = null;

    [System.NonSerialized] public Transform PrimaryTarget;
    [System.NonSerialized] public List<Transform> Targets = new();

    public Transform Target {
        get {
            if(PrimaryTarget != null){
                return PrimaryTarget;
            }
            else if(Targets.Count > 0){
                // get target with lowest z value
                var target = Targets[0];
                foreach(var t in Targets){
                    if(t && t.position.z < target.position.z){
                        target = t;
                    }
                }
                return target;
            }
            else {
                return null;
            }
        }
    }

    [SerializeField] CinemachineCamera vCam;

    void Awake() {
        Instance = this;
        PrimaryTarget = null;
        Targets = new();
    }

    void Update(){
        vCam.Target.TrackingTarget = Target;
    }
}
