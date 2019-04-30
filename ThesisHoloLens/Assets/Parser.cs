using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class Parser{

	public class TransferControlPoint{
	
		private Color color;
		private float isovalue;

		public TransferControlPoint(Color c, float i){
			color = c;
			isovalue = i;
		}

		public Color getColor(){
			return color;
		}

		public float getIsovalue(){
			return isovalue;
		}

	}

	private List<TransferControlPoint> transferPoints;

	public Parser(string file){
		ParseTransferFunction (file);
	}

	//Function responsible for extracting color/alpha associated to isovalues
	public void ParseTransferFunction(string fileFolder){
        //StreamReader file = new StreamReader(fileFolder, Encoding.UTF8, false);
        StreamReader file = new StreamReader(File.Open(fileFolder, FileMode.Open));

		string line;

		transferPoints = new List<TransferControlPoint>();

		while((line = file.ReadLine()) != null)
		{
			if (!line.Equals ("Color Isovalue")) {

				float[] color = new float[4];

				//Extract color field
				for (int i = 0; i < 4; i++) {
					int index = line.IndexOf (",");
					string factorS = line.Substring (0, index);
					float factor;
					float.TryParse (factorS, out factor);

					color [i] = factor;
					line = line.Substring (index + 2);
				}

				// Extract Isovalue field 
				string isovalueS = line;
				int isovalue;
				int.TryParse (isovalueS, out isovalue);

				//Create color variable and associate it to isovalue
				Color c = new Color(color[0], color[1], color[2], color[3]);
				TransferControlPoint tc = new TransferControlPoint (c, isovalue);
				transferPoints.Add(tc);
			}

		}

	}

	//Contruct transfer function with gradient to be used for coloring the 3D volume in the Loader Script
	//TODO construct gradient
	public Color[] TransferFunction() {

		Color[] transferFunction = new Color[256];

		for (int i = 0; i < 256; i++) {
			
			foreach (TransferControlPoint tc in transferPoints) {
				if (i == tc.getIsovalue()) {
					transferFunction[i] = tc.getColor ();
					break;
				}
			}

		}

		return transferFunction;
	}

}
