using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tile {

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
		EdgeRules = new Dictionary<EdgeDirection, EdgeRule> ();

		int pass = 0;

		foreach (EdgeDirection dir in System.Enum.GetValues(typeof(EdgeDirection))) {
			pass++;
			
			if (!AdjacentTiles.ContainsKey (dir)) {
				
				Debug.Log ("Tried to make edge rules but a tile did not have all edges accounted for.");

			} else {
				
				if (AdjacentTiles [dir] == null) {
					//edge of maze
					//Debug.Log("Pass " + pass + " was out of bounds");
					EdgeRules[dir] = EdgeRule.wall;

				} else {
					if (AdjacentTiles [dir] != null && AdjacentTiles [dir].EdgeRules != null && AdjacentTiles [OppositeDirections [dir]] != null) {
						EdgeRules [dir] = AdjacentTiles [dir].EdgeRules [OppositeDirections [dir]];
					} else {
						EdgeRules [dir] = EdgeRule.unknown;
					}

					//Debug.Log ("Pass " + pass + " has rule " + EdgeRules [dir]);
				}
			}
		}

	}
}
	
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
