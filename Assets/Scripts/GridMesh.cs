﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridMesh : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Vector2 minBound;
    public Vector2 maxBound;
    public float precision;
    public float scale;
    public RawImage texture;
    private int col;
    private int lin;
    private Vector3[,] meshVertices;


    private void Start()
    {
        col = Mathf.CeilToInt((maxBound.x - minBound.x) / precision) + 1;
        lin = Mathf.CeilToInt((maxBound.y - minBound.y) / precision) + 1;
        texture.texture = NoiseGenerator.GenerateTexture(1920, 1080, minBound, precision, 50);
    }

    void Update()
    {
        GenerateVertices();
        Triangulate();
    }

    private void GenerateVertices()
    {
        col = Mathf.CeilToInt((maxBound.x - minBound.x) / precision) + 1;
        lin = Mathf.CeilToInt((maxBound.y - minBound.y) / precision) + 1;

        meshVertices = new Vector3[lin, col];

        for (int y = 0; y < lin; y++)
        {
            for (int x = 0; x < col; x++)
            {
                float h = Mathf.Clamp(Mathf.PerlinNoise(x / scale, y / scale), 0, 10f);
                meshVertices[y, x] = new Vector3(minBound.x + (x * precision), h, minBound.y + (y * precision));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (meshVertices != null)
        {
            for (var i = 0; i < lin; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    Gizmos.DrawSphere(meshVertices[i, j], 0.1f);
                }
            }
        }
    }

    private void Triangulate()
    {
        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[col * lin];

        for (int y = 0; y < lin; y++)
        {
            for (int x = 0; x < col; x++)
            {
                int index = y * col + x;
                vertices[index] = meshVertices[y, x];
            }
        }

        int[] triangles = new int[col * lin * 6];
        int tris = 0;
        for (int y = 0; y < lin - 1; y++)
        {
            for (int x = 0; x < col - 1; x++)
            {
                int index = y * col + x;
                triangles[tris + 0] = index;
                triangles[tris + 1] = index + col + 1;
                triangles[tris + 2] = index + 1;
                triangles[tris + 3] = index;
                triangles[tris + 4] = index + col;
                triangles[tris + 5] = index + col + 1;
                tris += 6;
            }
        }

        m.SetVertices(vertices.ToList());
        m.SetTriangles(triangles, 0);
        m.RecalculateBounds();
        m.RecalculateNormals();

        meshFilter.mesh = m;
    }
}