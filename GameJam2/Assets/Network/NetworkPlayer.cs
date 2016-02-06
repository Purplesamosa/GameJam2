using UnityEngine;
using System.Collections;

public class NetworkPlayer : Photon.MonoBehaviour
{
    public TextMesh MyText;
    public MeshRenderer BodyMaterial;
    public GameObject MovementSphere;

    public enum UnitType
    {
        Plane,
        Soldier
    }
    public UnitType unitType;

    private CityTestScript.Team MyTeam;

    //public GameObject MyCamera;
    private bool isAlive = true;
    Vector3 networkposition;
    Quaternion networkrotation;
    float lerpSmoothing = 100.0f;
    bool bSelected = false;
    bool bMoving = false;
    Vector3 GoalPos;
    bool bIsMine = false;
    public bool bReady = true;
    bool bIsTargetted = false;

    private int HP = 10;

    // Use this for initialization
    void OnDestroy()
    {
        if (photonView.isMine)
        {
            GameObject.FindObjectOfType<TeamManager>().Units.Remove(this);
        }
    }

    void Start ()
    {
        if (photonView.isMine)
        {
            bIsMine = true;
            GetComponent<PhotonView>().RPC("ChangeMaterial", PhotonTargets.AllBufferedViaServer, (int)PhotonNetwork.player.customProperties["Team"]);
            GetComponent<PhotonView>().RPC("SetHP", PhotonTargets.AllBufferedViaServer, HP);
            GameObject.FindObjectOfType<TeamManager>().Units.Add(this);
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
            if (Time.deltaTime <= 0.0f)
                continue;

            float SqDist = Vector3.SqrMagnitude(networkposition - transform.position);
            if ( SqDist > 200.0f)
            {
                transform.position = networkposition;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, networkposition, Time.deltaTime * lerpSmoothing);
            }

            /*Quaternion NewRot = Quaternion.Lerp(transform.rotation, networkrotation, Time.deltaTime * lerpSmoothing);

            if (float.IsNaN(NewRot.w))
            {
                Debug.Log("NAN!!!");
                continue;
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, networkrotation, Time.deltaTime * lerpSmoothing);*/

            yield return null;
        }
    }

    public void TargetPlayer()
    {
        bIsTargetted = true;
    }

    public void ResetPlayer()
    {
        GetComponent<PhotonView>().RPC("ResetPlayerRPC", PhotonTargets.AllBufferedViaServer);

    }

    [PunRPC]
    void ResetPlayerRPC()
    {
        bIsTargetted = false;
        bMoving = false;
        bReady = true;
    }


    [PunRPC]
    public void ChangeMaterial(int _team)
    {
        if (_team == 0)
        {
            MyTeam = CityTestScript.Team.Red;
            BodyMaterial.material.color = new Color(1.0f, 0.0f, 0.0f);
        }
        else
        {
            MyTeam = CityTestScript.Team.Blue;
            BodyMaterial.material.color = new Color(0.0f, 0.0f, 1.0f);

        }
    }

    [PunRPC]
    public void SetHP(int _HP)
    {
        HP = _HP;
        MyText.text = HP.ToString();
    }

    [PunRPC]
    public void DecreaseHP()
    {
        HP--;
        MyText.text = HP.ToString();
        if (HP <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

        void Update()
    {
        if (!bIsMine)
        {
            if (Input.GetButtonDown("Fire1") && bIsTargetted)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        GetComponent<PhotonView>().RPC("DecreaseHP", PhotonTargets.AllBufferedViaServer);
                        bIsTargetted = false;
                    }
                }
            }
            return;
        }

        if(bMoving)
        {
            Vector3 vecTemp = transform.position - GoalPos;
            float fDist = Vector3.Dot(vecTemp, GetComponent<Rigidbody>().velocity.normalized);
            if (fDist > 0.1f)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                transform.position = GoalPos;
                
                if(unitType == UnitType.Soldier)
                {
                    //Check if there are any buildings
                    CityTestScript[] Buildings = FindObjectsOfType<CityTestScript>();
                    for (int i = 0; i < Buildings.Length; i++)
                    {
                        float sqMag = Vector3.SqrMagnitude(Buildings[i].transform.position - transform.position);

                        if (Buildings[i].MyTeam != MyTeam 
                            && sqMag < (20.0f * 20.0f))
                        {
                            Buildings[i].ChangeTeam(MyTeam);
                            GameObject.FindObjectOfType<TeamManager>().MyCities.Add(Buildings[i]);
                        }
                    }
                }

                //Check if there are any enemies within radius
                NetworkPlayer[] AllPlayers = FindObjectsOfType<NetworkPlayer>();
                for (int i = 0; i < AllPlayers.Length; i++)
                {
                    if (AllPlayers[i].MyTeam != MyTeam)
                    {
                        //Check if he's close
                        if (Vector3.SqrMagnitude(AllPlayers[i].transform.position - transform.position) < (48.0f * 48.0f))
                        {
                            //Spawn an arrow on him
                            PhotonNetwork.Instantiate("Arrow", new Vector3(AllPlayers[i].transform.position.x,
                                AllPlayers[i].transform.position.y + 20.0f,
                                AllPlayers[i].transform.position.z), Quaternion.identity, 0);
                            AllPlayers[i].TargetPlayer();
                        }
                    }
                }
            }
        }

        if (Input.GetButtonDown("Fire1") && bReady)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    bSelected = true;
                    MovementSphere.SetActive(true);
                }
                else if(bSelected)
                {
                    //TODOMIKE: Only move if within radius

                    //Instead of going to the raycast point, we'll use a grid
                    //and go to the center position of that current cell
                    GoalPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);

                    if (Vector3.SqrMagnitude(GoalPos - transform.position) > (48.0f * 48.0f))
                        return;

                    Vector3 direction = GoalPos - transform.position;
                    direction.Normalize();
                    GetComponent<Rigidbody>().velocity = direction * 100.0f;
                    bSelected = false;
                    bMoving = true;
                    transform.LookAt(GoalPos);
                    bReady = false;
                    MovementSphere.SetActive(false);
                }
            }
        }
    }

}
