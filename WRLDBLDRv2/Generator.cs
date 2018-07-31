using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WrldBldr
{
	/// <summary>
	/// 
	/// </summary>
	public class Generator : MonoBehaviour
	{
		#region STATIC_VARS

		private static Generator instance;
		#endregion

		#region INSTANCE_VARS

		[Header("Generation Options")]
		[SerializeField]
		private Blueprint blueprint = new Blueprint();

		[SerializeField]
		private Vector3 sectionScale = Vector3.one;

		[Header ("Tileset Options")]
		[SerializeField]
		private TileSet[] tileSets;

		#endregion

		#region STATIC_METHODS

		public static Generator GetInstance()
		{
			return instance;
		}
		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Debug.LogWarning ("[WB] An instance of " + typeof (Generator).Name + " already exists!");
#if UNITY_EDITOR
				UnityEditor.EditorGUIUtility.PingObject (instance);
#endif
				Destroy (this);
			}
		}

		public Vector3 GetSectionScale()
		{
			return sectionScale;
		}

		/// <summary>
		/// Create a dungeon using the current starting region
		/// </summary>
		public void Generate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			//compile regions into a queue
			Queue<Region> regions = new Queue<Region> ();
			TraverseRegionTree (blueprint.GetRegionRoot (), regions);

			//create a start section
			Section origin = Section.Create (regions.Peek (), Vector2.zero, false, Section.Archetype.start);
			origin.gameObject.name += " 0";

			//all the sections that can be generated from
			Queue<Section> activeSections = new Queue<Section> ();
			activeSections.Enqueue (origin);

			//main Region loop
			while (regions.Count > 0)
			{
				Region currRegion = regions.Dequeue ();

				//the section currently being processed
				Section currSection;

				//main Section loop
				while (currRegion.GetSectionCount() < currRegion.GetTargetSize())
				{
					//pop off the next section
					//if no active sections remain, return to the last room processed
					try
					{
						currSection = activeSections.Dequeue ();
					}
					catch (System.InvalidOperationException)
					{
						currSection = currRegion.GetSection (currRegion.GetSectionCount () - 1);
					}

					//place a random number of rooms in the available free spaces
					Section.AdjDirection[] prospects = currSection.GetFreeRooms ();
					if (prospects.Length > 0)
					{
						int subSections = Random.Range (1, prospects.Length);
						Shuffle (prospects, 1);
						for (int i = 0; i < subSections; i++)
						{
							Section s = TryMakeSection (currRegion, currSection, prospects[i]);
							if (s != null)
								activeSections.Enqueue (s);
						}
					}
				}
			}
		}

		private void TraverseRegionTree(Region r, Queue<Region> q)
		{
			if (r == null)
				return;

			r.Clear ();
			q.Enqueue (r);
			for (int i = 0; i < r.GetSubRegionCount (); i++)
				TraverseRegionTree (r.GetSubRegion (i), q);
		}

		private void Shuffle(Section.AdjDirection[] deck, int times)
		{
			for (int i = 0; i < times; i++)
			{
				for (int j = 0; j < deck.Length; j++)
				{
					int swapIndex = (int)(Random.value * deck.Length);
					Section.AdjDirection temp = deck[swapIndex];
					deck[swapIndex] = deck[j];
					deck[j] = temp;
				}
			}
		}

		private Section TryMakeSection(Region set, Section parent, Section.AdjDirection dir, Section.Archetype type = Section.Archetype.normal)
		{
			//check for overlap
			Collider2D col = Physics2D.OverlapPoint (Section.CalcDirection (dir, parent.IsFlipped ()) + (Vector2)parent.transform.position, Physics2D.AllLayers);
			if (col != null)
			{
				//found overlap, set link to overlap
				Section r = col.GetComponent<Section> ();
				if (r.CheckSet (set))
					parent.SetAdjRoom (dir, r);
				return null;
			}

			//no overlap, make a new room
			Section child = parent.AddAdjRoom (dir);
			child.gameObject.name += " " + set.GetSectionCount();
			set.AddSection (child);
			return child;
		}
		#endregion

		#region INTERNAL_TYPES
		#endregion
	}
}
