using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using Vuforia;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;

public class Menu : MonoBehaviourPun
{
    public Material Pressed;
    public Material Unpressed;
    public Material Pinned;
    public Material Unpinned;
    public Material RulerPressed;
    public Material RulerUnpressed;
    public Material RackUnpressed;
    public Material StopPressed;

    public GameObject StartButton;
    public GameObject EQMenu;
    public GameObject ProductStateManager;
    public GameObject RulerManager;
    public GameObject RulerButton;
    public GameObject PointerButton;
    public GameObject IndexButton;
    public GameObject RulerPanel;
    public GameObject OperatorDialog;

    static public GameObject FullConveyor;
    static public GameObject Queue;
    static public GameObject Source;
    static public GameObject Rack01;
    static public GameObject Rack02;

    public Error Error;
    public Ruler ruler;
    public bool TrackingPressed;
    static public bool Simulation;
    public DeviceTracker DTracker;

    public GameObject ConveyorImage;
    public GameObject RackImage;
    public GameObject CartImage;
    public GameObject managerPanel;
    public GameObject OperatorTrainingPanel;
    public RPCManager RPCManager;
    public GameObject PlayerPos;
    public Transform Room;

    //Stop image tracking when start
    private void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StartObjectTracker);
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopObjectTracker);

        CartImage.transform.Find("Cart1").Find("P1Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CartImage.transform.Find("Cart1").Find("P2Hitbox").GetComponent<MeshRenderer>().enabled = false;
        CartImage.transform.Find("Cart1").Find("CartPlatform").GetComponent<MeshRenderer>().enabled = false;
        GameObject Menu = GameObject.Find("SimulationHandMenu");

        //Minimize the size of both these panels so it can still be manipulated but not seen
        managerPanel.transform.localScale = new Vector3(0, 0, 0);
        OperatorTrainingPanel.transform.localScale = new Vector3(0, 0, 0);

        if (ChooseMenu.Operator)
        {
            Menu.SetActive(true);
            //Change the operator's menu
            Menu.transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(2).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(3).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(4).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(5).GetComponent<SolverHandler>().AdditionalOffset = new Vector3(0, 0.108f, 0);
            Menu.transform.GetChild(1).GetChild(2).GetChild(6).GetComponent<SolverHandler>().AdditionalOffset = new Vector3(0, 0.072f, 0);
            Menu.transform.GetChild(1).GetChild(2).GetChild(7).GetComponent<SolverHandler>().AdditionalOffset = new Vector3(0, 0.036f, 0);
            OperatorDialog.SetActive(true);
            OperatorTrainingPanel.SetActive(true);
            Debug.Log("as Operator");
        }
        if (ChooseMenu.Manager)
        {
            Menu.SetActive(true);
            //Change the manager's menu
            Menu.transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(2).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(3).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(4).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(5).gameObject.SetActive(true);
            Menu.transform.GetChild(1).GetChild(2).GetChild(6).gameObject.SetActive(false);
            Menu.transform.GetChild(1).GetChild(2).GetChild(7).GetComponent<SolverHandler>().AdditionalOffset = new Vector3(0, -0.108f, 0);
            OperatorDialog.SetActive(false);
            PlayerPos.SetActive(false);
            Debug.Log("as Manager");
        }
        if (ChooseMenu.LonelyManager)
        {
            Menu.SetActive(true);
            OperatorDialog.SetActive(false);
            PlayerPos.SetActive(true);
            OperatorTrainingPanel.SetActive(true);
            Debug.Log("as Lonely Manager");
        }

    }
    //To stop some errors related to HoloLens Buttons to show up when closing the application
    void OnApplicationQuit()
    {
        managerPanel.transform.localScale = new Vector3(1, 1, 1);
        OperatorTrainingPanel.transform.localScale = new Vector3(1, 1, 1);
    }

    // To stop object tracker vuforia
    void StopObjectTracker()
    {
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        objectTracker.Stop();

    }
    // To start object tracker vuforia
    void StartObjectTracker()
    {
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        objectTracker.Start();
    }

    //Open ruler panel
    public void StartRuler()
    {
        if (RulerManager.activeSelf == false)
        {
            RulerManager.SetActive(true);
            RulerPanel.SetActive(true);
        }
        else
        {
            //Destroy Ruler stuffs upon closing ruler
            GameObject[] lines = GameObject.FindGameObjectsWithTag("Lines");
            GameObject[] spheres = GameObject.FindGameObjectsWithTag("RulerSpheres");
            GameObject[] LText = GameObject.FindGameObjectsWithTag("LengthText");
            foreach (GameObject target in lines)
            {
                GameObject.Destroy(target);
            }
            foreach (GameObject target in spheres)
            {
                GameObject.Destroy(target);
            }
            foreach (GameObject target in LText)
            {
                GameObject.Destroy(target);
            }
            Ruler.Spheres.Clear();
            Ruler.LengthText.Clear();
            Ruler.LineCount = 0;
            Ruler.i = 1;
            RulerManager.SetActive(false);
            RulerPanel.SetActive(false);
        }
    }

    //Start image tracking
    public void StartImageTracking(GameObject BackPlate)
    {

        //If Image Tracking Button was pressed then unpress it, else then press it
        TrackingPressed = TrackingPressed == true ? false : true;

        //If the action is pressing the button
        if (TrackingPressed)
        {
            //If not in simulation then start image tracking
            if (!Simulation)
            {
                VuforiaARController.Instance.RegisterVuforiaStartedCallback(StartObjectTracker);
                BackPlate.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Pressed;
            }
            //If in simulation don't need to start tracking
            else
            {
                BackPlate.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Pressed;
                return;
            }
        }
        //If the action is unpressing the button
        else if (!TrackingPressed)
        {
            //If not in simulation then stop image tracking
            if (!Simulation)
            {
                VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopObjectTracker);
                BackPlate.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Unpressed;
            }
            //If in simulation don't need to stop tracking
            else
            {
                BackPlate.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = Unpressed;
                return;
            }
        }
    }

    //Open or close Equipment Menu
    public void EquipmentMenu()
    {
        if (EQMenu.activeSelf == true)
        {
            EQMenu.SetActive(false);
        }
        else
        {
            EQMenu.SetActive(true);
        }
    }

    //Spawn clear scene dialog
    public void ClearSceneSpawnDialog()
    {
        Error.SpawnYesNoDialog("Clear Scene?", "Do you want to clear all equipment?", "C");
    }

    //Open or close ManagerPanel
    public void ManagerPanel()
    {
        //Close
        if (managerPanel.transform.localScale.magnitude != 0 && managerPanel.GetComponent<SolverHandler>().UpdateSolvers)
        {
            managerPanel.transform.localScale = new Vector3(0, 0, 0);
        }
        //Teleport it to manager
        else if (managerPanel.transform.localScale.magnitude != 0 && !managerPanel.GetComponent<SolverHandler>().UpdateSolvers)
        {
            managerPanel.GetComponent<SolverHandler>().UpdateSolvers = true;
            managerPanel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = Unpressed;
        }
        //Open
        else if (managerPanel.transform.localScale.magnitude == 0)
        {
            managerPanel.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    //If start moving the manager or operator panel unpin the panel --> see quad object manipulator
    public void ManagerManipulateStart(GameObject MyPanel)
    {
        MyPanel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = Unpressed;
        MyPanel.GetComponent<SolverHandler>().UpdateSolvers = true;
    }
    //If stop moving the manager panel pin the panel
    public void ManagerManipulateStop(GameObject MyPanel)
    {
        MyPanel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = Pressed;
        MyPanel.GetComponent<SolverHandler>().UpdateSolvers = false;
    }

    //Spawn reset origin dialog
    public void ResetOriginSpawnDialog()
    {
        Error.SpawnYesNoDialog("Reset origin point?", "Do you want to reset the origin point?", "R");
    }

    //Reset camera
    public void ResetCamera(GameObject dialog)
    {
        StartObjectTracker();
        Vector3 ImageCalib_pos = GameObject.Find("ImageCalibration").transform.position;
        Quaternion ImageCalib_rot = GameObject.Find("ImageCalibration").transform.rotation;
        GameObject Cam = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject Room = GameObject.FindGameObjectWithTag("WorldAnchor");
        Cam.transform.position = ImageCalib_pos;
        Cam.transform.rotation = ImageCalib_rot;
        StopObjectTracker();
        Room.transform.position = Cam.transform.position;
        Room.transform.rotation = Quaternion.Euler(Cam.transform.localRotation.eulerAngles.x, Cam.transform.localRotation.eulerAngles.y, Cam.transform.localRotation.eulerAngles.z);
        Debug.Log("x: " + Cam.transform.localRotation.eulerAngles.x + ", y: " + Cam.transform.localRotation.eulerAngles.y + ", z: " + Cam.transform.localRotation.eulerAngles.z);
        Destroy(dialog);
    }

    //Opening or closing Training Panel
    public void OPTrainingPanel()
    {
        if (OperatorTrainingPanel.transform.localScale.magnitude != 0 && OperatorTrainingPanel.GetComponent<SolverHandler>().UpdateSolvers)
        {
            OperatorTrainingPanel.transform.localScale = new Vector3(0, 0, 0);
        }
        else if (OperatorTrainingPanel.transform.localScale.magnitude != 0 && !OperatorTrainingPanel.GetComponent<SolverHandler>().UpdateSolvers)
        {
            OperatorTrainingPanel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = Unpressed;
            OperatorTrainingPanel.transform.Find("Backplate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
            OperatorTrainingPanel.GetComponent<SolverHandler>().UpdateSolvers = true;
        }
        else
        {
            OperatorTrainingPanel.transform.localScale = new Vector3(1, 1, 1);
            OperatorTrainingPanel.transform.Find("Backplate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
        }
    }
    //Change the color of the button of the ruler panel
    public void ChangeColorRuler()
    {
        if (Ruler.raypointer == true)
        {
            PointerButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = RulerPressed;
            IndexButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = RulerUnpressed;
        }
        else
        {
            PointerButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = RulerUnpressed;
            IndexButton.GetComponentInChildren<Transform>().GetComponentInChildren<MeshRenderer>().material = RulerPressed;
        }
    }

    //Change Color of button
    public void ChangeColor(Material Unpinned, GameObject Button)
    {
        Button.GetComponentInChildren<Transform>().GetChild(1).GetChild(0).GetComponentInChildren<MeshRenderer>().material = Unpinned;
    }

    //Close Error Dialog
    public void CloseDialog(GameObject dialog)
    {
        Destroy(dialog);
    }

    // Setting photonview for cart image is true or false
    public void Thing1()
    {
        PhotonView photonView = PhotonView.Get(RPCManager);
        photonView.RPC("SetCartTrue", RpcTarget.All, 13);
    }
    public void Thing2()
    {
        PhotonView photonView = PhotonView.Get(RPCManager);
        photonView.RPC("SetCartFalse", RpcTarget.All, 13);
    }

    //Start or stop Simlulation
    public void StartSimulation()
    {
        //Start simulation
        if (!ProductStateManager.activeSelf)
        {
            PhotonView photonView = PhotonView.Get(RPCManager);
            //If missing equipment then can't stop
            if (!GameObject.FindGameObjectWithTag("Source"))
            {
                Error.SpawnDialog("ERROR", "Missing Source!", false);
                return;
            }
            else if (!GameObject.FindGameObjectWithTag("Queue"))
            {
                Error.SpawnDialog("ERROR", "Missing Queue!", false);
                return;
            }
            else if (!GameObject.FindGameObjectWithTag("Conveyor"))
            {
                Error.SpawnDialog("ERROR", "Missing Conveyor!", false);
                return;
            }
            else if (!GameObject.FindGameObjectWithTag("Rack01") || !GameObject.FindGameObjectWithTag("Rack02"))
            {
                Error.SpawnDialog("ERROR", "Missing Rack!", false);
                return;
            }
            //If rack position hasn't been choosed yet then return
            if (Factors.RackPosIndex != 1 && Factors.RackPosIndex != 2)
            {
                Error.SpawnDialog("ERROR", "Didn't choose rack position on Manager Panel!", false);
                return;
            }
            //Lock the rack position button
            if (managerPanel.activeSelf)
            {
                switch (Factors.RackPosIndex)
                {
                    case 1:
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Pressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Unpressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").GetComponent<BoxCollider>().enabled = false;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").GetComponent<BoxCollider>().enabled = false;
                        break;
                    case 2:
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Unpressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Pressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").GetComponent<BoxCollider>().enabled = false;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").GetComponent<BoxCollider>().enabled = false;
                        break;
                }
            }
            //Set cart relative stuff to true
            Thing1();

            //Set ProductManager to active to start simulation
            photonView.RPC("ActivateProductManager", RpcTarget.All, true);

            //Bool for checking in image tracking
            Simulation = true;

            //Change text on button 
            StartButton.GetComponentInChildren<Transform>().GetComponentInChildren<TextMeshPro>().text = "STOP";
            StartButton.transform.Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = StopPressed;
            photonView.RPC("OpenCloseOPdialog", RpcTarget.Others, false);
            photonView.RPC("StartSimActivation", RpcTarget.All);

            //If Image Tracking Button is not pressed then need to also start image tracking
            if (!TrackingPressed)
            {
                VuforiaARController.Instance.RegisterVuforiaStartedCallback(StartObjectTracker);
            }

            //Destroy Products on conveyor
            while (ProductState.InConveyor.Count != 0)
            {
                if (!ProductState.InConveyor.First().GetComponent<PhotonView>().IsMine)
                {
                    int id = ProductState.InConveyor.First().GetComponent<PhotonView>().ViewID;
                    photonView.RPC("OPdestroy", RpcTarget.Others, id);
                    ProductState.InConveyor.RemoveFirst();
                }
                else
                {
                    PhotonNetwork.Destroy(ProductState.InConveyor.First());
                    ProductState.InConveyor.RemoveFirst();
                }
            }

            //Destroy Products in queue
            while (ProductState.InQueue.Count != 0)
            {
                if (!ProductState.InQueue.First().GetComponent<PhotonView>().IsMine)
                {
                    int id = ProductState.InQueue.First().GetComponent<PhotonView>().ViewID;
                    photonView.RPC("OPdestroy", RpcTarget.Others, id);
                    ProductState.InQueue.RemoveFirst();
                }
                else
                {
                    PhotonNetwork.Destroy(ProductState.InQueue.First());
                    ProductState.InQueue.RemoveFirst();
                }
            }

            //Destroy Products on cart
            while (ProductState.InCart.Count != 0)
            {
                if (!ProductState.InCart.First().GetComponent<PhotonView>().IsMine)
                {
                    int id = ProductState.InCart.First().GetComponent<PhotonView>().ViewID;
                    photonView.RPC("OPdestroy", RpcTarget.Others, id);
                    ProductState.InCart.RemoveFirst();

                }
                else
                {
                    PhotonNetwork.Destroy(ProductState.InCart.First());
                    ProductState.InCart.RemoveFirst();
                }
            }

            //Destroy Products in Rack01
            while (ProductState.InRack01.Count != 0)
            {
                if (!ProductState.InRack01.Peek().GetComponent<PhotonView>().IsMine)
                {
                    int id = ProductState.InRack01.Peek().GetComponent<PhotonView>().ViewID;
                    photonView.RPC("OPdestroy", RpcTarget.Others, id);
                    ProductState.InRack01.Pop();
                }
                else
                {
                    PhotonNetwork.Destroy(ProductState.InRack01.Peek());
                    ProductState.InRack01.Pop();
                }
            }

            //Destroy Products in Rack02
            while (ProductState.InRack02.Count != 0)
            {
                if (!ProductState.InRack02.Peek().GetComponent<PhotonView>().IsMine)
                {
                    int id = ProductState.InRack02.Peek().GetComponent<PhotonView>().ViewID;
                    photonView.RPC("OPdestroy", RpcTarget.Others, id);
                    ProductState.InRack02.Pop();
                }
                else
                {
                    PhotonNetwork.Destroy(ProductState.InRack02.Peek());
                    ProductState.InRack02.Pop();
                }
            }
        }

        //Stop simulation
        else if (ProductStateManager.activeSelf)
        {
            PhotonView photonViewE = PhotonView.Get(Error);
            photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "Notification", "Simulation Stopped.", false);

            //Bool for checking in image tracking
            Simulation = false;

            //Set Spawner to inactive to stop simulation
            PhotonView photonView = PhotonView.Get(RPCManager);

            //Change text on button 
            StartButton.GetComponentInChildren<Transform>().GetComponentInChildren<TextMeshPro>().text = "START";
            StartButton.transform.Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
            photonView.RPC("StopSimDeactivation", RpcTarget.All);
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("LoadArea"));
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("UnloadArea01"));
            PhotonNetwork.Destroy(GameObject.FindGameObjectWithTag("UnloadArea02"));
            photonView.RPC("OpenCloseOPdialog", RpcTarget.Others, true);

            //Set cart relative stuff to false
            Thing2();
            photonView.RPC("ActivateProductManager", RpcTarget.All, false);

            //Unlock the rack position buttons
            if (managerPanel.activeSelf)
            {
                switch (Factors.RackPosIndex)
                {
                    case 1:
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Pressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Unpressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").GetComponent<BoxCollider>().enabled = true;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").GetComponent<BoxCollider>().enabled = true;
                        break;
                    case 2:
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Unpressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").Find("BackPlate").Find("Quad").GetComponent<MeshRenderer>().material = Pressed;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton01").GetComponent<BoxCollider>().enabled = true;
                        managerPanel.transform.Find("FactorPanel").Find("RackPosition").Find("RackButton02").GetComponent<BoxCollider>().enabled = true;
                        break;
                }
            }

            //If Image Tracking Button is not pressed then need to also stop image tracking
            if (!TrackingPressed)
            {
                VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopObjectTracker);
            }
        }
    }

    //Open Quit dialog
    public void QuitAppSpawnDialog()
    {
        Error.SpawnYesNoDialog("QUIT?", "Do you want to quit?", "Q");
    }

    //Quit application
    public void ExitApp()
    {
        Debug.Log("Quit application.");
        Application.Quit();
    }
}
