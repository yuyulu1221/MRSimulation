using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CartPosition : MonoBehaviourPun
{
    private GameObject Cam;
    private float y;
    private Transform T;
    [PunRPC] void Trans(Vector3 p, Quaternion r)
    {
        this.transform.position = p;
        this.transform.rotation = r;
    }
    [PunRPC] void Mesh(int id, bool a)
    {
        GameObject Cart = PhotonView.Find(id).gameObject;
        foreach(Transform child in Cart.transform)
        {
            child.GetComponent<MeshRenderer>().enabled = a;
        }
    }

    //When simulation starts this script will be enabled and it will find the camera position for backup cart platform
    void OnEnable()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (ChooseMenu.Operator || ChooseMenu.LonelyManager)
            {
                // request ownership for cart -- takeover
                this.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                base.photonView.RequestOwnership();
            }
        }
        //Only when it's the operator or the lonely manager
        if (ChooseMenu.Operator || ChooseMenu.LonelyManager)
        {
            if(GameObject.FindGameObjectWithTag("Source"))
            {
                y = GameObject.FindGameObjectWithTag("Source").transform.position.y;
                y = y - 0.36f;
                // where to revise x, z
            }
            else
            {
                y = GameObject.FindGameObjectWithTag("MainCamera").transform.position.y;
                y = y  - 1.4f;
            }
            Cam = GameObject.FindGameObjectWithTag("MainCamera");
            PhotonView photonView = PhotonView.Get(this);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Only when it's the operator or the lonely manager
        if (ChooseMenu.Operator || ChooseMenu.LonelyManager)
        {
            if(!PhotonNetwork.InRoom)
            {
                return;
            }
            //Setting the position of the backup cart platform
            float Ry = Cam.transform.rotation.eulerAngles.y;

            // set position x & z forward, but y is fixed
            Vector3 Forward = new Vector3(Cam.transform.forward.x, 0, Cam.transform.forward.z);
            Forward = Vector3.Normalize(Forward);
            this.transform.position = Cam.transform.position + Forward * 0.4f;
            Transform Old = this.transform.transform;
            this.transform.position = new Vector3(Old.position.x, y, Old.position.z);
            this.transform.rotation = Quaternion.Euler(0, Ry, 0); 
        }  
    }
}
