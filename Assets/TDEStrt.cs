/// ALL RIGHTS RESERVED /// Luca Paone - lucapaone@gmail.com 
using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	//TDEstrt represents a single trunk with 2 directions
	// thusm some attributes are shared:
	// some are explicetely declared when they are different: numLanFromTail
	// e.g.pointXYList is ordered showing point form tail to head (following TDe conventions)
	// thus to draw the other direction you should reverse the array
	[Serializable]
	public class TDEStrt {
        /// pointXYList is the list of shape point expressed in x,y coordinates (es a segment with 3 vertices is 1,0 1,1 2,1) .... !it is from tail,, from head must be reversed
        public List <SerializableVector3> pointXYList;
        /// is related to the pointXYList and it express the normalized vectors from one point to the next one. 
        /// !!!it has (pointXYList lenth MINUS 1)*2 since lat hlaf of point is from the reverse side (from head=true)!!!
        public List <SerializableVector3> pointXYdirection;
		
        /// distance from origin of the list of pointXY -> useful to place cars. The first point is then equal to 0 and list has the same length of pointXYList, it is from tail,, from head must be reversed
		public List <float> pointXYdistance;

		/// it is related to the pointXYdirection and it express the normal to the right using the pointXYdirection as the forward direction
		/// !!!it has (pointXYList lenth MINUS 1)*2 since lat hlaf of point is from the reverse side (from head=true)!!! 
		public List <SerializableVector3> pointXYtanRight;

		/// see pointXYtanRight, currently not used
		public List <SerializableVector3> pointXYtanLeft;

		/// pointXYList is the list of shape point expressed in x,y coordinates (es a segment with 3 vertices is 1,0 1,1 2,1)
		/// warning -> lat=y Lon=x in pointXYList
		public List <SerializableVector3> pointLatLonList;
		public int idno = -1;
		public int tail = -1;
		public int head = -1;
		public double maxSpedFromTail = -1;
		public double maxSpedFromHead = -1;
		double currentSpedFromTail = -1;
		double currentSpedFromHead = -1;
		public double hier = -1;
		public int numLanFromTail = 0;
		public int numLanFromHead = 0;
		public string name;
		public float roadLength = -1;
		public double capacity = -1;

		private List< VehicleWithGameObject >[] vehicleList = null;

		public TDEStrt(){
			pointXYList = new List <SerializableVector3> ();
			pointLatLonList = new List <SerializableVector3> ();
			pointXYdirection = new List <SerializableVector3> ();
			pointXYdistance = new List <float> ();
			pointXYtanLeft = new List <SerializableVector3> ();
			pointXYtanRight = new List <SerializableVector3> ();
			vehicleList = new List<VehicleWithGameObject>[numLanFromTail+numLanFromHead];

			idno = -1;
			tail = -1;
			head = -1;
			currentSpedFromTail = -1;
			currentSpedFromHead = -1;
			hier = -1;
			numLanFromTail = 0;
			numLanFromHead = 0;
			roadLength = -1;
			capacity = -1;
		}

		public static UnityEngine.Vector3[] toV3Array(List<SerializableVector3> listSerV3){
			UnityEngine.Vector3[] list = new UnityEngine.Vector3[listSerV3.Count];
			int i = 0;
			foreach (SerializableVector3 s in listSerV3) {
				list [i] = listSerV3 [i];
				i++;
			}
			return list;
		}

		public int GetNumLan(DirectionEnum dir){
			if(dir==DirectionEnum.FromTail){
				return numLanFromTail ;
			}
			return numLanFromHead;
		}

		public double GetMaxSpeed(DirectionEnum dir){
			if(dir==DirectionEnum.FromTail){
				return maxSpedFromTail ;
			}
			return maxSpedFromHead;
		}

		public double GetCurrentSpeed(DirectionEnum dir){
			if(dir==DirectionEnum.FromTail){
				return currentSpedFromTail;
			}
			return currentSpedFromHead;
		}

        public SerializableVector3[] pointXYdirectionFromTail(bool isFromTail)
        {
            int start = -1;
            int end = -1;

            if (isFromTail) {
                start = 0;
                end = pointXYdirection.Count / 2;
            }
            else
            {
                start = pointXYdirection.Count / 2;
                end = pointXYdirection.Count ;
            }
            SerializableVector3[] ser = new SerializableVector3[pointXYdirection.Count];
            int j = 0;
            for (int i = start; i < end; i++, j++)
            {
                ser[j] = pointXYdirection[i];
            }
            return ser;
            
        }

        public float[] pointXYdistanceFromTail(bool isFromTail)
        {
            if (isFromTail) return pointXYdistance.ToArray();
            else
            {
                float[] ser = new float[pointXYdistance.Count];
                int j = 0;
                for (int i = pointXYdistance.Count - 1; i >= 0; i--, j++)
                {
                    ser[j] = pointXYdistance[i];
                }
                return ser;
            }
        }

        public SerializableVector3[] pointXYListFromTail(bool isFromTail)
        {
            if (isFromTail) return pointXYList.ToArray();
            else
            {
                SerializableVector3[] ser = new SerializableVector3[pointXYList.Count];
                int j = 0;
                for (int i = pointXYList.Count - 1; i >= 0; i--, j++)
                {
                    ser[j] = pointXYList[i];
                }
                return ser;
            }
        }
    }
    public class TDENode {
		public int idnoNode = -1;
		public int[] idnoLinks = null;

	}
		
}

