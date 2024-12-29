using Unity.Cinemachine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
	new CinemachineCamera camera;
	void Update(){
		if(camera == null) {
			camera = CinemachineBrain.GetActiveBrain(0)
				.ActiveVirtualCamera as CinemachineCamera;
		}
		else {
			if(IsOwner){
				camera.Target.TrackingTarget = transform;
			}
		}
	}
}