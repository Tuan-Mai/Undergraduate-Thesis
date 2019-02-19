using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DicomFile : MonoBehaviour
{

    // Feb 17, 2019

    int _miDicomFileType;
	int _miInstanceNum;

    // (0008, 0016)	UI	SOP Class UID
    // (0008, 0018)	UI	SOP Instance UID
    //CString m_sSOPClassUID;
    //CString m_sSOPInstanceUID;
    string _msSOPClassUID;
	string _msSOPInstanceUID; 
	
	int _miImgRow;
	int _miImgCol;

    double[] _mdImgPos = new double[3]; // 0:X  1:Y  2:Z
	double _mdSliceThicknessOriginal = 0;
	double _mdSliceThickness = 0;
	double _mdSliceLocation = 0;
	//m_dWinCenter = 0;
	//m_dWinWidth = 0 ;

	double _mdImgXPixelSpacing = 0;
	double _mdImgYPixelSpacing = 0;

    
    string _msPatientID;
    //CArray<DicomFileRecord*, DicomFileRecord*> _marrpRecord;
    List<DicomFileRecord> _marrpRecord=new List<DicomFileRecord>();

    double _mulPixelDataLen;
    string _msFileName;

    bool _mbExplicitVR;

    void Init()
    {
        _miDicomFileType = 0;
        _miInstanceNum = 0;

        _msSOPClassUID=null;//Empty();
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

        _msFileName=null;//Empty();

    }


    bool Load(string sFileName)
    {
        _msFileName = sFileName;

        //TODO: set filename to the end of path 
        string path = "Assets/Datasets/CTDataset1/CT002000020.dcm";


        if (!File.Exists(path))
        {
            return false;
        }

        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            string sFileType = _msFileName.Substring(3);
            sFileType.ToUpper();

            if (sFileType == "DCM")
            {
                // skip first 128 + 4 bytes
                fs.Seek(132, SeekOrigin.Begin);

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
            DicomFileRecord pRecord = new DicomFileRecord();

            while ((fs.ReadByte() > -1) && bReadRecordOK)
            {

                // add record to the record list
                //pRecord = new DicomFileRecord;
                _marrpRecord.Add(pRecord);

                bReadRecordOK = ReadRecord(fs, pRecord);

            }
        }
    }
    bool ReadRecord(FileStream fp, DicomFileRecord pRecord){
        byte[] szTag = new byte[4];
            byte[] szVR=new byte[3];
        char[] pData;


    

        // set the position of the tag 
     //   pRecord->m_iFilePos = ftell(fp);

        // read group and element 
        // group		- 2 bytes 
        // element		- 2 bytes 
        int i;
        // fp.Read(szTag, 1, 4, fp);
        if ((i = fp.Read(szTag, 1, 4)) != 4)
        {
            if (fp.ReadByte() == -1) return true;

            return false;
        }

        pRecord._musGrp = (ushort)(szTag);
        pRecord._musEle = (ushort)(szTag + 2);
        pRecord._musTagLen = 4;

   

            if (pRecord->m_usGrp != 0x0002 && m_bExplicitVR == false)
            {
                // data length	- 4 bytes
                if (fread(&pRecord->m_ulLen, 1, 4, fp) != 4)
                {
                    return false;
                }
                pRecord->m_usTagLen += 4;
            }
            else
            { // pRecord->m_usGrp == 0x0002 || m_bExplicitVR == true
              // group 0x0002 is always explicit VR
              // read VR
                if (fread(szVR, 1, 2, fp) != 2)
                {
                    BBTErrMsgBox("Failed to read DICOM tag (VR): \n" + m_sFileName);
                    return false;
                }
                pRecord->m_usTagLen += 2;

                szVR[2] = '\0';
                pRecord->m_sVR.Format("%s", szVR);

                // read data length
                if (strcmp(szVR, "OB") == 0 ||
                    strcmp(szVR, "OW") == 0 ||
                    strcmp(szVR, "OF") == 0 ||
                    strcmp(szVR, "SQ") == 0 ||
                    strcmp(szVR, "UT") == 0 ||
                    strcmp(szVR, "UN") == 0)
                {

                    // skip unused 2 bytes
                    fseek(fp, 2, SEEK_CUR);
                    pRecord->m_usTagLen += 2;

                    // read data length as unsigned long 
                    if (fread(&pRecord->m_ulLen, 1, 4, fp) != 4)
                    {
                        BBTErrMsgBox("Failed to read DICOM tag length: \n" + m_sFileName);
                        return false;
                    }
                    pRecord->m_usTagLen += 4;
                }
                else
                {
                    // read data length as unsigned short 
                    unsigned short usLen;
                    if (fread(&usLen, 1, 2, fp) != 2)
                    {
                        return false;
                    }
                    pRecord->m_ulLen = usLen;
                    pRecord->m_usTagLen += 2;
                }
            }
        }


        // find the record from Dicom dictionary
        CBBTDicomDictRecord* pDictRecord = NULL;


        // only even groups are DICOM standard group
        // odd groups are private groups so 
        // So we find the group from DICOM dictionary only it is DICOM standard group 
        if (pRecord->m_usGrp % 2 == 0)
        {
            pDictRecord = gpDicomDict->Find(pRecord->m_usGrp, pRecord->m_usEle);
        }

        if (pDictRecord)
        {
            // set record name and VR 
            pRecord->m_sName = pDictRecord->m_sName;
            pRecord->m_sVR = pDictRecord->m_sVR;


            if (pDictRecord->m_sVR == "SQ" || pDictRecord->m_sVR == "??" || pRecord->m_ulLen == 0)
            {
                pRecord->m_pData = NULL;
                return true;
            }
        }

        // set data length to zero if length is 0xFFFFFFFF (undefined length please see DICOM standard)
        if (pRecord->m_ulLen == 0xFFFFFFFF)
        {
            pRecord->m_ulLen = 0;
            pRecord->m_pData = NULL;
            return true;
        }

        // read the data
        pData = (char*)malloc(pRecord->m_ulLen + 1);
        if (!pData)
        {
            BBTErrMsgBox("Out of Memmory!: Read DICOM data!");
            return false;
        }

        if (fread(pData, 1, pRecord->m_ulLen, fp) != pRecord->m_ulLen)
        {
            BBTErrMsgBox("Failed to read DICOM tag data: \n" + m_sFileName);
            delete pData;
            return false;
        }
// 
        // set data pointer
        pRecord->m_pData = pData;
        *(pData + pRecord->m_ulLen) = 0;

        //jctest
        //	if (pRecord->m_usGrp == 0x0008 && (pRecord->m_usEle == 0x0000 || pRecord->m_usEle == 0x0001)) {
        //		ULONG ul = *((ULONG *)pRecord->m_pData);
        //	}
        //jctest

        // check if it is Explict VR or Implicit VR
        if (pRecord->m_usGrp == 0x0002 && pRecord->m_usEle == 0x0010)
        {
            if (strcmp(pRecord->m_pData, "1.2.840.10008.1.2") == 0)
            {
                m_bExplicitVR = false;
            }
            else
            {  // 1.2.840.10008.1.2.1 is Explicit VR
                m_bExplicitVR = true;
            }
        }

        // clasify the dicom into POI/ROI/RTPLAN/CT
        if (m_iDicomFileType == BBT_UNKNOWN)
        {

            // check modality
            if (pRecord->m_usGrp == 0x0008 && pRecord->m_usEle == 0x0060)
            {
                if (strcmp(pRecord->m_pData, "CT") == 0)
                {

                    // set file type as CT
                    m_iDicomFileType = BBT_DICOM_FILE_CT;
                }
                else if (strcmp(pRecord->m_pData, "RTPLAN") == 0)
                {

                    // set file type as RTPLAN
                    m_iDicomFileType = BBT_DICOM_FILE_RTPLAN;

                    // set instance number for sorting
                    m_iInstanceNum = BBT_INSTANCE_NUM_RTPLAN;
                }
            }
            else if (pRecord->m_usGrp == 0x3006 && pRecord->m_usEle == 0x0004)
            {
                // check struct set name
                if (strcmp(pRecord->m_pData, "POI ") == 0)
                {

                    // set file type as POI
                    m_iDicomFileType = BBT_DICOM_FILE_POI;

                    // set instance number for sorting
                    m_iInstanceNum = BBT_INSTANCE_NUM_POI;
                }
                else if (strcmp(pRecord->m_pData, "ROI ") == 0 /*|| strcmp(pRecord->m_pData, "test") == 0*/)
                {

                    // set file type as ROI
                    m_iDicomFileType = BBT_DICOM_FILE_ROI;

                    // set instance number for sorting
                    m_iInstanceNum = BBT_INSTANCE_NUM_ROI;
                }
            }
        }


        // for debug
        //	char szData[1024];
        //	memcpy(szData, pData, min(sizeof(szData),pRecord->m_ulLen)); 
        return true;
    }

    CBBTDicomFileRecord* CBBTDicomFile::FindRecord(unsigned short usGrp, unsigned short usEle)
    {
        register CBBTDicomFileRecord *pRecord;
        register int i;
        for (i = 0; i < m_arrpRecord.GetSize(); i++)
        {
            pRecord = m_arrpRecord[i];
            if (pRecord->m_usGrp == usGrp && pRecord->m_usEle == usEle)
            {
                return pRecord;
            }
        }
        return NULL;

        return true;
    }
}

    /*
    void GetDicomFileCTInfo() {

        int i, k;

        //DicomFileRecord *pRecord;

        DicomFileRecord pRecord = new DicomFileRecord();

        for (i = 0; i < _marrpRecord.GetSize(); i++)
        {
            pRecord = m_arrpRecord[i];

            // instance number (image number)
            if (pRecord->m_usGrp == 0x0020 && pRecord->m_usEle == 0x0013)
            {
                m_iInstanceNum = atoi(pRecord->m_pData);
            }
            // (0008, 0016)	UI	SOP Class UID
            else if (pRecord->m_usGrp == 0x0008 && pRecord->m_usEle == 0x0016)
            {
                m_sSOPClassUID.Format("%s", pRecord->m_pData);
            }
            // (0008, 0018)	UI	SOP Instance UID
            else if (pRecord->m_usGrp == 0x0008 && pRecord->m_usEle == 0x0018)
            {
                m_sSOPInstanceUID.Format("%s", pRecord->m_pData);
            }
            // (0010, 0010)	Patient Name
            else if (pRecord->m_usGrp == 0x0010 && pRecord->m_usEle == 0x0010)
            {

                // the format of the patient name is last name + '^' + first name + '^'
                CString sPatientName;
                sPatientName.Format("%s", pRecord->m_pData);
                int iPos = sPatientName.Find('^');
                if (iPos != -1)
                {
                    m_sPatientNameLast = sPatientName.Left(iPos - 1);
                    m_sPatientNameFirst = sPatientName.Mid(iPos + 1);
                    m_sPatientNameFirst.Remove('^');
                }
                else
                {
                    // this case will not happen by dicom standard
                    m_sPatientNameFirst = sPatientName;
                    m_sPatientNameLast = sPatientName;
                }

                m_sPatientNameFirst.TrimRight();
                m_sPatientNameLast.TrimRight();
            }
            // (0010, 0020)	Patient ID
            else if (pRecord->m_usGrp == 0x0010 && pRecord->m_usEle == 0x0020)
            {
                m_sPatientID.Format("%s", pRecord->m_pData);
            }
            // (0010, 0040)	Patient Sex
            else if (pRecord->m_usGrp == 0x0010 && pRecord->m_usEle == 0x0040)
            {
                m_sPatientSex.Format("%s", pRecord->m_pData);
            }
            // (0028, 0010)	Image Rows (width)
            else if (pRecord->m_usGrp == 0x0028 && pRecord->m_usEle == 0x0010)
            {
                m_iImgRow = *((unsigned short*)(pRecord->m_pData));
    }
		// Image Columns	(0028, 0011)
		else if (pRecord->m_usGrp == 0x0028 && pRecord->m_usEle == 0x0011) {
			m_iImgCol = *((unsigned short*)(pRecord->m_pData));
		}
		// image position
		else if (pRecord->m_usGrp == 0x0020 && pRecord->m_usEle == 0x0032) {
			k = sscanf(pRecord->m_pData, "%lf\\%lf\\%lf", m_dImgPos, m_dImgPos+1, m_dImgPos+2);
ASSERT(k==3);
		}
		// slice location
		else if (pRecord->m_usGrp == 0x0020 && pRecord->m_usEle == 0x1041) {
			k = sscanf(pRecord->m_pData, "%lf", &m_dSliceLocation);
ASSERT(k==1);
		}
		// slice thinkness
		else if (pRecord->m_usGrp == 0x0018 && pRecord->m_usEle == 0x0050) {
			k = sscanf(pRecord->m_pData, "%lf", &m_dSliceThickness);
m_dSliceThicknessOriginal = m_dSliceThickness;
			m_dZdistance = m_dSliceThickness;  //add on oct 29,2007
			ASSERT(k==1);
		}
		// image XY pixel spacing
		else if (pRecord->m_usGrp == 0x0028 && pRecord->m_usEle == 0x0030) {
			k = sscanf(pRecord->m_pData, "%lf\\%lf", &m_dImgXPixelSpacing, &m_dImgYPixelSpacing);
ASSERT(k==2);
		}
		// Window center
		else if (pRecord->m_usGrp == 0x0028 && pRecord->m_usEle == 0x1050) {
			k = sscanf(pRecord->m_pData, "%lf", &m_dWinCenter);
ASSERT(k==1);
		}
		// Window Width
		else if (pRecord->m_usGrp == 0x0028 && pRecord->m_usEle == 0x1051) {
			k = sscanf(pRecord->m_pData, "%lf", &m_dWinWidth);
ASSERT(k==1);
		}
		// Pixel data (7FE0,0010)
		else if (pRecord->m_usGrp == 0x7FE0 && pRecord->m_usEle == 0x0010) {
			// set the pixel data length and pointer for reference convience
			m_ulPixelDataLen = pRecord->m_ulLen;
			m_pPixelData	 = pRecord->m_pData;
		}
		// table height (0018,1130)
		else if (pRecord->m_usGrp == 0x0018 && pRecord->m_usEle == 0x1130) {
		}
	}

    }
     // end of Feb 19th


    DicomFile() { }

    
  
    bool m_bInterpolationFileFlag;
    // POI/ROI/RTPLAN/CT
    int m_iDicomFileType;

    // Instance number for Dicom file
    // for CT:     set it to the value of tag (0x0020,0x0013)
    // for POI:    set it to BBT_INSTANCE_NUM_POI
    // for ROI:    set it to BBT_INSTANCE_NUM_ROI
    // for RTPLAN: set it to BBT_INSTANCE_NUM_RTPLAN
    int m_iInstanceNum;     // instance number

    // (0008, 0016)	UI	SOP Class UID
    // (0008, 0018)	UI	SOP Instance UID
    string m_sSOPClassUID;
    string m_sSOPInstanceUID;

    // Image Rows		(0028, 0010)
    // Image Columns	(0028, 0011)
    int m_iImgRow;
    int m_iImgCol;

    // Patient ID (0010, 0020)
    string m_sPatientID;

    // Patient Name (0010,0010)
    string m_sPatientNameFirst;
    string m_sPatientNameLast;

    // Patient Sex (0010, 0040)
    string m_sPatientSex;

    // image position (patient) tag (0x0020, 0x0032)
    double[] m_dImgPos=new double[3];  // 0:X  1:Y  2:Z

    // image XY pixel spacing tag (0x0028, 0x0030)  // unit mm
    double m_dImgXPixelSpacing;
    double m_dImgYPixelSpacing;
    // add on oct 29 
    double m_dZdistance;
    // Slice thickness tag (0x0018, 0x0050)  // unit mm
    double m_dSliceThicknessOriginal;

    // Slice thickness calculated by image z Position
    double m_dSliceThickness;

    // Slice Location tag (0x0020, 0x1041)
    double m_dSliceLocation;

    // window width and window center 
    double m_dWinCenter;    // (0x0028, 0x1050)
    double m_dWinWidth;     // (0x0028, 0x1051)

  

    // CT Pixel Data length and pointer (7FE0,0010)
    ulong m_ulPixelDataLen;
    byte[] m_pPixelData;
    double m_pEdgeDataSobel;
    byte[] m_pEdgeData;

   
    
    bool ReadRecord(DicomFileRecord pRecord)
    {


        return true;
    }
    void Free()
    {

    }

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/