using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureTool : MonoBehaviour
{
    public bool isNormal;
    public GameObject model;
    public AnimationClip anim;
    public List<Material> shaderAffected = new List<Material>();
    public int perSec = 25;
    public Vector2Int sizeOfImage = new Vector2Int(500, 500);
    private int curFrame = 0;
    public Camera mainCamera = null;

    public IEnumerator Capture(Action<Texture2D> onComplete)
    {
        int frameCount = (int)(anim.length * perSec);
        int perLine = PerLine(frameCount);
        Vector2Int totalSize = new Vector2Int(sizeOfImage.x * perLine, sizeOfImage.y * perLine);
        Vector2Int startPosition = new Vector2Int(0, totalSize.y - sizeOfImage.y);

        Texture2D outputImage = new Texture2D(totalSize.x, totalSize.y, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        CleanSlate(outputImage);

        RenderTexture frameImage = new RenderTexture(sizeOfImage.x, sizeOfImage.y, 24, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1,
            hideFlags = HideFlags.HideAndDontSave
        };
        mainCamera.targetTexture = frameImage;
        try
        {
            for (curFrame = 0; curFrame < frameCount; curFrame++)
            {
                var currentTime = (curFrame / (float)frameCount) * anim.length;
                GetFrame(currentTime);
                yield return null;

                if (isNormal)
                {
                    mainCamera.backgroundColor = new Color(0.5f, 0.5f, 1.0f, 0.0f);
                }
                else
                {
                    mainCamera.backgroundColor = Color.clear;
                }
                mainCamera.Render();
                Graphics.SetRenderTarget(frameImage);
                outputImage.ReadPixels(new Rect(0, 0, frameImage.width, frameImage.height), startPosition.x, startPosition.y);
                outputImage.Apply();

                startPosition.x += sizeOfImage.x;

                if ((curFrame + 1) % perLine == 0)
                {
                    startPosition.x = 0;
                    startPosition.y -= sizeOfImage.y;
                }
            }
        }
        finally
        {
            onComplete.Invoke(outputImage);
            Graphics.SetRenderTarget(null);
            mainCamera.targetTexture = null;
            DestroyImmediate(frameImage);
        }
    }
    private int PerLine(int i)
    {
        return Mathf.CeilToInt(Mathf.Sqrt(i));
    }
    public void GetFrame(float t)
    {
        if (!model || !anim || !mainCamera)
        {
            Debug.Log("Missing Something?");
            return;
        }
        else
        {
            anim.SampleAnimation(model, t);
            foreach (Material m in shaderAffected)
            {
                m.SetFloat("_Value", t);
            }
        }
    }
    private void CleanSlate(Texture2D tex)
    {
        Color[] colArray = new Color[tex.width * tex.height];
        for (int i = 0; i < colArray.Length; i++)
        {
            colArray[i] = Color.clear;
        }
        tex.SetPixels(colArray);
        tex.Apply();
    }
}
