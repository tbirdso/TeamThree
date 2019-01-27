using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour {

	#region Constants
	int TILE_WIDTH = 25;
	int TILE_LENGTH = 25;
	int TILE_HEIGHT = 0;

	#endregion

	#region Definitions
	public delegate void EventHandler(object sender, EventArgs e);

	public event EventHandler StartGenerateMaze;
	public event EventHandler EndGenerateMaze;
	#endregion

	#region Globals
	public List<GameObject> TilePrefabs;
	public Vector2 StartPoint;
	public Vector2 EndPoint;

	public Vector3 WorldStartPoint = new Vector3(0,0,0);

	private TileNode TilePrefabTree;

	private int _height = 1;
	private int height {
		set {
			if (value >= 1)
				_height = value;
		}
		get {
			return _height;
		}
	}

	private int _width = 1;
	private int width {
		set {
			if (value >= 1)
				_width = value;
		}
		get {
			return _width;
		}
	}

	//Pointer to array with _height tiles in y direction and _width tiles in x direction
	private Tile[,] MazeTilesArray;
	private GameObject[,] MazeTileObjectsArray;

	private List<Vector2> Path;

	#endregion




	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	#region Public Methods

	public void MakeMaze(int I = 3, int J = 3) {

		MazeTilesArray = new Tile[height,width];
		Dictionary<EdgeDirection,Tile> curDirs = new Dictionary<EdgeDirection, Tile>();

		for (int j = 1; j < J; j++) {
			for (int i = 1; i < I; i++) {

				curDirs.Add (EdgeDirection.north, GetTile(j-1, i));
				curDirs.Add (EdgeDirection.east, GetTile(j, i - 1));
				curDirs.Add (EdgeDirection.south, GetTile(j + 1, i));
				curDirs.Add (EdgeDirection.west, GetTile(j,i+1));
					
				MazeTilesArray [i, j].AdjacentTiles = curDirs;
				MazeTilesArray [i, j].MakeEdgeRules ();

			}
		}

		PlaceMazeTiles ();
	}


	#endregion

	#region Private Methods

	private void PlaceMazeTiles() {
		Quaternion rot = Quaternion.identity;
		Vector3 topLeftStartPos = WorldStartPoint;
		Vector2 gridPos = StartPoint;

		AdjustToTopLeft (ref gridPos, ref topLeftStartPos);
		Vector3 curPos = topLeftStartPos;

		for (int j = 0; j < height; j ++) {
			for (int i = 0; i < width; i++) {

				Tile curTile = GetTile (i, j);
				curPos.x = topLeftStartPos.x + i * TILE_WIDTH;
				curPos.y = topLeftStartPos.y + j * TILE_LENGTH;

				curTile.TileInstance = Instantiate (curTile.TilePrefab, curPos, rot);

				Debug.Log ("Instantiated prefab at " + curPos.x + " " + curPos.y + " " + curPos.z);
			}
		}


	}

	private void AdjustToTopLeft(ref Vector2 g, ref Vector3 w) {

		if (g.x < 0) g.x = 0;

		if (g.y < 0) g.y = 0;

		if (g.x > 0) {
			w.x -= TILE_WIDTH * g.x;
			g.x = 0;
		} 

		if (g.y > 0) {
			w.y -= TILE_LENGTH * g.y;
			g.y = 0;
		}

		Debug.Log ("Adjusted first instantiation position to " + w.x + " " + w.y + " " + w.z);

		return;
	}

	private void GeneratePath() {
		//FIXME
		//Check start and end points are within maze constraints
		//Add pathing algorithm
		Path = new List<Vector2>();

		Vector2 curLoc = StartPoint;
		Path.Add (curLoc);
		while ((curLoc.x != EndPoint.x && curLoc.y != EndPoint.y) &&
		      (curLoc.x < width && curLoc.y < height)) {

			if (UnityEngine.Random.Range (0, 1) == 0) {
				//Move x
				if (curLoc.x < EndPoint.x)
					curLoc.x++;
				else if (curLoc.x > EndPoint.x)
					curLoc.x--;

			} else {
				//Move y
				if (curLoc.y < EndPoint.y)
					curLoc.y++;
				else if (curLoc.y > EndPoint.y)
					curLoc.y--;
			}

			Path.Add (curLoc);
		}
	
		//Check path validity
	}

	private void FillPathTiles() {
		
		foreach (Vector2 tileLoc in Path) {
			Tile cur = GetTile (tileLoc);
			cur.TilePrefab = FindPrefab (cur.EdgeRules);
		}

	}

	private void FillNonPathTiles() {
		
		for(int j = 0; j < height; j++) {
			for (int i = 0; i < width; i++) {
				
				Tile cur = GetTile (i, j);


				if (cur.TilePrefab != null)
					FindPrefab (cur.EdgeRules);

			}
		}
	}

	private Tile GetTile(Vector2 pos) {
	return GetTile ((int)pos.x, (int)pos.y);
	}

	private Tile GetTile(int i, int j) {
		if(i < 0 || i >= width)
			return null;
		if(j < 0 || j > height)
			return null;

		return MazeTilesArray [i, j];
	}
		
	private GameObject FindPrefab(Dictionary<EdgeDirection,EdgeRule> edges) {
		
		TileNode curNode = TilePrefabTree;

		foreach (EdgeDirection dir in Enum.GetValues(typeof(EdgeDirection))) {
			if (edges [dir] == EdgeRule.pass) {
				curNode = curNode.left;
			} else if (edges [dir] == EdgeRule.wall) {
				curNode = curNode.right;
			} else {
				int lr = UnityEngine.Random.Range (0, 1);
				curNode = (lr == 0) ? curNode.left : curNode.right;
			}

		}

		GameObject retVal = curNode.prefabs.Dequeue ();
		curNode.prefabs.Enqueue (retVal);

		return retVal;
	}

	private void BuildGameObjectTree() {

		InitTree (TilePrefabTree);

		foreach (GameObject prefab in TilePrefabs) {
			TileData data = prefab.GetComponent<TileData> ();

			TileNode curPoint = TilePrefabTree;
			curPoint = data.North ? curPoint.left : curPoint.right;
			curPoint = data.East ? curPoint.left : curPoint.right;
			curPoint = data.South ? curPoint.left : curPoint.right;
			curPoint = data.West ? curPoint.left : curPoint.right;

			curPoint.prefabs.Enqueue (prefab);
		}
	}

	private void InitTree(TileNode NESWtree) {
		NESWtree = new TileNode ();

		int leafCount = 0;

		makeLevel (NESWtree, 0, 4, ref leafCount);

		Debug.Log ("Initialized tree with " + leafCount + " leaves");
	}

	private void makeLevel(TileNode thisNode, int level, int targetLevels, ref int leafCount) {
		if (level != targetLevels) {
			thisNode.left = new TileNode ();
			makeLevel (thisNode.left, level + 1, targetLevels, ref leafCount);

			thisNode.right = new TileNode ();
			makeLevel (thisNode.right, level + 1, targetLevels, ref leafCount);
		} else {
			thisNode.prefabs = new Queue<GameObject> ();	
			leafCount++;
		}
	}

	#endregion

	#region Event Handlers

	public void OnStartGenerateMaze(object sender, EventArgs e) {

		if (StartGenerateMaze != null)
			StartGenerateMaze (sender, e);
	}

	public void OnEndGenerateMaze (object sender, EventArgs e) {
		if (EndGenerateMaze != null)
			EndGenerateMaze (sender, e);
	}

	#endregion
}

public class TileNode {
	public Queue<GameObject> prefabs;
	public TileNode left;
	public TileNode right;
}
