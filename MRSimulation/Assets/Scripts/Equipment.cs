using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;
using Vuforia;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Equipment : MonoBehaviour
{
    public Menu Menu;
    public Error Error;
    public Material Pinned;
    public Material Unpinned;
    public List<GameObject> EQ;
    static GameObject TempOnHand;
    //a[] is an array of bool for checking if same type of equipment is already in the scene
    static public bool[] a = new bool[6];
    public static bool ConveyorFoundBool;
    public static bool RackFoundBool;
    static public bool IsTaptoPlaceConveyor;
    static public bool IsTaptoPlaceRack01;

    public GameObject ConveyorTarget;
    public GameObject Rack01Target;
    public GameObject DeleteButton;
    public GameObject SetPosButton;
    public GameObject TempSpawned;

    //When object was placed with tap to place and then disable the tap to place script
    [PunRPC]
    void StopTaptoPlace(int ID)
    {
        GameObject tap_to_place_script = PhotonView.Find(ID).gameObject;
        tap_to_place_script.GetComponent<SolverHandler>().enabled = false;
        tap_to_place_script.GetComponent<TapToPlace>().enabled = false;
    }

    //Set the rack01 or conveyor as the child of the corresponding image
    [PunRPC]
    void SetPTrans(int ID, float leftright, float behind, float lower, float Rx, float Ry, float Rz, string ImageName)
    {
        //check which image to attach to
        int i;
        switch (ImageName)
        {
            case "ImageTarget01": // scaled material of conveyer
                i = 11;
                break;
            case "ImageTarget02": // scaled material of rack
                i = 12;
                break;
            default:
                i = -1;
                break;
        }
        GameObject image_target = PhotonView.Find(i).gameObject;
        GameObject T = PhotonView.Find(ID).gameObject;
        T.transform.parent = image_target.transform;

        //Set the postition of the equipment(rack01 or conveyor) with local position under the image
        T.transform.localPosition = new Vector3(leftright, behind, lower);
        T.transform.localRotation = Quaternion.Euler(Rx, Ry, Rz);

        //Print to debug log the position of image target
        Debug.Log("Image name: " + ImageName + ", x: " + PhotonView.Find(i).gameObject.transform.localRotation.eulerAngles.x + ", y: " + PhotonView.Find(i).gameObject.transform.localRotation.eulerAngles.y + ", z: " + PhotonView.Find(i).gameObject.transform.localRotation.eulerAngles.z);

    }

    //Set the parent of the conveyor or rack01 as the image when set position button is unpressed
    [PunRPC]
    void SetP(string Equipment)
    {
        switch (Equipment)
        {
            case "ConveyorTarget(Clone)":
                dynamic target_conveyer = GameObject.FindGameObjectWithTag("Conveyor");
                target_conveyer.transform.parent = PhotonView.Find(11).transform;
                target_conveyer.transform.localPosition = new Vector3(0, -0.365f, -0.27f);
                target_conveyer.transform.localRotation = Quaternion.Euler(90, 90, 0);
                break;
            case "Rack01Target(Clone)":
                dynamic target_rack01 = GameObject.FindGameObjectWithTag("Rack01");
                target_rack01.transform.parent = PhotonView.Find(12).transform;
                target_rack01.transform.localPosition = new Vector3(0.2175f, -0.65f, -0.48f);
                target_rack01.transform.localRotation = Quaternion.Euler(0, 180, 90);
                break;
            default:
                break;
        }
    }

    //Set the rack01 or conveyor parent to null when set position button is pressed
    [PunRPC]
    void SetPnull(string Equipment)
    {
        switch (Equipment)
        {
            case "ConveyorTarget(Clone)":
                GameObject.FindGameObjectWithTag("Conveyor").transform.parent = null;
                break;
            case "Rack01Target(Clone)":
                GameObject.FindGameObjectWithTag("Rack01").transform.parent = null;
                break;
            default:
                break;
        }
    }
    //Show the delete button upon moving an equipment
    public void ShowDelete(GameObject A)
    {
        if (!A.transform.Find("DeleteButton(Clone)"))
        {
            GameObject B = Instantiate(DeleteButton, new Vector3(0, 0, 0), Quaternion.identity);
            B.transform.parent = A.transform;
            B.transform.localPosition = new Vector3(0, 0, -0.7f);
        }
        else
        {
            return;
        }
    }

    //Hide the delete button
    public void HideDelete(GameObject A)
    {
        if (A.transform.Find("DeleteButton(Clone)"))
        {
            GameObject B = A.transform.Find("DeleteButton(Clone)").gameObject;
            Destroy(B, 10);
        }
        else
        {
            return;
        }
    }

    public void DeleteAll(GameObject dialog)
    {
        for (int i = 0; i < 5; i++)
        {
            a[i] = false;
        }
        if (GameObject.FindGameObjectWithTag("Rack01"))
        {
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("Rack01").gameObject);
        }
        if (GameObject.FindGameObjectWithTag("Rack02"))
        {
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("Rack02").gameObject);
        }
        if (GameObject.FindGameObjectWithTag("Conveyor"))
        {
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("Conveyor").gameObject);
        }
        if (GameObject.FindGameObjectWithTag("Source"))
        {
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("Source").gameObject);
        }
        if (GameObject.FindGameObjectWithTag("Queue"))
        {
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("Queue").gameObject);
        }
        IsTaptoPlaceConveyor = false;
        IsTaptoPlaceRack01 = false;
        Destroy(dialog);
    }

    //Delete the equipment with delete button
    public void DeleteEQ(GameObject button)
    {
        GameObject A = button.transform.parent.gameObject;
        int n;
        switch (A.tag)
        {
            case ("Rack01"):
                n = 0;
                IsTaptoPlaceRack01 = false;
                break;
            case ("Rack02"):
                n = 1;
                break;
            case ("Source"):
                n = 2;
                break;
            case ("Conveyor"):
                n = 3;
                IsTaptoPlaceConveyor = false;
                if (A.transform.Find("Source(Clone)"))
                {
                    a[2] = false;

                }
                if (A.transform.Find("Queue(Clone)"))
                {
                    a[4] = false;

                }
                break;
            case ("Queue"):
                n = 4;
                break;
            default:
                n = 0;
                break;
        }
        a[n] = false;
        PhotonNetwork.Destroy(A);
    }

    //Destroy Rack01 or Conveyor to inactive when lost track
    public void ImageTrackLost(GameObject Image)
    {
        int n;
        string name;
        switch (Image.name)
        {
            case "ImageTarget01":
                n = 3;
                name = "ConveyorTarget(Clone)";
                break;
            case "ImageTarget02":
                n = 0;
                name = "Rack01Target(Clone)";
                break;
            default:
                n = 0;
                name = "";
                break;
        }
        if (Image.transform.childCount > 1)
        {
            PhotonNetwork.Destroy(Image.transform.Find(name).gameObject);
            Destroy(Image.transform.Find("SetPostionButton(Clone)").gameObject);
            a[n] = false;
            if (n == 3)
            {
                a[2] = false;
                a[4] = false;
            }
        }
        else if (Image.transform.childCount > 0)
        {
            Destroy(Image.transform.GetChild(0).gameObject);
        }
        else
        {
            return;
        }
    }

    //Spawn Rack01 or Conveyor when found image
    public void ImageTrackFound(GameObject Image)
    {
        GameObject x;
        int n;
        float behind;
        float lower;
        float leftright;
        float Rx;
        float Ry;
        float Rz;

        //Check which image is being tracked by name
        switch (Image.name)
        {
            case "ImageTarget01":
                x = ConveyorTarget;
                n = 3;
                leftright = 0;
                behind = -0.365f;
                lower = -0.27f;
                Rx = 90f;
                Ry = 90f;
                Rz = 0;
                break;
            case "ImageTarget02":
                x = Rack01Target;
                n = 0;
                leftright = 0.2175f;
                behind = -0.65f;
                lower = -0.48f;
                Rx = 0f;
                Ry = 180f;
                Rz = 90f;
                break;
            default:
                x = Rack01Target;
                n = 0;
                leftright = 1;
                behind = 1;
                lower = 1;
                Rx = 0f;
                Ry = 0f;
                Rz = 0f;
                break;
        }

        if (a[n] == false)
        {
            if (n == 3)
            {
                if (a[2] || a[4])
                {
                    Error.SpawnDialog("ERROR", "Source or Queue already exist! Please remove them first or use the tap to place function to place the conveyor!", false);
                    return;
                }

                a[2] = true;
                a[4] = true;
            }
            TempSpawned = PhotonNetwork.Instantiate(x.name, new Vector3(0, 0, 0), Quaternion.identity);
            int id = TempSpawned.GetComponent<PhotonView>().ViewID;
            PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));
            photonView.RPC("SetPTrans", RpcTarget.All, id, leftright, behind, lower, Rx, Ry, Rz, Image.name);
            GameObject S = Instantiate(SetPosButton, new Vector3(0, 0, 0), Quaternion.identity);
            Menu.ChangeColor(Unpinned, S);
            S.transform.parent = Image.transform;
            S.transform.localPosition = new Vector3(0.2f, 0.05f, 0);
            S.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            a[n] = true;
        }
        else if (n == 3 && a[n] == true && IsTaptoPlaceConveyor)
        {
            Error.SpawnDialog("ERROR", "Conveyor already exist! Please remove it first if you wish to use this image target, or use the tap to place function to place the queue and the source!", false);
            return;
        }
        else if (n == 0 && a[n] == true && IsTaptoPlaceRack01)
        {
            Error.SpawnDialog("ERROR", "Rack01 already exist! Please remove it first if you wish to use this image target!", false);
            return;
        }
        else if (!GameObject.Find("SetPostionButton(Clone)"))
        {
            GameObject S = Instantiate(SetPosButton, new Vector3(0, 0, 0), Quaternion.identity);
            Menu.ChangeColor(Pinned, S);
            S.transform.parent = Image.transform;
            S.transform.localPosition = new Vector3(0.2f, 0.05f, 0);
            S.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        }
        else
        {
            return;
        }
    }

    //Fix place Rack01 or Conveyor when pressed the set position button 
    public void TrackedEquipment(GameObject SetPosButton)
    {

        //For Conveyor
        if (SetPosButton.transform.parent.name == "ImageTarget01")
        {
            if (!GameObject.FindGameObjectWithTag("Conveyor"))
            {
                Error.SpawnDialog("ERROR", "No Conveyor!", false);
                return;

            }
            //If the button is not pressed which means equipment is not fixed to real world yet
            if (!ConveyorFoundBool)
            {
                //Set parent of Conveyor to null so it no longer follows the image target
                PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));
                photonView.RPC("SetPnull", RpcTarget.All, "ConveyorTarget(Clone)");
                //ConveyorFoundBool set to true
                ConveyorFoundBool = true;
                //Change color of button to green to indicate that it has been pressed
                Menu.ChangeColor(Pinned, SetPosButton);
                a[3] = true;//for conveyor
                a[2] = true;//for source
                a[4] = true;//for queue

            }
            //If the button is pressed which means equipment has been fixed to real world
            else if (ConveyorFoundBool)
            {
                //Set Conveyor parent back to the image for further tracking
                PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));
                photonView.RPC("SetP", RpcTarget.All, "ConveyorTarget(Clone)");
                //Change color of button
                Menu.ChangeColor(Unpinned, SetPosButton);
                //ConveyorFoundBool set to false
                ConveyorFoundBool = false;
                a[3] = false;
                a[2] = false;
                a[4] = false;
            }
        }

        //For Rack01
        else if (SetPosButton.transform.parent.name == "ImageTarget02")
        {
            if (!GameObject.FindGameObjectWithTag("Rack01"))
            {
                Error.SpawnDialog("ERROR", "No Rack01!", false);
                return;
            }

            //If the button is not pressed
            if (!RackFoundBool)
            {
                //Set parent of Rack01 to null
                PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));
                photonView.RPC("SetPnull", RpcTarget.All, "Rack01Target(Clone)");
                //RackFoundBool set to true
                RackFoundBool = true;
                //Change color of button
                Menu.ChangeColor(Pinned, SetPosButton);
                //Rack01 is placed in the scene so no more Rack01 can be placed set the checking bool to true
                a[0] = true;
            }
            //If the button is pressed
            else if (RackFoundBool)
            {
                //Set Conveyor parent back to the image for further tracking
                PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));
                photonView.RPC("SetP", RpcTarget.All, "Rack01Target(Clone)");
                //Change color of button
                Menu.ChangeColor(Unpinned, SetPosButton);
                RackFoundBool = false;
                a[0] = false;
            }
        }
    }

    //For clicking the button on the equipment scroll panel to spawn equipment for tap to place
    public void Click(int n)
    {
        //Only spawn when nothing is already on hand to prevent double clicking and also when the type of equipment is not in scene yet
        if (TempOnHand == null && a[n] == false)
        {
            if (n == 3)
            {
                IsTaptoPlaceConveyor = true;
            }
            if (n == 0)
            {
                IsTaptoPlaceRack01 = true;
            }
            //Spawn the equipment
            TempOnHand = PhotonNetwork.Instantiate(EQ[n].name, new Vector3(0, 0, 0), Quaternion.identity);
            int id = TempOnHand.GetComponent<PhotonView>().ViewID;
            PhotonView photonView = PhotonView.Get(GameObject.FindGameObjectWithTag("EquipmentPlacer"));

            //StopTaptoPlace only called for operator because the equipment is on managers hand if didn't disable for the operator the equipment will be controlled by multiple user's hands 
            photonView.RPC("StopTaptoPlace", RpcTarget.OthersBuffered, id);
            a[n] = true;
        }
        else if (a[n] == true)
        {
            Error.SpawnDialog("ERROR", "Equipment type already exist!", false);
            return;
        }
        else
        {
            return;
        }
    }

    //Stop tap to place for the manager
    public void StopPlace()
    {
        TempOnHand.GetComponent<SolverHandler>().enabled = false;
        TempOnHand.GetComponent<TapToPlace>().enabled = false;
        TempOnHand = null;
    }

    private void Update()
    {

    }

}
