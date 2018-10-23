using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingStatus : MonoBehaviour
{

    public GameObject building_state_1;
    public GameObject building_state_2;
    public GameObject building_state_3;
    public GameObject building_state_4;
    private int building_health;
    private int building_max_health;
    private Image health_bar;
    public Image fire_bar;
    public Image fire_barBG;
    public int BuildingHealth { get { return building_health; } }

    public Image setting_bar_bg;
    public Image setting_bar;
    public float fire_start_time = 7.0f;
    float time_left;
    public GameObject fire_started_text;


    private void Start()
    {
        building_health = 100;
        building_max_health = 100;
        //health_bar = transform.Find("BuildingCanvas").Find("HealthBG").Find("Health").GetComponent<Image>();
        time_left = fire_start_time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit()
    {
        if (building_health > 0)
        {
            building_health = building_health - 10;
            health_bar.fillAmount = (float)building_health / (float)building_max_health;
            Debug.Log(building_health);
        }
        else
        {
            Debug.Log("Building Dead");
        }
    }
    public void WaterHit()
    {
        if (building_health < building_max_health)
        {
            building_health = building_health + 10;
            health_bar.fillAmount = (float)building_health / (float)building_max_health;
            Debug.Log(building_health);
        }
        else
        {
            Debug.Log("Max health");
        }
    }
   /* public void FireStart()
    {
        fire_barBG.gameObject.SetActive(true);
        Debug.Log("Fire");
        while (fire_bar.fillAmount > 0)
        {
            fire_bar.fillAmount -= (Time.deltaTime);
        }
        if (fire_bar.fillAmount <= 0)
        {
            fire_bar.gameObject.SetActive(false);
        }
    }*/

    public void StartingFire()
    {
        //time_left = 7.0f;
        setting_bar_bg.gameObject.SetActive(true);
        setting_bar.gameObject.SetActive(true);
        if (time_left > 0.0f)
        {
            time_left -= Time.deltaTime;
            setting_bar.fillAmount = time_left / fire_start_time;
        }
        else
        {
            fire_started_text.SetActive(true);
            setting_bar_bg.gameObject.SetActive(false);
        }
    }
}
