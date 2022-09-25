using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.SFX;
using FNZ.Shared.Model.VFX;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Effect
{
	[XmlType("EffectData")]
	public class EffectData : DataDef
	{
		[XmlElement("alertsEnemies")]
		public bool alertsEnemies { get; set; } = false;

		[XmlElement("enemyAlertDistance")]
		public int enemyAlertDistance { get; set; } = 0;

		[XmlElement("delay")]
		public float delay { get; set; } = 0;

		[XmlElement("repetitions")]
		public int repetitions { get; set; } = 1;

		[XmlElement("repetitionTime")]
		public float repetitionTime { get; set; } = 0;

		[XmlElement("vfxRef")]
		public string vfxRef { get; set; }

		[XmlElement("sfxRef")]
		public string sfxRef { get; set; }

		[XmlElement("onBirthEffectRef")]
		public string onBirthEffectRef { get; set; }

		[XmlElement("realEffectData")]
		public RealEffectData RealEffectData { get; set; }

        [XmlElement("screenShake")]
        public float screenShake { get; set; }

        [XmlElement("hasBlood")] 
        public bool hasBlood { get; set; } = false;

        public bool HasRealEffect()
		{
			return RealEffectData != null;
		}

		public Type GetRealEffectDataType()
		{
			return RealEffectData.GetType();
		}

		public T GetRealEffectData<T>() where T : RealEffectData
		{
			return (T)RealEffectData;
		}

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

            XMLValidation.ValidateId<VFXData>(
                errorMessages,
                vfxRef,
                true,
                fileName,
                Id,
                "vfxRef"
            );

            XMLValidation.ValidateId<SFXData>(
               errorMessages,
               sfxRef,
               true,
               fileName,
               Id,
               "sfxRef"
           );

            XMLValidation.ValidateId<EffectData>(
              errorMessages,
              onBirthEffectRef,
              true,
              fileName,
              Id,
              "onBirthEffectRef"
          );

            return errorMessages.Count > 0;
		}
	}
}