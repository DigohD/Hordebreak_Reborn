using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.SFX
{
	[XmlType("SFXData")]
	public class SFXData : DataDef
	{
		[XmlElement("filePath")]
		public string filePath { get; set; }

		[XmlElement("distance")]
		public float distance { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(filePath))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"resourcePath for {Id} must exist and must not be empty."));
			else
			{
				var fileExtension = Path.GetExtension($"{Application.streamingAssetsPath}/{filePath}");

				if (fileExtension != ".wav")
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
						$"resourcePath '{filePath}' for {Id} did not return a valid audio file."));
			}

			return errorMessages.Count > 0;
		}
	}
}