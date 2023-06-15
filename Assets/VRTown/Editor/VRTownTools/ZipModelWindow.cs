#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO.Compression;

public class ZipModelWindow : EditorWindow
{
    string _pathZipFolder = "";
    string _pathNewZip = "";

    bool _canZip = false;

    [MenuItem("VRTown/Tools/Zip Model")]
    public static void ShowWindow()
    {
        GetWindow<ZipModelWindow>("Zip Model");
    }

    void OnGUI()
    {
        _canZip = !string.IsNullOrEmpty(_pathZipFolder) && !string.IsNullOrEmpty(_pathZipFolder);

        _pathZipFolder = EditorGUILayout.TextField("Source: ", _pathZipFolder);
        _pathNewZip = EditorGUILayout.TextField("Destination: ", _pathNewZip);

        if (GUILayout.Button("Choice Folder"))
        {
            _pathZipFolder = EditorUtility.OpenFolderPanel("Choice Zip Folder", "", "");
        }

        GUI.enabled = _canZip;
        if (GUILayout.Button("Zip Folder!"))
        {
            ZipFile.CreateFromDirectory(_pathZipFolder, _pathNewZip);
        }
    }
}
#endif