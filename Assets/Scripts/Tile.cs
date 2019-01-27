using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EdgeRule {
	pass,
	wall,
	unknown
}

public enum EdgeDirection {
	north,
	east,
	south,
	west
}

public class Tile : MonoBehaviour {

	public Dictionary<EdgeDirection,Tile> AdjacentTiles;
	public Dictionary<EdgeDirection,EdgeRule> EdgeRules;

	public GameObject TilePrefab;
	public GameObject TileInstance;

	private Dictionary<EdgeDirection,EdgeDirection> OppositeDirections = new Dictionary<EdgeDirection,EdgeDirection> () {
		{EdgeDirection.north,EdgeDirection.south},
		{EdgeDirection.east,EdgeDirection.west},
		{EdgeDirection.south,EdgeDirection.north},
		{EdgeDirection.west,EdgeDirection.east},
	};


	public void MakeEdgeRules() {

		foreach (EdgeDirection dir in System.Enum.GetValues(typeof(EdgeDirection))) {
			
			if (!AdjacentTiles.ContainsKey (dir)) {
				
				Debug.Log ("Tried to make edge rules but a tile did not have all edges accounted for.");

			} else {
				
				if (AdjacentTiles [dir] == null) {
					//edge of maze
					EdgeRules[dir] = EdgeRule.wall;

				} else {

					EdgeRules [dir] = AdjacentTiles [dir].EdgeRules [OppositeDirections [dir]];
				}
			}
		}

	}
}
	
