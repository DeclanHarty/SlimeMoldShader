using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Points : MonoBehaviour
{
    public struct Point
    {
        public Vector2 position;
        public float speed;
        public float angle;

        public Point(Vector2 p, float s, float a)
        {
            position = p;
            speed = s;
            angle = a;
        }
    }
    public Mesh mesh;
    public Material mat;

    public int numberOfPoints;
    ComputeBuffer pointsBuffer;

    [SerializeField]
    ComputeShader points;
    [SerializeField]
    ComputeShader trails;
    int trailsKern;
    int pointsKern;
    RenderTexture map;

    [SerializeField]
    Material mapMaterial;

    public uint width;
    public uint height;
    public int pointSpeed;
    static int positionsId = Shader.PropertyToID("points");

    public float evaporationSpeed;
    public float diffuseSpeed;

    public float angleChangeRate;
    public float maxAngle;

    public bool pausePoints;

    void EnablePoints()
    {
        pointsBuffer = new ComputeBuffer(numberOfPoints, sizeof(float) * 4);
        map = new RenderTexture(width.ConvertTo<int>(), height.ConvertTo<int>(), 0, RenderTextureFormat.ARGB32);
        map.filterMode = FilterMode.Point;
        map.enableRandomWrite = true;
        mapMaterial.mainTexture = map;
        Point[] input = CreateRandomPoints();
        pointsBuffer.SetData(input);
        Point[] output = new Point[numberOfPoints];

        pointsKern = points.FindKernel("Main");
        points.SetBuffer(pointsKern, "points", pointsBuffer);
        points.SetInt("width", (int)width);
        points.SetInt("height", (int)height);
        points.SetTexture(pointsKern, "map", map);
        points.SetFloat("maxAngle", maxAngle);
        points.SetFloat("angleChangeRate", angleChangeRate);
        
    }

    void EnableTrailDissapation()
    {
        trailsKern = trails.FindKernel("ProcessTrails");
        trails.SetTexture(trailsKern, "map", map);
        trails.SetFloat("evaporationSpeed", evaporationSpeed);
        trails.SetFloat("diffuseSpeed", diffuseSpeed);
        trails.SetInt("width", (int)width);
        trails.SetInt("height", (int)height);
    }

    void OnEnable()
    {
        EnablePoints();
        EnableTrailDissapation();
    }
    
    void Update()
    {
        UpdateFunctionOnGPU();
    }

    void OnDisable(){
        pointsBuffer.Release();
        pointsBuffer = null;
    }

    Point[] CreateRandomPoints()
    {
        Point[] points = new Point[numberOfPoints];
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 position = new Vector2(Random.Range(0, width - 1), Random.Range(0, height - 1));
            float direction = Random.Range(0, 1f);
            points[i] = new Point(position, pointSpeed, direction);
        }

        return points;
    }

    void UpdateFunctionOnGPU()
    {

        //points.SetBuffer(kernel, "points", pointsBuffer);
        //points.SetInt("boundSize", (int)boundSize);
        trails.SetFloat("diffuseSpeed", diffuseSpeed);
        trails.SetFloat("evaporationSpeed", evaporationSpeed);
        trails.SetFloat("deltaTime", Time.deltaTime);

        points.SetFloat("deltaTime", Time.deltaTime);
        points.SetFloat("time", Time.time);
        points.SetFloat("angleChangeRate", angleChangeRate);
        trails.Dispatch(trailsKern, Mathf.CeilToInt(width/8f), Mathf.CeilToInt(height/8f), 1);
        if (!pausePoints)
        {
            points.Dispatch(pointsKern, Mathf.CeilToInt(numberOfPoints / 32f), 1, 1);
        }
        
        
    }
}
