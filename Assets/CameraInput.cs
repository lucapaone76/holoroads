using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraInput : MonoBehaviour {
	static Vector3 defaultTransform = new Vector3(0,0,-100f);
	static float minHeigth = 5f;
	static float maxHeigth = 20000f;
	static float zoomStep = 20f;
	public static float switchToMicro = 210f;
	public List<GameObject> gobjectsInFrustrum  { get; set;}

	public Plane[] planes { get; set;} 

	CameraInput(){
		gobjectsInFrustrum = new List<GameObject> (5000);	
	}

	// Use this for initialization
	void Start () {
		Camera camera_main = GetComponent<Camera>();
		CenterCamera (camera_main);
		planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());

		float[] distances = new float[32];
		distances[8] = CameraInput.switchToMicro;//car leyer
		distances[9] = 1000f;//lane1 layer
		distances[10] = 5000f;//lane2 layer
		GetComponent<Camera>().layerCullDistances = distances;
	}
	public void CenterCamera( Camera cam ){
		Camera mainCamera = cam;
		GameObject[] roadTrunkList = GameObject.FindGameObjectsWithTag ("RoadTrunk"); 
		Bounds bound = new Bounds ();
		bound.center = Vector3.zero;
		foreach (GameObject rt in roadTrunkList) {
			MeshRenderer mr = rt.GetComponent<MeshRenderer> ();
			if (bound.center != Vector3.zero)
				bound.Encapsulate (mr.bounds);
			else
				bound = mr.bounds;
		}
		Vector3 pos = bound.center;
		pos.z = defaultTransform.z;
		mainCamera.transform.position = pos;
		UpdateGameObjectsInFrustrum ();
		Debug.Log("currently displayed from camera Gameobj. num:" + gobjectsInFrustrum.Count.ToString() );
	}
	float scaleShift(){
		float ret = -transform.position.z / 15 ;
		if (ret < 5)
			return 5;
		else
			return ret;
	}

	void Update()
	{
		if (Input.anyKey) {
			//transform.position = transform.position + new Vector3(1, 0, 0);
			//Debug.Log ("A key:"+Input.inputString);
			//float translation = Input.GetAxis("Vertical") * speed;
			float translateX = Input.GetAxis ("Horizontal") * scaleShift ();
			float translateY = Input.GetAxis ("Vertical") * scaleShift ();
			//Debug.Log ("A translation of :" + Input.GetAxis ("Horizontal"));
			//translation *= Time.deltaTime;
			if (Input.GetKey ("z") == true) {
				if (transform.position.z < -minHeigth)
					transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + zoomStep);
				else
					transform.position = new Vector3 (transform.position.x, transform.position.y, -minHeigth);
			}
			if (Input.GetKey ("x") == true) {
				if (transform.position.z > -maxHeigth)
					transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z - zoomStep);
				else
					transform.position = new Vector3 (transform.position.x, transform.position.y, -maxHeigth);
			}
			if (Input.GetKey ("m") == true) {
				if (transform.position.z < -minHeigth)
					transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + zoomStep);
				else
					transform.position = new Vector3 (transform.position.x, transform.position.y, -minHeigth);
			}
			if (Input.GetKey ("k") == true) {
				if (transform.position.z > -maxHeigth)
					transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z - zoomStep);
				else
					transform.position = new Vector3 (transform.position.x, transform.position.y, -maxHeigth);
			}
			//transform.Translate(translateX, translateY, 0);
			Vector3 pos = transform.position;
			pos.x = pos.x + translateX;
			pos.y = pos.y + translateY;
			transform.position = new Vector3 (pos.x, pos.y, pos.z);
			UpdateGameObjectsInFrustrum ();
		}
	}



	//NOT THREADSAFE!!!
	public void UpdateGameObjectsInFrustrum (){
		gobjectsInFrustrum.Clear ();
		Vector3 pos = this.transform.position;

		planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());

		GameObject[] go =GameObject.FindGameObjectsWithTag ("RoadTrunk");
		foreach (GameObject g  in go) {
			if (GeometryUtility.TestPlanesAABB (planes, g.GetComponent<Renderer> ().bounds)) {
				gobjectsInFrustrum.Add (g);
				//Debug.Log ("visible:"+g.name);
			} 
			else {
				//Debug.Log ("NOT visible:"+g.name);
			}

		}
	}


}
