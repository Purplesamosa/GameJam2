using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TeamManager : Photon.MonoBehaviour
{

    //We will get a message that the turn has changed.
    //If it's our turn, we re-activate all the buildings and the end turn button.
    //If not, then we don't

    public List<CityTestScript> MyCities;
    public List<NetworkPlayer> Units;
    public Button EndTurnButton;
    public CityTestScript.Team MyTeam;
    private bool bMyTurn;

    public void SetTeam(CityTestScript.Team _team)
    {
        MyTeam = _team;

        for (int i = MyCities.Count - 1; i >= 0; i--)
        {
            if (MyCities[i].MyTeam != _team)
            {
                MyCities.RemoveAt(i);
            }
        }
    }

    public void ChangeTurn(CityTestScript.Team _team)
    {
        if (_team == MyTeam)
        {

            for (int i = MyCities.Count - 1; i >= 0; i--)
            {
                if (MyCities[i].MyTeam != _team)
                {
                    MyCities.RemoveAt(i);
                }
            }

            for (int i = 0; i < MyCities.Count; i++)
            {
                MyCities[i].ActivateCity(true);
            }

            for (int i = 0; i < Units.Count; i++)
            {
                Units[i].ResetPlayer();
            }

            EndTurnButton.gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < MyCities.Count; i++)
            {
                MyCities[i].ActivateCity(false);
            }

            for (int i = 0; i < Units.Count; i++)
            {
                Units[i].bReady = false;
            }

            EndTurnButton.gameObject.SetActive(false);
        }
    }
}
