using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tile {

	public MazeManager mgr;

	public Dictionary<EdgeDirection,Tile> AdjacentTiles;
	public Dictionary<EdgeDirection,EdgeRule> EdgeRules;

	public GameObject TilePrefab;
	public GameObject TileInstance;

	public Vector2 gridPosition;

	private Dictionary<EdgeDirection,EdgeDirection> OppositeDirections = new Dictionary<EdgeDirection,EdgeDirection> () {
		{EdgeDirection.north,EdgeDirection.south},
		{EdgeDirection.east,EdgeDirection.west},
		{EdgeDirection.south,EdgeDirection.north},
		{EdgeDirection.west,EdgeDirection.east},
	};

	public void MakeEdgeRules() {
		if (EdgeRules == null) {
			EdgeRules = new Dictionary<EdgeDirection, EdgeRule> ();
		}

		int pass = 0;

		foreach (EdgeDirection dir in System.Enum.GetValues(typeof(EdgeDirection))) {
			pass++;
			
			if (!AdjacentTiles.ContainsKey (dir)) {
				
				Debug.Log ("Tried to make edge rules but a tile did not have all edges accounted for.");

			} else {

				Vector2 adj = gridPosition;
				switch (dir) {
				case EdgeDirection.north:
					adj.y += 1;
					break;
				case EdgeDirection.east:
					adj.x += 1;
					break;
				case EdgeDirection.south:
					adj.y += -1;
					break;
				case EdgeDirection.west:
					adj.x += -1;
					break;
				default:
					break;
				}

				Tile adjTile = mgr.GetTile (adj);

				if (!EdgeRules.ContainsKey (dir)) {
					if (adjTile == null) {
						//edge of maze
						//Debug.Log("Pass " + pass + " was out of bounds");
						EdgeRules [dir] = EdgeRule.wall;

					} else {
						if (adjTile != null && (adjTile.EdgeRules != null)) {
							if (EdgeRules.ContainsKey (dir) && adjTile.EdgeRules.ContainsKey (OppositeDirections [dir])) {
								EdgeRules [dir] = adjTile.EdgeRules [OppositeDirections [dir]];
							} else if (!EdgeRules.ContainsKey (dir) && adjTile.EdgeRules.ContainsKey (OppositeDirections [dir])) {
								EdgeRules.Add (dir, adjTile.EdgeRules [OppositeDirections [dir]]);
							}
						} else if (!EdgeRules.ContainsKey (dir)) {
							EdgeRules.Add (dir, EdgeRule.unknown);
						}
					}
				}
			}
		}

		string s = "Paths available for ";
		string t = "Paths unknown for ";
		foreach (KeyValuePair<EdgeDirection, EdgeRule> p in EdgeRules) {
			if (p.Value == EdgeRule.pass)
				s = string.Concat (s, p.Key);
			if (p.Value == EdgeRule.unknown)
				t = string.Concat (t, p.Key);
		}

		Debug.Log (s);
		Debug.Log (t);
		Debug.Log ("For tile at x = " + gridPosition.x + " y = " + gridPosition.y);

		//Debug.Log ("Pass " + pass + " has rule " + EdgeRules [dir]);
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
