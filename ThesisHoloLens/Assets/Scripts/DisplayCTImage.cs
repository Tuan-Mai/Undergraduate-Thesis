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

        DicomFileData dicomFileData;

        // Load each Dicom File (which will read the information)
        // foreach filename in filenamelist 
        foreach (string sFileName in _dicomFileNameList)
        {
            dicomFileData = new DicomFileData();

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
            string tempString = _dicomFileNameList[iNum];

            _dicomFile.Load(tempString);

            _pixelData = _dicomFile._mpPixelData;

            Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false, true);

            texture.LoadRawTextureData(_pixelData);

            texture.Apply();

            rawImage.GetComponent<RawImage>().material.mainTexture = texture;

        }
        */


        // TEST: Single CT image
        string tempString = "CT002002002.dcm";

        _dicomFile.Load(tempString);

        //int j = 0;


        _pixelData = new byte[20 * 512 * 512 + 1];

        
        byte[] temp = new byte[8 * 512 * 512];

        int j = 0;
        for (int i = 0; i < _dicomFile._mpPixelData.Length; i+=2)
        {
            temp[i] = 0;
            temp[i+1] = _dicomFile._mpPixelData[i];
            //temp[i+2] = _dicomFile._mpPixelData[i];
            //temp[i+3] = _dicomFile._mpPixelData[i];
            //j++;
        }



        //int j = 0; 

        byte[] _newPixelData = new byte[20 * 512 * 512];

        for (int i = 0; i < temp.Length; i+=4)
        {
            _pixelData[i] = 0;
            _pixelData[i+1] = (byte)(temp[i]);
            _pixelData[i+2] = (byte)(temp[i]);
            _pixelData[i+3] = (byte)(temp[i]);
        }

        for (int i = 0; i < _pixelData.Length - 1; i+=3)
        {
            _newPixelData[i] = (byte)((_pixelData[i] + _pixelData[i + 1] + _pixelData[i + 2])/3); 
        }



        /*
        ushort temp;
        //temp = new 
        byte temp1;
        byte[] _pixelDataTemp = new byte[512 * 512 + 1];

        for (int i = 0; i < _dicomFile._mpPixelData.Length - 1; i+=2)
        {
            temp = (ushort)(_dicomFile._mpPixelData[i] | _dicomFile._mpPixelData[i+1] >> 8);

            temp1 = (byte)(temp * 255 / 65535);

            _pixelDataTemp[i % 2] = temp1;
        }

        for (int i = 0; i < 3 * 512 * 512 - 1; i+=3)
        {
            temp1 = _pixelDataTemp[i % 3];

            _pixelData[i] = temp1;
            _pixelData[i+1] = temp1;
            _pixelData[i+2] = temp1;
        }
        */

        byte[] alphaByte = new byte[8 * 512 * 512];
       
        for (int i = 0; i < _dicomFile._mpPixelData.Length; i+=2)
        {
            //alphaByte[i] = 0;
            alphaByte[i] = _dicomFile._mpPixelData[i];
            alphaByte[i+1] = _dicomFile._mpPixelData[i];

        }


        
        //_pixelData = _dicomFile._mpPixelData;

        Texture2D texture = new Texture2D(512, 512, TextureFormat.R16, false, true);

        /*
        var colors = new Color32[_dicomFile._mpPixelData.Length/2];
        //int r = color.r;
        //int g = color.g;
        //int b = color.b;

        //int rgb = ((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3);

        //bytes[0] = (byte)rgb;
        //bytes[1] = (byte)(rgb >> 8);

        for (int i = 0; i < _dicomFile._mpPixelData.Length; i += 2)
        {
            colors[i / 2] = new Color32((byte)(_dicomFile._mpPixelData[i] & amp; 0xF8),
            (byte)(((rgb565Data[i] & amp; 7) &lt; &lt; 5) | ((rgb565Data[i + 1] & amp; 0xE0) >> 3)),
            (byte)((rgb565Data[i + 1] & amp; 0x1F) &lt; &lt; 3),
            (byte)1);
    }
    */
    

        //texture.LoadRawTextureData(_pixelData);
        texture.LoadRawTextureData(temp);
        //texture.LoadRawTextureData(alphaByte);
        //texture.LoadRawTextureData(_newPixelData);
        //texture.LoadRawTextureData(_dicomFile._mpPixelData);

        texture.Apply();

        //var pixelColor = texture.GetPixels32();

        /*
        Color32[] pixels = texture.GetPixels32();
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color32 pixel = pixels[x + y * texture.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, 1);
                texture.SetPixel(x, y, c);
                
            }
        }
        texture.Apply();
        */


        //RenderTexture rTex = null;

        //Graphics.Blit(texture, rTex);

        rawImage.GetComponent<RawImage>().material.mainTexture = texture;
        

        //int conversion = (_dicomFile._mpPixelData.Length - 1 / 65535) * 255;
        //_pixelData = _dicomFile._mpPixelData;

        //_pixelData = new byte[2 * 512 * 512 + 1];
        //_pixelData = _dicomFileDataList[0]._pData;

        //int j = 0;

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
