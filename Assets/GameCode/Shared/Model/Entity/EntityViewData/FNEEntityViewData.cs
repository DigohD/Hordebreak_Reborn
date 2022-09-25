using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.VFX;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.EntityViewData
{
	[XmlType("AnimationData")]
	public class AnimationData
	{
		[XmlElement("animPath")]
		public string animPath { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName = "def")
        {
            var animations = Importer.LoadAnimationsFromFile($"{Application.streamingAssetsPath}/{animPath}");
            if (animations == null || animations.Length == 0)
                errorMessages.Add(new Tuple<string, string>($"Error: {fileName}",
                    $"Xml entry 'animPath': {animPath} of {parentId} did not return any valid animations."));
        }
	}

	public class FNEEntityViewData : DataDef
	{
		[XmlElement("entityLightSourceData")]
		public EntityLightSourceData entityLightSourceData;

		[XmlElement("entityVfxData")]
		public EntityVFXData entityVfxData;

        [XmlElement("entityMeshData")]
        public string entityMeshData { get; set; }
        
        [XmlElement("entityTextureData")]
        public string entityTextureData { get; set; }

		[XmlElement("emissiveColor")]
		public string emissiveColor { get; set; }

		[XmlElement("isTransparent")]
		public bool isTransparent { get; set; }
		
		[XmlElement("isVegetation")]
		public bool isVegetation { get; set; }

		[XmlElement("onHitEffectRef")]
		public string onHitEffectRef { get; set; }

		[XmlElement("onDeathEffectRef")]
		public string onDeathEffectRef { get; set; }

		[XmlElement("viewIsGameObject")]
		public bool viewIsGameObject { get; set; }

		[XmlElement("scaleMod")]
		public float scaleMod { get; set; } = 1;

		[XmlArray("animations")]
		[XmlArrayItem(typeof(AnimationData))]
		public List<AnimationData> animations { get; set; }

        [XmlElement("heightPosition")]
        public float heightPos { get; set; } = 0;

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (animations.Count > 0)
			{
				foreach (var animData in animations)
				{
                    animData.ValidateXMLData(
                        errorMessages,
                        Id,
                        fileName
                    );
				}
			}

			if (entityVfxData != null)
			{
                entityVfxData.ValidateXMLData(
                    errorMessages,
                    Id,
                    fileName
                );
            }

            if (entityLightSourceData != null)
            {
                entityLightSourceData.ValidateXMLData(
                    errorMessages,
                    Id,
                    fileName
                );
            }

            XMLValidation.ValidateId<EffectData>(
               errorMessages,
               onHitEffectRef,
               true,
               fileName,
               Id,
               "onHitEffectRef"
           );

            XMLValidation.ValidateId<EffectData>(
               errorMessages,
               onDeathEffectRef,
               true,
               fileName,
               Id,
               "onDeathEffectRef"
           );

            XMLValidation.ValidateColor(
                errorMessages,
                emissiveColor,
                true,
                fileName,
                Id,
                "emissiveColor"
            ); 

            XMLValidation.ValidateId<FNEEntityMeshData>(
                 errorMessages,
                 entityMeshData,
                 false,
                 fileName,
                 "FNEEntityMeshData",
                 "entityMeshData",
                 "FNEEntityViewData"
             );

            return errorMessages.Count > 0;
		}
	}
}