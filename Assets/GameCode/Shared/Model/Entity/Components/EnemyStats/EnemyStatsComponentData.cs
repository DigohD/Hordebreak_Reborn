using FNZ.Shared.Model.Entity.Components.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.EnemyStats
{
	public enum EnemyBehaviour
	{
		Chill,		//Default state.
		Aggressive,	//Actively seeks out target destination.
		Fleeing,	//Actively moves away from target destination.
		HitAndRun,	//Given certain parameters, switches between Aggressive and Hiding.
		Hiding,		//Finds a hiding spot and moves there, otherwise it flees.
		Investigate	//Exclusive to HeardSound function.
	}

	[XmlType("EnemyStatsComponentData")]
	public class EnemyStatsComponentData : DataComponent
	{
		[XmlElement("behaviour")]
		public EnemyBehaviour behaviour;

		[XmlElement("agentRadius")]
		public float agentRadius;
		
		[XmlElement("aggroRange")]
		public int aggroRange;

		[XmlElement("hitboxRadius")]
		public float hitboxRadius;

		[XmlElement("scale")]
		public float scale;

		[XmlElement("minSpeed")]
		public float minSpeed;

		[XmlElement("maxSpeed")]
		public float maxSpeed;
		
		[XmlElement("lungeDistance")]
		public float lungeDistance = 0.0f;

		[XmlElement("lungeSpeedMod")]
		public float lungeSpeedMod = 1.0f;

		[XmlElement("projectileType")]
		public string projType = "";

		[XmlElement("damage")]
		public float damage;
		
		[XmlElement("damageTypeRef")]
		public string damageTypeRef;

		[XmlElement("attackCooldown")]
		public float attackCooldown;

		[XmlElement("attackRange")]
		public float attackRange;

		[XmlElement("attackTimestamp")]
		public float attackTimestamp;

		[XmlElement("slowFactor")]
		public float slowFactor;

		[XmlElement("slowTime")]
		public float slowTime;

		[XmlElement("stunTime")]
		public float stunTime;

		[XmlElement("budgetCost")]
		public ushort budgetCost { get; set; } = 0;

		public override Type GetComponentType()
		{
			return typeof(EnemyStatsComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            return true;
        }
    }
}
