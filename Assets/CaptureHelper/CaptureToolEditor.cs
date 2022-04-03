using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CustomEditor(typeof(CaptureTool))]
public class CaptureToolEditor : Editor
{
    private IEnumerator currentCaptureIE;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CaptureTool captureToolInstance = (CaptureTool)target;
        if (GUILayout.Button("Makey"))
        {
            StartCapture(captureToolInstance.Capture(Makey));
        }
    }
    private void StartCapture(IEnumerator captureIE)
    {
        currentCaptureIE = captureIE;
        EditorApplication.update += CheckIfStop;
    }
    private void CheckIfStop()
    {
        if (!currentCaptureIE.MoveNext())
        {
            EditorApplication.update -= CheckIfStop;
            currentCaptureIE = null;
        }
    }
    private void Makey(Texture2D imageOutput)
    {
        var savePath = EditorUtility.SaveFilePanel("output", "", "output", "png");
        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }
        File.WriteAllBytes(savePath, imageOutput.EncodeToPNG());
        AssetDatabase.Refresh();
    }
}
