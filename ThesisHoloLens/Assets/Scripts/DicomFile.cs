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
    CArray _marrpRecord;
    string _msFileName; 

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

       

        m_ulPixelDataLen = 0;

        _msFileName=null;//Empty();

    }


    bool Load(string sFileName)
    {
        _msFileName = sFileName;

        //set filename to the end of path 
        string path = "Assets/Datasets/CTDataset1/CT002000020.dcm";

        // Delete the file if it exists.
        if (File.Exists(path))
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                return true;
            }
        }
        else
        {
            return false;
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

    */
  
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

    bool m_bExplicitVR;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
