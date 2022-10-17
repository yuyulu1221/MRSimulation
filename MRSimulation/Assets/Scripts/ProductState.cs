using System.Collections;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using System.Linq;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ProductState : MonoBehaviour
{
    public Menu menu;
    public Error Error;
    public Material Transparent;
    public Material P1BoxMat;
    public Material P2BoxMat;
    
    public GameObject FollowAssessmentMenu;
    public GameObject OperatorTrainingPanel;
    public RPCManager RPCManager;
    public CartHitboxPos cartHitbox;
    public OperatorTraining OperatorTraining;

    public bool SelectBegin;
    public GameObject Source;
    public GameObject LA;
    public GameObject UA01;
    public GameObject UA02;

    static public float MinuteTimer = 0f;
    static public float timer = 40f;
    static public float FullTimer = 5f;
    static public float PassedTime = 0f;
    static public float OverflowTime = 0f;
    static public float BlockingTime = 0f;
    static public float SpawnTime = 12f;
    static public float SpawningProb01 = 0.5f;
    static public float force = 0.5f;
    static public int QueueSize = 5;
    
    static public LinkedList<GameObject> InConveyor = new LinkedList<GameObject>();
    static public LinkedList<GameObject> InQueue = new LinkedList<GameObject>();
    static public LinkedList<GameObject> InCart = new LinkedList<GameObject>();
    static public Stack<GameObject> InRack01 = new Stack<GameObject>();
    static public Stack<GameObject> InRack02 = new Stack<GameObject>();

    static public int ConveyorCount;
    static public int QueueCount;
    static public int CartCount;
    static public int Rack01Count;
    static public int Rack02Count;
    static public int CP1 = 0;
    static public int CP2 = 0;
    private float P1x = 0.15f;
    private float P1y = 0.1f;
    private float P1z = -0.4f;
    private float P2y = 0.1f;
    private float P2z = -0.4f;
    static public int TotalP1;
    static public int TotalP2;
    static public int Batch = 15;

    static public bool FIFO;
    static public int Block;
    static public int CarryCounts;
    static public int ErrorCounts;
    static public int HumanErrorCounts;
    static public float TempFullTime;
    static public int CYTotalProducts;
    static public float OldCyTime;
    static public int OldErrorCount;
    static public int OldCarryCount;
    static public int OldHumanErrorCounts;
    static public float OldTimePassed;
    static public float OldOverflowTime;
    static public float OldBlockingTime;

    //Tracking number of products on both racks within different levels
    static public int R1_1;
    static public int R1_2;
    static public int R1_3;
    static public int R1_4;
    static public int R2_1;
    static public int R2_2;
    static public int R2_3;
    static public int R2_4;
    static public int MissedP1;
    static public int MissedP2;
    static public int MissedProduct;
    static public int OldMissedP1;
    static public int OldMissedP2;
    static public int OldMissedProduct;
    int P1SoldperMin;
    int P2SoldperMin;
    
    //operator update count
    [PunRPC] 
    void OperatorUpdate(int Con, int Que, int Car, int R01, int R02)
    {
        ConveyorCount = Con;
        QueueCount = Que;
        CartCount = Car;
        Rack01Count = R01;
        Rack02Count = R02;
    }

    //human error update count
    [PunRPC] 
    void HumanErrorUpdate(int count)
    {
        HumanErrorCounts = count;
    }

    //Move cart hitbox P1
    [PunRPC] 
    void MoveP1BoxRPC(Vector3 pos)
    {
        GameObject C = GameObject.Find("Cart1");
        foreach (Transform child in C.transform)
        {
            if (child.tag == "CartDetection1")
            {
                child.transform.localPosition = pos;
            }
        }
    }

    //Move cart hitbox P2
    [PunRPC] 
    void MoveP2BoxRPC(Vector3 pos)
    {
        GameObject C = GameObject.Find("Cart1");
        foreach (Transform child in C.transform)
        {
            if (child.tag == "CartDetection2")
            {
                child.transform.localPosition = pos;
            }
        }
    }

    //hide cart hitbox if product placed
    [PunRPC] 
    void HideP12BoxesRPC()
    {
        GameObject C = GameObject.Find("Cart1");
        foreach (Transform child in C.transform)
        {
            if (child.tag == "CartDetection1")
            {
                Debug.Log("P1box Set to transparent");
                child.GetComponent<MeshRenderer>().material = Transparent;
            }
            if (child.tag == "CartDetection2")
            {
                Debug.Log("P2box Set to transparent");
                child.GetComponent<MeshRenderer>().material = Transparent;
            }
        }
    }

    //Show cart hitbox
    [PunRPC] 
    void ShowP12BoxesRPC()
    {
        GameObject C = GameObject.Find("Cart1");
        foreach (Transform child in C.transform)
        {
            if (child.tag == "CartDetection1")
            {
                Debug.Log("P1box Set to normal");
                child.GetComponent<MeshRenderer>().material = P1BoxMat;
            }
            if (child.tag == "CartDetection2")
            {
                Debug.Log("P2box Set to normal");
                child.GetComponent<MeshRenderer>().material = P2BoxMat;
            }
        }
    }

    //Keep the spawned product in the middle of the conveyor so they dont slipp off
    [PunRPC] 
    void StayMid(int id)
    {
        GameObject Moving = PhotonView.Find(id).gameObject;
        Vector3 localPos = Moving.transform.localPosition;
        Moving.transform.localPosition = new Vector3(0, localPos.y, localPos.z);
    }

    //Add RigidBody to the product so operator can click it
    [PunRPC] 
    void AddRigid(int ID)
    {
        GameObject P = PhotonView.Find(ID).gameObject;
        //P.AddComponent<Rigidbody>();
        P.GetComponent<Rigidbody>().isKinematic = true;
        P.GetComponent<Rigidbody>().useGravity = false;
    }

    //Move product's parent from QUEUE to CART (QtoC)
    [PunRPC] 
    void QtoC(int ID, float x, float y, float z)
    {
        PhotonView.Find(ID).transform.parent = GameObject.FindGameObjectWithTag("CartPlatform").transform.parent;
        PhotonView.Find(ID).transform.rotation = GameObject.FindGameObjectWithTag("CartPlatform").transform.rotation;
        PhotonView.Find(ID).transform.localPosition = GameObject.FindGameObjectWithTag("CartPlatform").transform.localPosition + new Vector3(x, y, z);
    }

    //Set the selected product's parent from cart to rack01
    [PunRPC] 
    void CtoR1(int ID, float x, float y, float z)
    {
        PhotonView.Find(ID).transform.parent = GameObject.FindGameObjectWithTag("Rack01").transform;
        PhotonView.Find(ID).transform.rotation = GameObject.FindGameObjectWithTag("Rack01").transform.rotation;
        PhotonView.Find(ID).transform.localPosition = new Vector3(x, y, z);
    }

    //Set the selected product's parent from queue to rack02
    [PunRPC] 
    void CtoR2(int ID, float x, float y, float z)
    {
        PhotonView.Find(ID).transform.parent = GameObject.FindGameObjectWithTag("Rack02").transform;
        PhotonView.Find(ID).transform.rotation = GameObject.FindGameObjectWithTag("Rack02").transform.rotation;
        PhotonView.Find(ID).transform.localPosition = new Vector3(x, y, z);
    }

    //Set the selected product's parent to the conveyor when it spawns - from source to conveyor
    [PunRPC] 
    void StoQ(int ID)
    {
        PhotonView.Find(ID).transform.parent = GameObject.FindGameObjectWithTag("Source").transform.parent;
    }

    //Set the loading/unloading areas' position
    [PunRPC] 
    void SetAreaPos(int id01, int id02, int id03)
    {
        GameObject Q = GameObject.FindGameObjectWithTag("Queue");
        GameObject R01 = GameObject.FindGameObjectWithTag("Rack01");
        GameObject R02 = GameObject.FindGameObjectWithTag("Rack02");

        //Spawn Loading Area
        if (Q != null)
        {
            PhotonView.Find(id01).tag = "LoadArea";
            PhotonView.Find(id01).transform.parent = Q.transform;
            PhotonView.Find(id01).transform.localPosition = new Vector3(-0.6f, 0f, -0.6f);
            PhotonView.Find(id01).transform.localRotation = Quaternion.Euler(0, -90, 90);

        }

        //Spawn Unloading Area for Rack01
        if (R01 != null)
        {
            PhotonView.Find(id02).tag = "UnloadArea01";
            PhotonView.Find(id02).transform.parent = R01.transform;
            PhotonView.Find(id02).transform.localPosition = new Vector3(0, 0.8f, 0.07f);
            PhotonView.Find(id02).transform.localRotation = Quaternion.Euler(-90, 0, 0);

        }

        //Spawn Unloading Area for Rack02
        if (R02 != null)
        {
            PhotonView.Find(id03).tag = "UnloadArea02";
            PhotonView.Find(id03).transform.parent = R02.transform;
            PhotonView.Find(id03).transform.localPosition = new Vector3(0, 0.8f, 0.07f);
            PhotonView.Find(id03).transform.localRotation = Quaternion.Euler(-90, 0, 0);

        }
    }

    //For the operator system to call the manager system to move product from queue to the cart when the operator selects a product
    [PunRPC] 
    void IfNotMasterQtoC(int ID)
    {
        GameObject P = PhotonView.Find(ID).gameObject;
        QueueToCart(P);
    }

    //For the operator system to call the manager system to move product from the cart to the racks when the operator selects a product
    [PunRPC] 
    void IfNotMasterCtoR(int ID, string Name)
    {
        SelectBegin = false;
        GameObject P = PhotonView.Find(ID).gameObject;
        CartToRack(P, Name);
    }

    //Function to change text on current suggestion
    [PunRPC] 
    void ChangeCurrentTrainingPQ(bool performance, bool quantity, bool time, bool route)
    {
        if (ChooseMenu.Manager)
        {
            return;
        }
        // performance -- cycle time
        switch (performance)
        {
            case true:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(0).GetComponent<TextMeshPro>().text = "Good";
                break;
            case false:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(0).GetComponent<TextMeshPro>().text = "Bad";
                break;
        }

        // quantity product which carrying in cart
        switch (quantity)
        {
            case true:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(1).GetComponent<TextMeshPro>().text = "More than 15";
                break;
            case false:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(1).GetComponent<TextMeshPro>().text = "Less than 15";
                break;
        }

        // time to do process
        switch (time)
        {
            case true:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(2).GetComponent<TextMeshPro>().text = "Continue";
                break;
            case false:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(2).GetComponent<TextMeshPro>().text = "Wait";
                break;
        }

        // route to transport the product
        switch (route)
        {
            case true:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(3).GetComponent<TextMeshPro>().text = "Clockwise";
                break;
            case false:
                OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection").GetChild(3).GetComponent<TextMeshPro>().text = "Counterclockwise";
                break;
        }
    }

    //Function to change text on previous suggestion
    [PunRPC] 
    void ChangePreviousOPTrainingText()
    {
        if (ChooseMenu.Manager)
        {
            return;
        }
        dynamic pre_suggestion_collection = OperatorTrainingPanel.transform.Find("Previous").Find("Suggestion Collection");
        dynamic now_suggestion_collection = OperatorTrainingPanel.transform.Find("Current").Find("Suggestion Collection");
        pre_suggestion_collection.GetChild(0).GetComponent<TextMeshPro>().text = now_suggestion_collection.GetChild(0).GetComponent<TextMeshPro>().text;
        pre_suggestion_collection.GetChild(1).GetComponent<TextMeshPro>().text = now_suggestion_collection.GetChild(1).GetComponent<TextMeshPro>().text;
        pre_suggestion_collection.GetChild(2).GetComponent<TextMeshPro>().text = now_suggestion_collection.GetChild(2).GetComponent<TextMeshPro>().text;
        pre_suggestion_collection.GetChild(3).GetComponent<TextMeshPro>().text = now_suggestion_collection.GetChild(3).GetComponent<TextMeshPro>().text;
        now_suggestion_collection.GetChild(0).GetComponent<TextMeshPro>().text = "No Data";
        now_suggestion_collection.GetChild(1).GetComponent<TextMeshPro>().text = "No Suggestion";
        now_suggestion_collection.GetChild(2).GetComponent<TextMeshPro>().text = "No Suggestion";
        now_suggestion_collection.GetChild(3).GetComponent<TextMeshPro>().text = "No Suggestion";
    }

    //Initializing all variables upon starting simulation
    void OnEnable()
    {
        if (ChooseMenu.Operator) // || ChooseMenu.Observer
        {
            cartHitbox.MoveP1Box(0);
            cartHitbox.MoveP2Box(0);
            return;
        }
        //Spawn the loading/unloading areas when simulation starts
        LA = PhotonNetwork.Instantiate("CartArea", new Vector3(0, 0, 0), Quaternion.identity);
        int ID01 = LA.GetComponent<PhotonView>().ViewID;
        UA01 = PhotonNetwork.Instantiate("CartArea", new Vector3(0, 0, 0), Quaternion.identity);
        int ID02 = UA01.GetComponent<PhotonView>().ViewID;
        UA02 = PhotonNetwork.Instantiate("CartArea", new Vector3(0, 0, 0), Quaternion.identity);
        int ID03 = UA02.GetComponent<PhotonView>().ViewID;
        PhotonView photonView = PhotonView.Get(this);

        //Call the RPC to set areas' position for everyone
        photonView.RPC("SetAreaPos", RpcTarget.All, ID01, ID02, ID03);
        photonView.RPC("ShowP12BoxesRPC", RpcTarget.All);
        photonView.RPC("OperatorUpdate", RpcTarget.All, 0, 0, 0, 0, 0);
        Debug.Log("Start Simulation");

        // set initial value of parameters
        MinuteTimer = 0f;
        timer = 40f; //run speed
        PassedTime = 0f;
        OverflowTime = 0f;
        BlockingTime = 0f;
        TotalP1 = 0;
        TotalP2 = 0;
        CP1 = 0;
        CP2 = 0;
        CarryCounts = 0;
        ErrorCounts = 0;
        TempFullTime = 0;
        CYTotalProducts = 0;
        HumanErrorCounts = 0;
        Block = 0;
        R1_1 = 0;
        R1_2 = 0;
        R1_3 = 0;
        R1_4 = 0;
        R2_1 = 0;
        R2_2 = 0;
        R2_3 = 0;
        R2_4 = 0;
        MissedP1 = 0;
        MissedP2 = 0;
        MissedProduct = 0;

        // set logic of previous parameters
        OldCarryCount = CarryCounts;
        OldCyTime = Assessment.cyTime;
        OldErrorCount = ErrorCounts;
        OldTimePassed = PassedTime;
        OldHumanErrorCounts = HumanErrorCounts;
        OldOverflowTime = OverflowTime;
        OldBlockingTime = BlockingTime;
        OldMissedP1 = MissedP1;
        OldMissedP2 = MissedP2;
        OldMissedProduct = OldMissedP1 + OldMissedP2;

        //Change previous suggestions on training panel, executed in the OnEnable becuase this should happen everytime restarting a simulation
        photonView.RPC("ChangePreviousOPTrainingText", RpcTarget.All);

        //Change old assessment text
        this.GetComponent<Assessment>().ChangeOldAssessmentText();
        cartHitbox.MoveP1Box(CP1);
        cartHitbox.MoveP2Box(CP2);
    }

    //Calls when selection of an object ends
    //SelectBegin is just for checking the end of a selection
    public void End()
    {
        SelectBegin = false;
    }

    //Moving products from queue to the cart
    public bool QueueToCart(GameObject TheProduct)
    {
        //If the product's parent is not the conveyor which means it's on the cart so don't do anything in this function
        if (!TheProduct.transform.parent.CompareTag("Conveyor"))
        {
            return false;
        }
        //If it's the operator then check for position and call the manager system to move the product
        if (ChooseMenu.Operator)
        {
            PhotonView photonView = PhotonView.Get(GameObject.Find("ProductStateManager"));
            //Check position
            if (PlayerPosition.InLoad == false)
            {
                Error.SpawnDialog("ERROR", "Cart not in loading area!", true);
                HumanErrorCounts += 1;
                photonView.RPC("HumanErrorUpdate", RpcTarget.All, HumanErrorCounts);
                return false;
            }
            int id = TheProduct.GetComponent<PhotonView>().ViewID;
            //Calls the manager system to do the work
            photonView.RPC("IfNotMasterQtoC", RpcTarget.Others, id);
            //SelectBegin bool set to true so it won't immediately thinks its selected again and enter the CarttoRack functions
            SelectBegin = true;
            return false;
        }

        //If there's only one manager then check for the manager's position 
        if (ChooseMenu.LonelyManager)
        {
            if (PlayerPosition.InLoad == false)
            {
                Error.SpawnDialog("ERROR", "Cart not in loading area!", true);
                HumanErrorCounts += 1;
                return false;
            }
        }

        //If cart is full then product can not be moved to the cart
        if ((CP1 + 2 * CP2) >= Batch)
        {
            PhotonView photonViewE = PhotonView.Get(Error);
            photonViewE.RPC("SpawnDialogforTarget", RpcTarget.Others, "ERROR", "CART IS FULL!", false);
            if (ChooseMenu.LonelyManager)
            {
                Error.SpawnDialog("ERROR", "CART IS FULL!", false);
            }
            return false;
        }

        //If nothing went wrong above then starts moving the product
        else
        {
            //If nothing is in the InQueue queue (data structure) then it means the operator picked too fast the product didn't fully stopped yet so it will also return
            if (InQueue.Count == 0)
            {
                return false;
            }

            //If FIFO is on and the operator didn't picked the first product returns error
            else if (FIFO && TheProduct != InQueue.First())
            {
                Debug.Log("Not FIFO");
                PhotonView photonViewE = PhotonView.Get(Error);
                if (ChooseMenu.LonelyManager)
                {
                    Error.SpawnDialog("ERROR", "First In First Out!", false);
                }
                else
                {
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.Others, "ERROR", "First In First Out!", false);
                }
                return false;
            }

            //If the product selected is the first product or FIFO is off then move it to the cart
            else
            {
                Destroy(TheProduct.GetComponent<StopMove>());
                PhotonView photonView = PhotonView.Get(GameObject.Find("ProductStateManager"));
                int id = TheProduct.GetComponent<PhotonView>().ViewID;
                photonView.RPC("AddRigid", RpcTarget.All, id);
                string tag = TheProduct.tag;
                int productNum = 0;
                //This for loop is to get which product in queue is being picked
                for (int i = InQueue.Count - 1; i >= 0; i--)
                {
                    if (TheProduct == InQueue.ElementAt(i))
                    {
                        productNum = i;
                        break;
                    }
                }

                //Move the following products forward according to the position of the product being picked
                if (!TheProduct.GetComponent<ProductMovement>().Elevated)
                {
                    for (int i = InQueue.Count - 1; i > productNum + 1; i--)
                    {
                        InQueue.ElementAt(i).transform.position = InQueue.ElementAt(i - 1).transform.position;
                        InQueue.ElementAt(i).transform.rotation = InQueue.ElementAt(i - 1).transform.rotation;
                    }
                    if (InQueue.Count > productNum + 1)
                    {
                        InQueue.ElementAt(productNum + 1).transform.localPosition = ProductMovement.OldPos;
                        InQueue.ElementAt(productNum + 1).transform.localRotation = ProductMovement.OldRot;

                    }

                }
                //Put the product to the right position on rack according to the number of the products on the cart
                if (tag == "Product01")
                {
                    CP1++;
                    P1x = CP1 % 2 == 0 ? 0.15f : 0.05f;
                    if (CP1 <= 8) // 1st layer = 1-8
                    {
                        P1z = 0.25f - (CP1 - 1) / 2 * 0.1f;
                        P1y = 0.1f;
                    }
                    else if (CP1 <= 16) // 2nd layer = 9 - 16
                    {
                        P1z = 0.25f - (CP1 - 8 - 1) / 2 * 0.1f;
                        P1y = 0.2f;
                    }
                    else if (CP1 <= 24) // 3rd layer = 17 - 24
                    {
                        P1z = 0.25f - (CP1 - 16 - 1) / 2 * 0.1f;
                        P1y = 0.3f;
                    }
                    else if (CP1 <= 32) // 4th layer = 25 - 32
                    {
                        P1z = 0.25f - (CP1 - 24 - 1) / 2 * 0.1f;
                        P1y = 0.4f;
                    }
                    else if (CP1 <= 40) // 5th layer = 33 - 40
                    {
                        P1z = 0.25f - (CP1 - 32 - 1) / 2 * 0.1f;
                        P1y = 0.5f;
                    }
                    
                    Vector3 A = cartHitbox.MoveP1Box(CP1);
                    photonView.RPC("MoveP1BoxRPC", RpcTarget.All, A);
                    InCart.AddLast(TheProduct);
                    photonView.RPC("QtoC", RpcTarget.All, id, P1x, P1y, P1z);
                }
                else
                {
                    CP2++;
                    if (CP2 <= 4) // 1st layer = 1-4
                    {
                        P2z = 0.25f - (CP2 - 1) * 0.1f;
                        P2y = 0.1f;

                    }
                    else if (CP2 <= 8) // 2nd layer = 5-8
                    {
                        P2z = 0.25f - (CP2 - 4 - 1) * 0.1f;
                        P2y = 0.2f;
                    }
                    else if (CP2 <= 12) // 3rd layer = 9-12
                    {
                        P2z = 0.25f - (CP2 - 8 - 1) * 0.1f;
                        P2y = 0.3f;
                    }
                    else if (CP2 <= 16) // 4th layer = 13-16
                    {
                        P2z = 0.25f - (CP2 - 12 - 1) * 0.1f;
                        P2y = 0.4f;
                    }
                    else if (CP2 <= 20) // 5th layer = 16-20
                    {
                        P2z = 0.25f - (CP2 - 16 - 1) * 0.1f;
                        P2y = 0.5f;
                    }

                    Vector3 B = cartHitbox.MoveP2Box(CP2);
                    photonView.RPC("MoveP2BoxRPC", RpcTarget.All, B);
                    InCart.AddLast(TheProduct);
                    photonView.RPC("QtoC", RpcTarget.All, id, -0.1f, P2y, P2z);
                }

                InQueue.Remove(TheProduct);
                //TheProduct.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                //If cart is full then tell the operator with an error dialog
                if ((CP1 + 2 * CP2) >= Batch)
                {
                    photonView.RPC("HideP12BoxesRPC", RpcTarget.All);
                    PhotonView photonViewE = PhotonView.Find(3);
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "ERROR", "Cart is full!", false);
                }
            }

            //Indicates that the selection has not ended yet
            SelectBegin = true;
            return true;
        }
    }

    //Move products from cart to racks
    public bool CartToRack(GameObject TheProduct, string RackDetectionName)
    {
        PhotonView photonView = PhotonView.Get(GameObject.Find("ProductStateManager"));
        //If there's only one manager than check for his position
        if (ChooseMenu.LonelyManager)
        {
            if (TheProduct.transform.parent.tag == "Cart" && PlayerPosition.InUnload01 == false && PlayerPosition.InUnload02 == false)
            {
                Error.SpawnDialog("ERROR", "Cart not in unloading area!", true);
                HumanErrorCounts += 1;
                photonView.RPC("HumanErrorUpdate", RpcTarget.All, HumanErrorCounts);
                return false;
            }
            else if (TheProduct.CompareTag("Product01") && !PlayerPosition.InUnload01 && PlayerPosition.InUnload02)
            {
                return false;
            }
            else if (TheProduct.CompareTag("Product02") && !PlayerPosition.InUnload02 && PlayerPosition.InUnload01)
            {
                return false;
            }
        }

        //If it is the operator than check for position and inform the manager's system to move the product
        if (ChooseMenu.Operator)
        {
            if (TheProduct.transform.parent.tag == "Cart" && PlayerPosition.InUnload01 == false && PlayerPosition.InUnload02 == false)
            {
                Error.SpawnDialog("ERROR", "Cart not in unloading area!", true);
                HumanErrorCounts += 1;
                photonView.RPC("HumanErrorUpdate", RpcTarget.All, HumanErrorCounts);
                return false;
            }
            else if (TheProduct.CompareTag("Product01") && !PlayerPosition.InUnload01 && PlayerPosition.InUnload02)
            {
                return false;
            }
            else if (TheProduct.CompareTag("Product02") && !PlayerPosition.InUnload02 && PlayerPosition.InUnload01)
            {
                return false;
            }
            int id = TheProduct.GetComponent<PhotonView>().ViewID;
            photonView.RPC("IfNotMasterCtoR", RpcTarget.Others, id, RackDetectionName);
            return false;
        }

        //If nothing went wrong above then starts moving the product
        else
        {
            //Use a for loop to check if the selected product is on the cart
            for (int i = 0; i < InCart.Count; i++)
            {
                //If it is on the cart then enters
                if (TheProduct == InCart.ElementAt(i))
                {
                    TheProduct.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    TheProduct.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                    Destroy(TheProduct.GetComponent<Rigidbody>());
                    //Put the product on the right position of the racks according to the numbers already on the racks
                    if (TheProduct.CompareTag("Product01"))
                    {
                        CP1--;
                        TotalP1++;
                        GameObject Rack01 = GameObject.FindGameObjectWithTag("Rack01");

                        float R1x = 0.45f;
                        float R1y = -0.15f;
                        float R1z = 0.65f;
                        if (RackDetectionName == "Cube4")
                        {
                            R1_4++;
                            R1x = 0.45f - ((R1_4 - 1) / 5 * 0.1f);
                            R1y = -0.15f + ((R1_4 - 1) % 5 * 0.1f);
                            R1z = 0.65f;
                        }
                        else if (RackDetectionName == "Cube3")
                        {
                            R1_3++;
                            R1x = 0.45f - ((R1_3 - 1) / 4 * 0.1f);
                            R1y = -0.15f + ((R1_3 - 1) % 4 * 0.1f);
                            R1z = 0.21f;
                        }
                        else if (RackDetectionName == "Cube2")
                        {
                            R1_2++;
                            R1x = 0.45f - ((R1_2 - 1) / 3 * 0.1f);
                            R1y = -0.15f + ((R1_2 - 1) % 3 * 0.1f);
                            R1z = -0.09f;
                        }
                        else if (RackDetectionName == "Cube1")
                        {
                            R1_1++;
                            R1x = 0.45f - ((R1_1 - 1) / 2 * 0.1f);
                            R1y = -0.15f + ((R1_1 - 1) % 2 * 0.1f);
                            R1z = -0.300f;
                        }
                        InRack01.Push(InCart.ElementAt(i));
                        int id = TheProduct.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("CtoR1", RpcTarget.All, id, R1x, R1y, R1z);
                        InCart.Remove(TheProduct);
                        //If the selected product is the last product on the cart then it counts as a full transfer so CarryCounts plus one
                        if (InCart.Count == 0)
                        {
                            Debug.Log("Last one");
                            Debug.Log("CHECK");
                            //RPC called to change the current suggestion text everytime the last product leaves the cart
                            bool tempP = OperatorTraining.Performance();
                            bool tempQ = OperatorTraining.Quantity();
                            bool tempT = OperatorTraining.Time();
                            bool tempR = OperatorTraining.Route();
                            photonView.RPC("ChangeCurrentTrainingPQ", RpcTarget.All, tempP, tempQ, tempT, tempR);
                            Vector3 A = cartHitbox.MoveP1Box(0);
                            Vector3 B = cartHitbox.MoveP2Box(0);
                            photonView.RPC("ShowP12BoxesRPC", RpcTarget.All);
                            photonView.RPC("MoveP1BoxRPC", RpcTarget.Others, A);
                            photonView.RPC("MoveP2BoxRPC", RpcTarget.Others, B);
                            CYTotalProducts = TotalP1 + TotalP2;
                            TempFullTime = PassedTime;
                            CarryCounts++;
                        }
                    }
                    else if (TheProduct.CompareTag("Product02"))
                    {
                        CP2--;
                        TotalP2++;
                        GameObject Rack02 = GameObject.FindGameObjectWithTag("Rack02");
                        float R2x = 0.4f;
                        float R2y = -0.15f;
                        float R2z = 0.65f;
                        if (RackDetectionName == "Cube4")
                        {
                            R2_4++;
                            R2x = 0.4f - ((R2_4 - 1) / 5 * 0.2f);
                            R2y = -0.15f + ((R2_4 - 1) % 5 * 0.1f);
                            R2z = 0.65f;
                        }
                        else if (RackDetectionName == "Cube3")
                        {
                            R2_3++;
                            R2x = 0.4f - ((R2_3 - 1) / 4 * 0.2f);
                            R2y = -0.15f + ((R2_3 - 1) % 4 * 0.1f);
                            R2z = 0.21f;
                        }
                        else if (RackDetectionName == "Cube2")
                        {
                            R2_2++;
                            R2x = 0.4f - ((R2_2 - 1) / 3 * 0.2f);
                            R2y = -0.15f + ((R2_2 - 1) % 3 * 0.1f);
                            R2z = -0.09f;
                        }
                        else if (RackDetectionName == "Cube1")
                        {
                            R2_1++;
                            R2x = 0.4f - ((R2_1 - 1) / 2 * 0.2f);
                            R2y = -0.15f + ((R2_1 - 1) % 2 * 0.1f);
                            R2z = -0.300f;
                        }
                        InRack02.Push(InCart.ElementAt(i));
                        int id = TheProduct.GetComponent<PhotonView>().ViewID;
                        photonView.RPC("CtoR2", RpcTarget.All, id, R2x, R2y, R2z);
                        InCart.Remove(TheProduct);
                        //If the selected product is the last product on the cart then it counts as a full transfer so CarryCounts plus one
                        if (InCart.Count == 0)
                        {
                            Debug.Log("Last one");
                            //RPC called to change the current suggestion text everytime the last product leaves the cart
                            bool tempP = OperatorTraining.Performance();
                            bool tempQ = OperatorTraining.Quantity();
                            bool tempT = OperatorTraining.Time();
                            bool tempR = OperatorTraining.Route();
                            photonView.RPC("ChangeCurrentTrainingPQ", RpcTarget.All, tempP, tempQ, tempT, tempR);
                            Vector3 A = cartHitbox.MoveP1Box(0);
                            Vector3 B = cartHitbox.MoveP2Box(0);
                            photonView.RPC("ShowP12BoxesRPC", RpcTarget.All);
                            photonView.RPC("MoveP1BoxRPC", RpcTarget.Others, A);
                            photonView.RPC("MoveP2BoxRPC", RpcTarget.Others, B);
                            CYTotalProducts = TotalP1 + TotalP2;
                            TempFullTime = PassedTime;
                            CarryCounts++;
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            return true;
        }
    }

    //Update function will spawn the products
    void Update()
    {
        PhotonView PV = PhotonView.Get(this);
        //If it is the operator then return and do nothing
        if (ChooseMenu.Operator) // || ChooseMenu.Observer)
        {
            return;
        }
        PV.RPC("OperatorUpdate", RpcTarget.All, InConveyor.Count(), InQueue.Count(), InCart.Count(), InRack01.Count(), InRack02.Count());
        Source = GameObject.FindGameObjectWithTag("Source");
       
        //If the time is up end the simulation and give a notification
        if (PassedTime >= FullTimer * 60)
        {
            menu.StartSimulation();
            PhotonView photonViewE = PhotonView.Get(Error);
            photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "Notification", "Simulation Finished!", false);
            return;
        }

        //Spawning products
        if (Source != null)
        {

            PassedTime += Time.deltaTime;
            timer += Time.deltaTime;
            PhotonView photonViewE = PhotonView.Get(Error);
            if (InQueue.Count >= QueueSize + 1)
            {
                OverflowTime += Time.deltaTime;
            }

            //if product more than limit conveyor then block
            if (InQueue.Count + InConveyor.Count > 25)
            {
                BlockingTime += Time.deltaTime;
                Block ++;
                if (Block ==1)
                {
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "Error", "Blocking is EXISTED in Input Station!", false);
                    return;
                }
                timer = 0f;
            }


            //Everytime the timer becomes larger then the selected spawn time spawns a product
            if (timer >= SpawnTime)
            {
                Rigidbody rb;
                //Random number generator
                float random = Random.Range(0f, 1f);
                //Use the generated random number to decide if the spawned product is product01 or product02
                if (random <= SpawningProb01)
                {
                    //Spawns product01 on the source
                    InConveyor.AddLast(PhotonNetwork.Instantiate("Product01", Source.transform.position + new Vector3(0, 0.475f, 0), Source.transform.rotation));
                    int id = InConveyor.Last().GetComponent<PhotonView>().ViewID;
                    PhotonView photonView = PhotonView.Get(GameObject.Find("ProductStateManager"));
                    photonView.RPC("StoQ", RpcTarget.All, id);
                }
                else
                {
                    //Spawns product02 on the source
                    InConveyor.AddLast(PhotonNetwork.Instantiate("Product02", Source.transform.position + new Vector3(0, 0.475f, 0), Source.transform.rotation));
                    int id = InConveyor.Last().GetComponent<PhotonView>().ViewID;
                    PhotonView photonView = PhotonView.Get(GameObject.Find("ProductStateManager"));
                    photonView.RPC("StoQ", RpcTarget.All, id);
                }
                //Set the speed of the spawned product
                rb = InConveyor.ElementAt(InConveyor.Count - 1).GetComponent<Rigidbody>();
                rb.velocity = rb.transform.up * force;
                //Set the timer back to 0 for next spawn
                timer = 0f;
            }
        }
        else
        {
            Debug.Log("Source is NULL");
            return;
        }
        //Selling products for consumer needs
        MinuteTimer += Time.deltaTime;
        if (MinuteTimer >= 60)
        {
            //Get the amount of demand set by the manager
            P1SoldperMin = int.Parse(Numpad.P1Sold.ToString());
            P2SoldperMin = int.Parse(Numpad.P2Sold.ToString());
            //Remove product from Rack01
            for (int j = P1SoldperMin; j > 0; j--)
            {
                //If not enough products on the rack
                if (InRack01.Count == 0)
                {
                    MissedP1 += j;
                    PhotonView photonViewE = PhotonView.Get(Error);
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "Error", "Not enough Product for customer demand!", false);
                    break;
                }
                //remove product last in first out LIFO
                //pop(): Removes the latest entered element from stack and decrement top by 1.
                //peek(): Returns the value of latest entered element.
                PhotonNetwork.Destroy(InRack01.Peek());
                InRack01.Pop();
            }

            //Remove product from Rack02
            for (int j = P2SoldperMin; j > 0; j--)
            {
                //If not enough products on the rack
                if (InRack02.Count == 0)
                {
                    MissedP2 += j;
                    PhotonView photonViewE = PhotonView.Get(Error);
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "Error", "Not enough Product for customer demand!", false);
                    break;
                }
                PhotonNetwork.Destroy(InRack02.Peek());
                InRack02.Pop();
            }

            MinuteTimer = 0;
            Debug.Log("timer demand zero");
            MissedProduct = MissedP1+MissedP2;
        }
        int i = 0;
        //Dealing with the physics of unity again
        while (InConveyor.Count > i)
        {
            //Use a while loop to make sure every product on conveyor stays in mid of the conveyor prevent from slipping off
            GameObject Moving = InConveyor.ElementAt(i);
            int ID = Moving.GetComponent<PhotonView>().ViewID;
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("StayMid", RpcTarget.All, ID);
            i++;
        }
    }
}
