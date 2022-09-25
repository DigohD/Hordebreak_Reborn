using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scenes.RoofPrototype.Code 
{
	public delegate void D_RooftypeChange(RoofType roofType);

	public enum RoofType
    {
		SQUARE = 0,
		RECTANGLE = 1,
		IRREGULAR_RECTANGLE = 2
    }
	public class SceneControl : MonoBehaviour
	{
		public RoofType RoofType = RoofType.SQUARE;
		public D_RooftypeChange d_RoofTypeChange;

		public static bool SymetricXModuloOn = true;
		public static int SymetricXModulo = 2;

		public Transform roofParent;

		public void SelectHouseType(RoofType roofType)
        {
			RoofType = roofType;
			d_RoofTypeChange?.Invoke(roofType);
		}

		public void OnGenerate()
        {
            switch (RoofType)
            {
				case RoofType.SQUARE:
					GenerateSquare();
					break;

				case RoofType.RECTANGLE:
					GenerateRectangle();
					break;

				case RoofType.IRREGULAR_RECTANGLE:
					GenerateIrregularRectangle();
					break;
			}
        }

		private void GenerateSquare()
        {
			Debug.Log("SQUARE");

			foreach(Transform t in roofParent)
            {
				Destroy(t.gameObject);
            }

			int r = Random.Range(1, 7);
			bool[,] net = new bool[r, r];
			for(int i = 0; i < r; i++)
            {
				for(int j = 0; j < r; j++)
                {
					net[i, j] = true;
				}
            }

			GetComponent<HouseGenerator>().GenerateBuilding(roofParent, net);
        }

		private void GenerateRectangle()
        {
			Debug.Log("RECTANGLE");

			foreach (Transform t in roofParent)
			{
				Destroy(t.gameObject);
			}

			int rW = Random.Range(1, 7);
			int rH = Random.Range(1, 7);
			bool[,] net = new bool[rW, rH];
			for (int i = 0; i < rH; i++)
			{
				for (int j = 0; j < rW; j++)
				{
					net[j, i] = true;
				}
			}

			GetComponent<HouseGenerator>().GenerateBuilding(roofParent, net);
		}

		private void GenerateIrregularRectangle()
		{
			Debug.Log("IRREGULAR RECTANGLE");

			foreach (Transform t in roofParent)
			{
				Destroy(t.gameObject);
			}

			int rW = Random.Range(8, 20);
			int rH = Random.Range(8, 20);

			bool[,] net = new bool[rW, rH];
			for (int i = 0; i < rW; i++)
			{
				for (int j = 0; j < rH; j++)
				{
					net[i, j] = true;
				}
			}

			int rW2, rH2;
			if (Random.Range(0, 100) < 40)
            {
				rW2 = Random.Range(1, rW / 2);
				rH2 = Random.Range(1, rH / 2);
				for (int i = 0; i < rW2; i++)
				{
					for (int j = 0; j < rH2; j++)
					{
						net[i, j] = false;
					}
				}
			}

			if (Random.Range(0, 100) < 40)
			{
				rW2 = Random.Range(1, rW / 2);
				rH2 = Random.Range(1, rH / 2);
				for (int i = 0; i < rW2; i++)
				{
					for (int j = 0; j < rH2; j++)
					{
						net[rW - 1 - i, j] = false;
					}
				}
			}

			if (Random.Range(0, 100) < 40)
			{
				rW2 = Random.Range(1, rW / 2);
				rH2 = Random.Range(1, rH / 2);
				for (int i = 0; i < rW2; i++)
				{
					for (int j = 0; j < rH2; j++)
					{
						net[i, rH - 1 - j] = false;
					}
				}
			}

			if (Random.Range(0, 100) < 40)
			{
				rW2 = Random.Range(1, rW / 2);
				rH2 = Random.Range(1, rH / 2);
				for (int i = 0; i < rW2; i++)
				{
					for (int j = 0; j < rH2; j++)
					{
						net[rW - 1 - i, rH - 1 - j] = false;
					}
				}
			}


			GetComponent<HouseGenerator>().GenerateBuilding(roofParent, net);
		}
	}
}