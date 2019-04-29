using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCTImage : MonoBehaviour {

    DicomFile _dicomFile;
    DicomDict gpDicomDict = new DicomDict();

    //DicomDict gpDicomDict = 

    //int _instanceNum;

    public int iNum;

    public byte[] _pixelData;

    //public ushort[] _pixelData;

    GameObject rawImage;

    string[] _dicomFileNameList;

    List<DicomFileData> _dicomFileDataList = new List<DicomFileData>();

    public RawImage _rImg;

    public struct DicomFileData
    {
        //public string _dataFileName;
        public int _instanceNum;
        //public List<DicomFileRecord> _dataRecord;
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
        // foreach filename in filenamelist 
        foreach (string sFileName in _dicomFileNameList)
        {
            dicomFileData = new DicomFileData();

            _dicomFile = new DicomFile();

            _dicomFile.Init(gpDicomDict);

            _dicomFile.Load(sFileName);

            dicomFileData._instanceNum = _dicomFile._miInstanceNum;
            //dicomFileData._dataRecord = _dicomFile._marrpRecord;
            dicomFileData._pData = _dicomFile._mpPixelData;

            _dicomFileDataList.Add(dicomFileData);
        }
    }

    public void DisplayCT()
    {

        rawImage = GameObject.Find("CTSlice");

        
        Renderer renderer = GetComponent<Renderer>();

        //_dicomFile = new DicomFile();

        //_dicomFile.Init(gpDicomDict);


        /*
        if (iNum == _dicomFileDataList[iNum]._instanceNum)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[iNum]._pData.Length - 1; i += 2)
            {
                _pixelData[i] = 0;
                _pixelData[i + 1] = _dicomFileDataList[iNum]._pData[i];
            }

            Texture2D texture = new Texture2D(512, 512, TextureFormat.R16, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            rawImage.GetComponent<RawImage>().material.mainTexture = texture;
        }

        else */

        /*
        if (iNum == _dicomFileDataList[iNum - 1]._instanceNum)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[iNum - 1]._pData.Length - 1; i += 2)
            {
                _pixelData[i] = 0;
                _pixelData[i + 1] = _dicomFileDataList[iNum - 1]._pData[i];
            }

            Texture2D texture = new Texture2D(512, 512, TextureFormat.R16, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            //rawImage.GetComponent<RawImage>().material.mainTexture = texture;
            //renderer.material.SetTexture("_MainTex", texture);
            //rawImage.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
            //rawImage.GetComponent<RawImage>().material.SetTexture("_MainTex", texture);
            rawImage.GetComponent<RawImage>().texture = texture; 
        }
        */



        /*
        else if (iNum == _dicomFileDataList[iNum + 1]._instanceNum)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[iNum + 1]._pData.Length - 1; i += 2)
            {
                _pixelData[i] = 0;
                _pixelData[i + 1] = _dicomFileDataList[iNum + 1]._pData[i];
            }

            Texture2D texture = new Texture2D(512, 512, TextureFormat.R16, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            rawImage.GetComponent<RawImage>().material.mainTexture = texture;
        }
        */

        //byte[] bytes;

        
        for (int idx = 0; idx < _dicomFileDataList.Count; idx++)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[idx]._pData.Length - 1; i += 4)
            {
                _pixelData[i] = _dicomFileDataList[idx]._pData[i];
                _pixelData[i + 1] = _dicomFileDataList[idx]._pData[i];
                _pixelData[i + 2] = _dicomFileDataList[idx]._pData[i];
                _pixelData[i + 3] = 1;
            }



            Texture2D texture = new Texture2D(512, 512, TextureFormat.RGBA32, false, true);

            texture.LoadRawTextureData(_pixelData);

            

            texture.Apply();

            //texture.ReadPixels(;
            byte[] bytes = texture.EncodeToPNG();

            rawImage.GetComponent<RawImage>().texture = texture;

            //Texture2D newTex = new Texture2D(512, 512, TextureFormat.RGBA32, false); 
            //newTex.ReadPixels(new Rect(700, 300, 512, 512), 0, 0);
            //newTex.Apply();

            //byte[] bytes = newTex.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath + string.Format("/Resources/TestDataset/IMG-0000-000{0}.png", idx + 1), bytes);
        }
        

        /*
        for (int idx = 0; idx < _dicomFileDataList.Count; idx++)
        {
            _pixelData = new byte[6 * 512 * 512];

            for (int i = 0; i < _dicomFileDataList[idx]._pData.Length - 1; i += 2)
            {
                _pixelData[i] = 0;
                _pixelData[i + 1] = _dicomFileDataList[idx]._pData[i];
            }
            

            Texture2D texture = new Texture2D(512, 512, TextureFormat.R16, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            rawImage.GetComponent<RawImage>().texture = texture;

            Texture2D newTex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            newTex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            newTex.Apply();

            byte[] bytes = newTex.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath + string.Format("/Resources/TestDataset/CTImage{0}.png", idx + 1), bytes);
        }
        */









    }

}
