using System;
using UnityEngine.Serialization;

namespace Siccity.GLTFUtility
{
	[Serializable]
	public class ImportSettings
	{
		public bool materials = true;
		[FormerlySerializedAs("shaders")]
		public ShaderSettings shaderOverrides = new ShaderSettings();
		public bool useLegacyClips;
	}
}