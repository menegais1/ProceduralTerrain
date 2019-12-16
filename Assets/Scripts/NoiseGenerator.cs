using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class NoiseGenerator
{
    public static RenderTexture GenerateTexture(int width, int height, Vector3 minBound, float precision,
        int pointNumber)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Point;
        rt.Create();
        ComputeShader compute = (ComputeShader) Resources.Load("NoiseGeneratorCompute");


        int kernel = compute.FindKernel("GenerateNoise");

        Vector3[] pointsList = new Vector3[pointNumber];

        for (int i = 0; i < pointsList.Length; i++)
        {
            pointsList[i] = new Vector3(Random.Range(0, width - 1), Random.Range(0, height - 1), 0);
        }

        ComputeBuffer points = new ComputeBuffer(pointNumber, 3 * sizeof(float));
        points.SetData(pointsList);

        compute.SetTexture(kernel, "result", rt);
        compute.SetBuffer(kernel, "points", points);
        compute.SetInt("pointsCount", pointsList.Length);
        compute.SetVector("minBound", minBound);
        compute.SetFloat("precision", precision);
        compute.Dispatch(kernel, Mathf.CeilToInt(rt.width / (float) 16), Mathf.CeilToInt(rt.height / (float) 16),
            1);

        return rt;
    }

//    public void SaveTexture()
//    {
//        byte[] bytes = toTexture2D(rt).EncodeToPNG();
//        System.IO.File.WriteAllBytes("WallPaper.png", bytes);
//    }
//
//    Texture2D toTexture2D(RenderTexture rTex)
//    {
//        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
//        RenderTexture.active = rTex;
//        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
//        tex.Apply();
//        return tex;
//    }
}