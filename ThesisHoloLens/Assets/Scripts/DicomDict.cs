/* 
 * Dicom Dictionary
 * Author: Tuan Mai
 * Purpose: Get the DICOM dictionary and compare it to the data from the DICOM file
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    string byteToString;
    byte[] stringToByte;

    public bool Load(string sFileName)
    {

        // The format of DICOM dictionary record
        // (9999 9999) name VR1 or VR2 VM RET

        DicomDictRecord pDictRecord;
        int i;

        _msFileName = sFileName;
        string path = "Assets/Datasets/" + _msFileName;

        if (!File.Exists(path))
        {
            return false;
        }

        using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        {
            szTemp = new byte[br.BaseStream.Length + 1];

            br.Read(szTemp, 0, szTemp.Length);
            byteToString = System.Text.Encoding.UTF8.GetString(szTemp);

            string[] lines = Regex.Split(byteToString, " \r\n");

            string tempString;

            string last = lines.Last();

            foreach (string line in lines)
            {

                if (line.Equals(last))
                {
                    break;
                }

                // Create new Dicom Dictionary record and add it to the array
                pDictRecord = new DicomDictRecord();
                _marrpRecord.Add(pDictRecord);

                // Set group and element of the tag 
                tempString = line.Substring(1, 4);

                pDictRecord._musGrp = ushort.Parse(tempString, NumberStyles.HexNumber);

                tempString = line.Substring(6, 4);

                pDictRecord._musEle = ushort.Parse(tempString, NumberStyles.HexNumber);

                // Set the rest info from the 12th character
                sTemp = line.Substring(12);

                // Get the RET info
                if (sTemp.Substring(sTemp.Length - 4, 4) == " RET")
                {
                    i = sTemp.Length;
                    pDictRecord._mbRet = true;
                    sTemp = sTemp.Remove(i - 4);
                }

                // Set the VM of the tag
                i = sTemp.LastIndexOf(' ');
                pDictRecord._msVM = sTemp.Substring(i + 1);
                sTemp = sTemp.Remove(i, pDictRecord._msVM.Length + 1);

                // Set the VR of the tag
                i = sTemp.LastIndexOf(' ');
                pDictRecord._msVR = sTemp.Substring(i + 1);
                sTemp = sTemp.Remove(i, pDictRecord._msVR.Length + 1);

                // Set the name of the tag
                pDictRecord._msName = sTemp;
            }

            br.Dispose();
            
        }
        return true;
    }

    public DicomDictRecord Find(ushort usGrp, ushort usEle)
    {

        // Find the specified Dicom tag by group and Element
        int i;
        DicomDictRecord pDictRecord = new DicomDictRecord();
        
        for (i = 0; i < _marrpRecord.Count; i++)
        {
            pDictRecord = _marrpRecord[i];

            if (pDictRecord._musGrp == usGrp && pDictRecord._musEle == usEle)
            {
                pDictRecord._isNotNull = true;
                // find
                return pDictRecord;
            }

            else
            {
                continue;
            }
        }

        pDictRecord._isNotNull = false;

        // Not found
        return pDictRecord;
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
