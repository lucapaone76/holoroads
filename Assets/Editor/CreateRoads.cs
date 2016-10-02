using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections;
using AssemblyCSharp;
using AssemblyCSharpEditor;

public class CreateRoadsFromCSV : MonoBehaviour
{
	
	/*
	Debug.Log("distance "+(TransforLatLonToXY(47.754098f,16.875000f).x-TransforLatLonToXY(47.754098f,15.820312f).x).ToString());
	 */

	static List<TDEStrt> listTDEStrt = new List<TDEStrt> ();
	static GameObject roadPrefab = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/RoadObject.prefab", typeof(GameObject)) as GameObject;
	static Dictionary<long,GameObject> dictionaryRoadTdeIdno_RoadGameObject = new Dictionary<long, GameObject>();

	public static GameObject GetRoadGameObjecyByRoadTdeIdnoTail(int idno,int tail){		
		GameObject go = null;
		dictionaryRoadTdeIdno_RoadGameObject.TryGetValue (((long)idno<<32)+tail, out go);
		return go;
	}

	/// <summary>
	/// Creates the roads (all the gameobject after reading the CSV file). During real gameplay we will load incrementally
	/// </summary>
	[MenuItem ("MyTools/CreateRoadsFromCSV")]
	static void CreateRoads ()
	{
		Debug.Log ("map center Lat Lon " + latitudeCenter + " ," + longitudeCenter);
		Debug.Log ("map center Y  X " + mapCenterXY.x + " ," + mapCenterXY.y);
		ReadCSVStrt ();
		CreateRoadsFilteredFromCSV (listTDEStrt,dictionaryRoadTdeIdno_RoadGameObject);
	}

	static IEnumerator CreateRoadsFilteredFromCSV (List<TDEStrt> _listTDEStrt, Dictionary<long,GameObject> dictionaryIdnoGameObject)
	{
		int i = 0;
		foreach (TDEStrt tdeStr in _listTDEStrt) {
			//for every direction
			for (int c=0; c < 2; c++) {
				bool isNewTrunk = false;
				if (c == 0)
					isNewTrunk = true;
				Vector3 starting_point;
				int tailOrHead = -1;
				int numLanFromTailOrHead = -1;

				if (isNewTrunk) {
					starting_point = tdeStr.pointXYList [0];
					tailOrHead = tdeStr.tail;
					numLanFromTailOrHead = tdeStr.numLanFromTail;
				} else {
					starting_point = tdeStr.pointXYList [tdeStr.pointXYList.Count - 1];
					tailOrHead = tdeStr.head;
					numLanFromTailOrHead = tdeStr.numLanFromHead;
				}
                if (numLanFromTailOrHead == 0)
                {
                    continue;
                }
                    GameObject roadTrunkGO = (GameObject)Instantiate (roadPrefab, new Vector3 (starting_point.x, starting_point.y), Quaternion.identity);
				roadTrunkGO.name = "trunk-idno" + tdeStr.idno.ToString () + "-fromNode" + tailOrHead.ToString ();
				dictionaryIdnoGameObject.Add (((long)tdeStr.idno<<32)+tailOrHead, roadTrunkGO);
				RoadTrunk roadTrunkScript = roadTrunkGO.GetComponent<RoadTrunk> ();
				roadTrunkScript.road_tdeInfo = tdeStr;
				roadTrunkScript.isFromTail = isNewTrunk;
				if (numLanFromTailOrHead == 1) {
					roadTrunkGO.layer = 9; //da 9 a 12
				}
				if (numLanFromTailOrHead == 2) {
					//C:\Users\Luca\Dropbox\VisumRoads\Assets\Photoscanned Asphalt Materials\Textures\AsphaltMio
					roadTrunkGO.GetComponent<MeshRenderer> ().sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath ("Assets/Photoscanned Asphalt Materials/Materials/TwoLaneMaterial.mat", typeof(Material));
					roadTrunkGO.layer = 10; //da 9 a 12
				}
				if (numLanFromTailOrHead == 3) {
					roadTrunkGO.GetComponent<MeshRenderer> ().sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath ("Assets/Photoscanned Asphalt Materials/Materials/ThreeLaneMaterial.mat", typeof(Material));
					roadTrunkGO.layer = 11; //da 9 a 12
				}
				if (numLanFromTailOrHead >= 4) {
					roadTrunkGO.GetComponent<MeshRenderer> ().sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath ("Assets/Photoscanned Asphalt Materials/Materials/TwoLaneMaterial.mat", typeof(Material));
					roadTrunkGO.layer = 12; //da 9 a 12
				}
				if (isNewTrunk) {				
					roadTrunkScript.InitPolygon (TDEStrt.toV3Array (tdeStr.pointXYList), roadTrunkGO, tdeStr.numLanFromTail);
				} else {
					Vector3[] pointXYList = new Vector3[tdeStr.pointXYList.Count];
					for (int k = tdeStr.pointXYList.Count-1;k>=0;k-- ){
						pointXYList [tdeStr.pointXYList.Count - 1 - k] = tdeStr.pointXYList [k];
					}
					roadTrunkScript.InitPolygon (pointXYList, roadTrunkGO,tdeStr.numLanFromHead);			
				}
			}
			i++;
			//Debug.Log(i.ToString()+" road:"+ roadTrunk.name);
			//yield return new WaitForSeconds(.1f);
		}
		return null;
	}

	/// <summary>
	/// Drops all roads as well as all loaded structs
	/// </summary>
	[MenuItem ("MyTools/DropAllRoadsAndStructs")]
	static void DropAllRoads ()
	{
		RoadTrunk[] objects = UnityEngine.Object.FindObjectsOfType<RoadTrunk> ();
		foreach (RoadTrunk obj in objects) {
			GameObject.DestroyImmediate (obj.gameObject);		
		}
		listTDEStrt.Clear ();
		dictionaryRoadTdeIdno_RoadGameObject.Clear ();


	}

	[MenuItem ("MyTools/DropAllVehicles")]
	static void DropAllVehicles ()
	{
		GameObject[] objects = UnityEngine.GameObject.FindGameObjectsWithTag ("Car");
		foreach (GameObject obj in objects) {
			GameObject.DestroyImmediate (obj);		
		}


	}
	//strange value to make fit my 3d maps with openstreetmap
	static float magic_value_y = -190f;

	[MenuItem ("MyTools/Trunk/CreateMap")]
	static void CreateMap ()
	{
		//immaagine presa da 
		//http://openstreetmap.gryph.de/bigmap.cgi?xmin=4456&xmax=4479&ymin=2832&ymax=2855&zoom=13&scale=256&baseurl=http%3A%2F%2Ftile.openstreetmap.org%2F%21z%2F%21x%2F%21y.png
		//Map is 24x24 tiles (6144x6144 px) at zoom 13, aspect 1:1
		//Bbox is 15.820312, 47.754098, 16.875000, 48.458352 (l,b,r,t)

		/*
33110267;10351629;Würtzlerstraße;16.409880900001 48.195145499999,16.409454300001 48.194889699999;2;27;0;18815437
33110267;18815437;Würtzlerstraße;16.409454300001 48.194889699999,16.409880900001 48.195145499999;1;27;0;10351629
33110268;10351628;Würtzlerstraße;16.408906600001 48.194561299999,16.409454300001 48.194889699999;1;27;0;18815437
33110268;18815437;Würtzlerstraße;16.409454300001 48.194889699999,16.408906600001 48.194561299999;2;27;0;10351628		 

		Bounds b = new Bounds();
		Vector2 left_bottom =  TransforLatLonToXY(48.1890f,16.3925f);
		Vector2 right_top = TransforLatLonToXY(48.2010f,16.4261f);
		b.center = new Vector3 ((right_top.x - left_bottom.x)/2 + left_bottom.x , (right_top.y - left_bottom.y)/2 + left_bottom.y);
		b.extents = new Vector3 ((right_top.x - left_bottom.x) + left_bottom.x, (right_top.y - left_bottom.y));
				 */
		//http://tile.openstreetmap.org/16/35756/22728.png
		//http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
		// una strada che dovrebbe starci sopra è trunk-idno461019207-fromNode460012987
		//
		Vector2 left_top_lonlat = TileToLonLatPos (35756, 22728, 16);
		Vector2 right_bottom_lonlat = TileToLonLatPos (35756 + 1, 22728 + 1, 16);
		Vector2 left_top = TransforLatLonToXY (left_top_lonlat.y, left_top_lonlat.x);
		Vector2 right_bottom = TransforLatLonToXY (right_bottom_lonlat.y, right_bottom_lonlat.x);
		GameObject terrainMap = GameObject.Find ("TerrainMap");
		TerrainAttr tertattr = terrainMap.GetComponent<TerrainAttr> ();
		tertattr.left_top_lonlat = left_top_lonlat;
		tertattr.left_top = left_top;
		tertattr.right_bottom_lonlat = right_bottom_lonlat;
		tertattr.right_bottom = right_bottom;

		terrainMap.transform.position = new Vector3 (0, (magic_value_y), 1);

		Mesh mesh = new Mesh ();
		MeshFilter mr = terrainMap.GetComponent<MeshFilter> ();
		mr.mesh = mesh;
		Vector3[] vertices = new Vector3[4];
		vertices [0] = left_top;
		vertices [1] = new Vector3 (right_bottom.x, left_top.y, 0);
		vertices [2] = right_bottom;
		vertices [3] = new Vector3 (left_top.x, right_bottom.y, 0);
		int[] triang = new int[6];
		triang [0] = 2;
		triang [1] = 0;
		triang [2] = 1;

		triang [3] = 2;
		triang [4] = 3;
		triang [5] = 0;

		Vector2[] uv = new Vector2[4];
		/*
		uv [0] = new Vector2(0,0);
		uv [1] = new Vector2(1,0);
		uv [2] = new Vector2(1,1);
		uv [3] = new Vector2(0,1);
		*/
		uv [0] = new Vector2 (1, 0);
		uv [1] = new Vector2 (0, 0);
		uv [2] = new Vector2 (0, 1);
		uv [3] = new Vector2 (1, 1);

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triang;
		mesh.RecalculateNormals ();

	}


	//
	//ritorna le coordinate in metri (x,y) rispetto al punto espresso in lat,lon
	static Vector2 TransforLatLonToXY (double pointLat, double pointLon)
	{
		Vector2 vec = new Vector2 ();
		MercatorProjection.lonToX (pointLon);
		vec.x = (float)(MercatorProjection.lonToX (pointLon)) - (float)mapCenterXY.x;
		vec.y = (float)MercatorProjection.latToY (pointLat) - (float)mapCenterXY.y;
		return vec;
	}

	static float latitudeCenter = 48.2082f;
	static float longitudeCenter = 16.3724f;
	static Vector2 mapCenterXY = TransforLatLonToXY (latitudeCenter, longitudeCenter);

	[MenuItem ("MyTools/Camera/CenterCamera")]
	static void CenterCamera ()
	{
		Camera mainCamera = Camera.main;
		CameraInput ci = mainCamera.GetComponent<CameraInput> ();
		ci.CenterCamera (mainCamera);
	}

	//When you calulate the tile number according to the wiki you will actually get a floating point number.
	//The integer part indicates which tile you are (or should be) looking at.
	//The fractional part indicates the position within the tile. As a tile is 256 pixel wide, multiplying the fractional part with 256 will give you the pixel position from the top left.
	static public Vector3 WorldToTilePos (double lon, double lat, int zoom)
	{
		float X;
		float Y;
		X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
		Y = (float)((1.0 - Math.Log (Math.Tan (lat * Math.PI / 180.0) +
		1.0 / Math.Cos (lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
		Vector3 p = new Vector3 (X, Y, 0);
		return p;
	}
	//from tile position to latitude(y) longitude(x)
	static public Vector3 TileToLonLatPos (double tile_x, double tile_y, int zoom)
	{
		double X;
		double Y;
		double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow (2.0, zoom));

		X = ((tile_x / Math.Pow (2.0, zoom) * 360.0) - 180.0);
		Y = (180.0 / Math.PI * Math.Atan (Math.Sinh (n)));

		Vector3 p = new Vector3 ((float)X, (float)Y, 0f);
		return p;
	}


	[MenuItem ("MyTools/File/FindAndCreateRoadsInTiles")]
	static public void FindAndCreateRoadsInTiles ()
	{
		//extract tiles info
		Vector2 left_top_lonlat = TileToLonLatPos (35756, 22728, 16);
		Vector2 right_bottom_lonlat = TileToLonLatPos (35756 + 1, 22728 + 1, 16);

		List<TDEStrt> newFilteredlistTde = new List<TDEStrt> ();
		int i = 0;
		for (; i < listTDEStrt.Count; i++) {
			TDEStrt str = listTDEStrt [i];
			bool found = false;
			foreach (UnityEngine.Vector3 pointLatLon in  str.pointLatLonList) {
				if (
					(pointLatLon.y > left_top_lonlat.x) && (pointLatLon.y < right_bottom_lonlat.x))
				if (
					(pointLatLon.x < left_top_lonlat.y) && (pointLatLon.x > right_bottom_lonlat.y)) {
					found = true;
					break;
				}
			}
			if (found) {
				newFilteredlistTde.Add (str);
			}
		}

		CreateRoadsFilteredFromCSV (newFilteredlistTde,dictionaryRoadTdeIdno_RoadGameObject);
	}

	[MenuItem ("MyTools/Trunk/ColorRoad")]
	static public void ColourRoad ()
	{
		GameObject road = GameObject.Find ("trunk-idno461000273-fromNode18000012");
		Renderer rend = road.GetComponent<Renderer> ();
		rend.material.color = (Color.yellow);

	}

	[MenuItem ("MyTools/Trunk/PrintTDEinfo")]
	static public void PrintTDEinfo ()
	{
		GameObject road = GameObject.Find ("trunk-idno461000273-fromNode18000012");
		RoadTrunk rt = road.GetComponent<RoadTrunk> ();
		Debug.Log (rt.road_tdeInfo);
	}

	[MenuItem ("MyTools/Trunk/PrintVertices")]
	static public void PrintLatLonVerticesPoint ()
	{
		if (Selection.activeGameObject.tag.Contains ("RoadTrunk")) {
			GameObject road = Selection.activeGameObject;
			RoadTrunk rt = road.GetComponent<RoadTrunk> ();
			var tde = rt.road_tdeInfo;
			float dist = (1.0f / (float)(tde.pointXYList.Count - 1));
			float i = 0;
			foreach (Vector3 v in tde.pointXYList) {
				GameObject s = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				s.transform.parent = road.transform; 
				//vehicle.transform.localScale = new Vector3(5f,5f,5f);
				s.name = "vertex " + v.ToString ();
				s.GetComponent<MeshRenderer> ().material.color = Color.Lerp (Color.green, Color.red, i);
				s.transform.position = v;
				i = i + dist;	
			}
		}
	}

	[MenuItem ("MyTools/Trunk/Set Road to Build Alone")]
	static public void BuildAlone ()
	{
		if (Selection.activeGameObject.tag.Contains ("RoadTrunk")) {
			GameObject road = Selection.activeGameObject;
			RoadTrunk rt = road.GetComponent<RoadTrunk> ();
			var tde = rt.road_tdeInfo;
			DropAllRoads ();
			List<TDEStrt> list_temp = new List<TDEStrt> ();
			list_temp.Add (tde);
			CreateRoadsFilteredFromCSV (list_temp,dictionaryRoadTdeIdno_RoadGameObject);
		}
	}

	[MenuItem ("MyTools/CountRoadTrunkGameObject")]
	static public void CountRoads ()
	{
		GameObject[] list = GameObject.FindGameObjectsWithTag ("RoadTrunk");
		Debug.Log ("road trunks are" + list.Length);
	}

	[MenuItem ("MyTools/Load car")]
	static public void LoadCarPrefab ()
	{
		GameObject car = AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/CartoonCarFree/Prefabs/Freecar.prefab");
		car.name = "new_Car";
	}

	[MenuItem ("MyTools/File/TODOCHANGESaveCSVStrtToFileBin")]
	static void SaveCSVStrtToFile ()
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
		stopwatch.Start ();
		SaveTde.SaveListTDEStrt (listTDEStrt);
		stopwatch.Stop ();
		Debug.Log (System.Reflection.MethodBase.GetCurrentMethod ().Name + " Time elapsed: " + stopwatch.Elapsed);
	}

	[MenuItem ("MyTools/File/TODOCHANGELoadCSVStrtFromFileBin")]
	static void LoadCSVStrtFromFileBin ()
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
		// Begin timing.
		stopwatch.Start ();
		//LOAd....
		listTDEStrt = SaveTde.LoadTde ();
		stopwatch.Stop ();
		Debug.Log (System.Reflection.MethodBase.GetCurrentMethod ().Name + " Time elapsed: " + stopwatch.Elapsed);
		Debug.Log ("listTDEStrt, read num lines: " + listTDEStrt.Count);

	}

	[MenuItem ("MyTools/File/TODOCHANGECreateRoadsFromLoadedTDEStrt")]
	static void CreateRoadsFromTDEStrt ()
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
		// Begin timing.
		stopwatch.Start ();
		GameObject gc = GameObject.Find ("GameController");
		CreateRoadsFilteredFromCSV (listTDEStrt,dictionaryRoadTdeIdno_RoadGameObject);
		stopwatch.Stop ();
		Debug.Log (System.Reflection.MethodBase.GetCurrentMethod ().Name + " Time elapsed: " + stopwatch.Elapsed);
	}

	//read street from CSV file saved as asset and create TDE list structure
	[MenuItem ("MyTools/File/ReadCSVStrt")]
	static void ReadCSVStrt ()
	{
		//SELECT idno,tail,name,ST_AsText(shap),nlan, sped,hier,head FROM strt 
		//WHERE shap && ST_MakeEnvelope(16.3724-0.005, 48.2082-0.005, 16.3724+0.005, 48.2082+0.005, 4326) limit 1000
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
		// Begin timing.
		stopwatch.Start ();

		listTDEStrt.Clear ();
		dictionaryRoadTdeIdno_RoadGameObject.Clear ();
		List<float> listIdno = new List<float> ();

		TextAsset bindata = Resources.Load ("tde/strt") as TextAsset;
		//var reader = new StreamReader (bindata.bytes);
		var reader = new StreamReader (new MemoryStream (bindata.bytes), Encoding.Default);
		reader.ReadLine ();//skip firs line for headers
		int i = 0;
		while (!reader.EndOfStream) {
			var line = reader.ReadLine ();
			var values = line.Split (';');
			TDEStrt str = null;
			bool isNewTrunk = false;
			if (i == 0)
				isNewTrunk = true;
			else if (listTDEStrt [listTDEStrt.Count - 1].idno != (int.Parse (values [0])))
				isNewTrunk = true;
			if (isNewTrunk) {
				str = new TDEStrt ();
				str.idno = int.Parse (values [0]);
				str.tail = int.Parse (values [1]);
				str.name = values [2];
				string shape = values [3];
				str.numLanFromTail = int.Parse (values [4]);
				str.maxSpedFromTail = double.Parse (values [5]);
				str.hier = double.Parse (values [6]);
				str.head = int.Parse (values [7]);
				str.roadLength = float.Parse (values [8].Replace (',', '.'));
				str.capacity = float.Parse (values [9]);

				var points = shape.Split (',');
				int count_point = 0;
				foreach (string point in points) {
					string[] coord = point.Split (' ');
					//Vector2 vector =  Vector2 (int.Parse (coord [0]), int.Parse (coord [1]))
					//geo:48.2082,16.3724?z=10 wien center
					Vector2 vector = TransforLatLonToXY (float.Parse (coord [1]), float.Parse (coord [0]));
					str.pointXYList.Add (new Vector3 (vector.x, vector.y, 0));
					Vector2 v2 = new Vector2 (float.Parse (coord [1]), float.Parse (coord [0]));
					str.pointLatLonList.Add (new Vector3 (v2.x, v2.y, 0));
                    //is an good approximation for short distance but the haversine function should be used //TODO:improve
                    if (count_point != 0) {

                        float distance = Vector3.Distance(str.pointXYList[count_point], str.pointXYList[count_point - 1]);
                        str.pointXYdistance.Add (distance + str.pointXYdistance[count_point-1]);
                    }
                    else {					
						str.pointXYdistance.Add (0f);
					}
					count_point++;
				}
                float ratio = str.roadLength / str.pointXYdistance[str.pointXYdistance.Count - 1];
                for (int idist=0;idist<str.pointXYdistance.Count;idist++)
                {
                    str.pointXYdistance[idist] = str.pointXYdistance[idist] * ratio;
                }
				listTDEStrt.Add (str);
			} else {
				str = listTDEStrt [listTDEStrt.Count - 1];
				str.numLanFromHead = int.Parse (values [4]);
				str.maxSpedFromHead = double.Parse (values [5]);
			}
			i++;
		}
		// Stop timing.
		stopwatch.Stop ();
		Debug.Log ("ReadCSVStrt Time elapsed: " + stopwatch.Elapsed);
		Debug.Log ("listTDEStrt, read num lines: " + listTDEStrt.Count);
	}

	[MenuItem ("MyTools/Simulation/CreateVehicleInSimAndDisplayThem")]
	static void CreateVehicleInSimAndDisplayThem()
	{
		if (listTDEStrt.Count == 0) {			
			Debug.LogError(  
				"you must first read streets!"); 
			return;
		}
		GameObject go = GameObject.Find("GameController");
		go.GetComponent<GameController> ().Create__TrunkTde_GameObjectHashMap ();//out of the editor this isusally is done in the GameController.Start function
		go.GetComponent<GameController> ().CreateAllVisibleVehicles ();
	}

	[MenuItem ("MyTools/DebugProc/PlaceVehiclesInTiles")]
	static void PlaceVehicle (){
		CreateRoadsFromCSV.DropAllRoads();
		CreateRoadsFromCSV.ReadCSVStrt ();
		CreateRoadsFromCSV.FindAndCreateRoadsInTiles ();
		CreateRoadsFromCSV.CenterCamera ();
		CreateRoadsFromCSV.CreateVehicleInSimAndDisplayThem ();
	}

    [MenuItem("MyTools/DebugProc/CreateNetworkInTiles")]
    static void CreateNetworkInTiles()
    {
        CreateRoadsFromCSV.DropAllRoads();
        CreateRoadsFromCSV.ReadCSVStrt();
        CreateRoadsFromCSV.FindAndCreateRoadsInTiles();
        CreateRoadsFromCSV.CenterCamera();
    }

}