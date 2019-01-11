using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSettings : MonoBehaviour {

	[SerializeField]
	private MapInfo m_defaultMap;
    #region Instance handling
    public static MatchSettings _instance;
    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);

        _instance = this;
        DontDestroyOnLoad(gameObject);

		MapID = m_defaultMap.name;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
    #endregion

    //List of players that belong to a specific team
    private IList<NetworkPlayer> m_crazyPeopleTeam = new List<NetworkPlayer>();
    private IList<NetworkPlayer> m_fireFigthersTeam = new List<NetworkPlayer>();

    /// <summary>
    /// Size of the Crazy people team
    /// </summary>
    public uint CrazyTeamSize
    {
        get
        {
            return (uint)m_crazyPeopleTeam.Count;
        }
    }
    /// <summary>
    /// Size of the fire fighters team
    /// </summary>
    public uint FirefightersTeamSize
    {
        get
        {
            return (uint)m_fireFigthersTeam.Count;
        }
    }

	/// <summary>
	/// Event called when the info of the map is changed
	/// </summary>
	public event Action MapInfoChanged;

    /// <summary>
    /// Make this variable change based on selected map
    /// </summary>
    public string MapID
    {
        get;
        private set;
    }

	/// <summary>
	/// Sets the new data
	/// </summary>
	/// <param name="info">The map Info</param>
	public void SetMap(string info)
	{
		MapID = info;
		if(MapInfoChanged != null)
		{
			MapInfoChanged.Invoke();
		}
	}


    /// <summary>
    /// Invoked when any team changed
    /// </summary>
    public event Action TeamsChanged;

    /// <summary>
    /// Get the team a new player should join to keep the match taem sizes balanced
    /// </summary>
    /// <returns>ETeams the player must joind</returns>
    public ETeams GetNewPlayerStartingTeam()
    {
        //If the crazy people team is empty join it
        if(m_crazyPeopleTeam.Count == 0)
        {
            return ETeams.CrazyPeople;
        }
        //If the fire fighters team is empty or there are less then 1 firefigther for every 2 crazy people join the fire fighters
        else if(m_fireFigthersTeam.Count == 0 || ((int)(m_crazyPeopleTeam.Count / 2.0f) > m_fireFigthersTeam.Count && m_fireFigthersTeam.Count < 2))
        {
            return ETeams.FireFighters;
        }

        //Any other condition just join the crazy people
        return ETeams.CrazyPeople;
    }

    /// <summary>
    /// Checks if a player can switch to a different team
    /// </summary>
    /// <param name="newTeam">The new team the players want to join</param>
    /// <returns>bool - If possible or not to switch</returns>
    public bool CanSwitchToTeam(ETeams newTeam)
    {
        if(newTeam == ETeams.CrazyPeople)
            return CrazyTeamSize < 4;
        return FirefightersTeamSize < 2;
    }

    /// <summary>
    /// Ties to add a player a given new team. However the player might not join it
    /// </summary>
    /// <param name="p">The ntwork player who wants to join/change a team</param>
    /// <param name="newTeam">The team the players want to join/change to</param>
    /// <returns>The team the player is now part of, might be the same as before if we could not switch</returns>
    public ETeams TryToAddPlayerToTeam(NetworkPlayer p, ETeams newTeam)
    {
        //Can we even sitch (aks is team we are trying to join full)
        if(CanSwitchToTeam(newTeam))
        {
            //Remove the player form the team, we are doing it in both list just in case its in two at the same time
            m_crazyPeopleTeam.Remove(p);
            m_fireFigthersTeam.Remove(p);
            //Join the apropiate team
            switch (newTeam)
            {
                case ETeams.CrazyPeople:
                    m_crazyPeopleTeam.Add(p);
                    break;
                case ETeams.FireFighters:
                    m_fireFigthersTeam.Add(p);
                    break;
            }
            //Invoke that a team changed
            if(TeamsChanged != null)
            {
                TeamsChanged.Invoke();
            }
            //Return the team it joined
            return newTeam;
        }
        else
        {
            //Just return the team it already is part of
            return p.Player_Team;
        }
    }

    /// <summary>
    /// Gets a list of all players id from a team
    /// </summary>
    /// <param name="team">The team we want to get the ids from</param>
    /// <returns>Arrays of ints (int[]) with all the players id</returns>
    public int[] GetTeamMembersId(ETeams team)
    {
        //Check for wicht team we want the data to be returned
        if (team == ETeams.CrazyPeople)
        {
            //Create the array based on the size of the team
            int[] returnValue = new int[CrazyTeamSize];
            //Get the ids
            for (int i = 0; i < CrazyTeamSize; i++)
            {
                returnValue[i] = m_crazyPeopleTeam[i].Player_ID;
            }
            return returnValue;
        }
        //Create the array based on the size of the team
        int[] firefightersIDS = new int[FirefightersTeamSize];
        //Get the ids
        for (int i = 0; i < FirefightersTeamSize; i++)
        {
            firefightersIDS[i] = m_fireFigthersTeam[i].Player_ID;
        }
        return firefightersIDS;
    }

    /// <summary>
    /// Sets the team list of players to the given list of players. It will ovewrite the current team list
    /// </summary>
    /// <param name="playersIDs">The list of players is</param>
    /// <param name="team">The team this players are part of</param>
    public void SetTeamFromIds(int[] playersIDs, ETeams team)
    {
        //Based on the team we do different things
        switch(team)
        {
            case ETeams.CrazyPeople:
                {
                    //Clear the player list
                    m_crazyPeopleTeam.Clear();
                    //Convert the array to list, this is to use functions like contains instead of nesting for loops myself
                    List<int> playersIDsList = new List<int>();
                    playersIDsList.AddRange(playersIDs);
                    //Find the NetworkPlayer object with the given ids and add them to the team list
                    foreach (NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
                    {
                        if (playersIDsList.Contains(p.Player_ID))
                        {
                            m_crazyPeopleTeam.Add(p);
                        }
                    }
                }
                break;
            case ETeams.FireFighters:
                {
                    //Clear the player list
                    m_fireFigthersTeam.Clear();
                    //Convert the array to list, this is to use functions like contains instead of nesting for loops myself
                    List<int> playersIDsList = new List<int>();
                    playersIDsList.AddRange(playersIDs);
                    //Find the NetworkPlayer object with the given ids and add them to the team list
                    foreach (NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
                    {
                        if (playersIDsList.Contains(p.Player_ID))
                        {
                            m_fireFigthersTeam.Add(p);
                        }
                    }
                }
                break;
        }
        //Invoke that a team changed
        if (TeamsChanged != null)
        {
            TeamsChanged.Invoke();
        }
    }

    /// <summary>
    /// Removes a player from all teams, usefull to handle disconnections
    /// </summary>
    /// <param name="p">The player to remove</param>
    public void RemovePlayer(NetworkPlayer p)
    {
        //Remove the player form all team as he needs to be in one of them
        m_crazyPeopleTeam.Remove(p);
        m_fireFigthersTeam.Remove(p);
        //Invoke that a team changed
        if (TeamsChanged != null)
        {
            TeamsChanged.Invoke();
        }
    }
}
