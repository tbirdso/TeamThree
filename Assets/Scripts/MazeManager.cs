using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour {

	#region Constants
	int TILE_WIDTH = 25;
	int TILE_LENGTH = 25;
	int TILE_HEIGHT = 0;

	bool DEBUG = true;

	/* CONVENTIONS
	*
	*	+Z = NORTH
	*	+X = EAST
	*	-Z = SOUTH
	*	-X = WEST
	*/

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

	private TileNode TilePrefabTree = new TileNode();


	/*private int _height = 1;
	public int height {
		set {
			if (value >= 1)
				_height = value;
		}
		get {
			return _height;
		}
	}

	private int _width = 1;
	public int width {
		set {
			if (value >= 1)
				_width = value;
		}
		get {
			return _width;
		}
	}*/
	public int width;
	public int height;


	//Pointer to array with _height tiles in y direction and _width tiles in x direction
	private Tile[,] MazeTilesArray;
	private GameObject[,] MazeTileObjectsArray;

	private List<Vector2> Path;

	#endregion




	// Use this for initialization
	void Start () {
		MakeMaze ();
	}

	#region Public Methods

	public void MakeMaze() {

		int J = height;
		int I = width;

		BuildGameObjectTree ();

		MazeTilesArray = new Tile[J,I];
		for (int j = 0; j < J; j++) {
			for (int i = 0; i < I; i++) {
				MazeTilesArray [j, i] = new Tile () {
					gridPosition = new Vector2() {
						x = i,
						y = j,
					},
					mgr = this,
				};

			}
		}


		Dictionary<EdgeDirection,Tile> curDirs = new Dictionary<EdgeDirection, Tile>();

		for (int j = 0; j < J; j++) {
			for (int i = 0; i < I; i++) {
				curDirs.Clear ();

				//Debug.Log ("j = " + j + ", i = " + i);

				curDirs.Add (EdgeDirection.north, GetTile(j + 1, i));
				curDirs.Add (EdgeDirection.east, GetTile(j,i+1));
				curDirs.Add (EdgeDirection.south, GetTile(j - 1, i));
				curDirs.Add (EdgeDirection.west, GetTile(j, i - 1));

				MazeTilesArray [j, i].AdjacentTiles = curDirs;

				//Debug.Log ("Value of adj tiles is " + MazeTilesArray [0, 0].AdjacentTiles [EdgeDirection.north]);

			}
		}

		Debug.Log ("Generating path");
		GeneratePath ();

		FillPathTiles ();

		FillNonPathTiles ();

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

				Tile curTile = GetTile (j, i);
				curPos.x = topLeftStartPos.x + i * TILE_WIDTH;
				curPos.z = topLeftStartPos.z + j * TILE_LENGTH;

				curTile.TileInstance = Instantiate (curTile.TilePrefab, curPos, rot);

//				Debug.Log ("Instantiated prefab at " + curPos.x + " " + curPos.y + " " + curPos.z);
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
			w.z -= TILE_LENGTH * g.y;
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
		while (curLoc.x != EndPoint.x || curLoc.y != EndPoint.y) {

			if (UnityEngine.Random.Range (0, 2) == 0) {
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

			if(!Path[Path.Count - 1].Equals(curLoc))
				Path.Add (curLoc);
		}

		Debug.Log ("Path: ");
		foreach (Vector2 val in Path) {
			Debug.Log (val);
		}
	
		//Check path validity
	}

	private void FillPathTiles() {

		if (Path.Count > 1) {

			int index = 0;
			Vector2 curPos = Path [index];


			foreach(Vector2 nextPos in Path) {
				if(!nextPos.Equals(new Vector2() { x = 0, y = 0})) {
					Tile cur = GetTile (curPos);
					if (cur.EdgeRules == null)
						cur.EdgeRules = new Dictionary<EdgeDirection, EdgeRule> ();

					if (curPos != null && nextPos != null && curPos.x != null && curPos.y != null && nextPos.x != null && nextPos.y != null) {
						if (curPos.y > nextPos.y || (cur.gridPosition.x == 0 && cur.gridPosition.y == 0)) {
							cur.EdgeRules [EdgeDirection.south] = EdgeRule.pass;


						} else if (curPos.y < nextPos.y) {
							cur.EdgeRules [EdgeDirection.north] = EdgeRule.pass;
						}

						if (curPos.x > nextPos.x) {
							cur.EdgeRules [EdgeDirection.west] = EdgeRule.pass;
						} else if (curPos.x < nextPos.x) {
							cur.EdgeRules [EdgeDirection.east] = EdgeRule.pass;
						}
					}
				}

				curPos = nextPos;
			}
		}

		foreach (Vector2 tileLoc in Path) {
			Debug.Log ("tileLoc is x = " + tileLoc.x + " y = " + tileLoc.y);

			if (tileLoc.x < width && tileLoc.y < height) {
				Tile cur = GetTile (tileLoc);

				cur.MakeEdgeRules ();
				cur.TilePrefab = FindPrefab (cur.EdgeRules);
				Debug.Log ("Found tile " + cur.TilePrefab.ToString () + "for [i,j] = [" + tileLoc.x + "," + tileLoc.y + "]");
			}
		}

	}

	private void FillNonPathTiles() {
		
		for(int j = 0; j < height; j++) {
			for (int i = 0; i < width; i++) {
				
				Tile cur = GetTile (j,i);
				if(cur.EdgeRules == null)
					cur.MakeEdgeRules ();

				if (cur.TilePrefab == null) {
					cur.TilePrefab = FindPrefab (cur.EdgeRules);
					Debug.Log ("Found tile " + cur.TilePrefab.ToString () + " for [i,j] = [" + i + "," + j + "]");
				} else {
					Debug.Log ("Prefab already exists");
				}

			}
		}
	}

	public Tile GetTile(Vector2 pos) {
		return GetTile ((int)pos.y, (int)pos.x);
	}

	public Tile GetTile(int j, int i) {

		if (i < 0 || i >= width)
			return null;
			/*
			return new Tile() {
				gridPosition = new Vector2() {
					x = -1,
					y = -1,
				}
			};*/
		if (j < 0 || j >= height)
			return null;
			/*return new Tile() {
				gridPosition = new Vector2() {
					x = -1,
					y = -1,
				}
			};*/
		
		return MazeTilesArray [j, i];
	}
		
	private GameObject FindPrefab(Dictionary<EdgeDirection,EdgeRule> edges) {

		GameObject retVal;

		TileNode curNode = TilePrefabTree;

		if (true) {
			string dirPath = "";

			foreach (EdgeDirection dir in Enum.GetValues(typeof(EdgeDirection))) {
				if(!edges.ContainsKey(dir)) {
					int lr = UnityEngine.Random.Range (0, 2);
					if (lr == 0) {
						curNode = curNode.left;
						dirPath = String.Concat (dirPath, "l");
					} else {
						curNode = curNode.right;
						dirPath = String.Concat (dirPath, "r");
					}
				} else if (edges [dir] == EdgeRule.pass) {
					curNode = curNode.left;
					dirPath = String.Concat (dirPath, "l");
				} else if (edges [dir] == EdgeRule.wall) {
					curNode = curNode.right;
					dirPath = String.Concat (dirPath, "r");
				} else {
					int lr = UnityEngine.Random.Range (0, 2);
					if (lr == 0) {
						curNode = curNode.left;
						dirPath = String.Concat (dirPath, "l");
					} else {
						curNode = curNode.right;
						dirPath = String.Concat (dirPath, "r");
					}

				}

			}
			if (curNode.prefabs.Count == 0)
				Debug.Log ("Found no prefabs for: " + dirPath);
			else
				Debug.Log ("Found " + curNode.prefabs.Count + " prefabs for " + dirPath);

			retVal = curNode.prefabs.Dequeue ();
			curNode.prefabs.Enqueue (retVal);
		} else {
			if (TilePrefabs.Count > 0) {
				retVal = TilePrefabs [0];
			} else {
				Debug.Log ("No prefabs available");
				retVal = null;
			}
		}

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

		int leafCount = 0;

		makeLevel (NESWtree, 0, 4, ref leafCount);
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
