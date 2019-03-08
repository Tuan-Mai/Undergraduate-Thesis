using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class DicomDict : MonoBehaviour
{
    string _msFileName;
    List<DicomDictRecord> _marrpRecord = new List<DicomDictRecord>();

    byte[] szTemp;
    string sTemp;

    public bool Load(string sFileName)
    {

        // the format of DICOM dictionary record
        // (9999 9999) name VR1 or VR2 VM RET
        szTemp = new byte[1024];

        DicomDictRecord pDictRecord;
        int i;

        _msFileName = sFileName;
        string path = "Assets/Datasets/" + _msFileName;

        if (!File.Exists(path))
        {
            return false;
        }

        //while (fgets(szTemp, sizeof(szTemp), br) != NULL) 

        using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        //using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {

            int l = 0;

            l = br.Read(szTemp, 0, szTemp.Length);




            // get rid of newline and spaces at the end of the record
            i = (szTemp.Length);

            while (szTemp[i - 1] == '\n' || szTemp[i - 1] == ' ') --i;
            //szTemp[i] = (byte)'0';

            //while (szTemp[i - 1] = Regex.Replace(szTemp[i - 1], @"\n|' '", ""))
            //{

            //                }

            // create new Dicom Dictionary record
            // and add it to the array
            pDictRecord = new DicomDictRecord();
            _marrpRecord.Add(pDictRecord);

            // set group and element of the tag 
            //sscanf(szTemp, "(%4hx,%4hx)", &pDictRecord->m_usGrp, &pDictRecord->m_usEle);


            // *** Test section for verification
            byte[] temp = new byte[2];

            Buffer.BlockCopy(szTemp, 0, temp, 0, 2);

            pDictRecord._musGrp = BitConverter.ToUInt16(temp, 0);

            Buffer.BlockCopy(szTemp, 2, temp, 0, 2);

            pDictRecord._musEle = BitConverter.ToUInt16(temp, 0);

            // set the rest info from the 12th character
            //sTemp.Format("%s", szTemp + 12);

            // set the rest info from the 12th character
            //sTemp = string.Format("{0}", szTemp[12]);

            /*
            var sb = new StringBuilder();
            for (int index = 12; index < szTemp.Length; index++)
            {
                sb.Append(String.Format("{0}", szTemp[index]));
            }

            sTemp = sb.ToString();
            */


            sTemp = System.Text.Encoding.UTF8.GetString(szTemp, 12, szTemp.Length - 12);
            sTemp = sTemp.Replace("\0", "");

            // get the RET info
            if (sTemp.Substring(sTemp.Length - 4, 4) == " RET")
            {
                i = sTemp.Length;
                pDictRecord._mbRet = true;
                sTemp.Remove(i - 4, 4);
            }

            // set the VM of the tag
            i = sTemp.LastIndexOf(' ');
            pDictRecord._msVM = sTemp.Substring(i + 1);
            sTemp.Remove(i, pDictRecord._msVM.Length + 1);

            // set the VR of the tag
            i = sTemp.LastIndexOf(' ');
            pDictRecord._msVR = sTemp.Substring(i + 1);
            Debug.Assert(pDictRecord._msVR.Length == 2);
            sTemp.Remove(i, pDictRecord._msVR.Length + 1);

            // set the name of the tag
            pDictRecord._msName = sTemp;


            br.Close();


            return true;
        }
    }

    public DicomDictRecord Find(ushort usGrp, ushort usEle)
    {

        // find the specified Dicom tag by group and Element
        int i;
        DicomDictRecord pDictRecord = new DicomDictRecord();
        //for (i = 0; i < _marrpRecord.GetSize(); i++)
        for (i = 0; i < _marrpRecord.Count; i++)
        {
            pDictRecord = _marrpRecord[i];
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
