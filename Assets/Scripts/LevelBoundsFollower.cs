using UnityEngine;
using System.Collections;

public class LevelBoundsFollower : MonoBehaviour {

    [SerializeField]
    GameObject m_Bread;

    float m_HighestHeight = 0f;

	void Update () {
        if (m_Bread.transform.position.y > m_HighestHeight)
        {
            m_HighestHeight = m_Bread.transform.position.y;
            transform.position = new Vector3(this.transform.position.x, m_HighestHeight, this.transform.position.z);
        }
	}
}
