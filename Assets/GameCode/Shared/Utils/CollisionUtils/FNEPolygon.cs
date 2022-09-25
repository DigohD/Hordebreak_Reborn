using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Utils.CollisionUtils
{
	public class FNEPolygon
	{
		private List<float2> m_Points = new List<float2>();
		private List<float2> m_Edges = new List<float2>();

		public void BuildEdges()
		{
			float2 p1;
			float2 p2;
			m_Edges.Clear();
			for (int i = 0; i < m_Points.Count; i++)
			{
				p1 = m_Points[i];
				if (i + 1 >= m_Points.Count)
				{
					p2 = m_Points[0];
				}
				else
				{
					p2 = m_Points[i + 1];
				}
				m_Edges.Add(p2 - p1);
			}
		}

		public List<float2> Edges
		{
			get { return m_Edges; }
		}

		public List<float2> Points
		{
			get { return m_Points; }
		}

		public float2 Center
		{
			get
			{
				float totalX = 0;
				float totalY = 0;
				for (int i = 0; i < m_Points.Count; i++)
				{
					totalX += m_Points[i].x;
					totalY += m_Points[i].y;
				}

				return new float2(totalX / (float)m_Points.Count, totalY / (float)m_Points.Count);
			}
		}

		public void Offset(float2 v)
		{
			Offset(v.x, v.y);
		}

		public void Offset(float x, float y)
		{
			for (int i = 0; i < m_Points.Count; i++)
			{
				float2 p = m_Points[i];
				m_Points[i] = new float2(p.x + x, p.y + y);
			}
		}

		public override string ToString()
		{
			string result = "";

			for (int i = 0; i < m_Points.Count; i++)
			{
				if (result != "") result += " ";
				result += "{" + m_Points[i].ToString() + "}";
			}

			return result;
		}
	}
}