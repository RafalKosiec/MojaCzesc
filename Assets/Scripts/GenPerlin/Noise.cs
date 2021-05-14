using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise{

    public enum NormalizeMode { Local, Global};

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode, float talusValue, float talusValueInverted, float movingConstant, bool useThermalErosion, bool useThermalErosionInverted, bool useHydraulicErosion, int iterationsThermalErosion, int iterationsThermalErosionInverted, int iterationsHydraulicErosion, int hydSeed, int erosionRadius, float inertia, float sedimentCapacityFactor, float minSedimentCapacity, float erodeSpeed, float depositSpeed, float evaporateSpeed, float gravity, int maxDropletLifetime, float initialWaterVolume, float initialSpeed)
    {
        float[,] differences = new float[3, 3];

        float[,] noiseMap = new float[mapWidth, mapHeight];
        
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        if (scale <= 0) scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y=0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i<octaves; i++)
                {
                    float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y- halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) *2 - 1; //bcs perlin value is 0:1, doing this makes perlin value -1:1
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight); //to 1.75 to jest estymowane, sprawdzone doświadczalnie. bo inaczej to byloby slabo w sensie za małe wartości
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight,0, int.MaxValue);
                }
            }
        }

        //code for erosion
        if (useThermalErosion)
        {
            for (int iter = 0; iter < iterationsThermalErosion; iter++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    for (int x = 1; x < mapWidth - 1; x++)
                    {
                        differences[0, 0] = noiseMap[x, y] - noiseMap[x - 1, y - 1];
                        differences[0, 1] = noiseMap[x, y] - noiseMap[x, y - 1];
                        differences[0, 2] = noiseMap[x, y] - noiseMap[x + 1, y - 1];

                        differences[1, 0] = noiseMap[x, y] - noiseMap[x - 1, y];
                        differences[1, 1] = noiseMap[x, y] - noiseMap[x, y];
                        differences[1, 2] = noiseMap[x, y] - noiseMap[x + 1, y];

                        differences[2, 0] = noiseMap[x, y] - noiseMap[x - 1, y + 1];
                        differences[2, 1] = noiseMap[x, y] - noiseMap[x, y + 1];
                        differences[2, 2] = noiseMap[x, y] - noiseMap[x + 1, y + 1];

                        float sumOfHeightDifferencesGreaterThanTalus = 0;
                        float dMax = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (differences[i, j] > talusValue) sumOfHeightDifferencesGreaterThanTalus += differences[i, j];
                                if (differences[i, j] > dMax) dMax = differences[i, j];
                            }
                        }

                        if (sumOfHeightDifferencesGreaterThanTalus > 0.0001) noiseMap[x, y] -= (dMax - talusValue) * movingConstant;

                        if (differences[0, 0] > talusValue) noiseMap[x - 1, y - 1] = noiseMap[x - 1, y - 1] + movingConstant * (dMax - talusValue) * (differences[0, 0] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[0, 1] > talusValue) noiseMap[x, y - 1] = noiseMap[x, y - 1] + movingConstant * (dMax - talusValue) * (differences[0, 1] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[0, 2] > talusValue) noiseMap[x + 1, y - 1] = noiseMap[x + 1, y - 1] + movingConstant * (dMax - talusValue) * (differences[0, 2] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[1, 0] > talusValue) noiseMap[x - 1, y] = noiseMap[x - 1, y] + movingConstant * (dMax - talusValue) * (differences[1, 0] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[1, 2] > talusValue) noiseMap[x + 1, y] = noiseMap[x + 1, y] + movingConstant * (dMax - talusValue) * (differences[1, 2] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[2, 0] > talusValue) noiseMap[x - 1, y + 1] = noiseMap[x - 1, y + 1] + movingConstant * (dMax - talusValue) * (differences[2, 0] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[2, 1] > talusValue) noiseMap[x, y + 1] = noiseMap[x, y + 1] + movingConstant * (dMax - talusValue) * (differences[2, 1] / sumOfHeightDifferencesGreaterThanTalus);
                        if (differences[2, 2] > talusValue) noiseMap[x + 1, y + 1] = noiseMap[x + 1, y + 1] + movingConstant * (dMax - talusValue) * (differences[2, 2] / sumOfHeightDifferencesGreaterThanTalus);

                    }
                }
            }
        }

        if (useThermalErosionInverted)
        {
            for (int iter = 0; iter < iterationsThermalErosionInverted; iter++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    for (int x = 1; x < mapWidth - 1; x++)
                    {
                        differences[0, 0] = noiseMap[x, y] - noiseMap[x - 1, y - 1];
                        differences[0, 1] = noiseMap[x, y] - noiseMap[x, y - 1];
                        differences[0, 2] = noiseMap[x, y] - noiseMap[x + 1, y - 1];

                        differences[1, 0] = noiseMap[x, y] - noiseMap[x - 1, y];
                        differences[1, 1] = noiseMap[x, y] - noiseMap[x, y];
                        differences[1, 2] = noiseMap[x, y] - noiseMap[x + 1, y];

                        differences[2, 0] = noiseMap[x, y] - noiseMap[x - 1, y + 1];
                        differences[2, 1] = noiseMap[x, y] - noiseMap[x, y + 1];
                        differences[2, 2] = noiseMap[x, y] - noiseMap[x + 1, y + 1];

                        float dMax = float.MinValue;
                        int l = 0, k = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (differences[i, j] > dMax)
                                {
                                    dMax = differences[i, j];
                                    l = i; k = j;
                                }
                            }
                        }

                        if (dMax > 0 && dMax <= talusValueInverted)
                        {
                            float deltaH = dMax * 0.5f;
                            noiseMap[x, y] -= deltaH;
                            noiseMap[x - 1 + k, y - 1 + l] += deltaH;
                        }

                    }
                }
            }
        }

        if (useHydraulicErosion)
        {
            HydraulicErosion hydraulicErosion = new HydraulicErosion(hydSeed, erosionRadius, inertia, sedimentCapacityFactor, minSedimentCapacity, erodeSpeed, depositSpeed, evaporateSpeed, gravity, maxDropletLifetime, initialWaterVolume, initialSpeed);
            float[] mapIn1D = new float[mapHeight * mapWidth];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    mapIn1D[mapWidth * y + x] = noiseMap[x, y];
                }
            }

            hydraulicErosion.Erode(mapIn1D, mapWidth, iterationsHydraulicErosion);

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = mapIn1D[mapWidth * y + x];
                }
            }
        }

        
        return noiseMap;
    }

}
