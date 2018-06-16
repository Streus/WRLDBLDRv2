using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a physical section of a generated Region.
/// Maintains references to all adjacent sections.
/// </summary>
namespace WrldBldr
{
	public class Section : MonoBehaviour
	{
		#region STATIC_VARS

		#endregion

		#region INSTANCE_VARS

		public bool selected = false;

		/// <summary>
		/// When true, this section is treated as if flipped over the x-axis
		/// </summary>
		private bool flipped = false;
		private Archetype archtype;
		private Section[] adjSections;
		private Region set;
		#endregion

		#region STATIC_METHODS

		/// <summary>
		/// Create a new Section instance
		/// </summary>
		/// <param name="position">Where (in wc) to create the Section</param>
		/// <param name="flipped">Whether this Section is flipped</param>
		/// <param name="type">The type of Section to create</param>
		/// <returns></returns>
		public static Section create(Vector2 position, bool flipped, Archetype type = Archetype.normal)
		{
			GameObject sec = new GameObject (typeof (Section).Name);
			Section s = sec.AddComponent<Section> ();
			sec.AddComponent<CircleCollider2D> ().radius = 0.45f;

			sec.transform.position = position;
			sec.transform.rotation = flipped ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
			sec.transform.localScale = Generator.getInstance ().getSectionScale ();

			s.setArchtype (type);
			s.flipped = flipped;
			return s;
		}

		/// <summary>
		/// Create a new Section instance as part of a given Region
		/// </summary>
		/// <param name="set">The Region this Section is a part of</param>
		/// <param name="position">Where (in wc) to create the Section</param>
		/// <param name="flipped">Whether this Section is flipped</param>
		/// <param name="type">The type of Section to create</param>
		/// <returns></returns>
		public static Section create(Region set, Vector2 position, bool flipped, Archetype type = Archetype.normal)
		{
			Section r = create (position, flipped, type);
			r.set = set;
			r.transform.SetParent (set.transform, true);
			return r;
		}

		/// <summary>
		/// Get the color associated with an Archetype
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Color getArchetypeColor(Archetype type)
		{
			switch (type)
			{
			case Archetype.normal:
				return new Color(0f, 0f, 0f, 0.5f);

			case Archetype.start:
				return new Color (0f, 0.7f, 0f, 0.5f);

			case Archetype.end:
				return new Color (0.7f, 0f, 0f, 0.5f);

			default:
				return new Color (1f, 0f, 1f, 0.5f);
			}
		}

		/// <summary>
		/// Gets a random direction
		/// </summary>
		/// <returns>AdjDirection representing a cardinal direction</returns>
		public static AdjDirection getRandomDirection()
		{
			int d = Random.Range (0, System.Enum.GetNames (typeof (AdjDirection)).Length - 1);

			return (AdjDirection)d;
		}

		public static AdjDirection offsetDirection(AdjDirection dir, int amount)
		{
			int max = System.Enum.GetNames (typeof (AdjDirection)).Length;
			while (amount < 0)
				amount += max;
			return (AdjDirection)(((int)dir + amount) % max);
		}

		public static Vector2 getDirection(AdjDirection dir, bool flipped)
		{
			int offset = (int)dir;
			float segments = System.Enum.GetNames (typeof (AdjDirection)).Length;
			float angle = (offset / segments) * 360f;
			if (flipped)
				angle = (angle + 180f) % 360f;
			Vector2 direction = Quaternion.Euler (0f, 0f, angle) * Vector2.right;
			direction.Scale (Generator.getInstance ().getSectionScale ());
			return direction;
		}
		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			adjSections = new Section[System.Enum.GetNames (typeof (AdjDirection)).Length];
		}

#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			//solid sphere
			if (selected)
				Gizmos.color = Color.yellow;
			else
				Gizmos.color = getArchetypeColor (getArchetype ());
			Gizmos.DrawSphere (transform.position, GetComponent<CircleCollider2D> ().radius);

			//wire sphere
			if (set != null)
				Gizmos.color = set.getDebugColor ();
			else
				Gizmos.color = new Color (1f, 0f, 1f, 0.5f);
			Gizmos.DrawWireSphere (transform.position, GetComponent<CircleCollider2D>().radius);

			//connections
			Gizmos.color = Color.white;
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] == null)
					continue;

				Gizmos.DrawLine (transform.position, adjSections[i].transform.position);
			}
		}
#endif

		public bool isFlipped()
		{
			return flipped;
		}

		public void assignSet(Region set)
		{
			this.set = set;
			transform.SetParent (set.transform, true);
		}

		/// <summary>
		/// Checks if this Section is part of the given set
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public bool checkSet(Region set)
		{
			return this.set == set;
		}

		public Archetype getArchetype()
		{
			return archtype;
		}

		public void setArchtype(Archetype type)
		{
			archtype = type;
		}

		public Section getAdjRoom(AdjDirection index)
		{
			return adjSections[(int)index];
		}

		/// <summary>
		/// Make a connection between this section and another. Also makes diagonal connections where valid.
		/// </summary>
		/// <param name="index">The direction relative to this section the connection is being made</param>
		/// <param name="room">The section being connected to</param>
		/// <param name="reverseConnections">Make connections from the other section to this section</param>
		public void setAdjRoom(AdjDirection index, Section room, bool reverseConnections = true)
		{
//			if (room.flipped == flipped)
//				throw new System.InvalidOperationException ("Cannot connect two sections with the same flipped state!\n" + this.name + ", " + room.name);

			adjSections[(int)index] = room;

			if (reverseConnections)
				setAdjRoom (index, this, false);
		}

		/// <summary>
		/// Add an adjacent room in the specified direction
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Section addAdjRoom(AdjDirection dir, Archetype type = Archetype.normal)
		{
			Vector2 direction = getDirection (dir, flipped);
			Section adj = create ((Vector2)transform.position + direction, !flipped, type);
			setAdjRoom (dir, adj);
			return adj;
		}

		public AdjDirection[] getFreeRooms()
		{
			List<AdjDirection> rooms = new List<AdjDirection> ();
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] == null)
					rooms.Add ((AdjDirection)i);
			}

			return rooms.ToArray ();
		}

		public bool isAdjSpaceFree(AdjDirection dir)
		{
			Vector2 d = getDirection (dir, flipped);
			Collider2D col = Physics2D.OverlapPoint (d + (Vector2)transform.position, Physics2D.AllLayers);
			return col == null;
		}

		public bool hasFreeAdjSpace()
		{
			for (int i = 0; i < adjSections.Length; i += 2)
			{
				if (isAdjSpaceFree ((AdjDirection)i))
					return true;
			}
			return false;
		}

		public int getAdjMask()
		{
			int mask = 0;
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] != null)
					mask |= 1 << (i);
			}

			return mask;
		}

		public void chooseTile(TileSet set)
		{
			float rot;
			GameObject tile = set.getTile (getAdjMask (), out rot);
			Instantiate (tile, transform, false).transform.rotation = Quaternion.Euler(0f, 0f, rot);
		}
		#endregion

		#region INTERNAL_TYPES

		public enum Archetype
		{
			normal, start, end
		}

		public enum AdjDirection
		{
			right, left, down
		}
		#endregion
	}
}
