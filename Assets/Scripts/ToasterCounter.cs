using UnityEngine;
using System.Collections;

public class ToasterCounter : MonoBehaviour {

    private int m_count;
    private GUIStyle m_style = new GUIStyle();

	void Start () {
	    m_count = 0;
        m_style.fontSize = 25;
        m_style.normal.textColor = Color.yellow;
	}
	
    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 50), new GUIContent("Toaster Count: " + m_count.ToString()), m_style);
    }

    public void Increment()
    {
        m_count++;
    }
}
