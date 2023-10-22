using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tools.Extensions
{
	public static class ListExtensions
	{
		public static T GetRandom<T>(this IList<T> list) => GetRandom(list, list.Count);
		public static T GetRandom<T>(this IList<T> list, ref Random.State state)
		{
			Random.State previousState = Random.state;
			Random.state = state;
			T random = GetRandom(list);
			state = Random.state;
			Random.state = previousState;
			return random;
		}
		public static T GetRandom<T>(this IList<T> list, int max)
		{
			if (list.IsEmpty()) return default;
			
			max = Mathf.Clamp(max, 0, list.Count);
			return list[Random.Range(0, max)];
		}
		
		public static T ExtractRandom<T>(this IList<T> list) => list.ExtractRandom(list.Count);
		public static T ExtractRandom<T>(this IList<T> list, ref Random.State state)
		{
			Random.State previousState = Random.state;
			Random.state = state;
			T random = list.ExtractRandom();
			state = Random.state;
			Random.state = previousState;
			return random;
		}
		public static T ExtractRandom<T>(this IList<T> list, int max)
		{
			if (list.IsEmpty()) return default;
			
			max = Mathf.Clamp(max, 0, list.Count);
			int index = Random.Range(0, max);
			T item = list[index];
			list.RemoveAt(index);
			return item;
		}
		
		public static void Shuffle<T>(this IList<T> list) => Shuffle(list, list.Count);
		public static void Shuffle<T>(this IList<T> list, int count)
		{
			count = Mathf.Clamp(count, 0, list.Count);
			for (int i = 0; i < count; i++)
			{
				int randomIndex = Random.Range(i, list.Count);
				(list[i], list[randomIndex]) = (list[randomIndex], list[i]);
			}
		}
		public static void Shuffle<T>(this IList<T> list, ref Random.State state)
		{
			Random.State previousState = Random.state;
			Random.state = state;
			list.Shuffle();
			state = Random.state;
			Random.state = previousState;
		}
		public static void Shuffle<T>(this IList<T> list, ref Random.State state, int count)
		{
			Random.State previousState = Random.state;
			Random.state = state;
			list.Shuffle(count);
			state = Random.state;
			Random.state = previousState;
		}

		public static void Modify<T>(this IList<T> list, Func<T, T> modificator, ref Random.State state)
		{
			Random.State previousState = Random.state;
			Random.state = state;
			list.Modify(modificator);
			state = Random.state;
			Random.state = previousState;
		}
		public static void Modify<T>(this IList<T> list, Func<T, T> modificator)
		{
			for (int i = 0; i < list.Count; i++) list[i] = modificator(list[i]);
		}
		
		public static void AddNotNull<T>(this IList<T> list, T item)
		{
			if (item == null) return;
			list.Add(item);
		}
		
		public static bool IsEmpty<T>(this ICollection<T> collection) => collection == null || collection.Count == 0;
	}
}