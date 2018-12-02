using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[RequireComponent(typeof(Interact))]
public class BuildingStatus : NetworkBehaviour
{
    #region UI Elements - All Clients
    //Building burn amount/health bars
    [SerializeField]
    private Image C_HealthBar;
    [SerializeField]
    private Image C_HealthBarBg;
    [SerializeField]
    private TextMeshProUGUI C_HealthCounter;

    //Building Wet bars
    [SerializeField]
    private Image C_WetBar;
    [SerializeField]
    private Image C_WetBarBg;
    #endregion

    #region UI Elements - Only one Client
    [SerializeField]
    private Image C_SettingBarBg;
    [SerializeField]
    private Image C_SettingBar;
    [SerializeField]
    private TextMeshProUGUI C_FireSettingCounter;
    [SerializeField]
    private GameObject C_FireStartedText;
    [SerializeField]
    private GameObject C_DampBuildingText;
    #endregion

    #region All Cients Variables
    //Material for the building
    private Material m_material;
    #endregion


    #region Server SyncVars
    [SyncVar]
    private float FireStartTime = 1.2f;
    [SyncVar]
    private float TimeLeft;
    [SyncVar(hook = "OnBuidlingHPUpdate")]
    private float BuildingHealth;
    [SyncVar]
    private bool Ablaze = false;
    [SyncVar]
    private bool OnFire = false;
    [SyncVar]
    private bool Dampening = false;
    [SyncVar]
    private float DampTime = 10.0f;
    #endregion

    #region Server only variables
    [SerializeField]
    private float BuildingMaxHealth;

    private float BurnTime, AblazeBurnTime;
    #endregion

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GetComponent<Interact>().ClientInteraction.AddListener(EmulateStartingFire);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<Interact>().ServerInteraction.AddListener(ServerStartingFire);
    }

    private void Start()
    {
        //Initialised Values
        BuildingHealth = 100;
        BuildingMaxHealth = 100;
        TimeLeft = FireStartTime;
        m_material = GetComponent<Renderer>().material;
    }


    void Update()
    {
        //Sever and client side update
        if(MainNetworkManager.Is_Server)
        {
            AblazeBurnTime = BurnTime;
            if (OnFire == true)
            {
                ServerBurningPhase();
            }

            if (AblazeBurnTime > 10.0f)
            {
                Ablaze = true;
            }

            if (Ablaze == true)
            {
                //if the building is on fire
                if (OnFire)
                {
                    //reduce health by time & render red
                    BuildingHealth -= (Time.deltaTime / 2f);
                    //activate health bars
                }
                //if building isn't on fire
                else
                {
                    //reduce health by 1/sec
                    BuildingHealth -= (1.0f * (Time.deltaTime / 2f));
                }
            }

            if (Ablaze == false && OnFire == false && Dampening == false)
            {
                ServerBuildingRegen();
            }

            if (Dampening == true)
            {
                //Building health stops changing
                BuildingHealth += 0 * Time.deltaTime;
                //Wet time decreases by 1/sec
                DampTime -= Time.deltaTime;
                //If wet time is less than 8 stop showing text

                //If wet time depletes, stop wet state and hide bars
                if (DampTime <= 0.0f)
                {
                    Dampening = false;
                }
            }
        }
        else
        {
            if (Ablaze == true)
            {
                //if the building is on fire
                if (OnFire)
                {
                    //activate health bars
                    if (C_HealthBar.isActiveAndEnabled == false)
                    {
                        C_HealthBar.gameObject.SetActive(true);
                        C_HealthBarBg.gameObject.SetActive(true);
                    }
                }
            }

            if (Dampening == true)
            {
                //Activate wet bars and reeduce fill parallel to wet timer
                C_WetBarBg.gameObject.SetActive(true);
                C_WetBar.gameObject.SetActive(true);
                C_WetBar.fillAmount = DampTime / 10.0f;
                //If wet time is less than 8 stop showing text
                if (DampTime < 8.0f)
                {
                    C_DampBuildingText.SetActive(false);
                }
                //If wet time depletes, stop wet state and hide bars
                if (DampTime <= 0.0f)
                {
                    C_WetBar.gameObject.SetActive(false);
                    C_WetBarBg.gameObject.SetActive(false);
                }
            }
        }
        #region common code
        //Common code to update the visual aspect
        //Really just mesh updates

        //AblazeBurnTime = BurnTime;
        //if building is on fire, trigger burning
        //if (OnFire == true)
        //{
        //    BurningPhase();
        //}  

        // Fire Burn Timer
        //if (AblazeBurnTime > 10.0f)
        //{
        //Ablaze = true;
        //}
        // Ablaze effects
        //if (Ablaze == true)
        //{
        //    //if the building is on fire
        //    if (OnFire)
        //    {
        //        //reduce health by time & render red
        //        BuildingHealth -= (Time.deltaTime / 2f);
        //        C_Rend.material.color = C_AltColour;
        //        //activate health bars
        //        if (C_HealthBar.isActiveAndEnabled == false)
        //        {
        //            C_HealthBar.gameObject.SetActive(true);
        //            C_HealthBarBg.gameObject.SetActive(true);
        //        }
        //    }
        //    //if building isn't on fire
        //    else
        //    {
        //        //reduce health by 1/sec
        //        BuildingHealth -= (1.0f * (Time.deltaTime / 2f));
        //    }
        //}

        //building regenerate if not on fire/ablaze/damp
        //if(Ablaze == false && OnFire == false && Dampening == false)
        //{
        //    BuildingRegen();
        //}

        //If damp
        //if(Dampening == true)
        //{
        //    //Building health stops changing
        //    BuildingHealth += 0 * Time.deltaTime;
        //    //Wet time decreases by 1/sec
        //    DampTime -= Time.deltaTime;
        //    //Activate wet bars and reeduce fill parallel to wet timer
        //    C_WetBarBg.gameObject.SetActive(true);
        //    C_WetBar.gameObject.SetActive(true);
        //    C_WetBar.fillAmount = DampTime / 10.0f;
        //    //If wet time is less than 8 stop showing text
        //    if (DampTime < 8.0f)
        //    {
        //        C_DampBuildingText.SetActive(false);
        //    }
        //    //If wet time depletes, stop wet state and hide bars
        //    if(DampTime <= 0.0f)
        //    {
        //        Dampening = false;
        //        C_WetBar.gameObject.SetActive(false);
        //        C_WetBarBg.gameObject.SetActive(false);
        //    }
        //}
#endregion
    }

    //TODO: Use Starting fire to set variables and then update

    [Client]
    ///<summary>Shows client lighting fire</summary>
    public void EmulateStartingFire(Character c)
    {
        //Only crazy people can start fire
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople)
        {
            if (Dampening)
            {
                C_DampBuildingText.SetActive(true);
                if (DampTime < 8)
                {
                    C_DampBuildingText.SetActive(false);
                }
                return;
            }
            //Activate fire starting bars
            C_SettingBarBg.gameObject.SetActive(true);
            C_SettingBar.gameObject.SetActive(true);

            //Display setting text with time left to light reducing by 1/sec
            C_FireSettingCounter.gameObject.GetComponent<TextMeshProUGUI>().SetText("Lighting fire in: " + (int)TimeLeft);
            TimeLeft -= Time.deltaTime;
        }
    }

    [TargetRpc]
    ///<<summary>Stops client lighting fire</summary>
    private void TargetStopLighting(NetworkConnection target)
    {
        C_SettingBar.gameObject.SetActive(false);
        C_SettingBarBg.gameObject.SetActive(false);
        TimeLeft = 1.2f;
    }

    [Server]
    ///<<summary>Target building begins to be set on fire</summary>
    public void ServerStartingFire(Character c)
    {
        //Only crazy people can start fire
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople)
        {
            if (Dampening)
            {
                Ablaze = false;
                if (DampTime <= 0)
                {
                    Dampening = false;
                }
                TargetStopLighting(MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient);
            }
            else
            {
                DampTime = 10.0f;

                //Fill bar based on time lighting
                C_SettingBar.fillAmount = TimeLeft / 1.2f;


                //if time depletes
                if (TimeLeft < 0)
                {
                    //display fire text
                    C_FireStartedText.SetActive(true);
                    //disable fire starting bars
                    C_SettingBarBg.gameObject.SetActive(false);
                    C_SettingBar.gameObject.SetActive(false);
                    //Set fire bool to true
                    OnFire = true;
                }
            }
        }
    }

    [Server]
    ///<<summary>Modifies burning building based on building health</summary>
    private void ServerBurningPhase()
    {
        //Reduce building health by 1/sec & fill health bar based on health/max health
        BuildingHealth -= Time.deltaTime;
        //Add to burn timer
        BurnTime += Time.deltaTime;
    }

    [Server]
    ///<summary>Extinguishes fire on building</summary>
    public void ServerExtinguish()
    {
        Dampening = true;
        //set on fire to false
        OnFire = false;
        //reset ablaze and burn time
        Ablaze = false;
        BurnTime = 0;
        //Reset fire lighting timer
        TimeLeft = 1.2f;
    }

    [Server]
    ///<summary>Building gains health based on current health and on it's status</summary>
    private void ServerBuildingRegen() //Not polished - needs designer input
    {
        //If building health is less than 100, heal 1/1.5 secs
        if(BuildingHealth < 100 && BuildingHealth <= 66)
        {
            BuildingHealth += 1.0f * (Time.deltaTime / 1.5f);
        }
        //If building health is less than 66, heal 1/1.5 secs up to 66 hp
        if(BuildingHealth < 66)
        {
            if(BuildingHealth != 66)
            {
                BuildingHealth += 1.0f * (Time.deltaTime / 1.5f);
            }
            if (BuildingHealth == 66)
            {
                BuildingHealth += 0;
            }
        }
        //If building health is less than 33, heal 1/1.5 secs up to 33 hp
        if(BuildingHealth < 33)
        {
            if(BuildingHealth != 33)
            {
                BuildingHealth += 1.0f * (Time.deltaTime / 1.5f);
            }
            if(BuildingHealth == 33)
            {
                BuildingHealth += 0 * Time.deltaTime;
            }
        }
        //If building is destroyed, stop building regeneration
        if(BuildingHealth <= 0)
        {
            BuildingHealth += 0 * Time.deltaTime;
        }
    }

    //Handle change of variable for HP
    private void OnBuidlingHPUpdate(float _newHP)
    {
        BuildingHealth = _newHP;
        //Activate building health/burn amount bars
        C_HealthBar.gameObject.SetActive(true);
        C_HealthBarBg.gameObject.SetActive(true);
        C_HealthBar.fillAmount = BuildingHealth / BuildingMaxHealth;
        //Display health counter
        C_HealthCounter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)BuildingHealth + "%");

        //If building health is between 66 and 33
        if (BuildingHealth < 66 && BuildingHealth > 33)
        {
            //Change building to be in its first burning status
            m_material.SetInt("_BurningTextureIndex", 1);
        }
        //If building health is between 33 and 0
        if (BuildingHealth < 33 && BuildingHealth > 0)
        {
            //Change building to be in its first burning status
            m_material.SetInt("_BurningTextureIndex", 2);
        }
        //If building health depletes
        if (BuildingHealth <= 0)
        {
            //Convert to ashes
            gameObject.transform.localScale = new Vector3(1.5f, 5.5f, 1.5f);
        }
    }
}
