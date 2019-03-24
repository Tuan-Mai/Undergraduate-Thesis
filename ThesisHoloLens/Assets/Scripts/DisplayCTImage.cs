using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCTImage : MonoBehaviour {

    DicomFile _dicomFile;
    DicomDict gpDicomDict = new DicomDict();

    //int _instanceNum;

    public int iNum;

    public byte[] _pixelData;

    GameObject rawImage;

    string[] _dicomFileNameList;

    List<DicomFileData> _dicomFileDataList = new List<DicomFileData>();



    public struct DicomFileData
    {
        //public string _dataFileName;
        public int _instanceNum;
        //public List<DicomFileRecord> _dataRecord;
        public byte[] _pData;
    }

    // Use this for initialization
    void Start () {

        _dicomFile = new DicomFile();
        

        // Load all Dicom Tags into a List to reference later
        gpDicomDict.Load("DICOM_Dictionary.txt");

        _dicomFile.Init(gpDicomDict);

        // Get file name of all Dicom Files from folder
        GetDicomFileNames();


        // Load all Dicom Files
        //LoadEveryDicomFile();


        // Display the CT image
        DisplayCT();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GetDicomFileNames()
    {
        // Get all the CTxxx.dcm filenames and save them in a string array

        // We will only be getting CT scans that are .dcm in our case
        _dicomFileNameList = Directory.GetFiles("Assets/Datasets/CTDataset2/", "CT*.dcm");

        for (int i = 0; i < _dicomFileNameList.Length; i++)
        {
            int j = _dicomFileNameList[i].LastIndexOf('/');
            _dicomFileNameList[i] = _dicomFileNameList[i].Substring(j + 1);
        }
    }

    public void LoadEveryDicomFile()
    {
        


        // Load each Dicom File (which will read the information)
        // foreach filename in filenamelist 
        foreach (string sFileName in _dicomFileNameList)
        {
            DicomFileData dicomFileData = new DicomFileData();
            
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

        //_dicomFile = new DicomFile();

        //_dicomFile.Init(gpDicomDict);

        iNum = 0;

        /*
        while (iNum <= _dicomFileNameList.Length + 1)
        {
            //string tempString = _dicomFileNameList[iNum];
            string tempString = "CT002002002.dcm";

            _dicomFile.Load(tempString);

            _pixelData = _dicomFile._mpPixelData;

            Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            rawImage.GetComponent<RawImage>().material.mainTexture = texture;

        }
        */

        string tempString = "CT002002002.dcm";

        _dicomFile.Load(tempString);

        _pixelData = _dicomFile._mpPixelData;

        Texture2D texture = new Texture2D(512, 512, TextureFormat.RG16, false, true);

        texture.LoadRawTextureData(_pixelData);

        texture.Apply();

        rawImage.GetComponent<RawImage>().material.mainTexture = texture;


        //int conversion = (_dicomFile._mpPixelData.Length - 1 / 65535) * 255;
        //_pixelData = _dicomFile._mpPixelData;

        //_pixelData = new byte[2 * 512 * 512 + 1];
        //_pixelData = _dicomFileDataList[0]._pData;

        int j = 0;

        /*
        for (int i = 0; i < 2 * 512 * 512; i++)
        {


            byte temp;


            //j = i/2;
            
            temp = _dicomFileDataList[0]._pData[i];
            //temp = _dicomFile._mpPixelData[i];

            //byte temp1 = (byte)(temp * 255.0 / 65535.0);

            _pixelData[j] = temp;

            //_pixelData[j + 1] = temp;
            //_pixelData[j + 2] = temp;
            //_pixelData[j + 3] = temp;


            j += 1;
        }
        */

        //System.Buffer.BlockCopy(_dicomFile._mpPixelData, 0, _pixelData, 0, 1024);

        //Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB565, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.R8, true, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.DXT5, false);

        /*
        // Best one for Dataset1 , but has colour. Need gray
        Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false, true);

        texture.LoadRawTextureData(_pixelData);

        texture.Apply();

        rawImage.GetComponent<RawImage>().material.mainTexture = texture;
        */
        

        //while (_dicomFileDataList)
        /*
        foreach (DicomFileData fileData in _dicomFileDataList)
        {

        }
        */
        
        

        
    }

}
