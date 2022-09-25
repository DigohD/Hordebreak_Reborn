using FNZ.Shared.Model;
using FNZ.Shared.Model.Ambience;
using FNZ.Shared.Model.Music;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.GameCode.Shared.Model.GameStateMusicAmbience
{
	[XmlType("GameStateMusicData")]
	public class GameStateMusicData : DataDef
	{
		[XmlArray("music")]
		[XmlArrayItem("sfxRef", typeof(string))]
		public List<string> musicTracks { get; set; }

		[XmlArray("ambience")]
		[XmlArrayItem("sfxRef", typeof(string))]
		public List<string> ambienceTracks { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			XMLValidation.ValidateIdList<MusicData>(errorMessages, musicTracks, true, fileName, Id, "music");
			XMLValidation.ValidateIdList<AmbienceData>(errorMessages, ambienceTracks, true, fileName, Id, "ambience");

			return errorMessages.Count > 0;
		}
	}
}
