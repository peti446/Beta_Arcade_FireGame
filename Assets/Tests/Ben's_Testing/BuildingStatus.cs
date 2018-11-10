using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingStatus : MonoBehaviour
{
    public Renderer rend; //used for testing
    public Color alt_colour = Color.red;

    private float building_health;
    private float building_max_health;

    //Building burn amount/health bars
    public Image health_bar;
    public Image health_bar_bg;
    public TextMeshProUGUI health_counter;

    //Building Wet bars
    public Image wet_bar;
    public Image wet_bar_bg;

    //Fire setting bars
    public Image setting_bar_bg;
    public Image setting_bar;
    public TextMeshProUGUI fire_setting_counter;

    //Time it takes to light building on fire
    public float fire_start_time = 1.2f;
    float time_left;

    public GameObject fire_started_text;
    public GameObject damp_building_text;
    bool ablaze = false;
    bool on_fire = false;
    bool dampening = false;
    bool planting = false;
    public float damp_time = 10.0f;
    public int setting_percent;

    public float burn_time;
    public float ablaze_burn_time;

    private void Start()
    {
        building_health = 100;
        building_max_health = 100;
        time_left = fire_start_time;
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ablaze_burn_time = burn_time;
        //if building is on fire, trigger burning
        if (on_fire == true)
        {
            BurningPhase();
        }
        // Starting Fire
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Status Script Working.");
            planting = true;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            Debug.Log("F released");
            planting = false;
        }
        // Fire Burn Timer
        if (ablaze_burn_time > 10.0f)
        {
            isAblaze();
        }
        // Ablaze effects
        if (ablaze == true)
        {
            if(on_fire)
            {
                building_health -= (Time.deltaTime / 2f);
                rend.material.color = alt_colour;
                if (health_bar.isActiveAndEnabled == false)
                {
                    health_bar.gameObject.SetActive(true);
                    health_bar_bg.gameObject.SetActive(true);
                }
            }
            else
            {
                building_health -= (1.0f * (Time.deltaTime / 2));
            }

        }
        if(ablaze == false && on_fire == false && dampening == false)
        {
            BuildingRegen();
        }
        if(dampening == true)
        {
            damp_time -= Time.deltaTime;
            wet_bar_bg.gameObject.SetActive(true);
            wet_bar.gameObject.SetActive(true);
            wet_bar.fillAmount = damp_time / 7.0f;
            if (damp_time < 8.0f)
            {
                damp_building_text.SetActive(false);
            }

            //wet_bar_bg.gameObject.SetActive(false);
           // wet_bar.gameObject.SetActive(false);
        }
    }

    public void StartingFire()
    {
        if (dampening == false)
        {
            damp_time = 10.0f;
            //Activate fire starting bars
            setting_bar_bg.gameObject.SetActive(true);
            setting_bar.gameObject.SetActive(true);

            //setting_percent = ((int)time_left / (int)fire_start_time) * 100;
            fire_setting_counter.gameObject.GetComponent<TextMeshProUGUI>().SetText("Lighting fire in: " + (int)time_left);
            time_left -= Time.deltaTime;
            setting_bar.fillAmount = time_left / 1.2f; //fire_start_time;


            //if time depletes
            if (time_left < 0)
            {
                //display fire text
                fire_started_text.SetActive(true);
                //disable fire starting bars
                setting_bar_bg.gameObject.SetActive(false);
                setting_bar.gameObject.SetActive(false);
                //Set fire bool to true
                on_fire = true;
            }
        }
        if (dampening == true)
        {
            damp_building_text.SetActive(true);
            
            if (damp_time < 8)
            {
                damp_building_text.SetActive(false);
                dampening = false;

            }
        }

    }

    public void BurningPhase()
    {
        //Activate building health/burn amount bars
        health_bar.gameObject.SetActive(true);
        health_bar_bg.gameObject.SetActive(true);

        building_health -= Time.deltaTime;
        health_bar.fillAmount = building_health / building_max_health;
        health_counter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)building_health + "%");
        burn_time += Time.deltaTime;


        //If building health is between 66 and 33
        if (building_health < 66 && building_health > 33)
        {
            //Change building
            gameObject.transform.localScale = new Vector3(1.5f, 8.5f, 1.5f);
        }
        //If building health is between 33 and 0
        if (building_health < 33 && building_health > 0)
        {
            //Change building
            gameObject.transform.localScale = new Vector3(1.5f, 7.0f, 1.5f);
        }
        //If building health depletes
        if (building_health <= 0)
        {
            //Destroy building & deactivate health bars
            gameObject.transform.localScale = new Vector3(1.5f, 5.5f, 1.5f);
            ///////////////////////////////health_bar.gameObject.SetActive(false);
            ///////////////////////////////health_bar_bg.gameObject.SetActive(false);
        }
    }

    public void Extinguish()
    {
        //set on fire to false
        on_fire = false;
        //Deactivate health bars
       //////////////////////////////// health_bar.gameObject.SetActive(false);
       //////////////////////////////// health_bar_bg.gameObject.SetActive(false);
        //reset ablaze
        ablaze = false;
        ablaze_burn_time = 0;
        //Reset fire lighting timer
        time_left = 1.2f;
        damp_time -= Time.deltaTime;
        //wetState();
    }

    public void StopLighting()
    {
        setting_bar.gameObject.SetActive(false);
        setting_bar_bg.gameObject.SetActive(false);
        time_left = 1.2f;
    }

    public void BuildingRegen()
    {
        if(building_health < 100)
        {
            building_health += 1.0f * (Time.deltaTime / 1.5f);
            health_bar.fillAmount = building_health/building_max_health;
            health_counter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)building_health + "%");
        }
    }

    public void isAblaze()
    {
        ablaze = true;
    }

}
