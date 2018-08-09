using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WrldBldr.Util
{
	/// <summary>
	/// Table of items sorted by their chance to drop
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class DropTable<T> : IEnumerable<DropTable<T>.Drop>, ISerializable
	{
		#region STATIC_VARS

		public const int MIN_DROP_CHANCE = 1;
		#endregion

		#region STATIC_METHODS

		#endregion

		#region INSTANCE_VARS

		/// <summary>
		/// The maximum possible roll value
		/// </summary>
		public int MaxRoll { get; private set; }
		private bool isMaxRollDynamic;
		private Node front;
		public int Size { get; private set; }

		/// <summary>
		/// Decreased bias increases the probability of a rarer drop.
		/// </summary>
		public int Bias { get; set; }
		#endregion

		#region INSTANCE_METHODS

		public DropTable()
		{
			MaxRoll = 0;
			isMaxRollDynamic = true;
			front = null;
		}
		public DropTable(int maxRoll)
		{
			if (maxRoll < 0)
				throw new ArgumentOutOfRangeException ("MaxRoll cannot be negative\nValue: " + maxRoll);

			MaxRoll = maxRoll;
			isMaxRollDynamic = false;
			front = null;
		}
		public DropTable(SerializationInfo info, StreamingContext context)
		{
			MaxRoll = info.GetInt32 ("maxroll");
			isMaxRollDynamic = info.GetBoolean ("ismaxrolldyn");
			Size = info.GetInt32 ("size");
			Bias = info.GetInt32 ("bias");

			Node curr = front, prev = null;
			for (int i = 0; i < Size; i++)
			{
				curr = (Node)info.GetValue ("item" + i, typeof (Node));
				if (prev != null)
					prev.next = curr;
				else
					front = curr;

				prev = curr;
			}
		}

		/// <summary>
		/// Add an object to the table with the given drop chance
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>False if the drop chance is not between 0.0 and 1.0 (exclusive)</returns>
		public bool Add(int dropChance, T obj)
		{
			if (dropChance < MIN_DROP_CHANCE)
				throw new ArgumentException ("Drop chance cannot be below " + MIN_DROP_CHANCE + "\nValue: " + dropChance);

			if (front == null)
			{
				front = new Node (dropChance, obj);
				if (isMaxRollDynamic)
					MaxRoll += dropChance;
			}
			else
			{
				Node curr = front, prev = null;
				while (curr != null)
				{
					if (curr.DropChance == dropChance)
					{
						curr.values.Add (obj);
						break;
					}
					else if (curr.DropChance < dropChance)
					{
						Node n = new Node (dropChance, obj, curr);
						if (prev != null)
							prev.next = n;

						if (curr == front)
							front = n;

						if (isMaxRollDynamic)
							MaxRoll += dropChance;
						break;
					}
					else if (curr.next == null)
					{
						Node n = new Node (dropChance, obj);
						curr.next = n;
						if (isMaxRollDynamic)
							MaxRoll += dropChance;
						break;
					}

					prev = curr;
					curr = curr.next;
				}
			}

			Size++;
			return true;
		}

		public void Add(ICollection<int> dropChances, ICollection<T> objs)
		{
			if (dropChances.Count != objs.Count)
				throw new ArgumentException ("Collection sizes are different: " + dropChances.Count + " v " + objs.Count);

			IEnumerator objIter = objs.GetEnumerator ();
			foreach (int chance in dropChances)
			{
				objIter.MoveNext ();
				Add (chance, (T)objIter.Current);
			}
		}

		/// <summary>
		/// Attempt to remove the given object from the table
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>False if the object could not be removed</returns>
		public bool Remove(T obj)
		{
			Node prev = null;
			for (Node curr = front; curr != null; curr = curr.next)
			{
				if (curr.values.Remove (obj))
				{
					Size--;
					if (curr.values.Count <= 0)
					{
						if (prev != null)
							prev.next = curr.next;
						else
							front = curr.next;

						if (isMaxRollDynamic)
							MaxRoll -= curr.DropChance;
					}
					return true;
				}
				prev = curr;
			}
			return false;
		}

		public bool Contains(T obj)
		{
			for (Node curr = front; curr != null; curr = curr.next)
			{
				for (int i = 0; i < curr.values.Count; i++)
				{
					if (curr.values[i].Equals (obj))
						return true;
				}
			}
			return false;
		}

		public void Clear()
		{
			front = null;
			if (isMaxRollDynamic)
				MaxRoll = 0;
			Size = 0;
		}

		/// <summary>
		/// Returns a object with the smallest drop chance greater than the roll.
		/// Null if none apply.
		/// </summary>
		/// <returns></returns>
		public T Get(int roll)
		{
			return GetAll (roll)[0];
		}

		/// <summary>
		/// Returns a object with the smallest drop chance greater than the roll.
		/// Null if none apply.
		/// </summary>
		/// <returns></returns>
		public T Get(RandomNumberGen generator, float choice)
		{
			int roll = generator (MIN_DROP_CHANCE, MaxRoll);
			return Get (roll, choice);
		}

		/// <summary>
		/// Returns a object with the smallest drop chance greater than the roll.
		/// Null if none apply.
		/// </summary>
		/// <param name="roll"></param>
		/// <returns></returns>
		public T Get(int roll, float choice)
		{
			if (choice < 0f || choice > 1f)
				throw new ArgumentException ("Choice must be between 0.0 and 1.0, inclusive");

			T[] choices = GetAll (roll);
			int index = (int)(choice * choices.Length);
			if (index == choices.Length)
				index = choices.Length - 1;
			return choices[index];
		}

		/// <summary>
		/// Returns all objects in the table with a drop chance equal to a value greater than or equal to the roll
		/// </summary>
		/// <param name="roll"></param>
		/// <returns></returns>
		public T[] GetAll(int roll)
		{
			roll += Bias;
			if (roll > MaxRoll)
				roll = MaxRoll;
			if (roll < MIN_DROP_CHANCE)
				roll = MIN_DROP_CHANCE;

			Node curr = front, prev = null;
			while (curr != null)
			{
				if (curr.DropChance == roll)
					return curr.values.ToArray ();
				else if (curr.DropChance < roll && prev != null)
					return prev.values.ToArray ();

				prev = curr;
				curr = curr.next;
			}
			if (prev != null)
				return prev.values.ToArray ();
			return null;
		}

		public override string ToString()
		{
			string str = GetType ().Name + " Size: " + Size + " MaxRoll: " + MaxRoll + " [";
			for (Node curr = front; curr != null; curr = curr.next)
			{
				str += "\n  (" + ((float)curr.DropChance / MaxRoll).ToString (".000000") + ") " + curr.ToString ();
			}
			return str + "\n]";
		}

		public override bool Equals(object obj)
		{
			DropTable<T> other = (DropTable<T>)obj;
			if (MaxRoll != other.MaxRoll
				|| Size != other.Size
				|| isMaxRollDynamic != other.isMaxRollDynamic
				|| Bias != other.Bias)
				return false;

			for (Node curr = front, otherCurr = other.front; curr != null; curr = curr.next)
			{
				if (!curr.Equals (otherCurr))
					return false;

				otherCurr = otherCurr.next;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public IEnumerator<Drop> GetEnumerator()
		{
			for (Node curr = front; curr != null; curr = curr.next)
			{
				for (int i = 0; i < curr.values.Count; i++)
				{
					Drop d = new Drop ();
					d.dropChance = curr.DropChance;
					d.obj = curr.values[i];
					yield return d;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator ();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("maxroll", MaxRoll);
			info.AddValue ("ismaxrolldyn", isMaxRollDynamic);
			info.AddValue ("size", Size);
			info.AddValue ("bias", Bias);

			int count = 0;
			for (Node curr = front; curr != null; curr = curr.next)
			{
				info.AddValue ("item" + count, curr);
				count++;
			}
		}
		#endregion

		#region INTERNAL_TYPES
		public delegate int RandomNumberGen(int min, int max);

		[Serializable]
		public struct Drop
		{
			public int dropChance;
			public T obj;
		}

		private class Node : ISerializable
		{
			public int DropChance { get; private set; }
			public Node next;
			public List<T> values;

			public Node(int dropChance, T value) : this (dropChance, value, null) { }
			public Node(int dropChance, T value, Node next)
			{
				DropChance = dropChance;
				values = new List<T> ();
				values.Add (value);
				this.next = next;
			}
			public Node(SerializationInfo info, StreamingContext context)
			{
				DropChance = info.GetInt32 ("dropchance");
				values = (List<T>)info.GetValue ("values", typeof (List<T>));
			}

			public override string ToString()
			{
				string str = DropChance + " [ ";
				for (int i = 0; i < values.Count; i++)
				{
					str += values[i].ToString ();
					if (i < values.Count - 1)
						str += ", ";
				}
				return str + " ]";
			}

			public override bool Equals(object obj)
			{
				Node other = (Node)obj;
				if (DropChance != other.DropChance
					|| values.Count != other.values.Count)
					return false;
				for (int i = 0; i < values.Count; i++)
				{
					if (!values[i].Equals (other.values[i]))
						return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode ();
			}

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("dropchance", DropChance);
				info.AddValue ("values", values);
			}
		}
		#endregion
	}
}
