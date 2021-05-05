using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh, FalloffMap};
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0,MeshGenerator.numberOfSupportedChunkSizes-1)]
    public int chunkSizeIndex;

    [Range(0, MeshGenerator.numberOfSupportedFlatshadedChunkSizes - 1)]
    public int flatshadedChunkSizeIndex;

    [Range(0,MeshGenerator.numberOfSupportedLODs-1)]
    public int editorPreviewLOD;
       
    public bool autoUpdate;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //przeniesc to pozniej

    [Space(10)]
    [Range(1,8)]
    public int DiaSqTerainScale;
    float diamRandomFirstMinValue = 0f;
    float diamRandomFirstMaxValue = 10f;
    [Range(0,800)]
    public float roughness;

    private void Awake()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            if (terrainData.usingDiamondSquare)
            {
                return TwoToTheNthPowerPlusOne(DiaSqTerainScale);
            }
            else
            {
                if (terrainData.useFlatShading)
                {
                    return MeshGenerator.supportedFlatshadedChunkSizes[flatshadedChunkSizeIndex] - 1;
                }
                else
                {
                    return MeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
                }
            }
        }
    }

    //for mapchunksize
    int TwoToTheNthPowerPlusOne(int power)
    {
        int result = 1;
        for (int i = 0; i < power; i++)
        {
            result *= 2;
        }
        result += 1;
        return result;
    }

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        else if (drawMode == DrawMode.Mesh) display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
        else if (drawMode == DrawMode.FalloffMap) display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadstart = delegate
        {
            MapDataThread(center, callback);
        };
        new Thread(threadstart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue) //to make sure only 1 thread executes this point
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadstart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadstart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
        lock (meshDataThreadInfoQueue) //to make sure only 1 thread executes this point
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap;
        if(!terrainData.usingDiamondSquare) noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center+ noiseData.offset, noiseData.normalizeMode, noiseData.talusValue, noiseData.talusValueInverted, noiseData.movingConstant, noiseData.useThermalErosion, noiseData.useThermalErosionInverted, noiseData.useHydraulicErosion, noiseData.iterationsThermalErosion, noiseData.iterationsThermalErosionInverted, noiseData.iterationsHydraulicErosion, noiseData.hydSeed, noiseData.erosionRadius, noiseData.inertia, noiseData.sedimentCapacityFactor, noiseData.minSedimentCapacity, noiseData.erodeSpeed, noiseData.depositSpeed, noiseData.evaporateSpeed, noiseData.gravity, noiseData.maxDropletLifetime, noiseData.initialWaterVolume, noiseData.initialSpeed);
        else noiseMap = DiamondSquareGen.GenerateHeightmapUsingDiamondSuare(DiaSqTerainScale, diamRandomFirstMinValue, diamRandomFirstMaxValue, roughness);
        if (terrainData.useFalloff)
        {

            if(falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize+2; y++)
            {
                for (int x = 0; x < mapChunkSize+2; x++)
                {
                    if (terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }
                }
            }
        }

        return new MapData(noiseMap);
    }

    private void OnValidate()
    {
        if(terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }

        if(textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


public struct MapData
{
    public readonly float[,] heightMap;
    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}