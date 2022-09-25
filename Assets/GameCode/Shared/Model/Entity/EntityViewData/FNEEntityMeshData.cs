using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.EntityViewData
{
    [XmlType("FNEEntityMeshData")]
    public class FNEEntityMeshData : DataDef
    {
        [XmlElement("meshPath")]
        public string MeshPath { get; set; }

        [XmlElement("albedoPath")]
        public string AlbedoPath { get; set; }

        [XmlElement("maskMapPath")]
        public string MaskMapPath { get; set; }

        [XmlElement("normalPath")]
        public string NormalPath { get; set; }

        [XmlElement("emissivePath")]
        public string EmissivePath { get; set; }

        [XmlElement("emissiveFactor")] 
        public float EmissiveFactor { get; set; } = 1.0f;

        public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
        {
            errorMessages = new List<Tuple<string, string>>();

            XMLValidation.ValidateMeshPath(
               errorMessages,
               MeshPath,
               fileName,
               Id,
               "meshPath"
            );

            // XMLValidation.ValidateTexturePath(
            //     errorMessages,
            //     AlbedoPath,
            //     false,
            //     fileName,
            //     Id,
            //     "albedoPath"
            // );
            //
            // XMLValidation.ValidateTexturePath(
            //     errorMessages,
            //     MaskMapPath,
            //     true,
            //     fileName,
            //     Id,
            //     "maskMapPath"
            // );
            //
            // XMLValidation.ValidateTexturePath(
            //     errorMessages,
            //     NormalPath,
            //     true,
            //     fileName,
            //     Id,
            //     "normalPath"
            // );
            //
            // XMLValidation.ValidateTexturePath(
            //     errorMessages,
            //     EmissivePath,
            //     true,
            //     fileName,
            //     Id,
            //     "emissivePath"
            // );

            return true;
        }
    }
}