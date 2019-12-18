using System;
using System.Collections.Generic;
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

    //Grid debug
    public int gridSubdivisions;


    private int col;
    private int lin;
    private Vector3[,] meshVertices;


    private void Start()
    {
        col = Mathf.CeilToInt((maxBound.x - minBound.x) / precision) + 1;
        lin = Mathf.CeilToInt((maxBound.y - minBound.y) / precision) + 1;
        //  texture.texture = NoiseGenerator.GenerateTexture(1920, 1080, minBound, precision, 50);
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

        if (grid != null)
        {
            for (var i = 0; i < grid.Count; i++)
            {
                Gizmos.DrawWireCube(grid[i].bounds.center, grid[i].bounds.size);
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

        VertexClusteringMeshSimplification(m, gridSubdivisions);
    }


    public class Vertex
    {
        public Vector3 vertex;
        public List<Vertex> adjacent;
        public int cluster;

        public Vertex()
        {
            adjacent = new List<Vertex>();
            cluster = -1;
        }

        public static void AddAdjacent(Vertex v1, Vertex v2)
        {
            v1.adjacent.Add(v2);
            v2.adjacent.Add(v1);
        }
    }

    public class AdjacencyMatrix
    {
        public List<Vertex> vertices;
        public int vertexNumber;
        public Bounds bounds;

        public AdjacencyMatrix(Mesh mesh)
        {
            vertexNumber = mesh.vertices.Length;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            bounds = mesh.bounds;

            this.vertices = new List<Vertex>(vertexNumber);
            for (int i = 0; i < vertexNumber; i++)
            {
                this.vertices.Add(new Vertex {vertex = vertices[i]});
            }

            for (int i = 0; i < triangles.Length - 3; i += 3)
            {
                Vertex v1 = this.vertices[triangles[i + 0]];
                Vertex v2 = this.vertices[triangles[i + 1]];
                Vertex v3 = this.vertices[triangles[i + 2]];
                Vertex.AddAdjacent(v1, v2);
                Vertex.AddAdjacent(v1, v3);
                Vertex.AddAdjacent(v2, v3);
            }
        }
    }

    public class GridCell
    {
        public Bounds bounds;
        public int cluster;
        public Vertex representative;
        public List<Vertex> clusterized;

        public GridCell(Bounds bounds, int cluster)
        {
            clusterized = new List<Vertex>();
            this.bounds = bounds;
            this.cluster = cluster;
        }
    }


    public List<GridCell> grid;

    public void VertexClusteringMeshSimplification(Mesh mesh, int gridSubdivisions)
    {
        AdjacencyMatrix matrix = new AdjacencyMatrix(mesh);

        grid = new List<GridCell>(gridSubdivisions);
        Vector3 gridCellExtent = matrix.bounds.size / gridSubdivisions;
        for (int i = 0; i < gridSubdivisions; i++)
        for (int j = 0; j < gridSubdivisions; j++)
        for (int k = 0; k < gridSubdivisions; k++)
            grid.Add(new GridCell(
                new Bounds(
                    matrix.bounds.min + new Vector3(gridCellExtent.x * i, gridCellExtent.y * j, gridCellExtent.z * k) +
                    gridCellExtent / 2,
                    gridCellExtent), k));


        for (var i = 0; i < grid.Count; i++)
        {
            for (var i1 = 0; i1 < matrix.vertices.Count; i1++)
            {
                if (grid[i].bounds.Contains(matrix.vertices[i].vertex))
                {
                    matrix.vertices[i].cluster = grid[i].cluster;
                    grid[i].clusterized.Add(matrix.vertices[i]);
                }
            }
        }
    }
}