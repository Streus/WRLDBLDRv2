using UnityEngine;

[CreateAssetMenu(menuName = "WrldBldr/World Tile Set")]
public class TileSet : ScriptableObject
{
	#region INSTANCE_VARS

	[SerializeField]
	private Tile[] tiles = new Tile[4];
	#endregion

	#region INSTANCE_METHODS

	private TileSet()
	{
		setToDefaults ();
	}

	public void setToDefaults()
	{

		tiles = new Tile[4];
		tiles[0 ] = new Tile ("Space",	0x7, 1);
		tiles[1 ] = new Tile ("Corner",	0x3, 3);
		tiles[2 ] = new Tile ("Wall",	0x1, 3);
		tiles[3 ] = new Tile ("Block",	0x0, 1);
	}

	public void Reset()
	{
		setToDefaults ();
	}

	public GameObject getTile(int adjMask, out float rotation)
	{
		rotation = 0f;

		for (int i = tiles.Length - 1; i >= 0; i--)
		{
			int cv = tiles[i].getCheckVector ();

			//check all orientations of the tile
			for (int rot = 0; rot < tiles[i].getRotations(); rot++)
			{
				if (cv == (cv & adjMask))
				{
					rotation = 120f * rot;
					return tiles[i].prefab;
				}

				//rotate the check vector
				cv = Tile.rotateCVLeft (cv, 1, 3);
			}
		}

		throw new System.ArgumentException ("Given mask (" + System.Convert.ToString (adjMask, 2).PadLeft (16, '0') + ") does not " +
			"map to a valid tile!");
	}
	#endregion

	#region INTERNAL_TYPES

	[System.Serializable]
	public struct Tile
	{
		/// <summary>
		/// Bitwise rotates an int left by amount within a given width
		/// </summary>
		/// <param name="original">The int to bitwise rotate</param>
		/// <param name="amount">The amount to rotate</param>
		/// <param name="bitwidth">The maximum width of the returned int</param>
		/// <returns>original rotated left by amount within bitwidth</returns>
		public static int rotateCVLeft(int original, int amount, int bitwidth)
		{
			int shifted = original << amount;
			int overflow = original >> (bitwidth - amount);
			int widthMask = (1 << bitwidth) - 1;
			return (shifted | overflow) & widthMask;
		}

		// For user-identification in the inspector
		public string name;

		// Used to identify this tile
		// Represents the adjacency conditions that must be met to place this tile
		[SerializeField]
		private int checkVector;

		// This tile represents this many distict tiles that are rotations of one another
		[SerializeField]
		private int rotations;

		// The object placed in the scene
		public GameObject prefab;

		public Tile(string name, int cv, int rotations)
		{
			this.name = name;
			checkVector = cv;
			this.rotations = rotations;
			prefab = null;
		}

		public int getCheckVector()
		{
			return checkVector;
		}

		public int getRotations()
		{
			return rotations;
		}
	}
	#endregion
}