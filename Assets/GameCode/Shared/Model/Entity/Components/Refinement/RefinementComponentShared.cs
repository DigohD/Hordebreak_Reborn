using FNZ.Shared.Model.Items;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.Refinement
{
	public class RefinementComponentShared : FNEComponent
	{
		public RefinementComponentData Data
		{
			get
			{
				return (RefinementComponentData) base.m_Data;
			}
		}

		protected RefinementRecipeData m_ActiveRecipe = null;
		protected Item[,] slots = new Item[3, 5];

		protected int m_BurnTicks;
		protected int m_ProcessTicks;
		protected bool m_IsWorking;

		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(m_ActiveRecipe != null);
			if (m_ActiveRecipe != null)
			{
				bw.Write(IdTranslator.Instance.GetIdCode<RefinementRecipeData>(m_ActiveRecipe.Id));
			}

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					bw.Write(slots[i, j] != null);
					if (slots[i, j] != null)
					{
						slots[i, j].Serialize(bw);
					}
				}
			}
		}

		public override void Deserialize(NetBuffer br)
		{
			if (br.ReadBoolean())
			{
				var recipeId = IdTranslator.Instance.GetId<RefinementRecipeData>(br.ReadUInt16());
				m_ActiveRecipe = DataBank.Instance.GetData<RefinementRecipeData>(recipeId);
			}
			
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					if (br.ReadBoolean())
					{
						slots[i, j] = Item.GenerateItem(br);
					}
				}
			}
		}

		public override ushort GetSizeInBytes() { return 0; }
	}
}
