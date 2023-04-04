# Tuan Mai's Honours Bachelor of Computer Science Undergraduate Thesis

## Thesis Abstract

This thesis presents a partial solution of a wearable augmented reality (AR) system that reconstructs the anatomy of a patient’s kidneys in an interactive three-dimensional (3D) model based on preoperative CTs scanned for improving the current treatment planning system of laparoscopic cyst decortication. This research provides a pathway to a more accurate treatment planning system utilizing wearable AR technology and describes how an AR 3D model with a high degree of detail can provide more information to medical professionals when doing procedural planning. The use of wearable augmented reality with medical interventions improves efficiency and effectiveness, allowing for 2D medical images to be reconstructed in 3D and spatial information. Having this incorporation between wearable AR and medical procedures will allow for more accurate and effective treatment planning. Further development will allow medical professionals to collaboratively interact with the 3D model in a 3D interactive environment real-time, resulting in a potential increase of quality in treatment planning. Enabling medical professionals to collaborate, view and interact with the same patient data is a unique feature of the Microsoft HoloLens which is the wearable device used in this application.

## Background

### Motivation
Polycystic Kidney Disease (PKD), a genetic disorder that does not have a cure, led to this research; delving into building a system that can help create and/or improve monitoring and planning procedures for patients with PKD. 

Initial work and efforts were geared towards building a system speciallizing in Laparoscopic Renal Cyst Decortication procedures. Over the course of the work, the system was built in such a way that it was generalized to work with any organ, body part. This is due to the system built to read DICOM files. DICOM files are the standard in medical imaging file formats, thus allowing the system to read and display any DICOM file. 

## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites
This project was built using [Unity](https://unity.com/) (required to run on local machine) and [Visual Studio](https://visualstudio.microsoft.com/downloads/) (required to deploy onto the HoloLens or run on HoloLens Emulator).

### Opening
Once the project is downloaded, open Unity and Open Project. 
Select the ThesisHoloLens folder as the project folder. 

Ensure that the scene `Main.unity` is currently open. 

If it is not the current scene, you can find it under:

```
Projects > Assets > Scenes > Main.unity
```

### Adding DICOM Datasets
DICOM datasets can be added to the project in the folder:

```
Projects > Assets > Datasets
```

However, currently the project will load DICOM files from a hardcoded folder name. 
To change this folder name, please change it in the script `DisplayCTImage.cs`, located under:

```
Projects > Assets > Scripts
```

## Author
* **Tuan Mai** 

## Acknowledgments 
[REngine](https://www.researchgate.net/publication/330210818_Volume_and_Surface_Rendering_of_3D_Medical_Datasets_in_Unity_R) - For 3D render of the DICOM data due to time constraints on the project
