using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;
using TMPro;

//The role choosing menu in the first scene
public class ChooseMenu : MonoBehaviour
{
    public static bool Manager;
    public static bool Operator;
    public static bool LonelyManager;
    public GameObject user;
    public GameObject StartMenu;
    public Material Pressed;
    public Material Unpressed;
    public Transform ImageOrigin;
    public Vector3 Origin_pos;
    public Quaternion Origin_rot;
 
    private void Start()
    {
        //Initialiaze AR camera vuforia
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StartObjectTracker);
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopObjectTracker);
        Debug.Log("Initialize AR Camera Vuforia");

    }
    //start track image
    void StopObjectTracker()
    {
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        objectTracker.Stop();
    }

    //stop track image
    void StartObjectTracker()
    {
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        objectTracker.Start();
    }

    //If choose operator
    public void ToOperator()
    {
        // LoadScene (1)= start; (2)= simulation
        SceneManager.LoadScene(1);
        Operator = true;
        Manager = false;
        LonelyManager = false;
        GameObject user = GameObject.Find("u_operator");
        user.GetComponent<MeshRenderer>().enabled = true;
        user.transform.position = ImageOrigin.position;
        user.transform.rotation = ImageOrigin.rotation;
        Debug.Log("Operator Join to Room");
    }

    //If choose manager
    public void ToManager()
    {
        SceneManager.LoadScene(1);
        Operator = false;
        Manager = true;
        LonelyManager = false;
        GameObject user = GameObject.Find("u_manager");
        user.GetComponent<MeshRenderer>().enabled = true;
        user.transform.position = ImageOrigin.position;
        user.transform.rotation = ImageOrigin.rotation;
        Debug.Log("Manager Join to Room");
    }

    //If choose only manager
    public void ToLonelyManager()
    {
        SceneManager.LoadScene(1);
        Operator = false;
        Manager = false;
        LonelyManager = true;
        GameObject user = GameObject.Find("u_setting");
        user.GetComponent<MeshRenderer>().enabled = true;
        user.transform.position = ImageOrigin.position;
        user.transform.rotation = ImageOrigin.rotation;
        Debug.Log("Setting Join to Room");
    }

    //Detect origin position based on image target
    public void StartCalibration()
    {
        StartMenu.transform.GetChild(0).GetChild(1).GetChild(3).GetChild(1).Find("FrontPlate").GetComponent<MeshRenderer>().material = Pressed;
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StartObjectTracker);
        Debug.Log("Start AR Camera");
        StartCoroutine(OriginPoint());
        Debug.Log("wait 5 seconds");
    }

    private IEnumerator OriginPoint()
    {
        yield return new WaitForSeconds(5);
        Origin_pos = ImageOrigin.position;
        Origin_rot = ImageOrigin.rotation;
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopObjectTracker);
        Debug.Log("Stop AR Camera");
        Debug.Log("Origin Position, x: " + Origin_pos.x + ", y: " + Origin_pos.y + ", z: " + Origin_pos.z);
        Debug.Log("Origin Rotation, x: " + Origin_rot.eulerAngles.x + ", y: " + Origin_rot.eulerAngles.y + ", z: " + Origin_rot.eulerAngles.z);
        StartMenu.transform.GetChild(0).GetChild(1).GetChild(3).GetChild(1).Find("FrontPlate").GetComponent<MeshRenderer>().material = Unpressed;
        Debug.Log("Change color");
    }
}
