using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//This script check for the position of the operator or only one manager
public class PlayerPosition : MonoBehaviour
{
    public GameObject CartPinButton;
    public GameObject ProductStateManager;
    static public GameObject B;
    static public GameObject C;
    static public bool InUnload01;
    static public bool InUnload02;
    static public bool InLoad;

    //When player enter to the area pop up the cart pin button loading/unloading
    private void OnTriggerEnter(Collider other)
    {
        string Tag = other.gameObject.tag;
        switch (Tag)
        {
            case "LoadArea":
                B = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                B.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Loading";
                InLoad = true;
                break;
            case "UnloadArea01":
                if(!InUnload02)
                {
                    C = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                    C.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Unloading";
                }
                InUnload01 = true;
                break;
            case "UnloadArea02":
                if (!InUnload01)
                {
                    C = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                    C.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Unloading";
                }
                InUnload02 = true;
                break;
            default:
                break;
        }
    }

    //When player exit from area 
    private void OnTriggerExit(Collider other)
    {
        string Tag = other.gameObject.tag;
        switch (Tag)
        {
            case "LoadArea":
                if (!B)
                {
                    return;
                }
                Destroy(B);
                InLoad = false;
                break;
            case "UnloadArea01":
                if (!C)
                {
                    return;
                }
                if (!InUnload02)
                {
                    Destroy(C);
                }
                InUnload01 = false;
                break;
            case "UnloadArea02":
                if (!C)
                {
                    return;
                }
                if (!InUnload01)
                {
                    Destroy(C);
                }
                InUnload02 = false;
                break;
            default:
                break;
        }
    }

    //Update method keeps this position indicator with the camera
    private void Update()
    {
        if(!ProductStateManager.activeSelf)
        {
            if(B)
            {
                Destroy(B);
            }
            if(C)
            {
                Destroy(C);
            }
        }

        //this.transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position + new Vector3(0, -0.5f, 0);
        this.transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position + new Vector3(0, 0, 0);
    }
}


