using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareGen
{
    public static float[,] GenerateHeightmapUsingDiamondSuare(int jakisInt, float diamRandomFirstMinValue, float diamRandomFirstMaxValue, float roughness)
    {
        int sideLength = TwoToTheNthPowerPlusOne(jakisInt);
        float[,] heightMap = new float[sideLength, sideLength];
        heightMap = DefineAndModifyCorners(heightMap, diamRandomFirstMinValue, diamRandomFirstMaxValue);
        float mRoughness = roughness;
        int chunkSize = sideLength - 1; //width of each square and diamond in the current iteration
        while (chunkSize > 1)
        {
            Debug.Log(mRoughness);
            int half = chunkSize / 2;
            SquareStep(heightMap, chunkSize, half, mRoughness);
            DiamondStep(heightMap, chunkSize, half, mRoughness);
            chunkSize /= 2;
            mRoughness /= 2f;
        }
        float minValue = FindMin(heightMap);
        float maxValue = FindMax(heightMap);
        MapValues(heightMap, minValue, maxValue);
        return heightMap;
    }

    static void MapValues(float[,] heightmap, float min, float max)
    {
        for (int x = 0; x < heightmap.GetLength(1); x++)
        {
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                heightmap[x, y] = Map(heightmap[x, y], min, max, 0, 1);
            }
        }
    }

    static float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    static float FindMin(float[,] heightmap)
    {
        float min = float.MaxValue;
        for(int x = 0; x < heightmap.GetLength(1); x++)
        {
            for(int y = 0; y < heightmap.GetLength(1); y++)
            {
                if (heightmap[x, y] < min) min = heightmap[x, y];
            }
        }
        return min;
    }

    static float FindMax(float[,] heightmap)
    {
        float max = float.MinValue;
        for (int x = 0; x < heightmap.GetLength(1); x++)
        {
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                if (heightmap[x, y] > max) max = heightmap[x, y];
            }
        }
        return max;
    }

    static void SquareStep(float[,] heightMap, int squareSize, int half, float mRoughness)
    {
        for (int y = 0 ; y < heightMap.GetLength(1)-1; y += squareSize)
        {
            for (int x = 0; x < heightMap.GetLength(1)-1; x += squareSize)
            {
                heightMap[y + half, x + half] = (heightMap[y, x] + heightMap[y, x + squareSize] + heightMap[y + squareSize, x] + heightMap[y + squareSize, x + squareSize]) / 4f + Random.Range(-mRoughness, mRoughness);
            }
        }
    }

    static void DiamondStep(float[,] heightMap, int squareSize, int half, float mRoughness)
    {
        for (int y = 0; y < heightMap.GetLength(1)-1; y += half)
        {
            for (int x = (y+half) % squareSize; x < heightMap.GetLength(1)-1; x += squareSize)
            {
                float average = 0;
                // Get the average of the corners
                average = heightMap[(x - half + heightMap.GetLength(1) - 1) % (heightMap.GetLength(1) - 1), y];
                average += heightMap[(x + half) % (heightMap.GetLength(1) - 1), y];
                average += heightMap[x, (y + half) % (heightMap.GetLength(1) - 1)];
                average += heightMap[x, (y - half + heightMap.GetLength(1) - 1) % (heightMap.GetLength(1) - 1)];
                average /= 4.0f;

                // Offset by a random value
                average += (Random.Range(-mRoughness, mRoughness));

                // Set the height value to be the calculated average
                heightMap[x, y] = average;

                // Set the height on the opposite edge if this is
                if (x == 0)
                {
                    heightMap[heightMap.GetLength(1) - 1, y] = average;
                }

                if (y == 0)
                {
                    heightMap[x, heightMap.GetLength(1) - 1] = average;
                }

            }
        }
    }

    static int TwoToTheNthPowerPlusOne(int power)
    {
        int result = 1;
        for (int i = 0; i < power; i++)
        {
            result *= 2;
        }
        result += 1;
        return result;
    }

    static float[,] DefineAndModifyCorners(float[,] array, float randomFirstMinValue, float randomFirstMaxValue)
    {
        array[0, 0] = Random.Range(randomFirstMinValue, randomFirstMaxValue);
        array[array.GetLength(1)-1, 0] = Random.Range(randomFirstMinValue, randomFirstMaxValue);
        array[0, array.GetLength(1) -1] = Random.Range(randomFirstMinValue, randomFirstMaxValue);
        array[array.GetLength(1) - 1, array.GetLength(1) - 1] = Random.Range(randomFirstMinValue, randomFirstMaxValue);
        return array;
    }

}
