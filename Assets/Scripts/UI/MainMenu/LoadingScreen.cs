using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {

    [SerializeField]
    private RectTransform m_screen;

    public static LoadingScreen _instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(this.gameObject);
        }
        _instance = this;
        Hide();
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>
    /// Shows the loading screen
    /// </summary>
    public void Show()
    {
        m_screen.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the loading screen
    /// </summary>
    public void Hide()
    {
        m_screen.gameObject.SetActive(false);
    }
}
