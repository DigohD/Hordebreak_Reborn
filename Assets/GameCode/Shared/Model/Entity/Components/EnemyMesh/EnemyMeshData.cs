using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.EnemyMesh 
{

	[XmlType("EnemyMeshData")]
	public class EnemyMeshData : DataDef
	{
		[XmlElement("path")]
		public string path { get; set; }

		[XmlElement("albedoPath")]
		public string albedoPath { get; set; }

		[XmlElement("maskMapPath")]
		public string maskMapPath { get; set; }

		[XmlElement("normalPath")]
		public string normalPath { get; set; }

		[XmlElement("emissivePath")]
		public string emissivePath { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

            XMLValidation.ValidateMeshPath(
                errorMessages,
                path,
                fileName,
                Id,
                "path"
            );

            XMLValidation.ValidateTexturePath(
                errorMessages,
                albedoPath,
                false,
                fileName,
                Id,
                "albedoPath"
            );

            XMLValidation.ValidateTexturePath(
                errorMessages,
                normalPath,
                true,
                fileName,
                Id,
                "normalPath"
            );

            XMLValidation.ValidateTexturePath(
                errorMessages,
                maskMapPath,
                true,
                fileName,
                Id,
                "maskMapPath"
            );

            XMLValidation.ValidateTexturePath(
                errorMessages,
                emissivePath,
                true,
                fileName,
                Id,
                "emissivePath"
            );

			return true;
		}
	}
}