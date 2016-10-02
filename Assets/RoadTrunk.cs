using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssemblyCSharp;
using System;
//using UnityEditor;

namespace AssemblyCSharp
{

	[Serializable]
	public class VehicleWithGameObject
	{
		public Vector3 prevDirection;
		public Vector3 currentPosition;
		public int idxNextPoint;
		public GameObject vehicleGameObject;
		public int assignedLane = -1;
		public int linkIdno = -1;
		public int tailIdno = -1;
		public int nodeIdno = -1;
		public float position = -1;//<distance in meter froms start
	}
	public enum DirectionEnum { FromTail,FromHead};

	public class RoadTrunk : MonoBehaviour
	{

		int GetCurrentTotalVehicles(DirectionEnum dir){
				if(dir==DirectionEnum.FromTail){
				return currentTotalVehiclesFromTail;
				}
			return currentTotalVehiclesFromHead;
		}

		//shape point in X,Y projections (mercator wgs 84)
		//public Vector3[] road_shapePoints;
		//shape point in lat,lon
		//public Vector3[] road_shapeLatLonPoints;

		public TDEStrt road_tdeInfo;
		public bool isFromTail;//<useful to know what direction we should access usgin the TDEstr structure road_tdeinfo
		public static GameObject carPrefab = null;
		//Vector3[] parr = { new Vector3 (0, 0, 0), new Vector3 (1, 0, 0), new Vector3 (2, 0, 0), new Vector3 (3, 1, 0) };
		//Vector3[] parr = { new Vector3 (0, 0, 0), new Vector3 (2, 0, 0) };
		//Vector3[] parr = { new Vector3 (0, 0, 0), new Vector3 (3, 0, 0) , new Vector3 (7, 3, 0)};
		//Vector3[] parr = { new Vector3 (0, 0, 0), new Vector3 (3, 0, 0) , new Vector3 (4, -7, 0)};
		//public Vector2[] newUV;

		// properties for roads
		// https://en.wikipedia.org/wiki/Lane
		// The widths of vehicle lanes typically vary from 9 to 15 feet (2.7 to 4.6 m). Lane widths are commonly narrower on low volume roads
		// and wider on higher volume roads.
		// The lane width depends on the assumed maximum vehicle width with an additional space to allow for lateral motion of the vehicle.
		public float LaneLength { get { return 3.5f; } }
		Dictionary<int,GameObject> dictionaryVehiSerial_VehiGameObj = new Dictionary<int, GameObject> ();

		// Update is called once per frame
		public Color colorStart = Color.red;
		public Color colorEnd = Color.green;
		public float duration = 1.0F;

		/* VEHICLE VARIABLES */
		public List<VehicleWithGameObject> vehicleList = new List<VehicleWithGameObject> ();
		public List<int> disactiveVehicleList;
		int currentTotalVehiclesFromTail = -1;
		int currentTotalVehiclesFromHead = -1;
		public float vehiclesSpeedFromTail = -1;
		public float vehiclesSpeedFromHead = -1;
		//50f*1000f/3600f ;//speed in m/s
		//int currentLaneForSpawnedVehicles = 0;
		double carLenght = 3.5f;
		//3 meters
		public double meterEveryVehiclesPerLanFromTail = -1;
		public double meterEveryVehiclesPerLanFromHead = -1;

		public double maxOccupancy = -1;

		// spawn new vehicles IF last vehicles is already at the minimum distance and if
		// at the medium distance with the current total number of vehicles
		static int carnum = 0;
		int index_last_created_or_activated_vehicle = 0;
		static List<Material> materialChoices= null;
		static float carHeightFromGround = 0.1f;
		/* END VEHICLE VARIABLES */
		void Awake ()
		{
			if (pointerMainCamera == null) {
				pointerMainCamera = Camera.main;
			}

			if (materialChoices== null) {
				materialChoices = new List<Material> ();

				Material matOrange = (Resources.Load("CartoonVehiclesAtlasOrange", typeof(Material))) as Material;
				materialChoices.Add (matOrange);

				Material matGreen = (Resources.Load("CartoonVehiclesAtlasGreen", typeof(Material))) as Material;
				materialChoices.Add (matGreen);

				Material matGrey = (Resources.Load("CartoonVehiclesAtlasGrey", typeof(Material))) as Material;
				materialChoices.Add (matGrey);

				Material matRed = (Resources.Load("CartoonVehiclesAtlasRed", typeof(Material))) as Material;
				materialChoices.Add (matRed);

				Material matYellow = (Resources.Load("CartoonVehiclesAtlasYellow", typeof(Material))) as Material;
				materialChoices.Add (matYellow);
			}
			if (carPrefab == null) {
				var g = GameObject.Find ("FreeCar");
				carPrefab = g;
				//AssetBundle asset = new AssetBundle ();
				//carPrefab = asset.LoadAsset()
			}
			TDEStrt tdes = getRoadAttributes ();
			if (currentTotalVehiclesFromTail == -1) {
				currentTotalVehiclesFromTail = (int)UnityEngine.Random.Range(3,(float)(tdes.numLanFromTail*tdes.roadLength/carLenght/2));//TODO change(tdes.capacity /2f);//TODO get from realtime data or from historical!!!
			}
			if (currentTotalVehiclesFromHead == -1) {
				currentTotalVehiclesFromHead = (int)UnityEngine.Random.Range(3,(float)(tdes.numLanFromHead*tdes.roadLength/carLenght/2));//TODO change(tdes.capacity /2f);//TODO get from realtime data or from historical!!!
			}

			if (meterEveryVehiclesPerLanFromTail == -1) {
				//calculateDistanceBetweenSpawnedVehicles
				meterEveryVehiclesPerLanFromTail = tdes.roadLength / 1 + currentTotalVehiclesFromTail * tdes.numLanFromTail;
			}
			if (vehiclesSpeedFromTail == -1) {
				vehiclesSpeedFromTail = (float)(tdes.maxSpedFromTail) * 1000f / 3600f;//TODO get from realtime data or from historical!!!
			}
			if (maxOccupancy == -1) {
				maxOccupancy = tdes.roadLength / carLenght * tdes.numLanFromTail; 
			}
		}

		void Start ()
		{
			
		}


		void CalculateTangentsForDirection (Vector3 orTodes, out Vector3 tan1, out Vector3 tan2)
		{
			/**
 * 
 * In a right-handed coordinate system, the rotations are as follows:

90 degrees CW about x-axis: (x, y, z) -> (x, -z, y)
90 degrees CCW about x-axis: (x, y, z) -> (x, z, -y)

90 degrees CW about y-axis: (x, y, z) -> (-z, y, x)
90 degrees CCW about y-axis: (x, y, z) -> (z, y, -x)

!!!! this is our case
90 degrees CW about z-axis: (x, y, z) -> (y, -x, z)
90 degrees CCW about z-axis: (x, y, z) -> (-y, x, z)

 */
			//vettore alla destra di orTodes
			tan1.x = orTodes.y;  
			tan1.y = -orTodes.x;  
			tan1.z = 0;
//		if (i == 0) {
//			first_tan1 = tan1;
//		}
			//vettore alla sinistra
			tan2.x = -orTodes.y;  
			tan2.y = orTodes.x;  
			tan2.z = 0;

			tan1.Normalize ();
			tan2.Normalize ();
				
		}

		//InitPolygon based on provided points of the road shape.
		/// <param name="shapePoints" > ordered sequence of point that draws the hsape of the road link </param> 
		/*
		public void InitPolygonOld_v1 (Vector3[] shapePoints, GameObject roadTrunk)
		{
			if (shapePoints.Length <= 1) {
				throw new System.Exception ("shape point must be at least 2");
			}
			//road_shapePoints = shapePoints;
			//RoadAttributes roadAtt = roadTrunk.GetComponent<RoadAttributes> ();
			int numLan = this.road_tdeInfo.numLanFromTail;
			Mesh mesh = new Mesh ();
			//public Vector2[] newUV = {;
			roadTrunk.GetComponent<MeshFilter> ().mesh = mesh;
			List<Vector3> newVertices = new List<Vector3> ();
			List<int> triangles = new List<int> ();
			//List<Vector3> normals = new List<Vector3> ();
			List<Vector2> UVlist = new List<Vector2> ();

			for (int i = 0; i < shapePoints.Length; i++) {
				int newVerticesLength = newVertices.Count;
				//System.Console.WriteLine(fibarray[i]);
				if (i == shapePoints.Length - 1)
					break;
				Vector3 orTodes = shapePoints [i + 1] - shapePoints [i];//a vector that point from vector origin to destination 

				Vector3 tan_dx;//dx vector
				Vector3 tan_sx;//sx vector

				CalculateTangentsForDirection (orTodes, out tan_dx, out tan_sx);

				road_tdeInfo.pointXYdirection.Add (orTodes.normalized);
				road_tdeInfo.pointXYtanRight.Add (tan_dx);
				road_tdeInfo.pointXYtanRight.Add (tan_sx);

				newVertices.Add (tan_dx * LaneLength * numLan / 2.0f + shapePoints [i] - shapePoints [0]);
				//normals.Add (Vector3.forward);
				newVertices.Add (tan_sx * LaneLength * numLan / 2.0f + shapePoints [i] - shapePoints [0]);
				//normals.Add (Vector3.forward);
				newVertices.Add (tan_sx * LaneLength * numLan / 2.0f + shapePoints [i + 1] - shapePoints [0]);
				//normals.Add (Vector3.forward);
				newVertices.Add (tan_dx * LaneLength * numLan / 2.0f + shapePoints [i + 1] - shapePoints [0]);
				//normals.Add (Vector3.forward);

				//TRIANGLES
				triangles.Add (newVerticesLength);
				triangles.Add (newVerticesLength + 1);
				triangles.Add (newVerticesLength + 2); 

				triangles.Add (newVerticesLength);
				triangles.Add (newVerticesLength + 2);
				triangles.Add (newVerticesLength + 3); 
				//CLOCkWISE
				//UV/
				UVlist.Add (new Vector2 (0, 0));
				UVlist.Add (new Vector2 (0, 1));
				UVlist.Add (new Vector2 (1.0f * orTodes.magnitude, 1));
				UVlist.Add (new Vector2 (1.0f * orTodes.magnitude, 0));
				// BEGIN now in case of curve we have to fill the empy triangle that has been created
				//if is not the first or second shapepoint
				if (i > 0) {
					int oldVerticesLength = newVerticesLength;
					newVerticesLength = newVertices.Count;

					Vector3 prevVertex1 = newVertices [oldVerticesLength - 2];
					Vector3 prevVertex2 = newVertices [oldVerticesLength - 1];
					if (!((prevVertex2 == newVertices [newVerticesLength - 4]) && (prevVertex1 == newVertices [newVerticesLength - 3]))) {
						Vector3 otherVertex1 = new Vector3 (prevVertex2.x, prevVertex2.y, prevVertex2.z);
						Vector3 otherVertex2 = new Vector3 (prevVertex1.x, prevVertex1.y, prevVertex1.z);
						Vector3 shapeVertex = new Vector3 (shapePoints [i].x - shapePoints [0].x, shapePoints [i].y - shapePoints [0].y, 0);
						Vector3 newv1 = new Vector3 (newVertices [oldVerticesLength].x, newVertices [oldVerticesLength].y, 0);
						Vector3 newv2 = new Vector3 (newVertices [oldVerticesLength + 1].x, newVertices [oldVerticesLength + 1].y, 0);

						Vector3 previousOrTodes = shapePoints [i] - shapePoints [i - 1];//a vector that point from vector origin to destination 
						Vector3 diff = orTodes - previousOrTodes;
						//Debug.Log ("diff " + diff);
						if (diff.y > 0F) {
							newVertices.Add (otherVertex1);
							newVertices.Add (newv1);
							newVertices.Add (shapeVertex);
							UVlist.Add (new Vector2 (0, 0f));
							UVlist.Add (new Vector2 ((newv1 - otherVertex1).magnitude, 0));
							UVlist.Add (new Vector2 (0, 0.05f));
							//first triangle
							triangles.Add (newVerticesLength);
							triangles.Add (newVerticesLength + 1);//shape vertex
							triangles.Add (newVerticesLength + 2);
						} else if (diff.y < 0F) {

							newVertices.Add (newv2);
							newVertices.Add (otherVertex2);
							newVertices.Add (shapeVertex);
							//TODO UVLIST to be corrected following the first ase, for now i have no time
							UVlist.Add (new Vector2 (0, 0f));
							UVlist.Add (new Vector2 ((newv1 - otherVertex1).magnitude, 0));
							UVlist.Add (new Vector2 (0, 0.05f));
							//second triangle
							triangles.Add (newVerticesLength);
							triangles.Add (newVerticesLength + 1);//shape vertex
							triangles.Add (newVerticesLength + 2);
						}
					} 

				}
				// END now in case ...
			}
			int[] reverseTriangle = new int[triangles.Count];

			for (int k = 0, r = triangles.Count / 3; k < triangles.Count / 3; k = k + 1 , r = r - 1) {
				reverseTriangle [k * 3] = triangles [r * 3 - 3]; 
				reverseTriangle [k * 3 + 1] = triangles [r * 3 - 2]; 
				reverseTriangle [k * 3 + 2] = triangles [r * 3 - 1]; 
			}
		
			mesh.vertices = newVertices.ToArray ();
			mesh.uv = UVlist.ToArray ();
			mesh.triangles = reverseTriangle;
			//mesh.normals = normals.ToArray ();

			mesh.RecalculateNormals ();
			//GenerateMarkers();
			//
			//BoxCollider collider = (BoxCollider)gameObject.GetComponent<BoxCollider> ();
			//MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
			//Bounds bds = meshFilter.sharedMesh.bounds; 
			//collider.center = bds.center;
			//collider.size = bds.size;
			//collider.isTrigger = true;
			//

		}
		*/
		//draw the road of the link (only one direction) and assign it to the given Gameobject
		public void InitPolygon (Vector3[] shapePoints, GameObject roadTrunk,int numLan)
		{
			if (shapePoints.Length <= 1) {
				throw new System.Exception ("shape point must be at least 2");
			}
            if (numLan < 1)
            {
                throw new System.Exception("InitPolygon must be called with numLan>0");
            }

            Mesh mesh = new Mesh ();
			//public Vector2[] newUV = {;
			roadTrunk.GetComponent<MeshFilter> ().mesh = mesh;
			List<Vector3> newVertices = new List<Vector3> ();
			List<int> triangles = new List<int> ();
			//List<Vector3> normals = new List<Vector3> ();
			List<Vector2> UVlist = new List<Vector2> ();

			for (int i = 0; i < shapePoints.Length -1; i++) {
				int newVerticesLength = newVertices.Count;
				//System.Console.WriteLine(fibarray[i]);

				Vector3 orTodes = shapePoints [i + 1] - shapePoints [i];//a vector that point from vector origin to destination 

				Vector3 tan_dx;//dx vector
				Vector3 tan_sx;//sx vector

				CalculateTangentsForDirection (orTodes, out tan_dx, out tan_sx);

				Vector3 shapeVertex = shapePoints [i] - shapePoints [0];

				road_tdeInfo.pointXYdirection.Add (orTodes.normalized);
				road_tdeInfo.pointXYtanRight.Add (tan_dx);
				road_tdeInfo.pointXYtanLeft.Add (tan_sx);//I will use this MAYBE for cars
				//WARNING DRAW AT THE RIGHT OF THE SHAPEPOINT, since we have 2 DIRECTIONS!!!
				newVertices.Add (tan_dx * LaneLength * numLan  + shapeVertex);
				//normals.Add (Vector3.forward);
				newVertices.Add (shapeVertex);
				//normals.Add (Vector3.forward);
				newVertices.Add (shapePoints [i + 1] - shapePoints [0]);
				//normals.Add (Vector3.forward);
				newVertices.Add (tan_dx * LaneLength * numLan + shapePoints [i + 1] - shapePoints [0]);
				//normals.Add (Vector3.forward);

				//TRIANGLES
				triangles.Add (newVerticesLength);
				triangles.Add (newVerticesLength + 1);
				triangles.Add (newVerticesLength + 2); 

				triangles.Add (newVerticesLength);
				triangles.Add (newVerticesLength + 2);
				triangles.Add (newVerticesLength + 3); 
				//CLOCkWISE
				//UV/
				UVlist.Add (new Vector2 (0, 0));
				UVlist.Add (new Vector2 (0, 1));
				UVlist.Add (new Vector2 (1.0f * orTodes.magnitude, 1));
				UVlist.Add (new Vector2 (1.0f * orTodes.magnitude, 0));
				// BEGIN now in case of curve we have to fill the empy triangle that has been created
				//if is not the first or second shapepoint
				if (i > 0) {
					newVerticesLength = newVertices.Count;

					//TODO forse inutkeif (diff.x >= 0) 
					{
						Vector3 oldTanDx = road_tdeInfo.pointXYtanRight [i - 1];
						Vector3 newTanDx = road_tdeInfo.pointXYtanRight [i];
						
						Vector3 prevVertex = oldTanDx * LaneLength * numLan + shapeVertex;
						Vector3 newVertex = newTanDx * LaneLength * numLan + shapeVertex;

						//PERCHE LO fACEVO?BOH TO Be ERASED if (!((prevVertex2 == newVertices [newVerticesLength - 4]) && (prevVertex1 == newVertices [newVerticesLength - 3]))) 
						{

							//Debug.Log ("diff " + diff);
							//if (diff.y > 0F) 
							{
								newVertices.Add (shapeVertex);
								newVertices.Add (prevVertex);
								newVertices.Add (newVertex);
								UVlist.Add (new Vector2 (0, 0f));
								UVlist.Add (new Vector2 (0, 1f));
								UVlist.Add (new Vector2 ((newVertex - prevVertex).magnitude, 1f ));//nota riseptto alle mie texture x va verso destra, y va verso il basso
								//first triangle
								triangles.Add (newVerticesLength);//shape vertex
								triangles.Add (newVerticesLength + 2);
								triangles.Add (newVerticesLength + 1);

							} 
						}
					}

				}
				// END now in case ...
			}
			int[] reverseTriangle = new int[triangles.Count];

			for (int k = 0, r = triangles.Count / 3; k < triangles.Count / 3; k = k + 1 , r = r - 1) {
				reverseTriangle [k * 3] = triangles [r * 3 - 3]; 
				reverseTriangle [k * 3 + 1] = triangles [r * 3 - 2]; 
				reverseTriangle [k * 3 + 2] = triangles [r * 3 - 1]; 
			}

			mesh.vertices = newVertices.ToArray ();
			mesh.uv = UVlist.ToArray ();
			mesh.triangles = reverseTriangle;
			//mesh.normals = normals.ToArray ();

			mesh.RecalculateNormals ();
			//GenerateMarkers();
			/*
			BoxCollider collider = (BoxCollider)gameObject.GetComponent<BoxCollider> ();
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
			Bounds bds = meshFilter.sharedMesh.bounds; 
			collider.center = bds.center;
			collider.size = bds.size;
			collider.isTrigger = true;
			*/

			//if single diretion retranslate it 
			/*if(road_tdeInfo.singleDirection){
				roadTrunk.transform.Translate (road_tdeInfo.pointXYtanLeft [0] * LaneLength * road_tdeInfo.numLan,Space.World);
			}*/
		}

		//too slow
		/*
	void GenerateMarkers ()
	{
		Renderer r = GetComponent<Renderer> ();
		Texture2D texture = (Texture2D)r.material.mainTexture;

		for (int y = 0; y < texture.height; y++) {
			for (int x = 0; x < texture.width; x++) {
				if ((y < texture.height / 2 + 10) && (y > texture.height / 2 - 10)) {
					Color color = Color.white;
					texture.SetPixel (x, y, color);
				}
			}
		}
		texture.Apply ();
	}
	*/

		/*
		void Update ()
		{
	
		}
		*/

		void setVibrateColors (Color _colorStart, Color _colorEnd)
		{
			this.colorStart = _colorStart;
			this.colorEnd = _colorEnd;
		}

		void updateVibrateColors (Color colorStart, Color colorEnd)
		{
			Renderer rend = GetComponent<Renderer> ();
			float lerp = Mathf.PingPong (Time.time, duration) / duration;
			rend.material.color = Color.Lerp (colorStart, colorEnd, lerp);	
		}

		public void UpdateColors ()
		{
			Color c = GetComponent<Renderer> ().material.color;
			if ((maxOccupancy / 4) < currentTotalVehiclesFromTail) {
				//updateVibrateColors (Color.white, Color.red);
				if (c != Color.red)
					c = Color.red;
			} else {
				//updateVibrateColors (Color.white, Color.green);
			}
		}
/*
		void  OnMouseOver ()
		{
			if (Input.GetMouseButtonDown (0)) {
				// !!! for our roads it is not enough (2 ore more objects are too close, we have to use raycast)
				Debug.Log ("you clicked" + this.name);
			}
		}
*/
		TDEStrt getRoadAttributes ()
		{
			return this.road_tdeInfo;
		}

		static Camera pointerMainCamera = null;
		/*
		void FixedUpdate ()
		{
			//updateColors ();
			Vector3 pos = pointerMainCamera.transform.position;
			if (pointerMainCamera.transform.position.z < -110f)
				return;
			Vector3 dist = (this.transform.position - pos);
			if (dist.sqrMagnitude > 1000 * 1000)
				return;//not
			if (GetComponent<Renderer> ().isVisible) {
				//if (GetComponent<Renderer> ().material.color != Color.cyan) {
					//GetComponent<Renderer> ().material.color = Color.cyan;
				//}
					SpawnNewVehicles ();
					UpdateVehiclesPositionAndDestroy ();
			} else {
				//GetComponent<Renderer> ().material.color = Color.white;			
			}
		}
		*/

		/*
		public void Old_SpawnNewVehicles ()
		{
			TDEStrt tdes = getRoadAttributes ();
			if (tdes.numLanFromTail == 0)
				return;//road not active
			//if (tdes.roadLength < 45)
			//	return;//road not active since is too short

			if ((vehicleList.Count >= currentTotalVehiclesFromTail) && (disactiveVehicleList.Count == 0)) {
				//no need to spawn new vehicles
				// if there are less active vehicel then needed, reactivate one if at the right distance
				return;//no need to create new object or reactivate disactive
			} else {
				//if not the right distance return
				if (vehicleList.Count > 0) {
					Vector3 distance = vehicleList [index_last_created_or_activated_vehicle].vehicleGameObject.transform.position - tdes.pointXYList [0];
					if (meterEveryVehiclesPerLanFromTail > distance.magnitude) {
						return;
					}
				} 

				//if(vehicleList has an inactive object)
				if (disactiveVehicleList.Count > 0) {
					//if necessary 
					if (disactiveVehicleList.Count < currentTotalVehiclesFromTail) {
						index_last_created_or_activated_vehicle = disactiveVehicleList [0]; 
						vehicleList [disactiveVehicleList [0]].vehicleGameObject.SetActive (true);
						disactiveVehicleList.RemoveAt (0);
					}
				} else {
					//create vehicle
					VehicleWithGameObject v = new VehicleWithGameObject ();
					v.assignedLane = UnityEngine.Random.Range (1, tdes.numLanFromTail+1);
					GameObject vehicle = (GameObject.Instantiate (carPrefab));
					vehicle.transform.parent = this.gameObject.transform;
					vehicle.name = "vehi" + (carnum++).ToString () + "-" + tdes.idno.ToString () + "-" + tdes.tail.ToString ();
					vehicle.GetComponent<MeshRenderer> ().material = materialChoices[UnityEngine.Random.Range(0, (materialChoices.Count)) ];
					//CartoonVehiclesAtlasOrange

					Vector3 direction = tdes.pointXYdirection[0];
					vehicle.transform.rotation = carPrefab.transform.rotation;
					v.currentPosition = tdes.pointXYList [0];
					vehicle.transform.position = v.currentPosition + 
						new Vector3(0,0,-carHeightFromGround) +
						v.currentPosition * ((v.assignedLane-0.5f) * LaneLength);
					vehicle.transform.Rotate(new Vector3 (0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg),Space.World);

					//vehi.vehicleGameObject.transform.Rotate(new Vector3 (0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg));
					v.vehicleGameObject = vehicle;
					//v.vehicleGameObject.tag = "vehicle";
					v.prevDirection = Vector3.forward;//impossible direction 
					v.idxNextPoint = 1;
					vehicleList.Add (v);
					index_last_created_or_activated_vehicle = vehicleList.Count - 1; 
				}
				//Vector3 direction = tdes.pointXYList [1] - tdes.pointXYList [0]; 
				//vehicle.transform.Rotate(Mathf.Atan2(direction.x,direction.y)*360f/2*Mathf.PI,0,0);
			}
		}
		*/
		// update all vehciels position AND if it is arrived at the end of road... the vehicles is destroyes
		//end of road in the future should take in account queues if existent
		/*
		public void Old_UpdateVehiclesPositionAndDestroy ()
		{
			TDEStrt tdes = getRoadAttributes ();
			List<int> indexesToDestroy = new List<int> ();
			float nextStepDistance = (Time.deltaTime * vehiclesSpeedFromTail);
		
			for (int i = 0; i < vehicleList.Count; i++) {
				VehicleWithGameObject vehi = vehicleList [i];			
				//if arrived destroy/recycle object
				Vector3 direction = tdes.pointXYList [vehi.idxNextPoint ] - vehi.currentPosition; 
				Vector3 direction_normalized = direction;
				direction_normalized.Normalize();
				bool arrivedAtNExtPoint = false;
				if (nextStepDistance*nextStepDistance > direction.sqrMagnitude) {
					arrivedAtNExtPoint = true;
				}
				Vector3 shift = direction_normalized * (nextStepDistance);
				vehi.vehicleGameObject.transform.position = vehi.vehicleGameObject.transform.position + shift; 
				vehi.currentPosition += shift; 
				//vehi.vehicleGameObject.transform.rotation = Quaternion.Euler (new Vector3 (-90, 0,0));
				//vehi.vehicleGameObject.transform.rotation = Quaternion.Euler (new Vector3 (0,0, Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg));

				if (arrivedAtNExtPoint == true) {
					if (vehi.idxNextPoint == (tdes.pointXYList.Count - 1)) {
						indexesToDestroy.Add (i);
					} else {
						vehi.idxNextPoint++;
						direction_normalized = tdes.pointXYdirection [vehi.idxNextPoint -1 ];
						vehi.vehicleGameObject.transform.rotation = carPrefab.transform.rotation;
						vehi.vehicleGameObject.transform.Rotate (new Vector3 (0, 0, Mathf.Atan2 (direction_normalized.y, direction_normalized.x) * Mathf.Rad2Deg), Space.World);
					}
				}
			}
			//DestroyVehicles (indexesToDestroy, vehicleList);
			Old_RecycleVehicles (indexesToDestroy, vehicleList);
		}*/

		void Old_DestroyVehicles (List<int> indexesToDestroy, List<VehicleWithGameObject> vehicleLists)
		{
			if (indexesToDestroy.Count == 0)
				return;
			int d = 0;
			for (int i = 0; i < indexesToDestroy.Count; i++) {
				GameObject.Destroy (vehicleList [i].vehicleGameObject);
				vehicleList.RemoveAt ((indexesToDestroy [i]) - d);
				d++;
			}
		}

		/*
		void Old_RecycleVehicles (List<int> indexesToDestroy, List<VehicleWithGameObject> vehicleLists)
		{
			if (indexesToDestroy.Count == 0)
				return;
			for (int i = 0; i < indexesToDestroy.Count; i++) {
				VehicleWithGameObject v = vehicleList [indexesToDestroy [i]];
				v.vehicleGameObject.SetActive (false);
				disactiveVehicleList.Add (indexesToDestroy [i]);
				v.vehicleGameObject.transform.position = getRoadAttributes ().pointXYList [0];
				v.currentPosition = v.vehicleGameObject.transform.position;
				v.vehicleGameObject.transform.position = v.vehicleGameObject.transform.position  + new Vector3(0,0,-carHeightFromGround) + 
					v.vehicleGameObject.transform.position * (v.assignedLane-0.5f) * LaneLength;
				v.prevDirection = Vector3.forward;//impossible direction 
				v.idxNextPoint = 1;
				Vector3 pos = v.vehicleGameObject.transform.position;
				v.vehicleGameObject.transform.rotation = carPrefab.transform.rotation;
				v.vehicleGameObject.transform.position = pos +  new Vector3(0,0,-carHeightFromGround);

				Vector3 direction_normalized = road_tdeInfo.pointXYdirection [0 ];
				v.vehicleGameObject.transform.Rotate (new Vector3 (0, 0, Mathf.Atan2 (direction_normalized.y, direction_normalized.x) * Mathf.Rad2Deg), Space.World);

			}
			indexesToDestroy.Clear ();
		}
		*/

	}

}