using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace WrldBldr
{
	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class Region
	{
		#region INSTANCE_VARS

		// List of all the sections that are members of this Region
		[SerializeField]
		private Section[] sections;

		// Number of sections that are currently members of this Region
		[SerializeField]
		private int size;

		// List of all the regions that branch from this Region
		[SerializeField]
		private Region[] subRegions;

		[SerializeField]
		private Color debugColor;

		#endregion

		#region INSTANCE_METHODS

		public Region(int targetSize, params Region[] regions)
		{
			sections = new Section[targetSize];
			size = 0;
			subRegions = regions;

			debugColor = new Color (1f, 1f, 1f, 0.5f);
		}

		/// <summary>
		/// Adds a Section to this Region. Fails if the target size has already been reached.
		/// </summary>
		/// <param name="s"></param>
		/// <returns>The success of the addition</returns>
		public bool AddSection(Section s)
		{
			if (size < sections.Length)
			{
				sections[size] = s;
				size++;
				s.AssignSet (this);
				return true;
			}
			return false;
		}

		public int GetTargetSize()
		{
			return sections.Length;
		}

		public int GetFullTargetSize()
		{
			int fts = GetTargetSize();
			for (int i = 0; i < subRegions.Length; i++)
			{
				if (subRegions[i] != null)
					fts += subRegions[i].GetFullTargetSize ();
			}
			return fts;
		}

		public Color GetDebugColor()
		{
			return debugColor;
		}

		public void SetDebugColor(Color c)
		{
			debugColor = c;
		}

		public int GetSectionCount()
		{
			return size;
		}

		public int GetFullSectionCount()
		{
			int fsc = GetSectionCount ();
			for (int i = 0; i < subRegions.Length; i++)
			{
				if (subRegions[i] != null)
					fsc += subRegions[i].GetFullSectionCount ();
			}
			return fsc;
		}

		public Section GetSection(int index)
		{
			return sections[index];
		}

		public int GetSubRegionCount()
		{
			return subRegions.Length;
		}

		public Region GetSubRegion(int index)
		{
			return subRegions[index];
		}

		public void Clear()
		{
			//destroy old map
			if (sections != null)
			{
				for (int i = 0; i < size; i++)
				{
					try
					{
						Object.Destroy (sections[i].gameObject);
					}
					catch (MissingReferenceException) { }
				}
				size = 0;
			}

			for (int i = 0; i < subRegions.Length; i++)
			{
				if(subRegions[i] != null)
					subRegions[i].Clear ();
			}
		}

		/// <summary>
		/// Traverses backwards through the section list to find a section with at least
		/// one free adjacent space
		/// </summary>
		/// <returns></returns>
		private Section FindFreeSection(ref int startingIndex)
		{
			for (; startingIndex >= 0; startingIndex--)
			{
				if (sections[startingIndex].HasFreeAdjSpace())
				{
					return sections[startingIndex];
				}
			}
			return null;
		}
		private Section FindFreeSection(int startingIndex)
		{
			return FindFreeSection (ref startingIndex);
		}
		#endregion

		#region INTERNAL_TYPES

		public delegate void BasicNotify();
		#endregion
	}
}
