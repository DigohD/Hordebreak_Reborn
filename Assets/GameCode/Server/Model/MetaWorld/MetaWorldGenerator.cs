using FNZ.Shared.Model;
using FNZ.Shared.Model.World.MetaWorld;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.MetaWorld 
{
	public class MetaWorldGenerator
	{
		private static readonly int INITIAL_PLACES = 5;
		private static readonly float MIN_RANGE = 0.25f;
		private static readonly float MIN_SPACE_BETWEEM = 0.5f;

		public static List<Place> GenerateStartWorld()
        {
			List<Place> places = new List<Place>();

			for(int i = 0; i < INITIAL_PLACES; i++)
            {
				var place = GeneratePlace(places, 1.0f);
				places.Add(place);
			}

			return places;
        }

		private static Place GeneratePlace(List<Place> currentPlaces, float maxRange)
        {
			var pos = GeneratePlaceCoords(currentPlaces, maxRange);

			var allSites = DataBank.Instance.GetAllDataDefsOfType(typeof(SiteData));

			List<SiteData> possibleSites = new List<SiteData>();

			foreach (var siteDef in allSites)
			{
				var data = DataBank.Instance.GetData<SiteData>(siteDef.Id);
				
				if (!data.showOnMap)
					continue;

				if (data.Difficulty > (maxRange / 10) + 1)
					continue;
				
				possibleSites.Add(data);
			}

			var site = possibleSites[FNERandom.GetRandomIntInRange(0, possibleSites.Count)];

			Place place = new Place(pos, "PLACE: " + Guid.NewGuid().ToString(), site.Id);

			return place;
        }

		private static float2 GeneratePlaceCoords(List<Place> currentPlaces, float maxRange)
        {
			int maxTries = 1000;
			float dist;
            while (maxTries > 0)
            {
				maxTries--;
				dist = FNERandom.GetRandomFloatInRange(MIN_RANGE, maxRange);
				var v = new Vector2(dist, 0);
				var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
				var positionToTest = new float2(0 + finalOffset.x, 0 + finalOffset.y);

				bool toClose = false;
				for(int i = 0; i < currentPlaces.Count; i++)
                {
					var distBetween = math.distance(
						positionToTest, 
						new float2(currentPlaces[i].Coords.x, currentPlaces[i].Coords.y)
					);
					if(distBetween < MIN_SPACE_BETWEEM)
                    {
						toClose = true;
						break;
                    }
                }

                if (!toClose)
                {
					return positionToTest;
				}
			}
			return float2.zero;
		}
	}
}