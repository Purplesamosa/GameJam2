using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{

    private const string VERSION = "v0.0.1";

    public string RoomName = "VVR";
    public string PlayerPrefabName = "SimpleTank";
    public Transform [] SpawnPoint;

    void Start ()
    {
        PhotonNetwork.ConnectUsingSettings(VERSION);
	}

    void OnJoinedLobby()
    {
        //PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
        PhotonNetwork.JoinRandomRoom();
    }

    void OnJoinedRoom()
    {
        int TeamNum = 0;
        if (PhotonNetwork.player.isMasterClient)
        {
            PhotonNetwork.player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Team", 0 } });
        }
        else
        {
            TeamNum = 1;
            PhotonNetwork.player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Team", 1 } });
        }

        PhotonNetwork.Instantiate(PlayerPrefabName, SpawnPoint[TeamNum].position, SpawnPoint[TeamNum].rotation, 0);
    }

    void OnPhotonRandomJoinFailed()
    {
        //RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        PhotonNetwork.CreateRoom(null);
    }
}
