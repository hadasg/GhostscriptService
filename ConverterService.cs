﻿/*******************************************************************************
	RIP2Image is a program that efficiently converts formats such as PDF or Postscript to image formats such as Jpeg or PNG.
    Copyright (C) 2013 XMPie Ltd.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

	This license covers only the RIP2Image files and not any file that RIP2Image links against or otherwise uses.
 
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;	

namespace RIP2Jmage
{
	/// <summary>
	/// Uniting all convert utilities.
	/// </summary>
	class ConverterService : IConverterService
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ConverterService()
		{
		}


	#region Methods
		/// <summary>
		/// Convert PDF to JPG.
		/// </summary>
		/// <param name="inConvertFilePath"></param>
		/// <param name="inNewFileTargetPath"></param>
		public bool ConvertPDF2JPG(string inConvertFilePath, string inNewFileTargetPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			bool conversionSucceed;

			try
			{
				CheckParamValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			}
			catch (System.Exception ex)
			{
				throw ex;
			}

			string OutputFileFullPath = PreFileConvert(inConvertFilePath, inNewFileTargetPath);
			inConvertFilePath = inConvertFilePath.Replace("\\", "\\\\");

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject();
			conversionSucceed = fileConvertor.Convert(inConvertFilePath, OutputFileFullPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			InstancesManager.PutObject(fileConvertor);

			// Rename JPG names to the correct page counter.
			RenameJPGNames(inNewFileTargetPath, inConvertFilePath);
			

			return conversionSucceed;
		}


		/// <summary>
		/// Convert all files type under inConvertFolderPath to JPG.
		/// </summary>
		/// <param name="inConvertFolderPath"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"> true if want to delete source file  </param>
		/// <param name="inSearchSubFolders"> true if want to convert PDF files in subfolders </param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
		/// <returns></returns>
		public bool ConvertPDFFolder2JPG(string inConvertFolderPath, string inTargetFolderPath, string inConvertFileWildCard, bool inDeleteSourcePDF, bool inSearchSubFolders, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			bool conversionSucceed;

			try
			{
				CheckParamValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			}
			catch (System.Exception ex)
			{
				throw ex;
			}
			
			inConvertFolderPath = new Uri(inConvertFolderPath).LocalPath;
			inTargetFolderPath = new Uri(inTargetFolderPath).LocalPath;
			System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(inConvertFolderPath);

			// Convert all files in folder.
			FileConverter fileConvertor = InstancesManager.GetObject();
			conversionSucceed = WalkDirectoryTree(fileConvertor, root, inTargetFolderPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inConvertFolderPath.Equals(inTargetFolderPath), inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			InstancesManager.PutObject(fileConvertor);
			
			return conversionSucceed;
		}

	#endregion

	#region Help Method

		/// <summary>
		/// Parameters preparation before conversion.
		/// </summary>
		/// <param name="inConvertFilePath"></param>
		/// <param name="inNewFileTargetPath"></param>
		/// <returns></returns>
		private string PreFileConvert(string inConvertFilePath, string inNewFileTargetPath)
		{
			// Generate new file type name.
			string fileName = GetFileName(inConvertFilePath) + "-%d.jpg";

			// Concatenate target path with file name.
			string OutputFileFullPath = inNewFileTargetPath + "\\" + fileName;

			return OutputFileFullPath.Replace("\\", "\\\\");
		}


		/// <summary>
		/// Extracting file name from inConvertFilePath.
		/// </summary>
		/// <param name="inConvertFilePath"></param>
		/// <returns></returns>
		private string GetFileName(string inConvertFilePath)
		{
			int lastDoubleSlashIndex = inConvertFilePath.LastIndexOf("\\");

			int inWildCardLength = inConvertFilePath.Length - inConvertFilePath.LastIndexOf(".");

			int fileNameLastIndex = inConvertFilePath.Length - inWildCardLength - 1;

			int fileNameLength = fileNameLastIndex - lastDoubleSlashIndex;

			return inConvertFilePath.Substring(lastDoubleSlashIndex + 1, fileNameLength);
		}

		/// <summary>
		/// Check parameters validation.
		/// </summary>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
		/// <returns></returns>
		private void CheckParamValidation(double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			if (inResolutionX <= 0 || inResolutionY <= 0)
			{
				throw new ArgumentException("Resolution cannot be <= 0");
			}
			else if (!(inGraphicsAlphaBitsValue == 1 || inGraphicsAlphaBitsValue == 2 || inGraphicsAlphaBitsValue == 4))
			{
				throw new ArgumentException("GraphicsAlphaBits values are 1, 2 or 4");
			}
			else if (!(inTextAlphaBitsValue == 1 || inTextAlphaBitsValue == 2 || inTextAlphaBitsValue == 4))
			{
				throw new ArgumentException("TextAlphaBits values are 1, 2 or 4");
			}
			else if (inQuality < 0 || inQuality > 100)
			{
				throw new ArgumentException("File quality range is 0-100");
			}
		}

		/// <summary>
		/// Walking traverse all folders under inRoot looking for appropriate files which their type need to convert to inNewFileType. 
		/// While find one convert it and put it under the same folder if inSameTrgetFolder==true, otherwise creating a folder under inTargetFolderPath with the
		/// same name as the original file located on.
		/// </summary>
		/// <param name="inFileConvertor"></param>
		/// <param name="inRoot"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"></param>
		/// <param name="inSearchSubFolders"></param>
		/// <param name="inSameTrgetFolder"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		private bool WalkDirectoryTree(FileConverter inFileConvertor, System.IO.DirectoryInfo inRoot, string inTargetFolderPath, string inConvertFileWildCard, bool inDeleteSourcePDF, bool inSearchSubFolders, bool inSameTrgetFolder, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			bool fileConversion;

			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			try
			{
				files = inRoot.GetFiles(inConvertFileWildCard);
			}
			catch (System.IO.DirectoryNotFoundException e)
			{
				Console.WriteLine(e.Message);
			}

			if (files != null)
			{
				foreach (System.IO.FileInfo file in files)
				{
					// Make file conversion.
					string inConvertFilePath = file.FullName;
					string OutputFileFullPath = PreFileConvert(inConvertFilePath, inTargetFolderPath);
					inConvertFilePath = inConvertFilePath.Replace("\\", "\\\\");
					fileConversion = inFileConvertor.Convert(inConvertFilePath, OutputFileFullPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
					if(!fileConversion)
						return false;

					//Delete old files.
					if (inDeleteSourcePDF)
					{
						file.Delete();
					}

					// Rename JPG names to the correct page counter.
					RenameJPGNames(inTargetFolderPath, file.FullName);
				}

				if (inSearchSubFolders)
				{
					// Now find all the subdirectories under this directory.
					subDirs = inRoot.GetDirectories();
					foreach (System.IO.DirectoryInfo dirInfo in subDirs)
					{
						if (!inSameTrgetFolder)
						{
							//Create a new sub folder under target folder path
							string newPath = System.IO.Path.Combine(inTargetFolderPath, dirInfo.Name);
							//Create the sub folder
							System.IO.Directory.CreateDirectory(newPath);
							//Recursive call for each subdirectory.
							WalkDirectoryTree(inFileConvertor, dirInfo, newPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTrgetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}
						else
						{
							// Recursive call for each subdirectory.
							WalkDirectoryTree(inFileConvertor, dirInfo, dirInfo.FullName, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTrgetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}

					}
				}

			}

			return true;
		}

		/// <summary>
		/// Rename JPG names to the correct page counter.
		/// </summary>
		/// <param name="inFileDir">Target folder path</param>
		/// <param name="inFileFullName">File full path name</param>
		private void RenameJPGNames(string inFileDir, string inFileFullName)
		{
			string fileNameWithoutCounter = GetFileName(inFileFullName) + "*";
			
			string[] filesNameWithTheSamePrefix = Directory.GetFiles(inFileDir, fileNameWithoutCounter);

			int filesCounter = 1;
			foreach (string fileName in filesNameWithTheSamePrefix)
			{
				if (fileName.Contains(".jpg"))
				{
					string pageNumberOutputFormat = GeneratePageNumberOutputFormat(filesCounter);
					string fileNewName = inFileDir + "\\" + GetFileName(inFileFullName) + pageNumberOutputFormat + filesCounter + ".jpg";
					// Rename file.
					File.Move(fileName, fileNewName);
					filesCounter++;
				}
			}
		}

		private string GeneratePageNumberOutputFormat(int inFilesCounter)
		{
			if (inFilesCounter >= 1 && inFilesCounter <= 9)
				return "_p00";
			else if (inFilesCounter >= 10 && inFilesCounter <= 99)
				return "_p0";
			else if (inFilesCounter >= 100 && inFilesCounter <= 999)
				return "_p";

			return null;
		}

	#endregion

	}
}
