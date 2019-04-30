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

        // the format of DICOM dictionary record
        // (9999 9999) name VR1 or VR2 VM RET
        //szTemp = new byte[1018];

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
            szTemp = new byte[br.BaseStream.Length + 1];
            //szTemp = new byte[200];

            //if (br.ReadBytes(szTemp.Length). == '\n')
            //{
            //    string test = System.Text.Encoding.UTF8.GetString(szTemp);
            //}

            //while (br.BaseStream.Position != br.BaseStream.Length )
            //while (br.ReadByte(szTemp[index], 0, 1) != szTemp.Length)

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

                // create new Dicom Dictionary record
                // and add it to the array
                pDictRecord = new DicomDictRecord();
                _marrpRecord.Add(pDictRecord);

                // set group and element of the tag 
                //sscanf(szTemp, "(%4hx,%4hx)", &pDictRecord->m_usGrp, &pDictRecord->m_usEle);
                tempString = line.Substring(1, 4);

                //pDictRecord._musGrp = Convert.ToUInt16(tempString, NumberStyles.HexNumber);

                pDictRecord._musGrp = ushort.Parse(tempString, NumberStyles.HexNumber);

                tempString = line.Substring(6, 4);

                //pDictRecord._musEle = Convert.ToUInt16(tempString);

                pDictRecord._musEle = ushort.Parse(tempString, NumberStyles.HexNumber);

                // set the rest info from the 12th character
                //sTemp = System.Text.Encoding.UTF8.GetString(szTemp, 12, szTemp.Length - 12);
                //sTemp = sTemp.Replace("\0", "");

                sTemp = line.Substring(12);

                // get the RET info
                if (sTemp.Substring(sTemp.Length - 4, 4) == " RET")
                {
                    i = sTemp.Length;
                    pDictRecord._mbRet = true;
                    sTemp = sTemp.Remove(i - 4);
                }

                // set the VM of the tag
                i = sTemp.LastIndexOf(' ');
                pDictRecord._msVM = sTemp.Substring(i + 1);
                sTemp = sTemp.Remove(i, pDictRecord._msVM.Length + 1);

                // set the VR of the tag
                i = sTemp.LastIndexOf(' ');
                pDictRecord._msVR = sTemp.Substring(i + 1);
                sTemp = sTemp.Remove(i, pDictRecord._msVR.Length + 1);

                // set the name of the tag
                pDictRecord._msName = sTemp;
            }

            /*  OLD CODE 

            //byte[] tempByte = new byte[index + 1];

            //Buffer.BlockCopy(szTemp, 0, tempByte, 0, index - 1);

            // get rid of newline and spaces at the end of the record
            //i = (szTemp.Length);

            //while (szTemp[i - 1] == '\n' || szTemp[i - 1] == ' ') --i;
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



            //string[] values = newString.Split('( ', ',', ')');


            // *** Test section for verification
            byte[] temp = new byte[2];

            //Buffer.BlockCopy(tempByte, 1, temp, 0, 2);

            //Array.Copy(tempByte, 1, temp, 0, 2);

            pDictRecord._musGrp = BitConverter.ToUInt16(temp, 0);

            //Buffer.BlockCopy(tempByte, 4, temp, 0, 2);

            //Array.Copy(tempByte, 3, temp, 0, 2);

            pDictRecord._musEle = BitConverter.ToUInt16(temp, 0);

            /*
            Buffer.BlockCopy(szTemp, 1, temp, 0, 2);

            pDictRecord._musGrp = BitConverter.ToUInt16(temp, 0);

            Buffer.BlockCopy(szTemp, 4, temp, 0, 2);

            pDictRecord._musEle = BitConverter.ToUInt16(temp, 0);
            */

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
            */






            br.Dispose();
            

        }
        return true;
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

        /*
        foreach (DicomDictRecord pDictRecordCheck in _marrpRecord)
        {
            if (pDictRecord._musGrp == usGrp && pDictRecord._musEle == usEle)
            {// find
                return pDictRecord;
            }

            else
            {
                continue;
            }
        }
        */

        pDictRecord._isNotNull = false;
        // not find
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
