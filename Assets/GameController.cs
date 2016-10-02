using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	VehicleManager vehiMan = null;
	VehicleDisplayer vehiDisplayer = null;
	Dictionary<long,GameObject> trunkGODictionary = null;
	// Use this for initialization
	GameController(){
		vehiMan = new VehicleManager();	
		trunkGODictionary = new Dictionary<long, GameObject> ();
	}

	void Start () {

        vehiDisplayer = GetComponentsInChildren<VehicleDisplayer>()[0];

        Create__TrunkTde_GameObjectHashMap();
		vehiDisplayer.setVehiManagerAndTrunkDictio(vehiMan,trunkGODictionary);
	}

	public void Create__TrunkTde_GameObjectHashMap (){
		trunkGODictionary.Clear ();
		if (trunkGODictionary.Count == 0) {
			GameObject[] gos = GameObject.FindGameObjectsWithTag ("RoadTrunk");
			for (int i = 0; i < gos.Length; i++) {
				int tailOrHead = -1;
				if (gos [i].GetComponent<RoadTrunk> ().isFromTail)
					tailOrHead = gos [i].GetComponent<RoadTrunk> ().road_tdeInfo.tail;
				else
					tailOrHead = gos [i].GetComponent<RoadTrunk> ().road_tdeInfo.head;
				long key = (((long)(gos [i].GetComponent<RoadTrunk> ().road_tdeInfo.idno))<<32 ) +(long)tailOrHead ;
				trunkGODictionary.Add(key,gos[i]);
			}
			vehiDisplayer.setVehiManagerAndTrunkDictio(vehiMan,trunkGODictionary);
		}
		return;
	}
	//OLD CODE before 10 02 2016
	/*
	List<GameObject> gameObjectsVisible = new List<GameObject>(5000);
	// Update is called once per frame
	void FixedUpdate ()
	{
		Camera 		cam = Camera.main;
		gameObjectsVisible = cam.GetComponent<CameraInput> ().gobjectsInFrustrum;

		Vector3 pos = cam.transform.position;
		  
			foreach (GameObject g in gameObjectsVisible) {
				//Debug.Log (g.name + " has been detected!");
				if (pos.z < -CameraInput.switchToMicro) {
					g.GetComponent<RoadTrunk> ().UpdateColors ();
				}
				else {
					//Debug.Log (g.name + " spawn!");
					g.GetComponent<RoadTrunk>().SpawnNewVehicles ();
					g.GetComponent<RoadTrunk>().UpdateVehiclesPositionAndDestroy ();
					g.GetComponent<RoadTrunk> ().UpdateColors ();
				}
			}		
			return;


	}
	*/
	List<GameObject> gameRoadTrunkObjectsVisible = new List<GameObject>(5000);

	//TODO THIS FUNCTION WILL HAVE TO BE CHANGED WITH REAL DATA
	public void CreateAllVisibleVehicles ()
	{
		Camera 		cam = Camera.main;
		cam.GetComponent<CameraInput> ().UpdateGameObjectsInFrustrum();
		gameRoadTrunkObjectsVisible = cam.GetComponent<CameraInput> ().gobjectsInFrustrum;

		Vector3 pos = cam.transform.position;

		foreach (GameObject g in gameRoadTrunkObjectsVisible) {
			//Debug.Log (g.name + " has been detected!");
			if (pos.z < -CameraInput.switchToMicro) {
				g.GetComponent<RoadTrunk> ().UpdateColors ();
			}
			else {
				//Debug.Log (g.name + " spawn!");
				TDEStrt tdeInfo = g.GetComponent<RoadTrunk>().road_tdeInfo;
				int idx_start = vehiMan.listVehicleOptimaCount();
				int numVehiFromTail = 10;
				int numVehiFromHead = 20;
				if (tdeInfo.numLanFromTail != 0) {
					vehiMan.PlaceVehiclesInTDEStrt (tdeInfo, DirectionEnum.FromTail, numVehiFromTail);
				}
				if (tdeInfo.numLanFromHead != 0) {
					vehiMan.PlaceVehiclesInTDEStrt (tdeInfo, DirectionEnum.FromHead, numVehiFromHead);
				}
			}
		}
		//NOW PLACE VEHICLES IN GUI
		for (int cnt = 0; cnt < vehiMan.listVehicleOptimaCount(); cnt++) {
			//Debug.Log (cnt  + " spawn!");
			vehiDisplayer.ProjectVehiclesFromSimToGui (cnt );//TODOFINISH		
		}
		return;


	}

	public void DropVehicleManager (){
		vehiMan.DropAllVehicles ();
	}



}
