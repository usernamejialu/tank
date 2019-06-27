using System;
using System.Collections.Generic;
using UnityRandom = UnityEngine.Random;

namespace Tanks.Extensions
{
  public static class IListExtensions
  {
    // Select an item from a list using a weighted selection.
    public static T WeightedSelection<T>(this IList<T> elements, int weightSum, Func<T, int> getElementWeight)
    {
      int index = elements.WeightedSelectionIndex(weightSum, getElementWeight);
      return elements[index];
    }


    // Select the index of an item from a list using a weighted selection.
    public static int WeightedSelectionIndex<T>(this IList<T> elements, int weightSum, Func<T, int> getElementWeight)
    {
      if (weightSum <= 0)
      {
        throw new ArgumentException("WeightSum should be a positive value", "weightSum");
      }

      int selectionIndex = 0;
      int selectionWeightIndex = UnityRandom.Range(0, weightSum);
      int elementCount = elements.Count;

      if (elementCount == 0)
      {
        throw new InvalidOperationException("Cannot perform selection on an empty collection");
      }

      int itemWeight = getElementWeight(elements[selectionIndex]);
      while (selectionWeightIndex >= itemWeight)
      {
        selectionWeightIndex -= itemWeight;
        selectionIndex++;

        if (selectionIndex >= elementCount)
        {
          throw new ArgumentException("Weighted selection exceeded indexable range. Is your weightSum correct?", "weightSum");
        }

        itemWeight = getElementWeight(elements[selectionIndex]);
      }

      return selectionIndex;
    }


    // Shuffle this List into a new array copy
    public static T[] Shuffle<T>(this IList<T> original)
    {
      int numItems = original.Count;
      T[] result = new T[numItems];

      for (int i = 0; i < numItems; ++i)
      {
        int j = UnityRandom.Range(0, i + 1);

        if (j != i)
        {
          result[i] = result[j];
        }

        result[j] = original[i];
      }

      return result;
    }
  }
}