using UnityEngine;
public class CameraControls:MonoBehaviour {
	private float minSize = 4;
	private Camera cam;
	private void Start() {
		cam = GetComponent<Camera>();
		cam.transform.LookAt(new Vector3(0,0,0));
	}
	private void Update() {
		float scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");
		if(scrollWheelAxis != 0) Zoom(scrollWheelAxis * 10); //Check Scroll Wheel, Zoom Camera
		if(Input.GetMouseButtonDown(1)) LookAtMouse(); //Check Right Click, Center Camera on Pointer in World Space
	}
	private void Zoom(float val) {
		val = (cam.orthographicSize - val < minSize) ? 0 : val; //If Value would Cause Camera to go Below MinSize, set 0, otherwise don't Alter
		if(cam.orthographicSize > minSize || val < 0) cam.orthographicSize -= val;
	}
	private void LookAtMouse() {
		Ray camToPointRay = cam.ScreenPointToRay(Input.mousePosition); //Create a Ray using Camera Location and Mouse Position
		RaycastHit hit; //If a collision occurs, provides access to GameObject
		if(Physics.Raycast(camToPointRay,out hit)) cam.transform.LookAt(hit.point);//cam.transform.LookAt(hit.point); //If Ray hit a Collider, Look at the Collision
	}
}
