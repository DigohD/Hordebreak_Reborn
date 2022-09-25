using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.SFX;
using FNZ.Shared.Model.World.Atmosphere;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Tile
{
	[XmlType("TileData")]
	public class TileData : DataDef
	{
		[XmlElement("isTransparent")]
		public bool isTransparent { get; set; }

		[XmlElement("TransparentTileData")]
		public TransparentTileData TransparentTileData { get; set; }

		[XmlElement("isWater")]
		public bool isWater { get; set; }

		[XmlElement("WaterTileData")]
		public WaterTileData WaterTileData { get; set; }
		
		[XmlElement("category")]
		public string category { get; set; }

		[XmlElement("albedopath")]
		public string albedopath { get; set; }

		[XmlElement("normalpath")]
		public string normalpath { get; set; }

		[XmlElement("maskmappath")]
		public string maskmappath { get; set; }
		
		[XmlElement("emissivepath")]
		public string emissivepath { get; set; }

		[XmlElement("textureSheet")]
		public string textureSheet { get; set; }

		[XmlElement("textureSheetIndex")]
		public byte textureSheetIndex { get; set; }

		[XmlElement("chanceToSpawnTileObject")]
		public int chanceToSpawnTileObject { get; set; }

		[XmlElement("hardEdges")]
		public bool hardEdges { get; set; }

		[XmlElement("isBlocking")]
		public bool isBlocking { get; set; }

		[XmlElement("overlapPriority")]
		public int overlapPriority { get; set; }

		[XmlArray("tileObjectGenDataList")]
		[XmlArrayItem(typeof(TileObjectGenData))]
		public List<TileObjectGenData> tileObjectGenDataList { get; set; }

		[XmlElement("timeEffectFrequency")]
		public float timeEffectFrequency { get; set; }

		[XmlArray("timeEffects")]
		[XmlArrayItem(typeof(TileTimeEffectData))]
		public List<TileTimeEffectData> timeEffects { get; set; }

		[XmlElement("mapColor")]
		public string mapColor { get; set; }

		[XmlArray("roomPropertyRefs")]
		[XmlArrayItem("propertyRef", typeof(string))]
		public List<string> roomPropertyRefs { get; set; }

        [XmlElement("atmosphereRef")]
        public string atmosphereRef { get; set; }

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

            if (isTransparent)
            {
				if(albedopath != null)
                {
					errorMessages.Add(
						new Tuple<string, string>(
							$"Error in {fileName}",
							$"albedopath of 'def' for '{Id}' should be empty for transparent tiles."
						)
					);
				}
            }
            else
            {
				XMLValidation.ValidateTexturePath(
					errorMessages,
					albedopath,
					false,
					fileName,
					Id,
					"albedoPath"
				);
			}

			

			if (normalpath != null)
			{
				if (isTransparent)
				{
					errorMessages.Add(
						new Tuple<string, string>(
							$"Error in {fileName}",
							$"normalpath of 'def' for '{Id}' should be empty for transparent tiles."
						)
					);
                }
                else
                {
					XMLValidation.ValidateTexturePath(
						errorMessages,
						normalpath,
						false,
						fileName,
						Id,
						"normalpath"
					);
				}
			}

			if (maskmappath != null)
			{
				if (isTransparent)
				{
					errorMessages.Add(
						new Tuple<string, string>(
							$"Error in {fileName}",
							$"maskmappath of 'def' for '{Id}' should be empty for transparent tiles."
						)
					);
				}
                else
                {
					XMLValidation.ValidateTexturePath(
						errorMessages,
						maskmappath,
						false,
						fileName,
						Id,
						"maskmappath"
					);
				}
					
			}

			if (tileObjectGenDataList.Count > 0)
			{
				foreach (var data in tileObjectGenDataList)
				{
					if (!DataBank.Instance.DoesIdExist<FNEEntityData>(data.objectRef))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
							$"objectRef '{data.objectRef}' of {Id} was not found in database."));
				}
			}

			if (timeEffects.Count > 0)
			{
				foreach (var effect in timeEffects)
				{
					if (!DataBank.Instance.DoesIdExist<EffectData>(effect.effectRef))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
								$"effectRef '{effect.effectRef}' of {Id} was not found in database."));
				}
			}

			if (roomPropertyRefs.Count > 0)
			{
				foreach (var prop in roomPropertyRefs)
				{
					if (!DataBank.Instance.DoesIdExist<RoomPropertyData>(prop))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
								$"property '{prop}' of {Id} was not found in database."));
				}
			}

			if (string.IsNullOrEmpty(mapColor))
			{
				errorMessages.Add(new Tuple<string, string>(
					$"Error: {fileName}",
					$"property 'mapColor' of {Id} must be specified."));
			}
			else if(!Regex.IsMatch(mapColor, @"^#[0-9, a-f, A-F]{6}$"))
			{
				errorMessages.Add(new Tuple<string, string>(
					$"Error: {fileName}",
					$"property 'mapColor' of {Id} is not a valid hex color format (#XXXXXX)."));
			}

            XMLValidation.ValidateId<AtmosphereData>(
                errorMessages,
                atmosphereRef,
                true,
                fileName,
                Id,
                "atmosphereRef"
            );

            return errorMessages.Count > 0;
		}
	}
}