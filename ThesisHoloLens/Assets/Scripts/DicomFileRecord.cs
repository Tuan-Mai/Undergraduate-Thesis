using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DicomFileRecord : MonoBehaviour
{

    void Free()
    {

    }

    void Init()
    {

    }

    int GetRecordLen() {
        return 0;
    }

    DicomFileRecord() { }
    

    ushort m_usGrp;
    ushort m_usEle;
    ulong m_ulLen;
    string m_sVR;

    // this is the byte length in the dicom file including
    // group		: 2 bytes:
    // element		: 2 bytes
    // VR			: 0 or 2 bytes 
    // data length	: 4 or 2 bytes 
    ushort m_usTagLen;
    long m_iFilePos;

    string m_sName;
    char[] m_pData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
