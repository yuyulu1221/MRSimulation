using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//This script check for the position of the operator or only one manager
public class PlayerPosition : MonoBehaviour
{
    public GameObject CartPinButton;
    public GameObject ProductStateManager;
    public static GameObject load_area;
    public static GameObject unload_area;
    public static bool InUnload01;
    public static bool InUnload02;
    public static bool InLoad;

    //When player enter to the area pop up the cart pin button loading/unloading
    private void OnTriggerEnter(Collider other)
    {
        string Tag = other.gameObject.tag;
        switch (Tag)
        {
            case "LoadArea":
                load_area = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                load_area.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Loading";
                InLoad = true;
                break;
            case "UnloadArea01":
                if(!InUnload02)
                {
                    unload_area = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                    unload_area.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Unloading";
                }
                InUnload01 = true;
                break;
            case "UnloadArea02":
                if (!InUnload01)
                {
                    unload_area = Instantiate(CartPinButton, new Vector3(0, 0, 0), Quaternion.identity);
                    unload_area.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = "Start Unloading";
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
                if (!load_area)
                {
                    return;
                }
                Destroy(load_area);
                InLoad = false;
                break;
            case "UnloadArea01":
                if (!unload_area)
                {
                    return;
                }
                if (!InUnload02)
                {
                    Destroy(unload_area);
                }
                InUnload01 = false;
                break;
            case "UnloadArea02":
                if (!unload_area)
                {
                    return;
                }
                if (!InUnload01)
                {
                    Destroy(unload_area);
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
            if(load_area)
            {
                Destroy(load_area);
            }
            if(unload_area)
            {
                Destroy(unload_area);
            }
        }

        //this.transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position + new Vector3(0, -0.5f, 0);
        this.transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position + new Vector3(0, 0, 0);
    }
}


