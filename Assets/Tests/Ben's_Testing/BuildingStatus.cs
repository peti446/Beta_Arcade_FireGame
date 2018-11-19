using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class BuildingStatus : NetworkBehaviour
{
    //Used for testing
    [SerializeField]
    private Renderer rend;
    [SerializeField]
    private Color alt_colour = Color.red;
    [SerializeField]
    private Color std_colour = Color.white;

    //Building health and max health
    [SyncVar(hook ="OnBuidlingHPUpdate")]
    private float building_health;

    private void OnBuidlingHPUpdate(float _newHP)
    {
        building_health = _newHP;
        //Activate building health/burn amount bars
        health_bar.gameObject.SetActive(true);
        health_bar_bg.gameObject.SetActive(true);
        health_bar.fillAmount = building_health / building_max_health;
        //Display health counter
        health_counter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)building_health + "%");
    }

    [SerializeField]
    private float building_max_health;

    //Building burn amount/health bars
    [SerializeField]
    private Image health_bar;
    [SerializeField]
    private Image health_bar_bg;
    [SerializeField]
    private TextMeshProUGUI health_counter;

    //Building Wet bars
    [SerializeField]
    private Image wet_bar;
    [SerializeField]
    private Image wet_bar_bg;

    //Fire setting bars
    [SerializeField]
    private Image setting_bar_bg;
    [SerializeField]
    private Image setting_bar;
    [SerializeField]
    private TextMeshProUGUI fire_setting_counter;

    //Time it takes to light building on fire
    [SerializeField]
    private float fire_start_time = 1.2f;
    [SerializeField]
    private float time_left;

    [SerializeField]
    private GameObject fire_started_text;
    [SerializeField]
    private GameObject damp_building_text;
    [SerializeField]
    private bool ablaze = false;
    [SerializeField]
    private bool on_fire = false;
    [SerializeField]
    private bool dampening = false;
    [SerializeField]
    private bool planting = false;
    [SyncVar]
    private float damp_time = 10.0f;
    [SerializeField]
    private int setting_percent;

    [SerializeField]
    private float burn_time, ablaze_burn_time;

    private void Start()
    {
        //Initialised Values
        building_health = 100;
        building_max_health = 100;
        time_left = fire_start_time;
        rend = GetComponent<Renderer>();
    }

    [ServerCallback]
    void Update()
    {
        ablaze_burn_time = burn_time;
        //if building is on fire, trigger burning
        if (on_fire == true)
        {
            BurningPhase();
        }
       

        // Fire Burn Timer
        if (ablaze_burn_time > 10.0f)
        {
            ablaze = true;
        }
        // Ablaze effects
        if (ablaze == true)
        {
            //if the building is on fire
            if (on_fire)
            {
                //reduce health by time & render red
                building_health -= (Time.deltaTime / 2f);
                rend.material.color = alt_colour;
                //activate health bars
                if (health_bar.isActiveAndEnabled == false)
                {
                    health_bar.gameObject.SetActive(true);
                    health_bar_bg.gameObject.SetActive(true);
                }
            }
            //if building isn't on fire
            else
            {
                //reduce health by 1/sec
                building_health -= (1.0f * (Time.deltaTime / 2f));
            }
        }

        //building regenerate if not on fire/ablaze/damp
        if(ablaze == false && on_fire == false && dampening == false)
        {
            BuildingRegen();
        }

        //If damp
        if(dampening == true)
        {
            //Building health stops changing
            building_health += 0 * Time.deltaTime;
            //Wet time decreases by 1/sec
            damp_time -= Time.deltaTime;
            //Activate wet bars and reeduce fill parallel to wet timer
            wet_bar_bg.gameObject.SetActive(true);
            wet_bar.gameObject.SetActive(true);
            wet_bar.fillAmount = damp_time / 10.0f;
            //If wet time is less than 8 stop showing text
            if (damp_time < 8.0f)
            {
                damp_building_text.SetActive(false);
            }
            //If wet time depletes, stop wet state and hide bars
            if(damp_time <= 0.0f)
            {
                dampening = false;
                wet_bar.gameObject.SetActive(false);
                wet_bar_bg.gameObject.SetActive(false);
            }
        }
    }

    [Client]
    public void EmulateStartingFire()
    {
        if (dampening)
        {
            damp_building_text.SetActive(true);
            if (damp_time < 8)
            {
                damp_building_text.SetActive(false);
            }
            return;
        }
        //Activate fire starting bars
        setting_bar_bg.gameObject.SetActive(true);
        setting_bar.gameObject.SetActive(true);

        //Display setting text with time left to light reducing by 1/sec
        fire_setting_counter.gameObject.GetComponent<TextMeshProUGUI>().SetText("Lighting fire in: " + (int)time_left);
        time_left -= Time.deltaTime;
    }

    [TargetRpc]
    private void TargetStopLighting(NetworkConnection target)
    {
        setting_bar.gameObject.SetActive(false);
        setting_bar_bg.gameObject.SetActive(false);
        time_left = 1.2f;
    }

    [Server]
    public void StartingFire(Character c)
    {
        if(dampening)
        {
            ablaze = false;
            if (damp_time <= 0)
            {
                dampening = false;
            }
        }
        else
        {
            TargetStopLighting(MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient);
            damp_time = 10.0f;

            //Fill bar based on time lighting
            setting_bar.fillAmount = time_left / 1.2f;


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
    }

    [Server]
    private void BurningPhase()
    {
        //Reduce building health by 1/sec & fill health bar based on health/max health
        building_health -= Time.deltaTime;
        //Add to burn timer
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
        }
    }

    [Server]
    public void Extinguish()
    {
        dampening = true;
        //set on fire to false
        on_fire = false;
        //reset ablaze and burn time
        ablaze = false;
        burn_time = 0;
        //Reset fire lighting timer
        time_left = 1.2f;
    }

    private void BuildingRegen() //Not polished - needs designer input
    {
        //If building health is less than 100, heal 1/1.5 secs
        if(building_health < 100 && building_health <= 66)
        {
            building_health += 1.0f * (Time.deltaTime / 1.5f);
            health_bar.fillAmount = building_health/building_max_health;
            health_counter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)building_health + "%");
        }
        //If building health is less than 66, heal 1/1.5 secs up to 66 hp
        if(building_health < 66)
        {
            if(building_health != 66)
            {
                building_health += 1.0f * (Time.deltaTime / 1.5f);
                health_bar.fillAmount = building_health / building_max_health;
            }
            if (building_health == 66)
            {
                building_health += 0;
                health_bar.fillAmount = building_health / building_max_health;
            }
        }
        //If building health is less than 33, heal 1/1.5 secs up to 33 hp
        if(building_health < 33)
        {
            if(building_health != 33)
            {
                building_health += 1.0f * (Time.deltaTime / 1.5f);
                health_bar.fillAmount = building_health / building_max_health;
            }
            if(building_health == 33)
            {
                building_health += 0 * Time.deltaTime;
                health_bar.fillAmount = building_health / building_max_health;
            }
        }
        //If building is destroyed, stop building regeneration
        if(building_health <= 0)
        {
            building_health += 0 * Time.deltaTime;
        }
    }

    public bool IsAblaze
    {
        get
        {
            return ablaze;
        }
    }
}
