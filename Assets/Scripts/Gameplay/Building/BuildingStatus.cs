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
    #region UI Elements - On all Clients, show or hide is bassed on server building update
    //Building burn amount/health bars
    [SerializeField]
    private Image C_HealthBar;
    [SerializeField]
    private Image C_HealthBarBg;

    //Building Wet bars
    [SerializeField]
    private Image C_WetBar;
    [SerializeField]
    private Image C_WetBarBg;
    #endregion

    #region UI Elements - Each client controls their
    [SerializeField]
    private Image C_SettingBarBg;
    [SerializeField]
    private Image C_SettingBar;
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
	[SerializeField]
	private float m_damperingRecoverTime = 5;
	[SerializeField]
	private int m_buildingMaxHP = 100;
	#endregion

	#region Server SyncVars
	[SyncVar(hook = "OnDampeningTimeUpdate")]
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
	private float mS_spreadTime = 5.0f;
	[SerializeField]
	private float mS_buildingHPS = 1.0f;
	private bool mS_isAblaze = false;
	private float mS_currentBurningTime, mS_currentAblazeTime = 0;
	private IDictionary<int, float> mS_characterStartingTime = new Dictionary<int, float>();
	#endregion

	private void Awake()
	{
		C_HealthBar.enabled = false;
		C_HealthBarBg.enabled = false;
		C_SettingBar.enabled = false;
		C_SettingBarBg.enabled = false;
		C_WetBar.enabled = false;
		C_WetBarBg.enabled = false;
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
        GetComponent<Interact>().ClientInteraction.AddListener(EmulateStartingFire);
		GetComponent<Interact>().ClientStopInteract.AddListener(EmulateStopSettingFire);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<Interact>().ServerInteraction.AddListener(ServerStartingFire);
		GetComponent<Interact>().ServerStopInteract.AddListener(ServerStopStartingFire);
		NetworkServer.Spawn(gameObject);
    }

    private void Start()
    {
        //Initialised Values
        BuildingHealth = m_buildingMaxHP;
		CurrentBuildingMaxHealth = m_buildingMaxHP;
        mC_material = GetComponent<Renderer>().material;
    }


    void Update()
    {
		//Update all
		ServerUpdate();
		ClientUpdate();
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
						TargetStopSettingFire(MainNetworkManager._instance.PlayersConnected[keypair.Key].connectionToClient);
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
			BuildingHealth = Mathf.Max(0, BuildingHealth - (mS_fireDPS * Time.deltaTime));
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
				//Reduce Max HP
				CurrentBuildingMaxHealth = Mathf.Max(0, CurrentBuildingMaxHealth - (mS_ablazeDPS * Time.deltaTime));
				//If we are at 0 set the building not on fire as it should be destroyed, so we otimise the network not needing to regen all the time
				if(CurrentBuildingMaxHealth == 0)
				{
					OnFire = false;
				}
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
			if( CurrentBuildingMaxHealth <= 0)
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
		//Add starting fire time
		if(mC_isStartingFire)
		{
			//Add local timer for setting fire
			mC_startingFireTime += Time.deltaTime;
			//Set the fill for the setting
			C_SettingBar.fillAmount = mC_startingFireTime / m_timeToStartFire;

			if (mC_startingFireTime > m_timeToStartFire)
			{
				//Hide setting ui
				LocaStopSettingFire();
			}
		} 
	}

    [Client]
    ///<summary>Shows client lighting fire</summary>
    public void EmulateStartingFire(Character c)
    {
        //Only crazy people can start fire
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople && CurrentBuildingMaxHealth > 0)
        {
			//If we are already on fire we cannot start it
			if (OnFire)
			{
				//Display already on fire
				return;
			}

			if (Dampening)
			{
				//Display building is dampening
				return;
			}

			//Start setting the fire
			mC_startingFireTime = 0;
			mC_isStartingFire = true;
			//ShowUI
			C_SettingBar.enabled = true;
			C_SettingBarBg.enabled = true;
			C_SettingBar.fillAmount = mC_startingFireTime / m_timeToStartFire;
		}
    }

	[Client]
	public void EmulateStopSettingFire(Character c)
	{
		//Only crazy people can start fire so only they can stop interact
		if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople)
		{
			//Stop setting the fire
			LocaStopSettingFire();
		}
	}

	[TargetRpc]
	private void TargetStopSettingFire(NetworkConnection target)
	{
		LocaStopSettingFire();
	}

	[Client]
	private void LocaStopSettingFire()
	{
		//Disable the setting
		C_SettingBar.enabled = false;
		C_SettingBarBg.enabled = false;
		//Set the fire starting to 0
		mC_startingFireTime = 0;
		mC_isStartingFire = false;
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
        if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople && !mS_characterStartingTime.ContainsKey(c.ControllingPlayerID) && CurrentBuildingMaxHealth > 0)
        {
			NetworkConnection character_connectionToClient = MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient;
			//If we are already on fire we cannot start it
			if (OnFire)
			{
				TargetBuildingIsAlreadyOnFire(character_connectionToClient);
				return;
			}

			//If we are dampering we cannot put the building on fire
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
	public void ServerStopStartingFire(Character c)
	{
		//Only crazy people can put fire one, and so only they can stop interact with it, also the character 
		if (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team == ETeams.CrazyPeople && mS_characterStartingTime.ContainsKey(c.ControllingPlayerID))
		{
			mS_characterStartingTime.Remove(c.ControllingPlayerID);
			TargetStopSettingFire(MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient);
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

	[Client]
	private void UpdateBuildingVisualStatus()
	{
		//Activate building health/burn amount bars
		C_HealthBar.enabled = OnFire && CurrentBuildingMaxHealth > 0;
		C_HealthBarBg.enabled = OnFire && CurrentBuildingMaxHealth > 0;
		C_HealthBar.fillAmount = BuildingHealth / m_buildingMaxHP;

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
			GetComponent<Renderer>().enabled = false;
		}
	}

	[Client]
	private void UpdateDamperingBarVisuals()
	{
		//Enable the dampering ui
		C_WetBar.enabled = Dampening;
		C_WetBarBg.enabled = Dampening;
		//Updathe the dampering fill amount
		C_WetBar.fillAmount = DampTime / m_damperingRecoverTime;
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

	private void OnDampeningTimeUpdate(float _newDamperingTime)
	{
		//Update the local time and then update the UI
		DampTime = _newDamperingTime;
		UpdateDamperingBarVisuals();
	}
}
