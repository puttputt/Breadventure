using UnityEngine;
using System.Collections;
using System;

public class ToasterController : MonoBehaviour {

    OTSprite m_Sprite;

    BreadController m_Bread;

    [SerializeField]
    float m_FloatDistance = 10f;

    [SerializeField]
    float m_FloatDistancePerFrame = 1.3f;

    [SerializeField]
    float m_ShakeDistance = 5f;

    [SerializeField]
    float m_ShakeDistancePerFrame = 0.5f;

    bool m_ShakingRight = true;
    bool m_isShaking = false;
    float m_originalY;
    bool m_floatingUp = true;

    ToasterCounter m_CounterRef;

	// Use this for initialization
	void Start () {
	    m_Sprite = GetComponent<OTSprite>();
        m_originalY = m_Sprite.position.y;   
	}

	// Update is called once per frame
	void Update () {
        if (!m_isShaking)
        {
            /* 
             * If the toaster is not shaking, then it means it is floating
             * -If floating up, the position is updated to increment the current y value by m_FloatDistancePerframe
             * if floating down, the Y value is decremented.
             * - The boolean value is updated if either direction exceed the threshold, m_FloatDistance.
             */
            if (m_floatingUp == true)
            {
                m_Sprite.position = new Vector2(m_Sprite.position.x, m_Sprite.position.y + m_FloatDistancePerFrame);
            }
            else
            {
                m_Sprite.position = new Vector2(m_Sprite.position.x, m_Sprite.position.y - m_FloatDistancePerFrame);
            }
            if (m_Sprite.position.y > m_originalY + m_FloatDistance)
            {
                m_floatingUp = false;
            }
            else if (m_Sprite.position.y < m_originalY - m_FloatDistance)
            {
                m_floatingUp = true;
            }
        }
        else
        {
            /*
             * If the toaster is shaking, that means that the bread is inside the toaster.
             * if its shaking in the right direction, rotation is decremented. If shaking in the left direction,
             * the rotation is incremented.
             * The direction is changed based on a thresholf of m_ShakeDistance>
             * */

            if (m_ShakingRight == true)
            {
                m_Sprite.rotation -= m_ShakeDistancePerFrame;
            }
            else
            {
                m_Sprite.rotation += m_ShakeDistancePerFrame;
            }
            if (m_Sprite.rotation < 180 &&  m_Sprite.rotation > m_ShakeDistance)
            {
                m_ShakingRight = true;
            }
            else if (m_Sprite.rotation > 180 && m_Sprite.rotation < 360 - m_ShakeDistance)
            {
                m_ShakingRight = false;
            }
        }
        //Debug.Log("ToasterController::Update() - Sprite Y Position: " + m_Sprite.position.y);
        //Debug.Log("ToasterController::Update() - Sprite Rotation: " + m_Sprite.rotation);
	}

    void OnTriggerEnter(Collider c)
    {
        if (c.tag == "Player")
        {
            //Change sprite to "primed" state
            m_Sprite.frameIndex = 1;

            //Reset position to original
            m_Sprite.position = new Vector2(m_Sprite.position.x, m_originalY);

            m_isShaking = true;
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.tag == "Player")
        {
            //Change sprite to "open" state
            m_Sprite.frameIndex = 0;

            m_isShaking = false;

            //Reset to original rotation, which is 0.
            m_Sprite.rotation = 0;

            //Destroy the toaster once it has been used
            Destroy(gameObject);
        }
    }

}
