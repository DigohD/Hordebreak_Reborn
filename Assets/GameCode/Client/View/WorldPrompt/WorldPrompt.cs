using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.WorldPrompts
{

	public class WorldPrompt : MonoBehaviour
	{
        [SerializeField]
        private TextMeshPro m_Text;

        private readonly float m_RiseSpeed = 1.8f;
        private float m_Lifetime = 1.2f;

        public void Init(string text, float2 position)
        {
            m_Text.text = text;

            transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, -1.2f);
            transform.LookAt(UnityEngine.Camera.main.transform, Vector3.back);
        }

        
        void Update()
	    {
            m_Lifetime -= Time.deltaTime;
            if(m_Lifetime < 0)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * -0.5f, Time.deltaTime * 5f);
                if (transform.localScale.x <= 0)
                    Destroy(gameObject);
            }

            transform.Translate(Vector3.back * m_RiseSpeed * Time.deltaTime, Space.World);
	    }
	}
}