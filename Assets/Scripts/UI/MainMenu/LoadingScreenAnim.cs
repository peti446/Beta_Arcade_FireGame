using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenAnim : MonoBehaviour
{

    [SerializeField]
    private Text m_loadingText;

    private bool m_forward;
    private ushort m_maxDotNumber;
    private float m_lastTimeUpdate;

    private void Awake()
    {
        m_lastTimeUpdate = 0;
        m_maxDotNumber = 5;
    }

    private void OnEnable()
    {
        //Reset variables every time we enable this
        m_loadingText.text = "Loading";
        m_forward = true;
    }

    private void LateUpdate()
    {
        //Make sure we only update it once every second
        if (Time.time - m_lastTimeUpdate < 1.0f)
            return;

        //Reset time
        m_lastTimeUpdate = Time.time;
        if (m_forward)
        {
            //Add dots if we are not yet at mox dots, if not remove and set mode to go backwards
            if (m_loadingText.text.Count(x => x == '.') < m_maxDotNumber)
                m_loadingText.text += ".";
            else
            {
                m_loadingText.text = m_loadingText.text.Substring(0, m_loadingText.text.Length - 1);
                m_forward = false;
            }
        }
        else
        {
            //Remove dots until there arent any more, if there are no dots left add one and change to go forward
            if (m_loadingText.text.Count(x => x == '.') > 0)
                m_loadingText.text = m_loadingText.text.Substring(0, m_loadingText.text.Length - 1);
            else
            {
                m_loadingText.text += ".";
                m_forward = true;
            }
        }
    }
}
