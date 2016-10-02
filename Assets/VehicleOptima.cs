using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	//vehicleOptima is a server-only vehicle representation
	public class VehicleOptima 
	{
		//assigned form the position of the vehcile in the activeList, to be used as his identificator for general purposes
		//it is not exactly thread safe out of the simulation step!!!
		public int serialId = -1;
		//assigned lane go from 1 to numLAne, 0 is non valid, -1,-2 ... is overtaking on other direction
		public int assignedLane = 0;
		//idno of the RoadTrunk, if == 0 is inside
		public int idnoStreet = -1;
		//idno of the Node,if not in a roadtrusnk 
		public int idnoNOde = -1;
		public DirectionEnum dir = DirectionEnum.FromTail;
		//km/h
		public float speed = -1;
		//position along the trunk (0.. trunkLength)
		public float position = 0;
		//coming form zone ..
		public int idnoFromZone = -1;
		//going to zone ..
		public int idnoToZone = -1;
		// list of possible paths from origin to destination,
		//BUT! porbably it sohld go in roadtrunk class ...
		List < List<int> > sequenceOfPAths;

		public VehicleOptima ()
		{
		}
	}
}

