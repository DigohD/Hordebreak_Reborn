using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.VFX
{
	[XmlType("VFXData")]
	public class VFXData : DataDef
	{
		[XmlElement("prefabPath")]
		public string prefabPath { get; set; }

		[XmlElement("assetBundlePath")]
		public string assetBundlePath { get; set; }

		[XmlElement("assetBundleName")]
		public string assetBundleName { get; set; }

		[XmlElement("effectName")]
		public string effectName { get; set; }

		[XmlElement("heightPosition")]
		public float heightPos { get; set; }

		[XmlElement("effectScale")]
		public float scale { get; set; } = 1f;

		[XmlElement("lifetime")]
		public float lifetime { get; set; } = 1f;

		[XmlElement("lightData")]
		public VFXLightData lightData { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (assetBundleName != null)
			{
				if (string.IsNullOrEmpty(assetBundlePath))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"assetBundlePath for {Id} must exist and must not be empty."));
				else
				{
					// if (!AssetBundleLoader.IsAssetBundleLoaded(assetBundleName))
					// {
					// 	if (!AssetBundleLoader.LoadAssetBundleFromPath(assetBundlePath))
					// 		errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"assetBundlePath for {Id} did not return a valid AssetBundle."));
					// }
				}

				if (string.IsNullOrEmpty(effectName))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"effectName for {Id} must exist and must not be empty."));
				else
				{
					// if (!AssetBundleLoader.IsAssetBundleLoaded(assetBundleName))
					// 	AssetBundleLoader.LoadAssetBundleFromPath(assetBundlePath);
					//
					// if (AssetBundleLoader.LoadAssetFromAssetBundle(assetBundleName, effectName) == null)
					// 	errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
					// 		$"effectName '{effectName}' was not found in AssetBundle '{assetBundleName}'"));
				}
			}
			else
			{
				if (Resources.Load<GameObject>(prefabPath) == null)
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"prefabPath for {Id} didn't return a prefab."));
			}

			return errorMessages.Count > 0;
		}
	}
}