using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Points : MonoBehaviour
{
    public struct Point
    {
        public Vector2 position;
        public float angle;

        public Point(Vector2 p, float a)
        {
            position = p;
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

    public int width;
    public int height;
    public float pointSpeed;
    static int positionsId = Shader.PropertyToID("points");

    public float evaporationSpeed;
    public float diffuseSpeed;

    public float angleChangeRate;
    public float maxAngle;

    public bool pausePoints;

    void EnablePoints()
    {
        pointsBuffer = new ComputeBuffer(numberOfPoints, sizeof(float) * 3);
        map = new RenderTexture(width.ConvertTo<int>(), height.ConvertTo<int>(), 0, RenderTextureFormat.ARGB32);
        map.enableRandomWrite = true;
        mapMaterial.mainTexture = map;
        Point[] input = CreateRandomPoints();
        pointsBuffer.SetData(input);
        Point[] output = new Point[numberOfPoints];

        pointsKern = points.FindKernel("Main");
        points.SetBuffer(pointsKern, "points", pointsBuffer);
        points.SetInt("width", width);
        points.SetInt("height", height);
        points.SetInt("numberOfPoints", numberOfPoints);
        points.SetTexture(pointsKern, "map", map);
        points.SetFloat("maxAngle", maxAngle);
        points.SetFloat("angleChangeRate", angleChangeRate);
        points.SetFloat("pointSpeed", pointSpeed);
        
    }

    void EnableTrailDissapation()
    {
        trailsKern = trails.FindKernel("ProcessTrails");
        trails.SetTexture(trailsKern, "map", map);
        trails.SetFloat("evaporationSpeed", evaporationSpeed);
        trails.SetFloat("diffuseSpeed", diffuseSpeed);
        trails.SetInt("width", width);
        trails.SetInt("height", height);
    }

    void OnEnable()
    {
        EnablePoints();
        EnableTrailDissapation();
    }
    
    void FixedUpdate()
    {
        for(int i = 0; i < 1; i++)
        {
            UpdateFunctionOnGPU();
        }
        
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
            float direction = Random.Range(0, 2 * 3.1415f);
            points[i] = new Point(position, direction);
        }

        return points;
    }

    void UpdateFunctionOnGPU()
    {

        //points.SetBuffer(kernel, "points", pointsBuffer);
        //points.SetInt("boundSize", (int)boundSize);
        trails.SetFloat("deltaTime", Time.fixedDeltaTime);

        points.SetFloat("deltaTime", Time.fixedDeltaTime);
        points.SetFloat("time", Time.time);
        trails.Dispatch(trailsKern, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);
        if (!pausePoints)
        {
            points.Dispatch(pointsKern, Mathf.CeilToInt(numberOfPoints / 32f), 1, 1);
        }
        
        
    }
}
