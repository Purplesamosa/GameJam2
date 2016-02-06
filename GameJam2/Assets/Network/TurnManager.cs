using UnityEngine;
using System.Collections;

public class TurnManager : Photon.MonoBehaviour
{
    //TODOMIKE: should start as neutral
    public CityTestScript.Team CurrentTeam = CityTestScript.Team.Red;

    void OnPhotonPlayerConnected(PhotonPlayer connected)
    {
        //Now we are two players, it's the red player's turn
        GetComponent<PhotonView>().RPC("ChangeTurnRPC", PhotonTargets.All);
    }

    public void ChangeTurn()
    {
        GameObject [] go = GameObject.FindGameObjectsWithTag("Arrow");
        for (int i = go.Length - 1; i >= 0; i--)
        {
            PhotonNetwork.Destroy(go[i]);
        }

        GetComponent<PhotonView>().RPC("ChangeTurnRPC", PhotonTargets.All);
        //GameObject.FindObjectOfType<TeamManager>().ChangeTurn(CurrentTeam);

    }

    [PunRPC]
    public void ChangeTurnRPC()
    {
        switch (CurrentTeam)
        {
            case CityTestScript.Team.Neutral:
            case CityTestScript.Team.Blue:
                CurrentTeam = CityTestScript.Team.Red;
                break;
            case CityTestScript.Team.Red:
                CurrentTeam = CityTestScript.Team.Blue;
                break;
        }
        GameObject.FindObjectOfType<TeamManager>().ChangeTurn(CurrentTeam);

    }

}
