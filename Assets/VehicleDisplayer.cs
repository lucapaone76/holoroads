using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using AssemblyCSharpEditor;

//main tasl -> map vehicles form vehicle manager TO vehicle gameobejcts trasforming vehicle.distance in the right coordinate
using System.Collections.Generic;


public class VehicleDisplayer : MonoBehaviour
{
	public static GameObject carPrefab = null;
	List<Material> materialChoices = null;

    public VehicleDisplayer()
    { }

    public void Init()
    {
            if ((materialChoices == null)|| (materialChoices.Count==0))
            {
                materialChoices = new List<Material>();

                Material matOrange = (Resources.Load("CartoonVehiclesAtlasOrange", typeof(Material))) as Material;
                materialChoices.Add(matOrange);

                Material matGreen = (Resources.Load("CartoonVehiclesAtlasGreen", typeof(Material))) as Material;
                materialChoices.Add(matGreen);

                Material matGrey = (Resources.Load("CartoonVehiclesAtlasGrey", typeof(Material))) as Material;
                materialChoices.Add(matGrey);

                Material matRed = (Resources.Load("CartoonVehiclesAtlasRed", typeof(Material))) as Material;
                materialChoices.Add(matRed);

                Material matYellow = (Resources.Load("CartoonVehiclesAtlasYellow", typeof(Material))) as Material;
                materialChoices.Add(matYellow);
            }
            if (carPrefab == null)
            {
                //var g;
                carPrefab = GameObject.Find("FreeCar");
                //AssetBundle asset = new AssetBundle ();
                //carPrefab = asset.LoadAsset()
            }

    }
    

    /*
		// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	, {
	
	}
	*/

    public void setVehiManagerAndTrunkDictio(VehicleManager _vehiManager , Dictionary<long,GameObject> _trunkGODictionary ){
		vehiManager = _vehiManager;
		trunkGODictionary = _trunkGODictionary;
	}

	VehicleManager vehiManager = null;
	Dictionary<long,GameObject> trunkGODictionary = null;

	//project vehicles From Sim To Gui witth id = idx_start 
	// it has to translate in 3d world the position in the trunk (or in the node) and to give right bearing and
	// to create the related vehicle gameobject
	public void ProjectVehiclesFromSimToGui (int idx_start){
			
		VehicleOptima v = vehiManager.GetVehicleOptima (idx_start);
		GameObject go = null;
		int tailOrHead = v.idnoNOde;
		//TODO for better performance i would like to avoit to use tihs dictionary
		long key = ((long)v.idnoStreet<<32)+(long)v.idnoNOde;
		trunkGODictionary.TryGetValue(key,out go);
		//TODO TO BE FINISHED .... every trunkroad should have this list of coupled  vehicle
		CreateVehicleGameObject(v,go);

	}

	public void CreateVehicleGameObject(VehicleOptima vopt,GameObject goRoadTrunk){
		
		//create vehicle game ovject
		GameObject vehicle = (GameObject.Instantiate (carPrefab));
		vehicle.name = "vehi" + (vopt.serialId).ToString () ;
		vehicle.GetComponent<MeshRenderer> ().material = materialChoices[UnityEngine.Random.Range(0, (materialChoices.Count)) ];

		TDEStrt tdes = goRoadTrunk.GetComponent<RoadTrunk>().road_tdeInfo;
        bool isFromTail = goRoadTrunk.GetComponent<RoadTrunk>().isFromTail;
        float[] tdes_pointXYdistance = tdes.pointXYdistanceFromTail(isFromTail);
        int found_position_idx=-1;
		for (int idx_distance = 1; idx_distance < tdes_pointXYdistance.Length; idx_distance++) {
            if (vopt.position < tdes_pointXYdistance[idx_distance])
            {
                found_position_idx = idx_distance - 1;//it points to previuos index
                break;
            }
		}
        if (vopt.position > tdes_pointXYdistance[tdes_pointXYdistance.Length - 1])
        {
            vopt.position = tdes_pointXYdistance[tdes_pointXYdistance.Length - 1];
            found_position_idx = tdes_pointXYdistance.Length -2;
        }
        //TODO:REMOVE && vopt.position < tdes_pointXYdistance [dir_idx+1] ) {
        float diffDistance = 0f;
        /// from 0 to 1 % distance between the 2 pointXYdistance of interest
        float percentageDiffDistance = 0f;
        diffDistance = vopt.position - tdes_pointXYdistance[found_position_idx];
        percentageDiffDistance = diffDistance / (tdes_pointXYdistance[found_position_idx + 1] - tdes_pointXYdistance[found_position_idx]);

        SerializableVector3[] tdes_pointXYList = tdes.pointXYListFromTail(isFromTail);
        SerializableVector3[] tdes_pointXYdirection = tdes.pointXYdirectionFromTail(isFromTail);

        Vector3 diffDistanceVector = tdes_pointXYList[found_position_idx].toVector3() - tdes_pointXYList[found_position_idx + 1].toVector3();
        float diffstanceFromXY = diffDistanceVector.magnitude * percentageDiffDistance;

        Vector3 posGameObject = tdes_pointXYList [found_position_idx].toVector3 ()
			+tdes_pointXYdirection[found_position_idx].toVector3() *diffstanceFromXY;
		vehicle.transform.position = posGameObject;
        vehicle.transform.Rotate(new Vector3(0, 0, Mathf.Atan2(diffDistanceVector.y, diffDistanceVector.x) * Mathf.Rad2Deg), Space.World);


        vehicle.transform.parent = goRoadTrunk.transform;
		//TODO:FINISH to be checked and finished with lanes, see below code to be used/extracted!!!

		//Vector3 direction = tdes.pointXYdirection[0];
		//vehicle.transform.rotation = carPrefab.transform.rotation;
		//vehicle.currentPosition = tdes.pointXYList [0];
		//vehicle.transform.position = v.currentPosition + 
		//	new Vector3(0,0,-carHeightFromGround) +
		//	v.currentPosition * ((v.assignedLane-0.5f) * LaneLength);
		//vehicle.transform.Rotate(new Vector3 (0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg),Space.World);

		//vehicle.tag = "vehicle";


	}
}
