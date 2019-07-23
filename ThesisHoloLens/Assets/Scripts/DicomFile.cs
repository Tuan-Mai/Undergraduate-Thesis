/* 
 * Dicom File 
 * Author: Tuan Mai
 * Purpose: Handles the reading of DICOM Files, and saves the data
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Globalization;

public class DicomFile : MonoBehaviour
{
    DicomDict _gpDicomDict;

    private int _miDicomFileType;

    public int _miInstanceNum;     // instance number

    // (0008, 0016)	UI	SOP Class UID
    // (0008, 0018)	UI	SOP Instance UID
    private string _msSOPClassUID;
    private string _msSOPInstanceUID;

    // Image Rows		(0028, 0010)
    // Image Columns	(0028, 0011)
    private int _miImgRow;
    private int _miImgCol;

    // Patient ID (0010, 0020)
    private string _msPatientID;

    // Patient Name (0010,0010)
    private string _msPatientNameFirst;
    private string _msPatientNameLast;

    // Patient Sex (0010, 0040)
    private string _msPatientSex;

    // image position (patient) tag (0x0020, 0x0032)
    private double[] _mdImgPos = new double[3];  // 0:X  1:Y  2:Z

    // image XY pixel spacing tag (0x0028, 0x0030)  // unit mm
    private double _mdImgXPixelSpacing;
    private double _mdImgYPixelSpacing;


    private double _mdZdistance;
    // Slice thickness tag (0x0018, 0x0050)  // unit mm
    private double _mdSliceThicknessOriginal;

    // Slice thickness calculated by image z Position
    private double _mdSliceThickness;

    // Slice Location tag (0x0020, 0x1041)
    private double _mdSliceLocation;

    // window width and window center 
    private double _mdWinCenter;    // (0x0028, 0x1050)
    private double _mdWinWidth;     // (0x0028, 0x1051)

    // CT Pixel Data length and pointer (7FE0,0010)
    private ulong _mulPixelDataLen;
    public byte[] _mpPixelData;

    // Create a list of DICOM file record
    private List<DicomFileRecord> _marrpRecord = new List<DicomFileRecord>();

    public string _msFileName;

    private bool _mbExplicitVR;


    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(DicomDict gpDicomDict)
    {
        _gpDicomDict = gpDicomDict;

        _miDicomFileType = 0;
        _miInstanceNum = 0;

        _msSOPClassUID = null;//Empty();
        _msSOPInstanceUID = null;//Empty();

        _miImgRow = 0;
        _miImgCol = 0;

        _mdImgPos[0] = _mdImgPos[1] = _mdImgPos[2] = 0;  // 0:X  1:Y  2:Z
        _mdSliceThicknessOriginal = 0;
        _mdSliceThickness = 0;
        _mdSliceLocation = 0;


        _mdImgXPixelSpacing = 0;
        _mdImgYPixelSpacing = 0;

        _msPatientID = null; //Empty();



        _mulPixelDataLen = 0;

        _msFileName = null; //Empty();

    }

    // Load in the DICOM file by searching for the filename that was passed in
    public bool Load(string sFileName)
    {
        _msFileName = sFileName;

        // TEST: Hardcode filenames
        //_msFileName = "CT002000025.dcm";
        //_msFileName = "CT002002002.dcm";

        //string path = "Assets/Datasets/CTDatasetTest/" + _msFileName;
        string path = "Assets/Datasets/CTDataset2/" + _msFileName;

        if (!File.Exists(path))
        {
            return false;
        }


        // Use Binary Reader to read in the file
        using (BinaryReader fs = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        {

            // Remove the file extension
            string sFileType = _msFileName.Substring(_msFileName.Length - 3, 3);

            if (sFileType == "dcm")
            {
                // Skip first 128 + 4 bytes (DICOM file Header, not used in our application)
                fs.BaseStream.Seek(132, SeekOrigin.Begin);

                // Take DICOM file as explicit VR by default
                _mbExplicitVR = true;
            }
            else
            {
                // img file is always implicit VR
                _mbExplicitVR = false;
            }


            // Read all records until end
            bool bReadRecordOK = true;
            DicomFileRecord pRecord;

           
            while ((fs.BaseStream.Position < fs.BaseStream.Length) && bReadRecordOK)
            {
                // Add record to the record list
                pRecord = new DicomFileRecord();
                _marrpRecord.Add(pRecord);

                bReadRecordOK = ReadRecord(fs, pRecord);

            }


            fs.Dispose();


            // Switch case 3
            GetDicomFileCTInfo();

            return true;
        }
    }

    // Read the data from the DICOM file (pass in the binary reader and current record)
    public bool ReadRecord(BinaryReader br, DicomFileRecord pRecord)
    {
        // Local variables for tags
        byte[] szTag = new byte[4];
        byte[] szVR = new byte[2];
        byte[] pData;

  
        // Set the position of the tag 
        pRecord._miFilePos = br.BaseStream.Position;

        // Read group and element 
        // group		- 2 bytes 
        // element		- 2 bytes 
        int i;

        if ((br.Read(szTag, 0, 4)) != 4)
        {
            if (br.BaseStream.Position == br.BaseStream.Length)
            {
                return true;
            }

            return false;
        }

        // Convert the byte data into unsigned int
        pRecord._musGrp = BitConverter.ToUInt16(szTag, 0);
        pRecord._musEle = BitConverter.ToUInt16(szTag, 2);
        pRecord._musTagLen = 4;

        
        if (pRecord._musGrp != Convert.ToUInt16("0x0002", 16) && _mbExplicitVR == false)
        {
            // If group != 0002 and is not explixit VR, data length is 4 bytes

            byte[] tmpByte = new byte[4];
            i = br.Read(tmpByte, 0, 4);

            if (i != 4)
            {
                return false;
            }

            pRecord._mulLen = BitConverter.ToUInt32(tmpByte, 0);

            pRecord._musTagLen += 4;

        }



        else
        {
          // group 0x0002 is always explicit VR
          // Read VR
            if (br.Read(szVR, 0, 2) != 2)
            {
                return false;
            }

            pRecord._musTagLen += 2;

            pRecord._msVR = System.Text.Encoding.UTF8.GetString(szVR);

            // Read data length

            if (pRecord._msVR.Equals("OB") ||
                pRecord._msVR.Equals("OW") ||
                pRecord._msVR.Equals("OF") ||
                pRecord._msVR.Equals("SQ") ||
                pRecord._msVR.Equals("UT") ||
                pRecord._msVR.Equals("UN"))
            {
                // Skip unused 2 bytes
                br.BaseStream.Seek(2, SeekOrigin.Current);
                pRecord._musTagLen += 2;

                byte[] tempByte = new byte[4];

                // Read data length as unsigned long 
                i = br.Read(tempByte, 0, 4);

                if (i != 4)
                {
                    // Failed to read Dicom tag length
                    return false;
                }

                pRecord._mulLen = BitConverter.ToUInt32(tempByte, 0);

                pRecord._musTagLen += 4;
            }

            else
            {
                // Read data length as unsigned short 
                ushort usLen = 0;


                byte[] tempByte = new byte[2];
                i = br.Read(tempByte, 0, 2);

                if (i != 2)
                {
                    return false;
                }

                usLen = BitConverter.ToUInt16(tempByte, 0);

                pRecord._mulLen = usLen;
                pRecord._musTagLen += 2;
            }

        }


        // Even groups are DICOM standard group
        // Odd groups are private groups 

        // Find the record from Dicom dictionary
        DicomDictRecord pDictRecord = new DicomDictRecord();
        
        if (pRecord._musGrp % 2 == 0)
        {
            pDictRecord = _gpDicomDict.Find(pRecord._musGrp, pRecord._musEle);
        }


        if (pDictRecord._isNotNull == true)
        {
            // Set record name and VR 
            pRecord._msName = pDictRecord._msName;
            pRecord._msVR = pDictRecord._msVR;


            if (pDictRecord._msVR == "SQ" || pDictRecord._msVR == "??" || pRecord._mulLen == 0)
            {
                pRecord._mpData = null;
                return true;
            }
        }
        

        // Set data length to zero if length is 0xFFFFFFFF (undefined length please see DICOM standard)
        if (pRecord._mulLen == Convert.ToUInt64("0xFFFFFFFF", 16))
        {
            pRecord._mulLen = 0;
            pRecord._mpData = null;
            return true;
        }

        // Read the data
        pData = new byte[pRecord._mulLen + 1];

        if (pData == null)
        {
            return false;
        }


        if (br.Read(pData, 0, (int)pRecord._mulLen) != (int)pRecord._mulLen)
        {
            // Failed to read Dicom
            return false;
        }

        // Set data
        pRecord._mpData = pData;
        string mpDataString = Encoding.UTF8.GetString(pRecord._mpData);

        pData[pRecord._mulLen] = 0;

        // Check for Explict VR or Implicit VR
        if (pRecord._musGrp == Convert.ToUInt16("0x0002", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
        {
            string dataString = Encoding.UTF8.GetString(pRecord._mpData);

            if (dataString.Equals("1.2.840.10008.1.2\0\0"))
            {
                _mbExplicitVR = false;
            }
            else
            {  // 1.2.840.10008.1.2.1 is Explicit VR
                _mbExplicitVR = true;
            }
        }

        return true;
    }

    // Get and set data retrieved from DICOM files
    public void GetDicomFileCTInfo()
    {
        int i, k;

        DicomFileRecord pRecord;

        for (i = 0; i < _marrpRecord.Count; i++)
        {
            pRecord = _marrpRecord[i];

            // Instance number (image number) 
            if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x0013", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _miInstanceNum = Convert.ToInt16(tempString);
            }

            // (0008, 0016)	UI	SOP Class UID 
            else if (pRecord._musGrp == Convert.ToUInt16("0x0008", 16) && pRecord._musEle == Convert.ToUInt16("0x0016", 16))
            {
                _msSOPClassUID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
            }

            // (0008, 0018)	UI	SOP Instance UID    
            else if (pRecord._musGrp == Convert.ToUInt16("0x0008", 16) && pRecord._musEle == Convert.ToUInt16("0x0018", 16))
            {
                _msSOPInstanceUID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
            }

            // (0010, 0010)	Patient Name    
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {

                // The format of the patient name is last name + '^' + first name + '^'
                string sPatientName;

                sPatientName = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
                int iPos = sPatientName.IndexOf('^');

                if (iPos != -1)
                {
                    _msPatientNameLast = sPatientName.Substring(0, iPos);
                    _msPatientNameFirst = sPatientName.Substring(iPos + 1);
                }

                else
                {
                    // This case will not happen by dicom standard
                    _msPatientNameFirst = sPatientName;
                    _msPatientNameLast = sPatientName;
                }

                _msPatientNameFirst.TrimEnd();
                _msPatientNameLast.TrimEnd();
            }

            // (0010, 0020)	Patient ID  
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0020", 16))
            {
                _msPatientID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
            }

            // (0010, 0040)	Patient Sex 
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0040", 16))
            {
                _msPatientSex = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0').TrimEnd();
            }

            // (0028, 0010)	Image Rows (width)  
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {
                _miImgRow = BitConverter.ToInt16(pRecord._mpData, 0);
            }

            // Image Columns	(0028, 0011)   
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0011", 16))
            {
                _miImgCol = BitConverter.ToInt16(pRecord._mpData, 0);
            }

            // Image position   
            else if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x0032", 16))
            {
                // Reads double 
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                var positions = tempString.Split('\\');

                _mdImgPos[0] = Convert.ToDouble(positions[0]);
                _mdImgPos[1] = Convert.ToDouble(positions[1]);
                _mdImgPos[2] = Convert.ToDouble(positions[2]);

                k = 3;

                Debug.Assert(k == 3);
            }

            // Slice location   
            else if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x1041", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _mdSliceLocation = Convert.ToDouble(tempString);

                k = 1;

                Debug.Assert(k == 1);
            }

            // Slice thinkness  
            else if (pRecord._musGrp == Convert.ToUInt16("0x0018", 16) && pRecord._musEle == Convert.ToUInt16("0x0050", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _mdSliceThickness = Convert.ToDouble(tempString);

                _mdSliceThicknessOriginal = _mdSliceThickness;
                _mdZdistance = _mdSliceThickness;

                k = 1;

                Debug.Assert(k == 1);
            }

            // Image XY pixel spacing   
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0030", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                var positions = tempString.Split('\\');

                _mdImgXPixelSpacing = Convert.ToDouble(positions[0]);
                _mdImgYPixelSpacing = Convert.ToDouble(positions[1]);

                k = 2;

                Debug.Assert(k == 2);
            }

            // Pixel data (7FE0,0010)
            else if (pRecord._musGrp == Convert.ToUInt16("0x7FE0", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                // Set the pixel data length and pointer for reference convience
                _mulPixelDataLen = pRecord._mulLen;
                _mpPixelData = pRecord._mpData;
            }
        }
    }
}