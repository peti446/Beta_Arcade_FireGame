using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadTile : MapTile
{

	protected override void Setup()
	{
		m_tileType = ETileType.Road;
	}

	protected override GameObject GetTileObject(ProceduralMapManager proceduralManager)
	{
		if(proceduralManager.MapBuildingsScripteableObject.Road == null)
		{
			Debug.LogError("Road kit prefab does not exist");
			return null;
		}

		//Store where roads are present near this tile
		NearbyRoadsPos nearbyRoads = NearbyRoadsPos.None;

		//Loop over each tile to find if we have roads or not
		foreach(KeyValuePair<NearbyRoadsPos, Vector2Int> dir in Directions)
		{
			ETileType type;
			//Check the type of tile in on the side
			if (GetTileAtPos(m_gridX + dir.Value.x, m_gridZ + dir.Value.y, out type))
			{
				if (type == ETileType.Road)
				{
					//Bitwise operation to add the current dir to the enum for later checks
					nearbyRoads |= dir.Key;
				}
			}
		}
		GameObject roadObject = Instantiate(proceduralManager.MapBuildingsScripteableObject.Road);

		#region Cross and Center
		//Check every posibility of flag composition to decide what kind of road goes in the current tile
		//Check if the road is a center pice
		if (Enum.GetValues(typeof(NearbyRoadsPos)).Cast<NearbyRoadsPos>().All((otherFlag) => { return (nearbyRoads & otherFlag) == otherFlag; }))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Center, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if it is a X crossroad
		if(nearbyRoads == (NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.East | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Cross, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}
		#endregion

		#region Closed End
		//Check if we have a closed top double sided pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.East | NearbyRoadsPos.North)) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedEnd, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}

		//Check if we have a closed bottom double sided pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.East | NearbyRoadsPos.South)) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedEnd, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if we have a closed right double sided pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.South | NearbyRoadsPos.North)) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedEnd, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}

		//Check if we have a closed left double sided pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.South | NearbyRoadsPos.North)) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedEnd, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}

		#endregion

		#region Double sided
		//Check if we got doubled sided upwards road
		if ((nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.West)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.South)) == (NearbyRoadsPos.North | NearbyRoadsPos.South))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.DoubleSided, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if we got doubled sided sidewards road
		if ((nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.West)) == (NearbyRoadsPos.East | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.DoubleSided, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}
		#endregion

		#region OpenL
		//Check if we got a onesided L top closed road
		if ((nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.North)) == 0 && (nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.South | NearbyRoadsPos.SouthEast)) == (NearbyRoadsPos.East | NearbyRoadsPos.South | NearbyRoadsPos.SouthEast))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OpenL, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}

		//Check if we got a L bottom closed road
		if ((nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.North | NearbyRoadsPos.NorthEast)) == (NearbyRoadsPos.East | NearbyRoadsPos.North | NearbyRoadsPos.NorthEast))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OpenL, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if we got an onesided reverse L top closed road
		if ((nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.North)) == 0 && (nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.South | NearbyRoadsPos.SouthWest)) == (NearbyRoadsPos.West | NearbyRoadsPos.South | NearbyRoadsPos.SouthWest))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OpenL, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}

		//Check if we got an onesided reverse L bottom closed road
		if ((nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.North | NearbyRoadsPos.NortWest)) == (NearbyRoadsPos.West | NearbyRoadsPos.North | NearbyRoadsPos.NortWest))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OpenL, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}
		#endregion

		#region OneSided
		//Check if we have an upwards onesided left pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.West)) == 0 && (nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.NorthEast | NearbyRoadsPos.SouthEast)) == (NearbyRoadsPos.East | NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.NorthEast | NearbyRoadsPos.SouthEast))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OneSided, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if we have an upwards onesided right pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.East)) == 0 && (nearbyRoads & (NearbyRoadsPos.West | NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.NortWest | NearbyRoadsPos.SouthWest)) == (NearbyRoadsPos.West | NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.NortWest | NearbyRoadsPos.SouthWest))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OneSided, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}

		//Check if we have an sidewards onesided top pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.North)) == 0 && (nearbyRoads & (NearbyRoadsPos.South | NearbyRoadsPos.East | NearbyRoadsPos.West | NearbyRoadsPos.SouthEast | NearbyRoadsPos.SouthWest)) == (NearbyRoadsPos.South | NearbyRoadsPos.East | NearbyRoadsPos.West | NearbyRoadsPos.SouthEast | NearbyRoadsPos.SouthWest))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OneSided, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}

		//Check if we have an sidewards onesided bottom pedestrian road
		if ((nearbyRoads & (NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.West | NearbyRoadsPos.NortWest | NearbyRoadsPos.NorthEast)) == (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.West | NearbyRoadsPos.NortWest | NearbyRoadsPos.NorthEast))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.OneSided, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}
		#endregion

		#region Closed T
		//Check if upwards T junctation
		if ((nearbyRoads & (NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.West)) == (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedT, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check if East T junctation
		if ((nearbyRoads & (NearbyRoadsPos.West)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.South)) == (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.South))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedT, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}

		//Check if downwards T junctation
		if ((nearbyRoads & (NearbyRoadsPos.North)) == 0 && (nearbyRoads & (NearbyRoadsPos.East | NearbyRoadsPos.South | NearbyRoadsPos.West)) == (NearbyRoadsPos.East | NearbyRoadsPos.South | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedT, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}

		//Check if West T junctation
		if ((nearbyRoads & (NearbyRoadsPos.East)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.West)) == (NearbyRoadsPos.North | NearbyRoadsPos.South | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedT, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}
		#endregion

		#region Cloed L
		//Check for upwards right L junctation
		if ((nearbyRoads & (NearbyRoadsPos.NorthEast | NearbyRoadsPos.South | NearbyRoadsPos.West)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.East)) == (NearbyRoadsPos.North | NearbyRoadsPos.East))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedL, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}

		//Check for upwards left L junctation
		if ((nearbyRoads & (NearbyRoadsPos.NortWest | NearbyRoadsPos.East | NearbyRoadsPos.South)) == 0 && (nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.West)) == (NearbyRoadsPos.North | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedL, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}

		//Check for downwards right L junctation
		if ((nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.SouthEast | NearbyRoadsPos.West)) == 0 && (nearbyRoads & (NearbyRoadsPos.South | NearbyRoadsPos.East)) == (NearbyRoadsPos.South | NearbyRoadsPos.East))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedL, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}

		//Check for downwards left L junctation
		if ((nearbyRoads & (NearbyRoadsPos.North | NearbyRoadsPos.East | NearbyRoadsPos.SouthWest)) == 0 && (nearbyRoads & (NearbyRoadsPos.South | NearbyRoadsPos.West)) == (NearbyRoadsPos.South | NearbyRoadsPos.West))
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.ClosedL, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}
		#endregion

		#region Corners
		//Check if we got a top right corner only road
		if ((nearbyRoads & NearbyRoadsPos.NorthEast) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Corner, RoadObject.RoadObjectOrientation.North);
			return roadObject;
		}
		//Check if we got a top left corner only road
		if ((nearbyRoads & NearbyRoadsPos.NortWest) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Corner, RoadObject.RoadObjectOrientation.West);
			return roadObject;
		}
		//Check if we got a top right corner only road
		if ((nearbyRoads & NearbyRoadsPos.SouthEast) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Corner, RoadObject.RoadObjectOrientation.East);
			return roadObject;
		}
		//Check if we got a right left corner only road
		if ((nearbyRoads & NearbyRoadsPos.SouthWest) == 0)
		{
			roadObject.GetComponent<RoadObject>().SetRoadType(RoadObject.RoadObjectType.Corner, RoadObject.RoadObjectOrientation.South);
			return roadObject;
		}
		#endregion

		return roadObject;
	}
}
