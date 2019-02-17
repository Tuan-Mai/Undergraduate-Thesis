﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DicomDict : MonoBehaviour
{
    string _msFileName;
    List<DicomDictRecord> m_arrpRecord;

    bool  Load(string sFileName)
    {

        // the format of DICOM dictionary record
        // (9999 9999) name VR1 or VR2 VM RET
        byte[] szTemp= new byte[1024];
        string sTemp;
        DicomDictRecord pDictRecord;
        FileStream fp;
        int i;

        _msFileName = sFileName;
        if ((File.Open(_msFileName, FileMode.Open, FileAccess.Read)) == null)
        {
            //BBTErrMsgBox("Failed to open DICOM Dictionary file:\n" + m_sFileName);
            return false;
        }


        while (fgets(szTemp, sizeof(szTemp), fp) != NULL)
        {


            // get rid of newline and spaces at the end of the record
            i = strlen(szTemp);
            while (szTemp[i - 1] == '\n' || szTemp[i - 1] == ' ') --i;
            szTemp[i] = '\0';

            // create new Dicom Dictionary record
            // and add it to the array
            pDictRecord = new DicomDictRecord();
            m_arrpRecord.Add(pDictRecord);

            // set group and element of the tag 
            sscanf(szTemp, "(%4hx,%4hx)", &pDictRecord->m_usGrp, &pDictRecord->m_usEle);

            // set the rest info from the 12th character
            sTemp.Format("%s", szTemp + 12);

            // get the RET info
            if (sTemp.Right(4) == " RET")
            {
                i = sTemp.GetLength();
                pDictRecord->m_bRet = true;
                sTemp.Delete(i - 4, 4);
            }

            // set the VM of the tag
            i = sTemp.ReverseFind(' ');
            pDictRecord->m_sVM = sTemp.Mid(i + 1);
            sTemp.Delete(i, pDictRecord->m_sVM.GetLength() + 1);

            // set the VR of the tag
            i = sTemp.ReverseFind(' ');
            pDictRecord->m_sVR = sTemp.Mid(i + 1);
            ASSERT(pDictRecord->m_sVR.GetLength() == 2);
            sTemp.Delete(i, pDictRecord->m_sVR.GetLength() + 1);

            // set the name of the tag
            pDictRecord->m_sName = sTemp;
        }


        fclose(fp);


        return true;
    }

    CBBTDicomDictRecord* CBBTDicomDict::Find(unsigned short usGrp, unsigned short usEle)
    {

        // find the specified Dicom tag by group and Element
        register int i;
        CBBTDicomDictRecord* pDictRecord = NULL;
        for (i = 0; i < m_arrpRecord.GetSize(); i++)
        {
            pDictRecord = m_arrpRecord[i];
            if (pDictRecord->m_usGrp == usGrp && pDictRecord->m_usEle == usEle)
                // find
                return pDictRecord;
        }

        // not find
        return NULL;
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