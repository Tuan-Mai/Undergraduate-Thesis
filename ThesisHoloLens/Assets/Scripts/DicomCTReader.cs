using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicomReader : MonoBehaviour
{

    DicomFile[] m_arrpDicomFileList;

    bool LoadAllDicomFile()
    {


        // load all the dicom files
        int i;
        DicomFile pDicomFile;

        for (i = 0; i < m_arrpDicomFileList.Length; i++)
        {
            pDicomFile = m_arrpDicomFileList[i];


            // load the dicom file
            if (!pDicomFile->Load(pDicomFile->m_sFileName))
            {
                return false;
            }
        }

        // sort the dicom file list by instance number
        // the sorted order will be
        // slice 1~ slice n;  POI, ROI, RTPLAN
        SortDicomFileList();

        // set dicom file index for POI/ROI/RTPLAN/CT
        for (i = 0; i < gpCtrlFile->m_arrpDicomFileList.GetSize(); i++)
        {

            pDicomFile = gpCtrlFile->m_arrpDicomFileList[i];

            switch (pDicomFile->m_iDicomFileType)
            {
                case BBT_DICOM_FILE_POI:
                    m_iIdxPOI = i;
                    break;
                case BBT_DICOM_FILE_ROI:
                    m_iIdxROI = i;
                    break;
                case BBT_DICOM_FILE_RTPLAN:
                    m_iIdxRTPlan = i;
                    break;
                case BBT_DICOM_FILE_CT:
                    if (m_iIdxFirstCT == BBT_UNKNOWN)
                    {
                        m_iIdxFirstCT = i;
                        m_iIdxLastCT = i;
                    }
                    else
                    {
                        m_iIdxLastCT = i;
                    }

                    break;
                default:
                    ErrMsgBox("Unknown type of DICOM file:\n" + pDicomFile->m_sFileName);
                    //return false;
            }
        }

        // set the max slice

        m_iMaxSlice = m_iIdxLastCT - m_iIdxFirstCT + 1;
        m_iMaxSliceInterpolation = m_iMaxSlice;
        

        // check 
        if (m_iIdxFirstCT == BBT_UNKNOWN)
        {
            ErrMsgBox("Failed to find any DICOM CT files under:\n" + m_sDicomFolder);
            return false;
        }

     

        // check image instance number of CT Images
        if (!CheckCTInstanceNum())
        {
            return false;
        }


        // calculate slice thickness by image Z location
        CalcSliceThickness();


        

        // Calc target volume Contour region overlaped with next slice
        // and target volume wholw Contour region with next slice
        if (!CalcTargetVol())
        {
            return false;
        }

      


#if 0
	FILE *fp;
	fp = fopen("C:\\DICOM\\Image Position.txt", "w");
	CString sMsg, sTemp;
	char  szTemp[1024];
	CBBTDicomFileRecord *pRecord = NULL;
	for (i=0; i<m_arrpDicomFileList.GetSize(); i++) {
		pDicomFile = m_arrpDicomFileList[i];
		szTemp[0] = 0;

		// CT image
		if (pDicomFile->m_iDicomFileType == BBT_DICOM_FILE_CT) {
			strcpy(szTemp, pDicomFile->m_sFileName);
			
			// image position
			pRecord = pDicomFile->FindRecord(0x0020, 0x0032);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);
			
			// Slice Thickness
			pRecord = pDicomFile->FindRecord(0x0018, 0x0050);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);

			// instance number 
			pRecord = pDicomFile->FindRecord(0x0020, 0x0013);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);

			// slice location 
			pRecord = pDicomFile->FindRecord(0x0020, 0x1041);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);

			strcat(szTemp, "\n");
			fputs(szTemp, fp);
		}
		// RTPLAN
		else if (pDicomFile->m_iDicomFileType == BBT_DICOM_FILE_RTPLAN) {

			// instance number 
			pRecord = pDicomFile->FindRecord(0x0020, 0x0013);
			
			// isocenter position
			pRecord = pDicomFile->FindRecord(0x300A, 0x012C);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);

			// Gantry angle
			pRecord = pDicomFile->FindRecord(0x300A, 0x011E);
			strcat(szTemp, " ");
			strcat(szTemp, pRecord->m_pData);

			strcat(szTemp, "\n");
			fputs(szTemp, fp);
		}

	}
	fclose(fp);
#endif
        ////

        return true;
        return true;
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
