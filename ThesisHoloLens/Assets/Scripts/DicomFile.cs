using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicomFile : MonoBehaviour
{

	
    void GetDicomFileCTInfo() { }

    DicomFile() { }

    bool Load(string sFileName) { return false; }

    void Init() { }
  
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
