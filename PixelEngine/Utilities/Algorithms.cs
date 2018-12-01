using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelEngine.Utilities
{
	public static class Algorithms
	{
		public static T[] Concat<T>(params T[][] items)
		{
			T[] result = new T[items.Sum(t => t.Length)];

			int counter = 0;

			foreach (T[] item in items)
				foreach (T t in item)
					result[counter++] = t;

			return result;
		}
		public static List<T> Concat<T>(params List<T>[] items)
		{
			List<T> result = new List<T>(items.Sum(t => t.Count));

			int counter = 0;

			foreach (List<T> item in items)
				foreach (T t in item)
					result[counter++] = t;

			return result;
		}

		public static void Sort<T>(this T[] items) => Sort(items, Comparer<T>.Default.Compare);
		public static void Sort<T>(this List<T> items) => Sort(items, Comparer<T>.Default.Compare);
		public static void Sort<T>(this T[] items, Comparison<T> comparision)
		{
			int Partition(T[] arr, int left, int right)
			{
				T pivot = arr[left];

				while (true)
				{
					while (comparision(arr[left], pivot) < 0)
						left++;

					while (comparision(arr[right], pivot) > 0)
						right--;

					if (left < right)
					{
						if (comparision(arr[left], arr[right]) == 0)
							return right;

						T temp = arr[left];
						arr[left] = arr[right];
						arr[right] = temp;
					}
					else
					{
						return right;
					}
				}
			}

			void QuickSort(T[] arr, int left, int right)
			{
				if (left < right)
				{
					int pivot = Partition(arr, left, right);

					if (pivot > 1)
						QuickSort(arr, left, pivot - 1);

					if (pivot + 1 < right)
						QuickSort(arr, pivot + 1, right);
				}
			}

			QuickSort(items, 0, items.Length - 1);
		}
		public static void Sort<T>(this List<T> items, Comparison<T> comparision)
		{
			int Partition(List<T> arr, int left, int right)
			{
				T pivot = arr[left];

				while (true)
				{
					while (comparision(arr[left], pivot) < 0)
						left++;

					while (comparision(arr[right], pivot) > 0)
						right--;

					if (left < right)
					{
						if (comparision(arr[left], arr[right]) == 0)
							return right;

						T temp = arr[left];
						arr[left] = arr[right];
						arr[right] = temp;
					}
					else
					{
						return right;
					}
				}
			}

			void QuickSort(List<T> arr, int left, int right)
			{
				if (left < right)
				{
					int pivot = Partition(arr, left, right);

					if (pivot > 1)
						QuickSort(arr, left, pivot - 1);

					if (pivot + 1 < right)
						QuickSort(arr, pivot + 1, right);
				}
			}

			QuickSort(items, 0, items.Count - 1);
		}

		public static void Randomize<T>(this List<T> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				int r = Randoms.RandomInt(i, items.Count);

				T temp = items[i];
				items[i] = items[r];
				items[r] = temp;
			}
		}
		public static void Randomize<T>(this T[] items)
		{
			for (int i = 0; i < items.Length; i++)
			{
				int r = Randoms.RandomInt(i, items.Length);

				T temp = items[i];
				items[i] = items[r];
				items[r] = temp;
			}
		}

		public static T Search<T>(this IEnumerable<T> items, T item) => Search(items, (t, i) => t.Equals(item));
		public static T Search<T>(this IEnumerable<T> items, Func<T, bool> condition) => Search(items, (t, i) => condition(t));
		public static T Search<T>(this IEnumerable<T> items, Func<T, int, bool> condition)
		{
			int index = 0;

			foreach (T item in items)
				if (condition(item, index++))
					return item;

			return default;
		}
	}
}