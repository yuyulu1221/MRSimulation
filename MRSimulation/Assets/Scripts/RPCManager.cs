using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;


public class RPCManager : MonoBehaviourPun
{
    //object is not move in runtime
    static public GameObject FullConveyor;
    static public GameObject Queue;
    static public GameObject Source;
    static public GameObject Rack01;
    static public GameObject Rack02;

    //Image target
    public GameObject ConveyorImage;
    public GameObject Rack01Image;
    public GameObject CartImage;
    
    public GameObject ProductStateManager;
    public GameObject OperatorDialog;
    public Error Error;

    //RPC to destroy product
    [PunRPC] void OPdestroy(int id)
    {
        GameObject product = PhotonView.Find(id).gameObject;
        PhotonNetwork.Destroy(product);
    }

    //RPC to activate product state manager
    [PunRPC] void ActivateProductManager(bool a)
    {
        ProductStateManager.SetActive(a);
    }

   //Activation and deactivation that needs to be done when START simulation
    [PunRPC] void StartSimActivation()
    {
        FullConveyor = GameObject.FindGameObjectWithTag("Conveyor");
        Queue = GameObject.FindGameObjectWithTag("Queue");
        Source = GameObject.FindGameObjectWithTag("Source");
        Rack01 = GameObject.FindGameObjectWithTag("Rack01");
        Rack02 = GameObject.FindGameObjectWithTag("Rack02");
        if (FullConveyor.name == "Conveyor(Clone)")
        {
            FullConveyor.GetComponent<NearInteractionGrabbable>().enabled = false;
            FullConveyor.GetComponent<ObjectManipulator>().enabled = false;
            FullConveyor.GetComponent<BoundsControl>().enabled = false;
        }
        FullConveyor.GetComponent<BoxCollider>().enabled = false;

        Queue.GetComponent<BoxCollider>().enabled = false;
        Queue.GetComponent<NearInteractionGrabbable>().enabled = false;
        Queue.GetComponent<ObjectManipulator>().enabled = false;
        Queue.GetComponent<BoundsControl>().enabled = false;

        Source.GetComponent<NearInteractionGrabbable>().enabled = false;
        Source.GetComponent<ObjectManipulator>().enabled = false;
        Source.GetComponent<BoundsControl>().enabled = false;

        Rack01.GetComponent<NearInteractionGrabbable>().enabled = false;
        Rack01.GetComponent<ObjectManipulator>().enabled = false;
        Rack01.GetComponent<BoundsControl>().enabled = false;

        Rack02.GetComponent<NearInteractionGrabbable>().enabled = false;
        Rack02.GetComponent<ObjectManipulator>().enabled = false;
        Rack02.GetComponent<BoundsControl>().enabled = false;
        ConveyorImage.SetActive(false);
        Rack01Image.SetActive(false);
    }

    //Activation and deactivation that needs to be done when STOP simulation
    [PunRPC] void StopSimDeactivation()
    {
        FullConveyor = GameObject.FindGameObjectWithTag("Conveyor");
        Queue = GameObject.FindGameObjectWithTag("Queue");
        Source = GameObject.FindGameObjectWithTag("Source");
        Rack01 = GameObject.FindGameObjectWithTag("Rack01");
        Rack02 = GameObject.FindGameObjectWithTag("Rack02");
        if (FullConveyor.name == "Conveyor(Clone)")
        {
            FullConveyor.GetComponent<ObjectManipulator>().enabled = true;
            FullConveyor.GetComponent<BoundsControl>().enabled = true;
            FullConveyor.AddComponent<NearInteractionGrabbable>();

        }
        FullConveyor.GetComponent<BoxCollider>().enabled = true;
        Queue.GetComponent<BoxCollider>().enabled = true;
        Queue.GetComponent<ObjectManipulator>().enabled = true;
        Queue.GetComponent<BoundsControl>().enabled = true;
        Queue.AddComponent<NearInteractionGrabbable>();
        Source.GetComponent<ObjectManipulator>().enabled = true;
        Source.GetComponent<BoundsControl>().enabled = true;
        Source.AddComponent<NearInteractionGrabbable>();

        if(Rack01.name == "Rack01(Clone)")
        {
            Rack01.GetComponent<ObjectManipulator>().enabled = true;
            Rack01.GetComponent<BoundsControl>().enabled = true;
            Rack01.AddComponent<NearInteractionGrabbable>();
        }

        Rack02.GetComponent<ObjectManipulator>().enabled = true;
        Rack02.GetComponent<BoundsControl>().enabled = true;
        Rack02.AddComponent<NearInteractionGrabbable>();
        ConveyorImage.SetActive(true);
        Rack01Image.SetActive(true);
    }

    //Set image tracking of cart to ACTIVE image 3 - cart 1
    [PunRPC] void SetCartImageActive()
    {
        CartImage.transform.Find("Cart1").Find("P1Hitbox").GetComponent<MeshRenderer>().enabled = true;
        CartImage.transform.Find("Cart1").Find("P2Hitbox").GetComponent<MeshRenderer>().enabled = true;
        CartImage.transform.Find("Cart1").Find("CartPlatform").GetComponent<MeshRenderer>().enabled = true;
    }

    //Set image tracking of cart to INACTIVE image 3 - cart 1
    [PunRPC] void SetCartImageInactive()
    {
        CartImage.transform.Find("Cart1").Find("P1Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CartImage.transform.Find("Cart1").Find("P2Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CartImage.transform.Find("Cart1").Find("CartPlatform").GetComponent<MeshRenderer>().enabled = false;
    }

    //Open or close operator dialog
    [PunRPC] void OpenCloseOPdialog(bool a)
    {
        OperatorDialog.SetActive(a);
    }

    //setting when cart image is true
    [PunRPC] void SetCartTrue(int id)
    {
        GameObject CI;
        // id= 13 for cart 1
        CI = PhotonView.Find(13).gameObject;
        if((ChooseMenu.Operator||ChooseMenu.LonelyManager) && !CI.GetPhotonView().IsMine)
        {
            CI.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            CI.transform.Find("Cart1").GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            //base.photonView.RequestOwnership();
        }
        CI.transform.Find("Cart1").Find("P1Hitbox").GetComponent<MeshRenderer>().enabled = true;
        CI.transform.Find("Cart1").Find("P2Hitbox").GetComponent<MeshRenderer>().enabled = true;
        CI.transform.Find("Cart1").Find("CartPlatform").GetComponent<MeshRenderer>().enabled = true;
        CI.transform.Find("Cart1").Find("P1Hitbox").GetComponent<BoxCollider>().enabled = true;
        CI.transform.Find("Cart1").Find("P2Hitbox").GetComponent<BoxCollider>().enabled = true;
        CI.transform.Find("Cart1").Find("CartPlatform").GetComponent<BoxCollider>().enabled = true;
        CI.transform.GetChild(0).GetComponent<CartPosition>().enabled = true;
    }

    // when cart image is false
    [PunRPC] void SetCartFalse(int id)
    {
        GameObject CI;
        CI = PhotonView.Find(13).gameObject;
        CI.transform.Find("Cart1").Find("P1Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CI.transform.Find("Cart1").Find("P2Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CI.transform.Find("Cart1").Find("CartPlatform").GetComponent<MeshRenderer>().enabled = false;
        CI.transform.GetChild(0).GetComponent<CartPosition>().enabled = false;
    }

    // when only lonely manager or operator -- transfer ownership of cart
    [PunRPC] void CartTransferOwner()
    {
        GameObject ImageTarget03;
        ImageTarget03 = PhotonView.Find(13).gameObject;
        if(ChooseMenu.Operator||ChooseMenu.LonelyManager)
        {
            ImageTarget03.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            base.photonView.RequestOwnership();
        }
    }
}
