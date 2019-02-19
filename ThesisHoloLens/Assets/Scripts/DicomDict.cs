﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class DicomDict : MonoBehaviour
{
    string _msFileName;
    List<DicomDictRecord> m_arrpRecord;

    byte[] szTemp;
    string sTemp;

    bool  Load(string sFileName)
    {

        // the format of DICOM dictionary record
        // (9999 9999) name VR1 or VR2 VM RET
        szTemp= new byte[1024];
        
        DicomDictRecord pDictRecord;
        FileStream fp;
        int i;

        _msFileName = sFileName;
        if ((fp = File.Open(_msFileName, FileMode.Open, FileAccess.Read)) == null)
        {
            //BBTErrMsgBox("Failed to open DICOM Dictionary file:\n" + m_sFileName);
            return false;
        }

        StreamReader sr = new StreamReader(fp);

        //while (fp.Read(szTemp, 1, Marshal.SizeOf(szTemp)) != null)
        while (sr.ReadLine() != null)
        {


            // get rid of newline and spaces at the end of the record
            i = (szTemp.Length);
            while (szTemp[i - 1] == '\n' || szTemp[i - 1] == ' ') --i;
            szTemp[i] = (byte)'\0';

            // create new Dicom Dictionary record
            // and add it to the array
            pDictRecord = new DicomDictRecord();
            m_arrpRecord.Add(pDictRecord);

            // set group and element of the tag 
            sscanf(szTemp, "(%4hx,%4hx)", &pDictRecord->m_usGrp, &pDictRecord->m_usEle);


            

            // set the rest info from the 12th character
            sTemp = string.Format("%s", szTemp[12]);
            

            // get the RET info
            if (sTemp.Substring(4) == " RET")
            {
                i = sTemp.Length;
                pDictRecord._mbRet = true;
                sTemp.Remove(i - 4, 4);
            }

            // set the VM of the tag
            i = sTemp.ReverseFind(' ');
            pDictRecord._msVM = sTemp.Substring(i + 1);
            sTemp.Remove(i, pDictRecord._msVM.Length + 1);

            // set the VR of the tag
            i = sTemp.ReverseFind(' ');
            pDictRecord._msVR = sTemp.Substring(i + 1);
            Debug.Assert(pDictRecord._msVR.Length == 2);
            sTemp.Remove(i, pDictRecord._msVR.Length + 1);

            // set the name of the tag
            pDictRecord._msName = sTemp;
        }

        fp.Close();


        return true;
    }

    DicomDictRecord Find(ushort usGrp, ushort usEle)
    {

        // find the specified Dicom tag by group and Element
        int i;
        DicomDictRecord pDictRecord = null;
        //for (i = 0; i < m_arrpRecord.GetSize(); i++)
        for (i = 0; i < m_arrpRecord.Count; i++)
        {
            pDictRecord = m_arrpRecord[i];
            if (pDictRecord._musGrp == usGrp && pDictRecord._musEle == usEle)
                // find
                return pDictRecord;
        }

        // not find
        return null;
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
