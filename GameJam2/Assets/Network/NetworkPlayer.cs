using UnityEngine;
using System.Collections;

public class NetworkPlayer : Photon.MonoBehaviour
{
    //public GameObject MyCamera;
    private bool isAlive = true;
    Vector3 networkposition;
    Quaternion networkrotation;
    float lerpSmoothing = 5.0f;
    bool bSelected = false;
    bool bMoving = false;
    Vector3 GoalPos;
    bool bIsMine = false;

    // Use this for initialization
    void Start ()
    {
        if (photonView.isMine)
        {
            bIsMine = true;
            GetComponent<PhotonView>().RPC("ChangeMaterial", PhotonTargets.AllBufferedViaServer, (int)PhotonNetwork.player.customProperties["Team"]);
           // MyCamera.SetActive(true);
            //Turn on gravity use in rigidbodies, for example
        }
        else
        {
            StartCoroutine("Alive");
        }
	
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkposition = (Vector3)stream.ReceiveNext();
            networkrotation = (Quaternion)stream.ReceiveNext();
        }
    }

    //while alive do this state machine
    IEnumerator Alive()
    {
        while (isAlive)
        {
            transform.position = Vector3.Lerp(transform.position, networkposition, Time.deltaTime * lerpSmoothing);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkrotation, Time.deltaTime * lerpSmoothing);

            yield return null;
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

    void Update()
    {
        if (!bIsMine)
            return;

        if(bMoving)
        {
            if (Vector3.SqrMagnitude(GoalPos - transform.position) < 0.5f)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    bSelected = true;
                }
                else if(bSelected)
                {
                    //Instead of going to the raycast point, we'll use a grid
                    //and go to the center position of that current cell
                    GoalPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                    Vector3 direction = GoalPos - transform.position;
                    direction.Normalize();
                    GetComponent<Rigidbody>().velocity = direction * 100.0f;
                    bSelected = false;
                    bMoving = true;
                }
            }
        }
    }

}
