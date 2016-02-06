using UnityEngine;
using System.Collections;

public class CityTestScript : Photon.MonoBehaviour
{
    public Material BlueMat;
    public Material RedMat;


    public enum BuildingType
    {
        Base,
        Factory,
        Tower
    }

    public enum Team
    {
        Neutral = -1,
        Red,
        Blue
    }

    public BuildingType Type;
    public Team MyTeam;
    public Transform SpawnPoint;
    public bool bReady = false;
    private bool bInitialized;

    void OnJoinedRoom()
    {
        if (bInitialized)
            return;
        if (MyTeam != Team.Neutral)
        {
            ChangeTeam(MyTeam);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    //We don't use all buffered via server because there are only two players in this game,
                    //so we don't need to buffer it for later players
                    if (bReady)
                    {
                        bReady = false;
                        switch (Type)
                        {
                            case BuildingType.Factory:
                                PhotonNetwork.Instantiate("HumanSoldier", SpawnPoint.position, SpawnPoint.rotation, 0);
                                break;
                            case BuildingType.Tower:
                                PhotonNetwork.Instantiate("HumanFighter", SpawnPoint.position, SpawnPoint.rotation, 0);
                                break;
                        }
                    }

                    //GetComponent<PhotonView>().RPC("ChangeMaterial", PhotonTargets.All, (int)PhotonNetwork.player.customProperties["Team"]);
                }
            }
        }
    }

    public void ActivateCity(bool _flag)
    {
        bReady = true;
    }

    public void ChangeTeam(CityTestScript.Team _team)
    {
        GetComponent<PhotonView>().RPC("ChangeMaterial", PhotonTargets.All, (int)_team);
    }

    [PunRPC]
    public void ChangeMaterial(int _team)
    {
        bInitialized = true;
        MeshRenderer[] MyRenderer = transform.GetComponentsInChildren<MeshRenderer>();
        if (_team == 0)
        {
            MyTeam = Team.Red;

            for (int i = 0; i < MyRenderer.Length; i++)
            {
                for (int j = 0; j < MyRenderer[i].materials.Length; j++)
                {
                    MyRenderer[i].materials[j].color = new Color(1.0f, 0.0f, 0.0f);
                }
            }
        }
        else
        {
            MyTeam = Team.Blue;
            for (int i = 0; i < MyRenderer.Length; i++)
            {
                for (int j = 0; j < MyRenderer[i].materials.Length; j++)
                {
                    MyRenderer[i].materials[j].color = new Color(0.0f, 0.0f, 1.0f);
                }
            }

        }
    }

}
