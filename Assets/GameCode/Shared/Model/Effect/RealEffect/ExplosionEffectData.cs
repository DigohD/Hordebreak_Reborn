using System.Xml.Serialization;

namespace FNZ.Shared.Model.Effect.RealEffect
{
	[XmlType("ExplosionEffectData")]
	public class ExplosionEffectData : RealEffectData
	{
		public ExplosionEffectData()
		{
			minRadius = 2f;
			maxRadius = 2f;
			targetsEnemies = true;
			targetsPlayers = true;
		}

		[XmlElement("minRadius")]
		public float minRadius { get; set; }

		[XmlElement("maxRadius")]
		public float maxRadius { get; set; }

		[XmlElement("damage")]
		public float damage { get; set; }

		[XmlElement("healing")]
		public float healing { get; set; }

		[XmlElement("damageTypeRef")]
		public string damageTypeRef { get; set; }

		[XmlElement("targetsPlayers")]
		public bool targetsPlayers { get; set; }

		[XmlElement("targetsEnemies")]
		public bool targetsEnemies { get; set; }

		[XmlElement("onHitEffectRef")]
		public string onHitEffectRef { get; set; }

	}
}