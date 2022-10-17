using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;
using Photon.Pun;

public class ProductMovement : MonoBehaviour
{
    public ProductState productState;
    public Error Error;
    static public Vector3 OldPos;
    static public Quaternion OldRot;
    public bool Elevated;
    public static bool NO;
    private bool Cart1D;
    private bool Cart2D;
    private bool R1D1;
    private bool R1D2;
    private bool R1D3;
    private bool R1D4;
    private bool R2D1;
    private bool R2D2;
    private bool R2D3;
    private bool R2D4;
    static string D1name;
    static string D2name;
    static int TempOnQ;
    static public int HECount;

    //variable old transport update
    [PunRPC] void OldTransUpdate(Vector3 Oldp, Quaternion Oldr, bool Ele)
    {
        Elevated = Ele;
        OldPos = Oldp;
        OldRot = Oldr;
    }

    //trigger enter box of cart and rack
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CartDetection1":
                Cart1D = true;
                break;
            case "CartDetection2":
                Cart2D = true;
                break;
            case "Rack01Detection1":
                R1D1 = true;
                D1name = other.name;
                break;
            case "Rack01Detection2":
                R1D2 = true;
                D1name = other.name;
                break;
            case "Rack01Detection3":
                R1D3 = true;
                D1name = other.name;
                break;
            case "Rack01Detection4":
                R1D4 = true;
                D1name = other.name;
                break;
            case "Rack02Detection1":
                R2D1 = true;
                D2name = other.name;
                break;
            case "Rack02Detection2":
                R2D2 = true;
                D2name = other.name;
                break;
            case "Rack02Detection3":
                R2D3 = true;
                D2name = other.name;
                break;
            case "Rack02Detection4":
                R2D4 = true;
                D2name = other.name;
                break;
            default:
                break;
        }

        if (other.gameObject.tag == "UnloadArea01" || other.gameObject.tag == "UnloadArea02")
        {
            this.GetComponent<ObjectManipulator>().enabled = true;
            this.GetComponent<NearInteractionGrabbable>().enabled = true;
            this.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    //exit the trigger
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CartDetection1":
                Cart1D = false;
                break;
            case "CartDetection2":
                Cart2D = false;
                break;
            case "Rack01Detection1":
                R1D1 = false;
                break;
            case "Rack01Detection2":
                R1D2 = false;
                break;
            case "Rack01Detection3":
                R1D3 = false;
                break;
            case "Rack01Detection4":
                R1D4 = false;
                break;
            case "Rack02Detection1":
                R2D1 = false;
                break;
            case "Rack02Detection2":
                R2D2 = false;
                break;
            case "Rack02Detection3":
                R2D3 = false;
                break;
            case "Rack02Detection4":
                R2D4 = false;
                break;
            default:
                break;
        }
    }

    //when product start to move to the queue by itself
    public void newStartManip()
    {
        PhotonView photonView = PhotonView.Get(this);
        this.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        TempOnQ = ProductState.QueueCount;
        OldPos = this.transform.localPosition;
        OldRot = this.transform.localRotation;
        photonView.RPC("OldTransUpdate", RpcTarget.All, OldPos, OldRot, Elevated);
        this.GetComponent<Rigidbody>().isKinematic = true;
        if (this.GetComponent<StopMove>())
        {
            Destroy(this.GetComponent<StopMove>());
        }
    }

    //when product stop moving, and time to pickup by operator
    public void StopManip()
    {
        PhotonView photonView = PhotonView.Get(this);
        PhotonView photonViewPSM = PhotonView.Get(GameObject.Find("ProductStateManager"));
        bool CheckQtoC;
        bool CheckCtoR;
        string Name = null;
        switch (this.tag)
        {
            case "Product01":
                CheckQtoC = Cart1D;
                if (R1D1 || R1D2 || R1D3 || R1D4)
                {
                    CheckCtoR = true;
                    Name = D1name;
                }
                else
                {
                    CheckCtoR = false;
                    Name = null;
                }
                break;
            case "Product02":
                CheckQtoC = Cart2D;
                if (R2D1 || R2D2 || R2D3 || R2D4)
                {
                    CheckCtoR = true;
                    Name = D2name;
                }
                else
                {
                    CheckCtoR = false;
                    Name = null;
                }
                break;
            default:
                CheckQtoC = false;
                CheckCtoR = false;
                break;
        }
        if (CheckQtoC && this.transform.parent.tag == "Conveyor")
        {

            if (productState.QueueToCart(this.gameObject))
            {
                this.GetComponent<ObjectManipulator>().enabled = false;
                this.GetComponent<NearInteractionGrabbable>().enabled = false;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = true;
                return;
            }
            else
            {
                if (TempOnQ == 1 && ProductState.QueueCount > 1)
                {
                    Elevated = true;
                    OldPos = OldPos + new Vector3(0, 0, -0.1f);
                    photonView.RPC("OldTransUpdate", RpcTarget.All, OldPos, OldRot, Elevated);
                }
                this.transform.localPosition = OldPos;
                this.transform.localRotation = OldRot;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                return;
            }

        }
        else if (CheckCtoR && this.transform.parent.tag == "Cart")
        {
            if (productState.CartToRack(this.gameObject, Name))
            {
                this.GetComponent<ObjectManipulator>().enabled = false;
                this.GetComponent<NearInteractionGrabbable>().enabled = false;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                return;
            }
            else
            {
                this.transform.localPosition = OldPos;
                this.transform.localRotation = OldRot;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                return;
            }
        }
        else if (!CheckCtoR && this.transform.parent.tag == "Cart")
        {
            if ((R1D1 || R1D2 || R1D3 || R1D4) || (R2D1 || R2D2 || R2D3 || R2D4))
            {
                Error.SpawnDialog("ERROR", "Wrong rack!", true);
                HECount += 1;
                photonViewPSM.RPC("HumanErrorUpdate", RpcTarget.All, HECount);
                this.transform.localPosition = OldPos;
                this.transform.localRotation = OldRot;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                return;
            }
            else
            {
                this.transform.localPosition = OldPos;
                this.transform.localRotation = OldRot;
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                return;
            }

        }
        else
        {
            if (this.transform.parent.tag == "Conveyor" && TempOnQ == 1 && ProductState.QueueCount > 1)
            {
                Elevated = true;
                Debug.Log("The old position is " + OldPos);
                OldPos = OldPos + new Vector3(0, 0, -0.1f);
                photonView.RPC("OldTransUpdate", RpcTarget.All, OldPos, OldRot, Elevated);
                Debug.Log("The new old position is " + OldPos);
            }
            this.transform.localPosition = OldPos;
            this.transform.localRotation = OldRot;
            this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            return;
        }
    }
}
