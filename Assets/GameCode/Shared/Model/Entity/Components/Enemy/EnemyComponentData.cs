using FNZ.Shared.Model.Entity.EntityViewData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.Entity.Components.Enemy 
{
	[XmlType("EnemyComponentData")]
	public class EnemyComponentData : DataComponent
	{
		[XmlElement("baseSpeed")]
		public float baseSpeed;

		[XmlElement("enemyMeshRef")]
		public string enemyMeshRef { get; set; }

		[XmlElement("accessoryMeshRef")]
		public string accessoryMeshRef { get; set; }

        [XmlElement("scaleMod")]
        public float scaleMod { get; set; }
        
        [XmlElement("lootTable")]
        public LootTableData lootTable { get; set; }

        public override Type GetComponentType()
		{
			return typeof(EnemyComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                enemyMeshRef,
                false,
                fileName,
                parentId,
                "enemyMeshRef",
                compName
            );

            XMLValidation.ValidateId<FNEEntityMeshData>(
                errorMessages,
                accessoryMeshRef,
                false,
                fileName,
                parentId,
                "accessoryMeshRef",
                compName
            );

            return true;
        }
    }
}