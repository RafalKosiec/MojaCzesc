using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public Noise.NormalizeMode normalizeMode;

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    [Space(20)]
    //for erosion
    public bool useThermalErosion;

    [Range(1, 30)]
    public int iterationsThermalErosion;

    [Range(0, 0.25f)]
    public float talusValue;
    [Range(0, 1)]
    public float movingConstant;

    [Space(20)]
    public bool useThermalErosionInverted;
    [Range(1, 30)]
    public int iterationsThermalErosionInverted;
    [Range(0, 0.5f)]
    public float talusValueInverted;

    [Space(20)]
    public bool useHydraulicErosion;
    [Range(0,70000)]
    public int iterationsHydraulicErosion;
    public int hydSeed;
    [Range(2, 8)]
    public int erosionRadius = 3;
    [Range(0, 1)]
    public float inertia = .05f; // At zero, water will instantly change direction to flow downhill. At 1, water will never change direction. 
    public float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
    public float minSedimentCapacity = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain
    [Range(0, 1)]
    public float erodeSpeed = .3f;
    [Range(0, 1)]
    public float depositSpeed = .3f;
    [Range(0, 1)]
    public float evaporateSpeed = .01f;
    public float gravity = 4;
    public int maxDropletLifetime = 30;

    public float initialWaterVolume = 1;
    public float initialSpeed = 1;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

        base.OnValidate();
    }

#endif

}

