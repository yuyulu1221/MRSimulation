using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//This script is for the queue and source to snap to place with the conveyor when they are not placed by image tracking but the tap to place
public class SnapToPlace : MonoBehaviour
{
    public GameObject Moving;
    public GameObject TargetObject;
    public Transform st;
    public Quaternion stRotate;
    public Transform qt;
    public Quaternion qtRotate;
    private float dis;
    private bool Snaped;
    private Quaternion rot;
    private Vector3 pos;
    
    //Set the position of the queue or source to the right place
    [PunRPC] void SetP()
    {
        this.transform.parent = TargetObject.transform.parent;
        this.transform.localPosition = pos;
        this.transform.localRotation = rot;
    }

    //When stop configuration
    public void StopMan()
    {
        Snaped = true;
        this.GetComponent<SnapToPlace>().enabled = false;
    }
    //When start configuration
    public void StartMan()
    {
        this.GetComponent<SnapToPlace>().enabled = true;
    }
    void Update()
    {
        //Check what equipment this script is attached to
        switch(this.tag)
        {
            case "Source":
                TargetObject = GameObject.FindGameObjectWithTag("SourceIndicator");
                pos = new Vector3(0,0.95f,0);
                rot = Quaternion.Euler(180, 0, 0);
                break;
            case "Queue":
                TargetObject = GameObject.FindGameObjectWithTag("QueueIndicator");
                pos = new Vector3(0, -1.1f, 0.02f);
                rot = Quaternion.Euler(180, 180, 0);
                break;
            default:
                break;
        }

        //TargetObject is null means the conveyor is not placed yet
        if(TargetObject == null)
        {
            return;
        }

        //Get the distance of this equipment and the indicator of the right position this equipment shoukd snap to place
        dis = Vector3.Distance(TargetObject.transform.position, this.transform.position);
        //If the distance is smaller then 1 meter and it's not snapped yet then teleport it to the right place
        if(dis <=1f && Snaped == false)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SetP", RpcTarget.All);
        }

        //If the distance is larger then 1 meter and then set snapped to false
        if (dis > 1f)
        {
            Snaped = false;
        }
    }
}
