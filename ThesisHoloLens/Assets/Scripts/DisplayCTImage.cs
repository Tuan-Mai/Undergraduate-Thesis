using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCTImage : MonoBehaviour {

    DicomFile _dicomFile; 

    byte[] _pixelData;

    GameObject rawImage;

	// Use this for initialization
	void Start () {

        rawImage = GameObject.Find("CTSlice");

        _dicomFile = new DicomFile();

        _dicomFile.Start();


        //int conversion = (_dicomFile._mpPixelData.Length - 1 / 65535) * 255;
        //_pixelData = _dicomFile._mpPixelData;

        
        _pixelData = new byte[4*512*512+1];

        int j = 0;

        for (int i = 0; i < 2*512*512; i++)
        {


            byte temp;

            
            //j = i/2;

            temp = _dicomFile._mpPixelData[i];

            //byte temp1 = (byte)(temp * 255.0 / 65535.0);

            _pixelData[j] = temp;

            _pixelData[j+1] = temp;
            //_pixelData[j+2] = temp;


            j += 2;
        }
        

        //System.Buffer.BlockCopy(_dicomFile._mpPixelData, 0, _pixelData, 0, 1024);

        //Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB565, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.R8, true, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.DXT5, false);

        // Best one for Dataset1 , but has colour. Need gray
        Texture2D texture = new Texture2D(512, 512, TextureFormat.RGHalf, false, true);
        
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.ETC2_RGBA1, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false);
        //Texture2D texture = new Texture2D(512, 512, TextureFormat.Alpha8, false);

        texture.LoadRawTextureData(_pixelData);

        texture.Apply();

        //Graphics.DrawTexture(new Rect(0, 0, 512, 512), texture);

        rawImage.GetComponent<RawImage>().material.mainTexture = texture;

      

        //rawImage.GetComponent<RawImage>().texture = texture;
        //GetComponent<>().material.mainTexture = texture;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
