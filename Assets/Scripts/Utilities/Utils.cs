using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public class Utils
{
    public static List<NormalItem.eNormalType> GetBalancedInitialNormalTypes(int totalCount)
    {
        List<NormalItem.eNormalType> types = Enum.GetValues(typeof(NormalItem.eNormalType))
            .Cast<NormalItem.eNormalType>()
            .ToList();

        List<NormalItem.eNormalType> result = new List<NormalItem.eNormalType>(totalCount);
        if (types.Count == 0 || totalCount <= 0)
        {
            return result;
        }

        int tripleBlocks = totalCount / 3;
        if (tripleBlocks < types.Count)
        {
            while (result.Count < totalCount)
            {
                result.Add(types[result.Count % types.Count]);
            }

            Shuffle(result);
            return result;
        }

        int[] blocksPerType = new int[types.Count];
        for (int i = 0; i < types.Count; i++)
        {
            blocksPerType[i] = 1;
        }

        tripleBlocks -= types.Count;
        while (tripleBlocks > 0)
        {
            int index = URandom.Range(0, types.Count);
            blocksPerType[index]++;
            tripleBlocks--;
        }

        for (int i = 0; i < types.Count; i++)
        {
            for (int j = 0; j < blocksPerType[i] * 3; j++)
            {
                result.Add(types[i]);
            }
        }

        while (result.Count < totalCount)
        {
            result.Add(types[URandom.Range(0, types.Count)]);
        }

        Shuffle(result);
        return result;
    }

    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = URandom.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }

    public static NormalItem.eNormalType GetRandomNormalType()
    {
        Array values = Enum.GetValues(typeof(NormalItem.eNormalType));
        NormalItem.eNormalType result = (NormalItem.eNormalType)values.GetValue(URandom.Range(0, values.Length));

        return result;
    }

    public static NormalItem.eNormalType GetRandomNormalTypeExcept(NormalItem.eNormalType[] types)
    {
        List<NormalItem.eNormalType> list = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().Except(types).ToList();

        int rnd = URandom.Range(0, list.Count);
        NormalItem.eNormalType result = list[rnd];

        return result;
    }
}
