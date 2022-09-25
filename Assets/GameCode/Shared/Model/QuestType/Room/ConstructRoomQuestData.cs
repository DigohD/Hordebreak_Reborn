using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.Room
{

	public enum RoomQuestType
	{
		[XmlEnum("Size")]
		SIZE = 0,
		[XmlEnum("Property")]
		PROPERTY = 1
	}

	[XmlType("ConstructRoomQuestData")]
	public class ConstructRoomQuestData : QuestTypeData
	{
		[XmlElement("RoomQuestType")]
		public RoomQuestType roomQuestType;

		[XmlElement("width")]
		public byte width;

		[XmlElement("height")]
		public byte height;

		[XmlElement("propertyRef")]
		public string propertyRef;

		[XmlElement("propertyLevel")]
		public byte propertyLevel;
	}
}