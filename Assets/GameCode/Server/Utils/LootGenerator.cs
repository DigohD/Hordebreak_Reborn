using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FNZ.Server.Utils
{
	public class LootGenerator
	{
		public static List<Item> GenerateLoot(LootTableData table)
		{
			
			if (table.lootChanceInPercent > 0 && FNERandom.GetRandomIntInRange(0, 100) > table.lootChanceInPercent)
			{
				// Quick code. Yes, it hurts my soul as well.
				return new List<Item>();
			}

			int totalWeight = 0;

			var rollRandom = FNERandom.GetRandomIntInRange(table.minRolls, table.maxRolls + 1);

			foreach (var item in table.table)
			{
				totalWeight += item.probability;
			}

			Dictionary<string, Item> lootDict = new Dictionary<string, Item>();

			for (int i = 0; i < rollRandom; i++)
			{
				var itemRandom = FNERandom.GetRandomIntInRange(1, totalWeight + 1);

				foreach (var lootEntry in table.table)
				{
					if (itemRandom <= lootEntry.probability)
					{
						var itemId = lootEntry.itemRef;

						if (!lootDict.ContainsKey(itemId))
						{
							lootDict.Add(itemId, Item.GenerateItem(itemId, 1, true));
						}
						else
						{
							if (lootEntry.unique)
							{
								itemRandom -= lootEntry.probability;
								continue;
							}

							lootDict[itemId].amount++;
						}
						break;
					}
					else
					{
						itemRandom -= lootEntry.probability;
					}
				}
			}

			foreach (var entry in table.table)
			{
				var itemId = entry.itemRef;

				bool guaranteedLoot = entry.guaranteed;

				if (guaranteedLoot && !lootDict.ContainsKey(itemId))
				{
					lootDict.Add(itemId, Item.GenerateItem(itemId, 1, true));
				}
			}

			return lootDict.Values.ToList();
		}
	}
}