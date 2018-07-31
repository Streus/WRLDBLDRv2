using System.Collections.Generic;
using UnityEngine;

namespace WrldBldr
{
	/// <summary>
	/// Represents a physical section of a generated Region.
	/// Maintains references to all adjacent sections.
	/// </summary>
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
		public static Section Create(Vector2 position, bool flipped, Archetype type = Archetype.normal)
		{
			GameObject sec = new GameObject (typeof (Section).Name);
			Section s = sec.AddComponent<Section> ();
			sec.AddComponent<CircleCollider2D> ().radius = 0.45f;

			sec.transform.position = position;
			sec.transform.rotation = flipped ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
			sec.transform.localScale = Generator.GetInstance ().GetSectionScale ();

			s.SetArchtype (type);
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
		public static Section Create(Region set, Vector2 position, bool flipped, Archetype type = Archetype.normal)
		{
			Section r = Create (position, flipped, type);
			set.AddSection (r);
			return r;
		}

		/// <summary>
		/// Get the color associated with an Archetype
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Color GetArchetypeColor(Archetype type)
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
		public static AdjDirection GetRandomDirection()
		{
			int d = Random.Range (0, System.Enum.GetNames (typeof (AdjDirection)).Length - 1);

			return (AdjDirection)d;
		}

		public static AdjDirection CalcOffsetDirection(AdjDirection dir, int amount)
		{
			int max = System.Enum.GetNames (typeof (AdjDirection)).Length;
			while (amount < 0)
				amount += max;
			return (AdjDirection)(((int)dir + amount) % max);
		}

		public static Vector2 CalcDirection(AdjDirection dir, bool flipped)
		{
			int offset = (int)dir;
			float segments = System.Enum.GetNames (typeof (AdjDirection)).Length;
			float angle = (offset / segments) * 360f;
			if (flipped)
				angle = (angle + 180f) % 360f;
			Vector2 direction = Quaternion.Euler (0f, 0f, angle) * Vector2.right;
			direction.Scale (Generator.GetInstance ().GetSectionScale ());
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
			Vector3[] triPoints = new Vector3[] {
				new Vector3 (0f, -0.5f),
				new Vector3 (-0.433f, 0.25f),
				new Vector3 (0.433f, 0.25f)};

			for (int i = 0; i < triPoints.Length; i++)
			{
				if (flipped)
					triPoints[i] *= -1;
				triPoints[i] += transform.position;
			}
#if UNITY_EDITOR
			//wire triangle
			if (selected)
				Gizmos.color = Color.yellow;
			else
				Gizmos.color = GetArchetypeColor (GetArchetype ());
			for (int i = 0; i < triPoints.Length; i++)
			{
				int ipo = (i + 1) % triPoints.Length;
				Gizmos.DrawLine (triPoints[i], triPoints[ipo]);
			}

			//solid triangle
			if (set != null)
				UnityEditor.Handles.color = set.GetDebugColor ();
			else
				UnityEditor.Handles.color = new Color (1f, 0f, 1f, 0.5f);

			UnityEditor.Handles.DrawAAConvexPolygon(triPoints);
#endif

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

		public bool IsFlipped()
		{
			return flipped;
		}

		public void AssignSet(Region set)
		{
			this.set = set;
		}

		/// <summary>
		/// Checks if this Section is part of the given set
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public bool CheckSet(Region set)
		{
			return this.set == set;
		}

		public Archetype GetArchetype()
		{
			return archtype;
		}

		public void SetArchtype(Archetype type)
		{
			archtype = type;
		}

		public Section GetAdjRoom(AdjDirection index)
		{
			return adjSections[(int)index];
		}

		/// <summary>
		/// Make a connection between this section and another. Also makes diagonal connections where valid.
		/// </summary>
		/// <param name="index">The direction relative to this section the connection is being made</param>
		/// <param name="room">The section being connected to</param>
		/// <param name="reverseConnections">Make connections from the other section to this section</param>
		public void SetAdjRoom(AdjDirection index, Section room, bool reverseConnections = true)
		{
//			if (room.flipped == flipped)
//				throw new System.InvalidOperationException ("Cannot connect two sections with the same flipped state!\n" + this.name + ", " + room.name);

			adjSections[(int)index] = room;

			if (reverseConnections)
				SetAdjRoom (index, this, false);
		}

		/// <summary>
		/// Add an adjacent room in the specified direction
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Section AddAdjRoom(AdjDirection dir, Archetype type = Archetype.normal)
		{
			Vector2 direction = CalcDirection (dir, flipped);
			Section adj = Create ((Vector2)transform.position + direction, !flipped, type);
			SetAdjRoom (dir, adj);
			return adj;
		}

		public AdjDirection[] GetFreeRooms()
		{
			List<AdjDirection> rooms = new List<AdjDirection> ();
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] == null)
					rooms.Add ((AdjDirection)i);
			}

			return rooms.ToArray ();
		}

		public bool IsAdjSpaceFree(AdjDirection dir)
		{
			Vector2 d = CalcDirection (dir, flipped);
			Collider2D col = Physics2D.OverlapPoint (d + (Vector2)transform.position, Physics2D.AllLayers);
			return col == null;
		}

		public bool HasFreeAdjSpace()
		{
			for (int i = 0; i < adjSections.Length; i += 2)
			{
				if (IsAdjSpaceFree ((AdjDirection)i))
					return true;
			}
			return false;
		}

		public int GetAdjMask()
		{
			int mask = 0;
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] != null)
					mask |= 1 << (i);
			}

			return mask;
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
