using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Site 
{
	[XmlType("SiteData")]
	public class SiteData : DataDef
	{

		[XmlElement("filePath")]
		public string filePath { get; set; }

		[XmlElement("width")]
		public short width { get; set; } = 0;

		[XmlElement("height")]
		public short height { get; set; } = 0;

		[XmlElement("siteName")]
		public string siteName { get; set; } = "";

		[XmlElement("siteTypeRef")]
		public string siteTypeRef { get; set; } = "";

		[XmlArray("possibleLoot")]
		[XmlArrayItem(typeof(SiteLootData))]
		public List<SiteLootData> possibleLoot { get; set; }

		[XmlElement("mapIconRef")]
		public string mapIconRef { get; set; }

		[XmlElement("showOnMap")]
		public bool showOnMap { get; set; }

		[XmlElement("difficulty")]
		public byte Difficulty { get; set; } = 0;

		[XmlElement("enemyBudget")]
		public ushort enemyBudget { get; set; } = 0;

		[XmlArray("enemySpawning")]
		[XmlArrayItem(typeof(EnemySpawnData))]
		public List<EnemySpawnData> enemySpawning { get; set; }

		[XmlElement("flavourText")]
		public string flavourText { get; set; } = "";
		
		[XmlElement("atmosphereRef")]
		public string atmosphereRef { get; set; } = "";

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(filePath))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"filePath for {Id} must exist and must not be empty."));
			else
			{
				if (!File.Exists(Application.streamingAssetsPath + "/" + filePath))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"filePath '{filePath}' for {Id} does not exist."));
			}

			return errorMessages.Count > 0;
		}
	}
}