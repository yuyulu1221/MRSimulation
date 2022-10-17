using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

//This script spawns error dialogs
public class Error : MonoBehaviour
{
    public GameObject ErrorDialog;
    public GameObject YesNoDialog;

    [PunRPC] void SpawnDialogforTarget(string Title, string Message, bool IsOP)
    {
        this.SpawnDialog(Title, Message, IsOP);
    }
    //This function spawns dialog to suggest manager to stop the simulation when the operator pressed the button
    public void InformManager()
    {
        GameObject D = GameObject.FindGameObjectWithTag("ErrorDialog");
        string Title;
        string Message;
        Message = D.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text;
        Title = D.transform.Find("TitleText").GetComponent<TextMeshPro>().text;
        PhotonView photonView = PhotonView.Find(3);
        if (ChooseMenu.LonelyManager)
        {
            photonView.RPC("SpawnDialogforTarget", RpcTarget.All, "Operator Suggestion", "The operator has encountered " + "[" + Message + "] error and suggesting you to stop the simulation.", false);

        }
        else
        {
            photonView.RPC("SpawnDialogforTarget", RpcTarget.Others, "Operator Suggestion", "The operator has encountered " + "[" + Message + "] error and suggesting you to stop the simulation.", false);
        }
    }
    public void SpawnDialog(string Title, string Message, bool IsOP)
    {
        GameObject D;
        //If it's the operator than use the operator dialog format which has another button to inform the manager
        if(IsOP)
        {
            //If the dialog is already spawned just change the text to prevent multiple spawned dialog overlaying each other
            if (GameObject.FindGameObjectWithTag("ErrorDialog"))
            {
                D = GameObject.FindGameObjectWithTag("ErrorDialog");
                D.GetComponent<AudioSource>().Play();
                //this is to set the right buttons for operator dialog or normal dialog
                D.transform.Find("ButtonParent").GetChild(0).gameObject.SetActive(false);
                D.transform.Find("ButtonParent").GetChild(1).gameObject.SetActive(true);
                D.transform.Find("ButtonParent").GetChild(2).gameObject.SetActive(true);
                D.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
                D.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;

            }
            //Else spawn the dialog
            else
            {
                D = Instantiate(ErrorDialog);
                D.GetComponent<AudioSource>().Play();
                D.transform.Find("ButtonParent").GetChild(0).gameObject.SetActive(false);
                D.transform.Find("ButtonParent").GetChild(1).gameObject.SetActive(true);
                D.transform.Find("ButtonParent").GetChild(2).gameObject.SetActive(true);
                D.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
                D.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;

            }
        }
        else
        {
            //If the dialog is already spawned just change the text to prevent multiple spawned dialog overlaying each other
            if (GameObject.FindGameObjectWithTag("ErrorDialog"))
            {
                D = GameObject.FindGameObjectWithTag("ErrorDialog");
                D.GetComponent<AudioSource>().Play();
                D.transform.Find("ButtonParent").GetChild(0).gameObject.SetActive(true);
                D.transform.Find("ButtonParent").GetChild(1).gameObject.SetActive(false);
                D.transform.Find("ButtonParent").GetChild(2).gameObject.SetActive(false);
                D.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
                D.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;

            }
            //Else spawn the dialog
            else
            {
                D = Instantiate(ErrorDialog);
                D.GetComponent<AudioSource>().Play();
                D.transform.Find("ButtonParent").GetChild(0).gameObject.SetActive(true);
                D.transform.Find("ButtonParent").GetChild(1).gameObject.SetActive(false);
                D.transform.Find("ButtonParent").GetChild(2).gameObject.SetActive(false);
                D.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
                D.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;
            }
        }
    }
    
    public void SpawnYesNoDialog(string Title, string Message, string type)
    {
        GameObject Dialog;
        

        //If the dialog is already spawned just change the text to prevent multiple spawned dialog overlaying each other
        if (GameObject.FindGameObjectWithTag("YesNoDialog"))
        {
            Dialog = GameObject.FindGameObjectWithTag("YesNoDialog");
            Dialog.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
            Dialog.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;

        }
        //Else spawn the dialog
        else
        {
            Dialog = Instantiate(YesNoDialog);
            Dialog.GetComponent<AudioSource>().Play();
            Dialog.transform.Find("DescriptionText").GetComponent<TextMeshPro>().text = Message;
            Dialog.transform.Find("TitleText").GetComponent<TextMeshPro>().text = Title;

        }
        switch (type)
        {
            
            case "Q":
                Dialog.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
                Dialog.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                Dialog.transform.GetChild(2).GetChild(4).gameObject.SetActive(false);
                break;
            case "C":
                Dialog.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
                Dialog.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                Dialog.transform.GetChild(2).GetChild(4).gameObject.SetActive(false);
                break;
            case "R":
                Dialog.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
                Dialog.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                Dialog.transform.GetChild(2).GetChild(4).gameObject.SetActive(true);
                break;
            default:
                return;
        }
    }
}
