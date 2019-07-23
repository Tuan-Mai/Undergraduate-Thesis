/* 
 * Display CT Image
 * Author: Tuan Mai
 * Purpose: Display CT image after reading DICOM files
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCTImage : MonoBehaviour {

    DicomFile _dicomFile;
    DicomDict gpDicomDict = new DicomDict();

    public int iNum;

    public byte[] _pixelData;

    GameObject rawImage;

    string[] _dicomFileNameList;

    List<DicomFileData> _dicomFileDataList = new List<DicomFileData>();

    public RawImage _rImg;

    public struct DicomFileData
    {
        public int _instanceNum;
        public byte[] _pData;
    }

    // Use this for initialization
    void Start () {

        iNum = 1; 

        // Load all Dicom Tags into a List to reference later
        gpDicomDict.Load("DICOM_Dictionary.txt");

        // Get file name of all Dicom Files from folder
        GetDicomFileNames();


        // Load all Dicom Files
        LoadEveryDicomFile();


        // Display the CT image
        DisplayCT();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            iNum++;
            DisplayCT();
        }
		
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            iNum--;
            DisplayCT();
        }
    }

    public void GetDicomFileNames()
    {
        // Get all the CTxxx.dcm filenames and save them in a string array

        // We will only be getting CT scans that are .dcm in our case
        //_dicomFileNameList = Directory.GetFiles("Assets/Datasets/CTDatasetTest/", "CT*.dcm");
        _dicomFileNameList = Directory.GetFiles("Assets/Datasets/CTDataset2/", "CT*.dcm");

        for (int i = 0; i < _dicomFileNameList.Length; i++)
        {
            int j = _dicomFileNameList[i].LastIndexOf('/');
            _dicomFileNameList[i] = _dicomFileNameList[i].Substring(j + 1);
        }
    }

    public void LoadEveryDicomFile()
    {

        DicomFileData dicomFileData;

        // Load each Dicom File (which will read the information)
        foreach (string sFileName in _dicomFileNameList)
        {
            dicomFileData = new DicomFileData();

            _dicomFile = new DicomFile();

            _dicomFile.Init(gpDicomDict);

            _dicomFile.Load(sFileName);

            dicomFileData._instanceNum = _dicomFile._miInstanceNum;

            dicomFileData._pData = _dicomFile._mpPixelData;

            _dicomFileDataList.Add(dicomFileData);
        }
    }

    public void DisplayCT()
    {
        // Find rawImage named CTSlice
        rawImage = GameObject.Find("CTSlice");

        Renderer renderer = GetComponent<Renderer>();

        for (int idx = 0; idx < _dicomFileDataList.Count; idx++)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[idx]._pData.Length - 1; i += 1)
            {
                _pixelData[i] = _dicomFileDataList[idx]._pData[i];
                //_pixelData[i + 1] = _dicomFileDataList[idx]._pData[i];
                //_pixelData[i + 2] = _dicomFileDataList[idx]._pData[i];
                //_pixelData[i + 3] = 0;
            }

            // Create new Unity texture
            Texture2D texture = new Texture2D(512, 512, TextureFormat.RGBA32, false, true);

            // Load pixel data into texture
            texture.LoadRawTextureData(_pixelData);

            // Apply texture to the rawImage
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();

            // Save each CT image as PNG
            File.WriteAllBytes(Application.dataPath + string.Format("/Resources/TestDataset/IMG-0000-000{0}.png", idx + 1), bytes);

            Loader loader = new Loader();
        }
    }
}
