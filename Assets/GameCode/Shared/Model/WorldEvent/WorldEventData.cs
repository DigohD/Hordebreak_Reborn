using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FNZ.Shared.Model.World.Site;

namespace FNZ.Shared.Model.WorldEvent 
{
	[XmlType("WorldEventType")]
	public enum WorldEventType
	{
		[XmlEnum("Survival")]
		Survival,
		
		[XmlEnum("ReplaceTileObject")]
		ReplaceTileObject,
		
		[XmlEnum("ConstantSpawningAmbush")]
		ConstantSpawningAmbush,

		[XmlEnum("DropPod")]
		DropPod,
	};

	[XmlType("WorldEventData")]
	public class WorldEventData : DataDef
	{
		[XmlElement("onSuccessEventRef")]
		public string OnSuccessEventRef { get; set; }
		
		[XmlElement("successNotificationColorRef")]
		public string SuccessNotificationColorRef { get; set; }
		
		[XmlElement("successNotificationTextRef")]
		public string SuccessNotificationTextRef { get; set; }

		[XmlElement("successIconRef")]
		public string SuccessIconRef { get; set; }

		[XmlElement("onFailureEventRef")]
		public string OnFailureEventRef { get; set; }
		
		[XmlElement("failedNotificationColorRef")]
		public string FailedNotificationColorRef { get; set; }
		
		[XmlElement("failedNotificationTextRef")]
		public string FailedNotificationTextRef { get; set; }
		
		[XmlElement("failedIconRef")]
		public string FailedIconRef { get; set; }
		
		[XmlElement("transformedEntityRef")]
		public string TransformedEntityRef { get; set; }
		
		[XmlElement("eventType")]
		public WorldEventType EventType { get; set; }
		
		[XmlElement("effectRef")]
		public string EffectRef { get; set; }
		
		[XmlElement("enemySpawnEffectRef")]
		public string EnemySpawnEffectRef { get; set; }
		
		[XmlElement("playerRangeRadius")]
		public int PlayerRangeRadius { get; set; }
		
		[XmlElement("spawnRadius")]
		public int SpawnRadius { get; set; }
		
		[XmlElement("nameRef")]
		public string NameRef { get; set; }
		
		[XmlElement("descriptionRef")]
		public string DescriptionRef { get; set; }

		[XmlElement("duration")]
		public float Duration { get; set; }

		[XmlElement("difficulty")]
		public byte Difficulty { get; set; }

		[XmlArray("enemies")]
		[XmlArrayItem(typeof(EnemySpawnData))]
		public List<EnemySpawnData> Enemies { get; set; }

		[XmlArray("rewards")]
		[XmlArrayItem("itemRef", typeof(string))]
		public List<string> Rewards { get; set; }

		[XmlElement("spawnFrequency")]
		public float SpawnFrequency { get; set; }
		
		[XmlElement("spawnBudget")]
		public int SpawnBudget { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();
			return true;
		}
	}
}