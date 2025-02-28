﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Diagnostics;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Editor window for importing datasets.
    /// </summary>
    public class RAWDatasetImporterEditorWindow : EditorWindow
    {
        private string fileToImport;

        private int dimX;
        private int dimY;
        private int dimZ;
        private int bytesToSkip = 0;
        private DataContentFormat dataFormat = DataContentFormat.Int16;
        private Endianness endianness = Endianness.LittleEndian;

        public RAWDatasetImporterEditorWindow(string filePath)
        {
            fileToImport = filePath;

            if (Path.GetExtension(fileToImport) == ".ini")
                fileToImport = fileToImport.Replace(".ini", ".raw");

            // Try parse ini file (if available)
            DatasetIniData initData = DatasetIniReader.ParseIniFile(fileToImport + ".ini");
            if (initData != null)
            {
                dimX = initData.dimX;
                dimY = initData.dimY;
                dimZ = initData.dimZ;
                bytesToSkip = initData.bytesToSkip;
                dataFormat = initData.format;
                endianness = initData.endianness;
            }

            this.minSize = new Vector2(300.0f, 200.0f);
        }

        private void ImportDataset()
        {
            RawDatasetImporter importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
            VolumeDataset dataset = importer.Import();

            if (dataset != null)
            {
                if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                {
                    if (EditorUtility.DisplayDialog("Optional DownScaling",
                        $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                    {
                        dataset.DownScaleData();
                    }
                }
                VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                
                // Calculate Iso-surface Similarity
                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                //VolumeObjectFactory.CreateIsosurfacePair(dataset); // Compare Isosurface Pair

                //sw.Stop();
                //TimeSpan ts2 = sw.Elapsed;
                //UnityEngine.Debug.Log("total time (ms) : " + ts2.TotalMilliseconds);
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to import datset");
            }

            this.Close();
        }

        private void OnGUI()
        {
            dimX = EditorGUILayout.IntField("X dimension", dimX);
            dimY = EditorGUILayout.IntField("Y dimension", dimY);
            dimZ = EditorGUILayout.IntField("Z dimension", dimZ);
            bytesToSkip = EditorGUILayout.IntField("Bytes to skip", bytesToSkip);
            dataFormat = (DataContentFormat)EditorGUILayout.EnumPopup("Data format", dataFormat);
            endianness = (Endianness)EditorGUILayout.EnumPopup("Endianness", endianness);

            if (GUILayout.Button("Import"))
                ImportDataset();

            if (GUILayout.Button("Cancel"))
                this.Close();
        }
    }
}
