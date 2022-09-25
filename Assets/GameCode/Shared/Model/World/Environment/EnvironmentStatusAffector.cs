using System.Xml.Serialization;

namespace FNZ.Shared.Model.World
{
	[XmlType("EnvironmentStatusAffector")]
	public class EnvironmentStatusAffector
	{
		[XmlElement("typeRef")]
		public string typeRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}