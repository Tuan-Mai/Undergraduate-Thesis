using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Globalization;

public class DicomFile : MonoBehaviour
{

    // Feb 17, 2019
    //DicomDict _gpDicomDict = new DicomDict();
    DicomDict _gpDicomDict;

    bool _mbInterpolationFileFlag;
    // POI/ROI/RTPLAN/CT
    int _miDicomFileType;

    // Instance number for Dicom file
    // for CT:     set it to the value of tag (0x0020,0x0013)
    // for POI:    set it to BBT_INSTANCE_NUM_POI
    // for ROI:    set it to BBT_INSTANCE_NUM_ROI
    // for RTPLAN: set it to BBT_INSTANCE_NUM_RTPLAN
    public int _miInstanceNum;     // instance number

    // (0008, 0016)	UI	SOP Class UID
    // (0008, 0018)	UI	SOP Instance UID
    string _msSOPClassUID;
    string _msSOPInstanceUID;

    // Image Rows		(0028, 0010)
    // Image Columns	(0028, 0011)
    int _miImgRow;
    int _miImgCol;

    // Patient ID (0010, 0020)
    string _msPatientID;

    // Patient Name (0010,0010)
    string _msPatientNameFirst;
    string _msPatientNameLast;

    // Patient Sex (0010, 0040)
    string _msPatientSex;

    // image position (patient) tag (0x0020, 0x0032)
    double[] _mdImgPos = new double[3];  // 0:X  1:Y  2:Z

    // image XY pixel spacing tag (0x0028, 0x0030)  // unit mm
    double _mdImgXPixelSpacing;
    double _mdImgYPixelSpacing;
    // add on oct 29 
    double _mdZdistance;
    // Slice thickness tag (0x0018, 0x0050)  // unit mm
    double _mdSliceThicknessOriginal;

    // Slice thickness calculated by image z Position
    double _mdSliceThickness;

    // Slice Location tag (0x0020, 0x1041)
    double _mdSliceLocation;

    // window width and window center 
    double _mdWinCenter;    // (0x0028, 0x1050)
    double _mdWinWidth;     // (0x0028, 0x1051)



    // CT Pixel Data length and pointer (7FE0,0010)
    ulong _mulPixelDataLen;
    public byte[] _mpPixelData;

    double _mpEdgeDataSobel;
    byte[] _mpEdgeData;

    //CArray<DicomFileRecord*, DicomFileRecord*> _marrpRecord;
    public List<DicomFileRecord> _marrpRecord = new List<DicomFileRecord>();

    

    //double _mulPixelDataLen;
    public string _msFileName;

    bool _mbExplicitVR; 

    string tempString = "";



    



    // Start is called before the first frame update
    public void Start()
    {
        

        //Init();
        //Load();
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

        _msPatientID = null;//Empty();



        _mulPixelDataLen = 0;

        _msFileName = null;//Empty();


        // Load all Dicom Tags into a List to reference later
        //_gpDicomDict.Load("DICOM_Dictionary.txt");


        //Load();

        // Load each Dicom File (which will read the information)
        // foreach filename in filenamelist 

        /*
        foreach (string sFileName in _dicomFileNameList)
        {
            DicomFileData dicomFileData = new DicomFileData();
            
            Load(sFileName);

            dicomFileData._dataFileName = sFileName;
            dicomFileData._dataRecord = _marrpRecord;

            _dicomFileDataList.Add(dicomFileData);
        }
        */

    }


    //public bool Load(string sFileName)
    public bool Load(string sFileName)
    {
        _msFileName = sFileName;
        //_msFileName = "CT002000025.dcm";
        //_msFileName = "CT002002002.dcm";

        //TODO: set filename to the end of path 
        string path = "Assets/Datasets/CTDataset2/" + _msFileName;


        if (!File.Exists(path))
        {
            return false;
        }

        //_gpDicomDict.Load(_msFileName);

        
        using (BinaryReader fs = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        {

            string sFileType = _msFileName.Substring(_msFileName.Length - 3, 3);
            //sFileType.ToUpper();

            if (sFileType == "dcm")
            {
                // skip first 128 + 4 bytes
                fs.BaseStream.Seek(132, SeekOrigin.Begin);

                // take DICOM file as explicit VR by default
                _mbExplicitVR = true;
            }

            byte[] tempByte = new byte[fs.BaseStream.Length];
            fs.Read(tempByte, 0, tempByte.Length);
            tempString = System.Text.Encoding.UTF8.GetString(tempByte, 0, tempByte.Length);

            fs.Close();
        }
        

        //using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open)))
        using (BinaryReader fs = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        {

            string sFileType = _msFileName.Substring(_msFileName.Length - 3, 3);
            //sFileType.ToUpper();

            if (sFileType == "dcm")
            {
                // skip first 128 + 4 bytes
                fs.BaseStream.Seek(132, SeekOrigin.Begin);

                // take DICOM file as explicit VR by default
                _mbExplicitVR = true;
            }
            else
            {
                // img file is always implicit VR
                _mbExplicitVR = false;
            }

            

            // read all records until end
            bool bReadRecordOK = true;
            DicomFileRecord pRecord;

            
           
            while ((fs.BaseStream.Position < fs.BaseStream.Length) && bReadRecordOK)
            {
                //fs.Read(fileData, 0, bytesToRead);
                // add record to the record list
                pRecord = new DicomFileRecord();
                _marrpRecord.Add(pRecord);

                bReadRecordOK = ReadRecord(fs, pRecord);

            }


            fs.Close();

            /*
            if (!bReadRecordOK)
            {
                return false;
            }*/

            // switch case 3
            GetDicomFileCTInfo();

            return true;
        }

        //return true;
    }
    public bool ReadRecord(BinaryReader br, DicomFileRecord pRecord)
    {
        //BinaryReader br = new BinaryReader(fp);
        byte[] szTag = new byte[4];
        byte[] szVR = new byte[2];
        byte[] pData;



        // set the position of the tag 
        //pRecord._miFilePos = ftell(fp);
        pRecord._miFilePos = br.BaseStream.Position;

        // read group and element 
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
        // fp.Read(szTag, 1, 4, fp);
        //if ((i = fp.Read(szTag, 1, 4)) != 4)

        /*
        if ((i = fp.Read(fileData, 1, 4)) != 4)
        {
            if (i == 0) return true;

            return false;
        }
        */

        //br.Read(szTag, 0, 4);


        pRecord._musGrp = BitConverter.ToUInt16(szTag, 0);
        pRecord._musEle = BitConverter.ToUInt16(szTag, 2);
        pRecord._musTagLen = 4;

        if (pRecord._musGrp != Convert.ToUInt16("0x0002", 16) && _mbExplicitVR == false)
        {
            // data length	- 4 bytes

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
        { // pRecord._musGrp == 0x0002 || m_bExplicitVR == true
          // group 0x0002 is always explicit VR
          // read VR
            if (br.Read(szVR, 0, 2) != 2)
            {
                //BBTErrMsgBox("Failed to read DICOM tag (VR): \n" + m_sFileName);
                return false;
            }

            pRecord._musTagLen += 2;

            //szVR[2] = (byte)'\0';

            pRecord._msVR = System.Text.Encoding.UTF8.GetString(szVR);

            //pRecord._msVR = string.Format("{0}", string.Join("", szVR));


            // read data length

            if (pRecord._msVR.Equals("OB") ||
                pRecord._msVR.Equals("OW") ||
                pRecord._msVR.Equals("OF") ||
                pRecord._msVR.Equals("SQ") ||
                pRecord._msVR.Equals("UT") ||
                pRecord._msVR.Equals("UN"))
            {
                // skip unused 2 bytes
                br.BaseStream.Seek(2, SeekOrigin.Current);
                pRecord._musTagLen += 2;

                byte[] tempByte = new byte[4];
                // read data length as unsigned long 
                //if (br.Read(BitConverter.GetBytes(pRecord._mulLen), 1, 4) != 4)
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
                // read data length as unsigned short 
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


        // find the record from Dicom dictionary
        DicomDictRecord pDictRecord = new DicomDictRecord();


        // only even groups are DICOM standard group
        // odd groups are private groups so 
        // So we find the group from DICOM dictionary only it is DICOM standard group 
       
        
        if (pRecord._musGrp % 2 == 0)
        {
            pDictRecord = _gpDicomDict.Find(pRecord._musGrp, pRecord._musEle);
        }


        if (pDictRecord._isNotNull == true)

        //if (_marrpRecord != null)
        {
            // set record name and VR 
            pRecord._msName = pDictRecord._msName;
            pRecord._msVR = pDictRecord._msVR;


            if (pDictRecord._msVR == "SQ" || pDictRecord._msVR == "??" || pRecord._mulLen == 0)
            //if (pRecord._mulLen == 0)
            {
                pRecord._mpData = null;
                return true;
            }
        }
        
        

        // set data length to zero if length is 0xFFFFFFFF (undefined length please see DICOM standard)
        if (pRecord._mulLen == Convert.ToUInt64("0xFFFFFFFF", 16))
        {
            pRecord._mulLen = 0;
            pRecord._mpData = null;
            return true;
        }

        // read the data
        pData = new byte[pRecord._mulLen + 1];
        //pData = BitConverter.GetBytes(pRecord._mulLen);

        if (pData == null)
        {
            //BBTErrMsgBox("Out of Memmory!: Read DICOM data!");
            return false;
        }

        //if (br.Read(pData, 1, (int)pRecord._mulLen) != (int)pRecord._mulLen)
        /*
        if (br.Read(pData, 1, (int)pRecord._mulLen))
        {
            //BBTErrMsgBox("Failed to read DICOM tag data: \n" + _msFileName);
            //delete pData;
            return false;
        }
        */

        if (br.Read(pData, 0, (int)pRecord._mulLen) != (int)pRecord._mulLen)
        {
            // Failed to read Dicom
            // Delete pData
            //pData
            return false;
        }

        //br.Read(pData, 0, (int)pRecord._mulLen);

        // 
        // set data pointer
        pRecord._mpData = pData;
        string mpDataString = Encoding.Default.GetString(pRecord._mpData);

        pData[pRecord._mulLen] = 0;

        //jctest
        //	if (pRecord._musGrp == 0x0008 && (pRecord._musEle == 0x0000 || pRecord._musEle == 0x0001)) {
        //		ULONG ul = *((ULONG *)pRecord._mpData);
        //	}
        //jctest

        // check if it is Explict VR or Implicit VR
        if (pRecord._musGrp == Convert.ToUInt16("0x0002", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
        {
            string dataString = Encoding.Default.GetString(pRecord._mpData);

            if (dataString.Equals("1.2.840.10008.1.2\0\0"))
            {
                _mbExplicitVR = false;
            }
            else
            {  // 1.2.840.10008.1.2.1 is Explicit VR
                _mbExplicitVR = true;
            }
        }

        // clasify the dicom into POI/ROI/RTPLAN/CT
        if (_miDicomFileType == -1)
        {

            // check modality
            if (pRecord._musGrp == 0x0008 && pRecord._musEle == 0x0060)
            {
                if (pRecord._mpData.Equals("CT"))
                {

                    // set file type as CT
                    _miDicomFileType = 3;
                }
                else if (pRecord._mpData.Equals("RTPLAN"))
                {

                    // set file type as RTPLAN
                    //_miDicomFileType = BBT_DICOM_FILE_RTPLAN;
                    _miDicomFileType = 2;

                    // set instance number for sorting
                    //_miInstanceNum = BBT_INSTANCE_NUM_RTPLAN;
                    _miInstanceNum = Int32.MaxValue - 1;
                }
            }
            else if (pRecord._musGrp == 0x3006 && pRecord._musEle == 0x0004)
            {
                // check struct set name
                if (pRecord._mpData.Equals("POI "))
                {

                    // set file type as POI
                    //_miDicomFileType = BBT_DICOM_FILE_POI;
                    _miDicomFileType = 1;

                    // set instance number for sorting
                    //_miInstanceNum = BBT_INSTANCE_NUM_POI;
                    _miInstanceNum = Int32.MaxValue - 3;
                }
                else if (pRecord._mpData.Equals("ROI ") /*|| strcmp(pRecord._mpData, "test") == 0*/)
                {

                    // set file type as ROI
                    //_miDicomFileType = BBT_DICOM_FILE_ROI;
                    _miDicomFileType = 0;

                    // set instance number for sorting
                    //_miInstanceNum = BBT_INSTANCE_NUM_ROI;
                    _miInstanceNum = Int32.MaxValue - 2;
                }
            }
        }


        // for debug
        //	char szData[1024];
        //	memcpy(szData, pData, min(sizeof(szData),pRecord->m_ulLen)); 
        return true;
    }

    public void GetDicomFileCTInfo()
    {

        int i, k;

        //DicomFileRecord *pRecord;

        DicomFileRecord pRecord;

        for (i = 0; i < _marrpRecord.Count; i++)
        {
            pRecord = _marrpRecord[i];


            // instance number (image number)   Y
            if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x0013", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _miInstanceNum = Convert.ToInt16(tempString);

                //_miInstanceNum = BitConverter.ToInt16(pRecord._mpData, 0);
            }
            // (0008, 0016)	UI	SOP Class UID   Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0008", 16) && pRecord._musEle == Convert.ToUInt16("0x0016", 16))
            {
                _msSOPClassUID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
                //_msSOPClassUID = _msSOPClassUID.TrimEnd('\0');
                //_msSOPClassUID = string.Format("{0}", pRecord._mpData);
            }
            // (0008, 0018)	UI	SOP Instance UID    Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0008", 16) && pRecord._musEle == Convert.ToUInt16("0x0018", 16))
            {
                _msSOPInstanceUID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
                //_msSOPInstanceUID = string.Format("{0}", pRecord._mpData);
            }
            // (0010, 0010)	Patient Name    Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {

                // the format of the patient name is last name + '^' + first name + '^'
                string sPatientName;

                sPatientName = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
                //sPatientName = string.Format("{0}", pRecord._mpData);
                int iPos = sPatientName.IndexOf('^');
                if (iPos != -1)
                {
                    _msPatientNameLast = sPatientName.Substring(0, iPos);
                    _msPatientNameFirst = sPatientName.Substring(iPos + 1);
                 //   _msPatientNameFirst.Remove('^');
                }
                else
                {
                    // this case will not happen by dicom standard
                    _msPatientNameFirst = sPatientName;
                    _msPatientNameLast = sPatientName;
                }

                _msPatientNameFirst.TrimEnd();
                _msPatientNameLast.TrimEnd();
            }
            // (0010, 0020)	Patient ID  Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0020", 16))
            {
                _msPatientID = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');
                //_msPatientID = string.Format("{0}", pRecord._mpData);
            }
            // (0010, 0040)	Patient Sex Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0010", 16) && pRecord._musEle == Convert.ToUInt16("0x0040", 16))
            {
                _msPatientSex = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0').TrimEnd();
                //_msPatientSex = string.Format("{0}", pRecord._mpData);
            }
            // (0028, 0010)	Image Rows (width)  M
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {
                //string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');


                _miImgRow = BitConverter.ToInt16(pRecord._mpData, 0);
            }

            // Image Columns	(0028, 0011)    M
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0011", 16))
            {
                //string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _miImgCol = BitConverter.ToInt16(pRecord._mpData, 0);
            }
            // image position   Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x0032", 16))
            {
                //reads double 
                //k = sscanf(pRecord._mpData, "%lf\\%lf\\%lf", _mdImgPos, _mdImgPos + 1, _mdImgPos + 2);
                //k = sscanf(pRecord._mpData, "%lf\\%lf\\%lf", _mdImgPos, _mdImgPos + 1, _mdImgPos + 2);
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                var positions = tempString.Split('\\');

                _mdImgPos[0] = Convert.ToDouble(positions[0]);
                _mdImgPos[1] = Convert.ToDouble(positions[1]);
                _mdImgPos[2] = Convert.ToDouble(positions[2]);

                //_mdImgPos[0] = BitConverter.ToSingle(pRecord._mpData, 0);
                //_mdImgPos[1] = BitConverter.ToSingle(pRecord._mpData, 5);
                //_mdImgPos[2] = BitConverter.ToSingle(pRecord._mpData, 10);
                k = 3;

                Debug.Assert(k == 3);
            }
            // slice location   Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0020", 16) && pRecord._musEle == Convert.ToUInt16("0x1041", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _mdSliceLocation = Convert.ToDouble(tempString);

                //k = sscanf(pRecord._mpData, "%lf", &_mdSliceLocation);
                //k = sscanf(pRecord._mpData, "%lf", &_mdSliceLocation);
                //_mdSliceLocation = Int32.Parse(pRecord._mpData[0]);
                //_mdSliceLocation = BitConverter.ToDouble(pRecord._mpData, 0);
                k = 1;

                Debug.Assert(k == 1);
            }
            // slice thinkness  Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0018", 16) && pRecord._musEle == Convert.ToUInt16("0x0050", 16))
            {
                //k = sscanf(pRecord._mpData, "%lf", &_mdSliceThickness);

                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                _mdSliceThickness = Convert.ToDouble(tempString);

                //_mdSliceThickness = BitConverter.ToSingle(pRecord._mpData, 0);
                _mdSliceThicknessOriginal = _mdSliceThickness;
                _mdZdistance = _mdSliceThickness;  //add on oct 29,2007
                k = 1;

                Debug.Assert(k == 1);
            }
            // image XY pixel spacing   Y
            else if (pRecord._musGrp == Convert.ToUInt16("0x0028", 16) && pRecord._musEle == Convert.ToUInt16("0x0030", 16))
            {
                //k = sscanf(pRecord._mpData, "%lf\\%lf", &_mdImgXPixelSpacing, &_mdImgYPixelSpacing);
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                var positions = tempString.Split('\\');

                _mdImgXPixelSpacing = Convert.ToDouble(positions[0]);
                _mdImgYPixelSpacing = Convert.ToDouble(positions[1]);

                //_mdImgXPixelSpacing = BitConverter.ToDouble(pRecord._mpData, 0);
                //_mdImgYPixelSpacing = BitConverter.ToDouble(pRecord._mpData, 8);

                k = 2;

                Debug.Assert(k == 2);
            }

            /*
            // Window center
            else if (pRecord._musGrp == 0x0028 && pRecord._musEle == 0x1050)
            {
                k = sscanf(pRecord._mpData, "%lf", &_mdWinCenter);
                ASSERT(k == 1);
            }
            // Window Width
            else if (pRecord._musGrp == 0x0028 && pRecord._musEle == 0x1051)
            {
                k = sscanf(pRecord._mpData, "%lf", &_mdWinWidth);
                ASSERT(k == 1);
            }
            */


            // Pixel data (7FE0,0010)
            else if (pRecord._musGrp == Convert.ToUInt16("0x7FE0", 16) && pRecord._musEle == Convert.ToUInt16("0x0010", 16))
            {
                string tempString = Encoding.UTF8.GetString(pRecord._mpData).TrimEnd('\0');

                // set the pixel data length and pointer for reference convience
                _mulPixelDataLen = pRecord._mulLen;
                _mpPixelData = pRecord._mpData;
            }

            /*
            // table height (0018,1130)
            else if (pRecord._musGrp == 0x0018 && pRecord._musEle == 0x1130)
            {
            }
            */
        }
    }

    // end of Feb 19th


    //DicomFile() { }

}