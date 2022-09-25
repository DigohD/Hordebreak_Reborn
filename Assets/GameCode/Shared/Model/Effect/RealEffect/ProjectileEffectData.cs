using System.Xml.Serialization;

namespace FNZ.Shared.Model.Effect.RealEffect
{
	[XmlType("ProjectileEffectData")]
	public class ProjectileEffectData : RealEffectData
	{
		public ProjectileEffectData()
		{
			lifetime = 1;
			speed = 10;
			pellets = 1;
			targetsEnemies = true;
			targetsPlayers = true;
		}

		[XmlElement("lifetime")]
		public float lifetime { get; set; }

		[XmlElement("speed")]
		public float speed { get; set; }

		[XmlElement("inaccuracy")]
		public float inaccuracy { get; set; }

		[XmlElement("pellets")]
		public float pellets { get; set; }

		[XmlElement("damage")]
		public float damage { get; set; }

		[XmlElement("healing")]
		public float healing { get; set; }

		[XmlElement("targetsPlayers")]
		public bool targetsPlayers { get; set; }

		[XmlElement("targetsEnemies")]
		public bool targetsEnemies { get; set; }

		[XmlElement("damageTypeRef")]
		public string damageTypeRef { get; set; }

		[XmlElement("projectileVfxRef")]
		public string projectileVfxRef { get; set; }

		[XmlElement("onDeathEffectRef")]
		public string onDeathEffectRef { get; set; }

		[XmlElement("onHitEffectRef")]
		public string onHitEffectRef { get; set; }

		[XmlElement("invertDirection")]
		public bool invertDirection { get; set; }
	}
}