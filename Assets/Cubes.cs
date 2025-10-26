using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Cubes : MonoBehaviour
{
    public float startX;
    public float endX;
    public int resolution;

    float stepSize;

    public GameObject pointPrefab;

    public Transform[] cubeTransforms;

    void Awake()
    {
        stepSize = (endX - startX) / (resolution - 1);
        cubeTransforms = new Transform[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float currentX = stepSize * i + startX;
            float currentY = Mathf.Sin(currentX);
            GameObject gameObject = Instantiate(pointPrefab, new Vector3(currentX, currentY), Quaternion.identity);
            cubeTransforms[i] = gameObject.transform;
        }

    }

    void Update()
    {
        for (int i = 0; i < resolution; i++)
        {
            Vector3 pos = cubeTransforms[i].position;
            cubeTransforms[i].position = new Vector3(pos.x, Mathf.Sin(pos.x + Time.time));
        }
    }
}
