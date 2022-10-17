using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Vuforia;
using TMPro;

//This script handles the image tracking of the cart
public class CartImageTracking : MonoBehaviourPun
{
    public bool Pinned;
    public static GameObject Cart1;
    public Material Pressed;
    public Material Unpressed;
    [PunRPC] void FoundImage()
    {
        GameObject C1;
        C1 = PhotonView.Find(1).gameObject;
        // Disable the backup platform
        C1.GetComponent<CartPosition>().enabled = false;
        C1.transform.localPosition = new Vector3(0, -0.55f, 0.25f);
        C1.transform.localRotation = Quaternion.Euler(0, 0, 0);
        C1.GetComponent<Collider>().enabled = true;
        int i = 0;
        //Show all the products on the platform
        while (i < C1.transform.childCount)
        {
            C1.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
            C1.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
            i++;
        }
    }
    [PunRPC] void LostImage()
    {
        Debug.Log("Cart Lost Tracked");
        GameObject C1;
        C1 = PhotonView.Find(1).gameObject;
        int i = 0;
        C1.GetComponent<Collider>().enabled = true;
        //Still show the products (Because originally vuforia hides the target if track lost)
        while (i < C1.transform.childCount)
        {
            C1.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
            C1.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
            i++;
        }
        //Enable the backup plan
        C1.GetComponent<CartPosition>().enabled = true;
    }
    
    //When cart image target is being tracked
    public void CartFoundTracked(GameObject Cart)
    {
        if (Pinned)
        {
            return;
        }
        if(ChooseMenu.Operator || ChooseMenu.LonelyManager)
        {
            Debug.Log("Found1");
            /*PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("FoundImage",RpcTarget.All);*/
            
            
            //Set the position of the cart relative to the image localy
            Debug.Log("Cart Tracked");
            GameObject C1;
            C1 = PhotonView.Find(1).gameObject;

            //Disable the backup plan
            C1.GetComponent<CartPosition>().enabled = false;
            //Position cart under the image target 3
            C1.transform.localPosition = new Vector3(0, -0.55f, 0.25f);
            C1.transform.localRotation = Quaternion.Euler(0, 0, 0);
            C1.GetComponent<Collider>().enabled = true;
            int i = 0;
            
            //Show all the products on the platform
            while (i < Cart.transform.childCount)
            {
                C1.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                C1.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
                i++;
            }
        }
    }
    //When cart image target is lost
    public void CartLostTracked(GameObject Cart)
    {
        if (Pinned)
        {
            return;
        }
        if (ChooseMenu.Operator || ChooseMenu.LonelyManager)
        {
            
            Debug.Log("Lost1");
            /*PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("LostImage",RpcTarget.All);*/
            
            Debug.Log("Cart Lost Tracked");
            GameObject C1;
            C1 = PhotonView.Find(1).gameObject;
            int i = 0;
            C1.GetComponent<Collider>().enabled = true;
            //Still show the products (Because originally vuforia hides the target if track lost)
            while (i < Cart.transform.childCount)
            {
                C1.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                C1.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
                i++;
            }
            //Enable the backup plan
            C1.GetComponent<CartPosition>().enabled = true;
        }
    }

    public void CartPin(GameObject PinButton)
    {
        GameObject Image = GameObject.FindGameObjectWithTag("ImageTarget03");
        GameObject Cart1 = Image.transform.Find("Cart1").gameObject;

        // when player in loading area, the pin button will show start loading
        if (Pinned)
        {
            Image.GetComponent<ImageTargetBehaviour>().enabled = true;
            Cart1.GetComponent<CartPosition>().enabled = true;
            PinButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Unpressed;
            if(PlayerPosition.InLoad)
            {
                PinButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Loading";
            }
            else if(PlayerPosition.InUnload01 || PlayerPosition.InUnload02)
            {
                PinButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Unloading";
            }
            Pinned = false;
        }
        else
        {
            Image.GetComponent<ImageTargetBehaviour>().enabled = false;
            Cart1.GetComponent<CartPosition>().enabled = false;
            PinButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Pressed;
            if (PlayerPosition.InLoad)
            {
                PinButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Stop Loading";
            }
            else if (PlayerPosition.InUnload01 || PlayerPosition.InUnload02)
            {
                PinButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Stop Unloading";
            }
            Pinned = true;
        }
    }
}
