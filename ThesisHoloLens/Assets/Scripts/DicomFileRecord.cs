/* 
 * Dicom File Record 
 * Author: Tuan Mai
 * Purpose: Create a DICOM file record
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DicomFileRecord : MonoBehaviour
{
    // This is the byte length in the dicom file including
    // group		: 2 bytes:
    // element		: 2 bytes
    // VR			: 0 or 2 bytes 
    // data length	: 4 or 2 bytes 

    public ushort _musGrp = 0;
    public ushort _musEle = 0;
    public ulong _mulLen = 0;
    public string _msName;
    public string _msVR;
    public ushort _musTagLen = 0;
    public long _miFilePos = 0;
    public byte[] _mpData;

    int GetRecordLen() {
        if (_msVR.Equals("SQ") || (_musGrp == 0xfffe && _musEle == 0xe000))
            // Do not include the data lengn if it is a Sequence or an item
            return _musTagLen;

        // else
            ulong temp = _musTagLen + _mulLen;
            return (int)temp;
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
