using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class VehicleManager
	{
		//list of active vehicle
		private List<VehicleOptima> listVehicleOptima = null;

		List<VehicleOptima> listVehicleOptimaDisabled = null;

		int maxNumVehicle = 10000;

		public VehicleManager ()
		{
			listVehicleOptima = new List<VehicleOptima> (10);	
			listVehicleOptimaDisabled = new List<VehicleOptima> (maxNumVehicle);	
			foreach (VehicleOptima v in listVehicleOptimaDisabled) {
				//v.InitVehicle ();
			}

		}

		//vehicleManager updates vehicle only on given TDeStrt,
		//the other vehicles are frozen (because they are invisible)
		// when a vehicle goes out of a listed TDEStrt it goes in disactive lsit and can be reused elsewhere
		// : to know quickly if it is out of bounds we use the calcualted frustrum (or in future the calucalte in some other way bounderies)
		void Update (List<TDEStrt> listTDEStrt, Bounds bounds)
		{
			if (bounds == null) {
				;
			}
		}

		void ActivateVehicle ()
		{

		}

		void DisactivateVehicle ()
		{

		}

		//PLace vehicles at initialization, no vehicle present
		//it distribute omogenously cars along the roadTrunk direction
		public void PlaceVehiclesInTDEStrt (TDEStrt str, DirectionEnum direction, int numVehicles)
		{
			int numLan = str.GetNumLan (direction);
			//empy trunk, currently not used
			if (numLan == 0)
				return;
			float length = str.roadLength;
			int numVehicPerLane = numVehicles / numLan;
			float distanceBwVehiclePerLane = length/(float) numVehicPerLane ;
			float offset_distance = distanceBwVehiclePerLane * 0.5f; 
			//let's place veicles
			for (int idxLane = 1; idxLane <= numLan; idxLane++) {
				for (int idxVehiPerLane = 0; idxVehiPerLane < numVehicPerLane; idxVehiPerLane++) {
					VehicleOptima v = null;
					int c_min1 = listVehicleOptimaDisabled.Count - 1;
					//try to use already created vehicle
					if (c_min1 >= 0) {
						v = listVehicleOptimaDisabled [c_min1];
						listVehicleOptimaDisabled.RemoveAt (c_min1);
					} else {
						v = new VehicleOptima ();
						//TOADD in the future ZONES variable!not paths thy should be somewehere availalbe maybe
					}
					listVehicleOptima.Add (v);
					v.serialId = listVehicleOptima.Count - 1;
					v.position = idxVehiPerLane * distanceBwVehiclePerLane + offset_distance ;
					v.assignedLane = idxLane;
					v.idnoStreet = str.idno;
					v.dir = direction;
					if (v.dir == DirectionEnum.FromTail)
						v.idnoNOde = str.tail;
					else
						v.idnoNOde = str.head;
				}
			}
		}

		public int listVehicleOptimaCount(){
			return this.listVehicleOptima.Count;
		}

		//get vehicle by idx
		public VehicleOptima GetVehicleOptima(int idx){
			return this.listVehicleOptima [idx];
		}

		public void DropAllVehicles(){
			listVehicleOptima.Clear ();
			listVehicleOptimaDisabled.Clear ();
		}
	
	}

}

