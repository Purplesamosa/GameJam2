using UnityEngine;
using System.Collections;

public class CityTestScript : MonoBehaviour
{
    public Material redmat;
    public Material bluemat;


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
                    GetComponent<PhotonView>().RPC("ChangeMaterial", PhotonTargets.All, (int)PhotonNetwork.player.customProperties["Team"]);
                }
            }
        }
    }

    [PunRPC]
    public void ChangeMaterial(int _team)
    {
        if (_team == 0)
        {
            GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.0f, 1.0f);

        }
    }

}
