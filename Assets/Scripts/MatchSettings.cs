using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSettings : MonoBehaviour {

    public static MatchSettings _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private IList<NetworkPlayer> m_crazyPeopleTeam = new List<NetworkPlayer>();
    private IList<NetworkPlayer> m_fireFigthersTeam = new List<NetworkPlayer>();

    public uint CrazyTeamSize
    {
        get
        {
            return (uint)m_crazyPeopleTeam.Count;
        }
    }
    public uint FirefightersTeamSize
    {
        get
        {
            return (uint)m_fireFigthersTeam.Count;
        }
    } 

    public ETeams GetNewPlayerStartingTeam()
    {
        if(m_crazyPeopleTeam.Count == 0)
        {
            return ETeams.CrazyPeople;
        }
        else if(m_fireFigthersTeam.Count == 0 || ((int)(m_crazyPeopleTeam.Count / 2.0f) > m_fireFigthersTeam.Count && m_fireFigthersTeam.Count < 2))
        {
            return ETeams.FireFighters;
        }

        return ETeams.CrazyPeople;
    }

    public bool CanSwitchToTeam(ETeams newTeam)
    {
        if(newTeam == ETeams.CrazyPeople)
            return CrazyTeamSize < 4;
        return FirefightersTeamSize < 2;
    }

    public ETeams TryToAddPlayerToTeam(NetworkPlayer p, ETeams newTeam)
    {
        if(CanSwitchToTeam(newTeam))
        {
            m_crazyPeopleTeam.Remove(p);
            m_fireFigthersTeam.Remove(p);
            switch (newTeam)
            {
                case ETeams.CrazyPeople:
                    m_crazyPeopleTeam.Add(p);
                    break;
                case ETeams.FireFighters:
                    m_fireFigthersTeam.Add(p);
                    break;
            }
            return newTeam;
        }
        else
        {
            return p.Player_Team;
        }
    }

    public int[] GetTeamMembersId(ETeams team)
    {
        if (team == ETeams.CrazyPeople)
        {
            int[] returnValue = new int[CrazyTeamSize];
            for (int i = 0; i < CrazyTeamSize; i++)
            {
                returnValue[i] = m_crazyPeopleTeam[i].ID;
            }
            return returnValue;
        }

        int[] firefightersIDS = new int[FirefightersTeamSize];
        for (int i = 0; i < FirefightersTeamSize; i++)
        {
            firefightersIDS[i] = m_fireFigthersTeam[i].ID;
        }
        return firefightersIDS;
    }

    public void SetTeamFromIds(int[] playersIDs, ETeams team)
    {
        switch(team)
        {
            case ETeams.CrazyPeople:
                {
                    m_crazyPeopleTeam.Clear();
                    List<int> playersIDsList = new List<int>();
                    playersIDsList.AddRange(playersIDs);
                    foreach (NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
                    {
                        if (playersIDsList.Contains(p.ID))
                        {
                            m_crazyPeopleTeam.Add(p);
                        }
                    }
                }
                break;
            case ETeams.FireFighters:
                {
                    m_fireFigthersTeam.Clear();
                    List<int> playersIDsList = new List<int>();
                    playersIDsList.AddRange(playersIDs);
                    foreach (NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
                    {
                        if (playersIDsList.Contains(p.ID))
                        {
                            m_fireFigthersTeam.Add(p);
                        }
                    }
                }
                break;
        }
    }
}
