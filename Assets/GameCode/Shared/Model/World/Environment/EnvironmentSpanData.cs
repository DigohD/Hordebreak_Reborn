using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Environment 
{

	[XmlType("EnvironmentSpanData")]
	public class EnvironmentSpanData
	{
		[XmlElement("environmentRef")]
		public string environmentRef  { get; set;}

		[XmlElement("lowPoint")]
		public float lowPoint { get; set; }

		[XmlElement("highPoint")]
		public float highPoint { get; set; }

        public bool IsValueInSpan(float value)
        {
            return value <= highPoint && value >= lowPoint;
        }

        public float GetOptimalModifier(float value)
        {
            if(highPoint == lowPoint)
            {
                Debug.LogError("Span is zero. This is impossible!");
                return 0;
            }

            float range = highPoint - lowPoint;
            float relativePoint = value - lowPoint;
            float midPoint = range / 2;

            return 1 - (Mathf.Abs(relativePoint - midPoint) / midPoint);
        }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName)
        {
            XMLValidation.ValidateId<EnvironmentData>(
                    errorMessages,
                    environmentRef,
                    false,
                    fileName,
                    parentId,
                    "EnvironmentSpanData environmentRef",
                    compName
            );
        }
    }
}