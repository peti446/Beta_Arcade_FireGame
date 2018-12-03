using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Linq;

[RequireComponent(typeof(Interact))]
[RequireComponent(typeof(Renderer))]
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
	private float C_TimeLeft;
    #endregion

    #region All Cients Variables
    //Material for the building
    private Material mC_material;
	private bool mC_isStartingFire = false;
	private float mC_startingFireTime = 0;
	#endregion

	#region Varables Client/Server
	[SerializeField]
	private float m_timeToStartFire = 5;
	#endregion

	#region Server SyncVars
	[SyncVar]
	private float DampTime = 0;
    [SyncVar]
    private bool OnFire = false;
    [SyncVar]
    private bool Dampening = false;
	[SyncVar(hook = "OnBuidlingHPUpdate")]
	private float BuildingHealth;
	[SyncVar(hook = "OnBuildingCurrenMaxHPUpdate")]
	private float CurrentBuildingMaxHealth = 100;
	#endregion

	#region Server only variables
	[SerializeField]
	private float mS_fireDPS = 1.0f;
	[SerializeField]
	private float mS_ablazeDPS = 1.0f;
	[SerializeField]
	private float mS_timeBurningForAblaze = 10.0f;
	[SerializeField]
	private int mS_buildingMaxHP = 100;
	[SerializeField]
	private float mS_spreadTime = 5.0f;
	[SerializeField]
	private float mS_buildingHPS = 1.0f;
	private bool mS_isAblaze = false;
	private float mS_currentBurningTime, mS_currentAblazeTime = 0;
	private IDictionary<int, float> mS_characterStartingTime = new Dictionary<int, float>();
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
        BuildingHealth = mS_buildingMaxHP;
		CurrentBuildingMaxHealth = mS_buildingMaxHP;
        mC_material = GetComponent<Renderer>().material;
    }


    void Update()
    {
        if(MainNetworkManager.Is_Server)
		{
			ServerUpdate();
		}
		else
		{
			ClientUpdate();
		}
		
		
		
		/*//Sever and client side update
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
                    //reduce health by time
                    BuildingHealth -= (Time.deltaTime / 2f);
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
			else if(C_SettingBar.enabled)
			{

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
				}
			} 
			else if(C_DampBuildingText.activeSelf)
			{
				if (DampTime < 8)
				{
					C_DampBuildingText.SetActive(false);
				}
			}
        }*/
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

	[ServerCallback]
	private void ServerUpdate()
	{
		//If we are starting a fire update the variables
		if(mS_characterStartingTime.Count > 0)
		{
			//As mutiple character can start the fire at the same time for the building, lets update all of them and just consider the higest to start the fire
			List<int> listKeys = mS_characterStartingTime.Keys.ToList();
			foreach (int id in listKeys)
			{
				//Update the fire time
				mS_characterStartingTime[id] += Time.deltaTime;
				//Check if the building should be on fire
				if (mS_characterStartingTime[id] >= m_timeToStartFire)
				{
					//Set on fire
					OnFire = true;
					//Clear variables
					mS_currentAblazeTime = 0;
					mS_currentBurningTime = 0;

					//Let every client know that they should disable thei on fire ui
					foreach(KeyValuePair<int,float> keypair in mS_characterStartingTime)
					{
						TargetDisableSettingUI(MainNetworkManager._instance.PlayersConnected[keypair.Key].connectionToClient);
					}
					//Ckear and return
					mS_characterStartingTime.Clear();
					break;
				}
			}
		}

		if (OnFire)
		{
			//Reduce building health by the damage every second
			BuildingHealth -= mS_fireDPS * Time.deltaTime;
			//Add to burn timer
			mS_currentBurningTime += Time.deltaTime;

			//If we burned more than the time for ablaze set to ablaze
			if(mS_currentBurningTime >= mS_timeBurningForAblaze)
			{
				mS_isAblaze = true;
			}

			//If we are ablaze we take permanent damage
			if(mS_isAblaze)
			{
				CurrentBuildingMaxHealth -= mS_ablazeDPS * Time.deltaTime;
			}
		}
		else if(Dampening)
		{
			//Update the dampening of the building
			DampTime -= Time.deltaTime;
			Dampening = DampTime > 0;
			if(!Dampening)
			{
				DampTime = 0;
			}
		} 
		else
		{
			//Regen the Building if we are not ablaze, onfire or dampening
			//If building is destroyed, stop building regeneration
			if (BuildingHealth <= 0)
				return;

			//Heal the building
			BuildingHealth += Time.deltaTime * mS_buildingHPS;

			//Make sure we never go over the current max hp and never below 0
			BuildingHealth = Mathf.Max(0, Mathf.Min(BuildingHealth, CurrentBuildingMaxHealth));
		}
	}

	[ClientCallback]
	private void ClientUpdate()
	{

	}

    [Client]
    ///<summary>Shows client lighting fire</summary>
    public void EmulateStartingFire(Character c)
    {
        //Only crazy people can start fire
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople)
        {
			//If we are already on fire we cannot start it
			if (OnFire)
			{
				return;
			}

			if (Dampening)
			{
				return;
			}

			//Start the fire
			mC_startingFireTime = 0;
			mC_isStartingFire = true;
		}
    }

	[TargetRpc]
	private void TargetDisableSettingUI(NetworkConnection target)
	{

	}

	[TargetRpc]
	private void TargetBuildingIsAlreadyOnFire(NetworkConnection target)
	{

	}

	[TargetRpc]
	private void TargetBuildingIsDampening(NetworkConnection traget)
	{

	}

    [Server]
    ///<<summary>Target building begins to be set on fire</summary>
    public void ServerStartingFire(Character c)
    {
        //Only crazy people can start fire
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople && !mS_characterStartingTime.ContainsKey(c.ControllingPlayerID))
        {
			NetworkConnection character_connectionToClient = MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient;
			//If we are already on fire we cannot start it
			if (OnFire)
			{
				TargetBuildingIsAlreadyOnFire(character_connectionToClient);
				return;
			}

			if(Dampening)
			{
				TargetBuildingIsDampening(character_connectionToClient);
				return;
			}

			//Add the character to the list
			mS_characterStartingTime.Add(c.ControllingPlayerID, 0);
        }
    }

    [Server]
    ///<summary>Extinguishes fire on building</summary>
    public void ServerExtinguish()
    {
		//Set Dampering
        Dampening = true;
		OnFire = false;
		mS_isAblaze = false;
    }

	[ClientCallback]
	private void UpdateBuildingVisualStatus()
	{
		//Activate building health/burn amount bars
		C_HealthBar.gameObject.SetActive(true);
		C_HealthBarBg.gameObject.SetActive(true);
		C_HealthBar.fillAmount = BuildingHealth / mS_buildingMaxHP;
		//Display health counter
		C_HealthCounter.GetComponent<TextMeshProUGUI>().SetText("Health: " + (int)BuildingHealth / (int)mS_buildingMaxHP + "%");

		//If building health is between 66 and 33
		if (BuildingHealth < 66 && BuildingHealth > 33)
		{
			//Change building to be in its first burning status
			mC_material.SetInt("_BurningTextureIndex", 1);
		}
		//If building health is between 33 and 0
		if (BuildingHealth < 33 && BuildingHealth > 0)
		{
			//Change building to be in its first burning status
			mC_material.SetInt("_BurningTextureIndex", 2);
		}
		//If building health depletes
		if (BuildingHealth <= 0)
		{
			//TODO: Convert Building to ashes
		}
	}

	//Handle change of variable for HP
	private void OnBuidlingHPUpdate(float _newHP)
    {
        BuildingHealth = _newHP;
		//Update  building visible state
		UpdateBuildingVisualStatus();
	}

	private void OnBuildingCurrenMaxHPUpdate(float _newMaxHP)
	{
		CurrentBuildingMaxHealth = _newMaxHP;
		//Update building visible state
		UpdateBuildingVisualStatus();
	}
}
