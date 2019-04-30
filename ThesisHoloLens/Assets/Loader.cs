#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO; // to get BinaryReader
using System.Linq; // to get array's Min/Max

public class Loader : MonoBehaviour {

	[Header("Folder with images to create 3DTexture")]
	[SerializeField]
	public string folder;
	[Header("File with Transfer Function")]
	[SerializeField]
	public string transferFolder;
	public string path = @"Assets/";
	public string filename = "skull";
	public string extension = ".raw";

	private int[] size = new int[3];
	private Texture2D[] slices;
	private Color[] volumeColors;
	private Color[] transferFunction;
	private Vector3[] gradients;

	void Start() {
		Texture3D texture;

		if(!folder.Equals (string.Empty)){
			slices = Resources.LoadAll<Texture2D> (folder) as Texture2D[];
			Texture2D slice = slices [0];

			size [0] = slice.width;
			size [1] = slice.height;
			size [2] = NearestSuperiorPow2 (slices.Length);

			GenerateVolumeTexture ();
		}
		else {
			// load the raw data
			size [0] = 256;
			size [1] = 256;
			size [2] = 256;
			LoadRAWFile();
		}

		texture = new Texture3D (size [0], size [1], size [2], TextureFormat.ARGB32, true);
		texture.wrapMode = TextureWrapMode.Clamp;

		//Obtain Transfer Function and save it
		Parser parser = new Parser(transferFolder);
		transferFunction = parser.TransferFunction();

		//Apply Transfer Function
		ApplyTransferFunction();

		texture.SetPixels (volumeColors);
		texture.Apply ();
		// assign it to the material of the parent object
		GetComponent<Renderer>().sharedMaterial.SetTexture("_Data", texture);
		GetComponent<Renderer> ().sharedMaterial.SetFloat("_SliceAxis1Min", 0);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_SliceAxis1Max", 1);
		GetComponent<Renderer> ().sharedMaterial.SetFloat("_SliceAxis2Min", 0);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_SliceAxis2Max", 1);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_SliceAxis3Min", 0);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_SliceAxis3Max", 1);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_DataMin", 0);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_DataMax", 1);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_Iterations", 2048);

		// save it as an asset for re-use
		#if UNITY_EDITOR
		AssetDatabase.CreateAsset (texture, path + filename + ".asset");
		#endif

	}

	int NearestSuperiorPow2(int n)
	{
		int x = 2;

		while (x < n) {
			x *= 2;
		}
		
		return x;

	}
		
	private void LoadRAWFile()
	{
		Debug.Log ("Opening file "+path+filename+extension);
		FileStream file = new FileStream(path+filename+extension, FileMode.Open);
		Debug.Log ("File length = "+file.Length+" bytes, Data size = "+size[0]*size[1]*size[2]+" points -> "+file.Length/(size[0]*size[1]*size[2])+" byte(s) per point");

		BinaryReader reader = new BinaryReader(file);
		byte[] buffer = new byte[size[0] * size[1] * size[2]]; // assumes 8-bit data
		reader.Read(buffer, 0, sizeof(byte) * buffer.Length);
        reader.Dispose();

		volumeColors = new Color[buffer.Length];
		Color color = Color.black;
		for (int i = 0; i < buffer.Length; i++)
		{
			color.a = (float)buffer[i] / byte.MaxValue; //scale the scalar values to [0, 1]
			volumeColors[i] = color;
		}

	}

	private void GenerateVolumeTexture()
	{
		var w = size[0];
		var h = size[1];
		var d = size[2];

		// skip some slices if we can't fit it all in
		var countOffset = (slices.Length - 1) / (float)d;

		volumeColors = new Color[w * h * d];

		var sliceCount = 0;
		var sliceCountFloat = 0f;
		for(int z = 0; z < d; z++)
		{
			sliceCountFloat += countOffset;
			sliceCount = Mathf.FloorToInt(sliceCountFloat);
			for(int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					var idx = x + (y * w) + (z * (w * h));

					Color c = slices[sliceCount].GetPixelBilinear(x / (float)w, y / (float)h); 

					if (!(c.r < 0.1f && c.g < 0.1f && c.b < 0.1f))
						volumeColors [idx] = c;

				}
			}
		}

	}

	//Function that applies transfer function into the 3D Texture
	void ApplyTransferFunction(){

		if(volumeColors != null) {
			for (int i = 0; i < volumeColors.Length; i++) {
				Color c = volumeColors [i];
				int intensidade = Mathf.RoundToInt(255*(c.r + c.g + c.b) / 3);
				c = transferFunction[intensidade];
				volumeColors[i] = c;
			}

		}

//		for (int i = 0; i < volumeColors.Length; i++)
//		{
//			Color c = volumeColors [i];
//			int gray_value = Mathf.RoundToInt(255*(c.r + c.g + c.b) / 3);
//			c.a = TransferFunction(0.8f, gray_value);
//
//			if (c.a > 0.1f)
//				volumeColors [i] = new Color (0.93f, 0.92f, 0.84f, c.a + 0.2f);
//			else
//				volumeColors [i] = Color.clear;
//		}
			
	}

	float TransferFunction(float transferslide, int intensity)
	{

		if (intensity == 0)
			return 0;
		else
		{
			float result = (1f / 255f) * ((float)intensity - (transferslide * 255));
			return result;
		}
	}
		
	//Shading Part
//	private void generateGradients(int sampleSize, Color[] colors)
//	{
//		int n = sampleSize;
//		Vector3 normal = Vector3.zero;
//
//		Vector4 s1 = new Vector4 ();
//		Vector4 s2 = new Vector4 ();
//
//		var w = size[0];
//		var h = size[1];
//		var d = size[2];
//
//		gradients = new Vector3[w * h * d];
//
//		int index = 0;
//		for (int z = 0; z < d; z++)
//		{
//			for (int y = 0; y < h; y++)
//			{
//				for (int x = 0; x < w; x++)
//				{
//					s1.x = sampleVolume(x - n, y, z);
//					s2.x = sampleVolume(x + n, y, z);
//					s1.y = sampleVolume(x, y - n, z);
//					s2.y = sampleVolume(x, y + n, z);
//					s1.z = sampleVolume(x, y, z - n);
//					s2.z = sampleVolume(x, y, z + n);
//
//					gradients[index++] = Vector3.Normalize(s2 - s1);
//					if (float.IsNaN(gradients[index - 1].x))
//						gradients[index - 1] = Vector3.zero;
//				}
//			}
//		}
//
//	}
//
//	private float sampleVolume(int x, int y, int z)
//	{
//		int w = size [0];
//		int h = size [1];
//		int d = size [2];
//
//		x = (int)Mathf.Clamp(x, 0, w - 1);
//		y = (int)Mathf.Clamp(y, 0, h - 1);
//		z = (int)Mathf.Clamp(z, 0, d - 1);
//
//		return (float)volumeColors[x + (y * w) + (z * w * h)].a;
//	}
//
//	private void filterNxNxN(int n)
//	{
//		int w = size [0];
//		int h = size [1];
//		int d = size [2];
//
//		int index = 0;
//		for (int z = 0; z < d; z++)
//		{
//			for (int y = 0; y < h; y++)
//			{
//				for (int x = 0; x < w; x++)
//				{
//					gradients[index++] = sampleNxNxN(x, y, z, n);
//				}
//			}
//		}
//	}
//
//	private Vector3 sampleNxNxN(int x, int y, int z, int n)
//	{
//		n = (n - 1) / 2;
//
//		Vector3 average = Vector3.zero;
//		int num = 0;
//
//		for (int k = z - n; k <= z + n; k++)
//		{
//			for (int j = y - n; j <= y + n; j++)
//			{
//				for (int i = x - n; i <= x + n; i++)
//				{
//					if (isInBounds(i, j, k))
//					{
//						average += sampleGradients(i, j, k);
//						num++;
//					}
//				}
//			}
//		}
//
//		average /= (float)num;
//		if (average.x != 0.0f && average.y != 0.0f && average.z != 0.0f)
//			average.Normalize();
//
//		return average;
//	}
//
//	private bool isInBounds(int x, int y, int z)
//	{
//		int w = size [0];
//		int h = size [1];
//		int d = size [2];
//
//		return ((x >= 0 && x < w) &&
//			(y >= 0 && y < h) &&
//			(z >= 0 && z < d));
//	}
//
//	private Vector3 sampleGradients(int x, int y, int z)
//	{
//		int w = size [0];
//		int h = size [1];
//
//		return gradients[x + (y * w) + (z * w * h)];
//	}

}
