using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Factors : MonoBehaviour
{
    public GameObject AmountCollection;
    public GameObject RackPositionGroup;
    public GameObject Numpad;
    static public int NumpadSwitch;
    static public int RackPosIndex;
    //public Material Round;
    //public Material Square;
    public Material Pressed;
    public Material Unpressed;
    public bool FIFO;
    float Sec;
    float Speed;
    float Ratio;
    float Qsize;
    float AllBatch;
    float Time;

    //Set SpawnTime -- Inter-arrival Rate (IAR)
    public void SpawnRate(GameObject ThumbRoot)
    {
        // This is to deal with the problem of the thumbroot position don't always range from -0.105 to 0.105
        // pinch slider - range distance
        //Sometimes even though it's moved to the very left it doesn't reach -0.125 or exceed, same for the very right
        
        //Here I force the number to 4 when the position is less than -0.105
        if (ThumbRoot.transform.localPosition.x < -0.105f)
        {
            Sec = 1;
        }

        //Here I force the number to 8 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            Sec = 30;
        }
        //This part is for the rest of the number in the middle
        else
        {

            Sec = (ThumbRoot.transform.localPosition.x + 0.105f) * 133.34f +1;
            Sec = Mathf.Round(Sec);
        }

        // to change the text (string) of IAR
        AmountCollection.transform.GetChild(0).GetComponent<TextMeshPro>().text = Sec.ToString();
        ProductState.SpawnTime = Sec;
    }

    //Set initial speed of the products -- conveyor speed (CS)
    public void InitialSpeed(GameObject ThumbRoot)
    {
        //Here I force the number to 4 when the position is less than -0.105
        if (ThumbRoot.transform.localPosition.x < -0.105f)
        {
            Speed = 0.1f;
        }

        //Here I force the number to 8 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            Speed = 1;
        }
        //This part is for the rest of the number in the middle
        else
        {
            Speed = (ThumbRoot.transform.localPosition.x + 0.105f) * 3.57f + 0.13f;
            Speed = Mathf.Round(Speed * 10) / 10;
        }

        AmountCollection.transform.GetChild(1).GetComponent<TextMeshPro>().text = (Speed*10).ToString();
        ProductState.force = Speed;
    }

    //Set spawning ratio -- product ratio (PR)
    public void SpawningRatio(GameObject ThumbRoot)
    {
        //Here I force the number to 4 when the position is less than -0.105
        if (ThumbRoot.transform.localPosition.x < -0.105f)
        {
            Ratio = 0;
        }

        //Here I force the number to 8 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            Ratio = 1;
        }
        //This part is for the rest of the number in the middle
        else
        {
            Ratio = (ThumbRoot.transform.localPosition.x + 0.105f) * 4.69f + 0.01f;
            Ratio = Mathf.Round(Ratio * 100) / 100;
        }
        AmountCollection.transform.GetChild(2).GetComponent<TextMeshPro>().text = (Ratio).ToString();
        ProductState.SpawningProb01 = Ratio;
    }

    //Set queue size (QS)
    public void QueueSize(GameObject ThumbRoot)
    {
        //Here I force the number to 4 when the position is less than -0.105
        if (ThumbRoot.transform.localPosition.x < -0.105f)
        {
            Qsize = 1;
        }

        //Here I force the number to 8 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            Qsize = 10;
        }
        //This part is for the rest of the number in the middle
        else
        {
            Qsize = (ThumbRoot.transform.localPosition.x + 0.105f) * 36.2f+ 1.3f;
            Qsize = Mathf.Round(Qsize);
        }
        
        AmountCollection.transform.GetChild(3).GetComponent<TextMeshPro>().text = (Qsize).ToString();
        ProductState.QueueSize = (int)Qsize;
    }

    //Set the product batch -- cart batch size (CBS)
    public void ProductBatch(GameObject ThumbRoot)
    {
        //Here I force the number to 5 when the position is less than -0.105
        if (ThumbRoot.transform.localPosition.x < -0.105f)
        {
            AllBatch = 5;
        }

        //Here I force the number to 30 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            AllBatch = 60;
        }
        //This part is for the rest of the number in the middle
        else
        {
            AllBatch = (ThumbRoot.transform.localPosition.x + 0.105f) * 238.1f + 5;
            AllBatch = Mathf.Round(AllBatch);

        }
        AmountCollection.transform.GetChild(4).GetComponent<TextMeshPro>().text = (AllBatch).ToString();
        ProductState.Batch = (int)AllBatch;
    }

    //Set the simulation time (ST)
    public void SimTime(GameObject ThumbRoot)
    {
        //Here I force the number to 1 when the position is less than -0.105
        if(ThumbRoot.transform.localPosition.x < -0.105f)
        {
            Time = 1;
        }

        //Here I force the number to 20 when the position is more than 0.105
        else if (ThumbRoot.transform.localPosition.x > 0.105f)
        {
            Time = 30;
        }
        //This part is for the rest of the number in the middle
        else
        {
            Time = (ThumbRoot.transform.localPosition.x + 0.105f) * 133.34f + 1;
            Time = Mathf.Round(Time);
        }
        AmountCollection.transform.GetChild(5).GetComponent<TextMeshPro>().text = (Time).ToString();
        ProductState.FullTimer = Time;
    }

    //To switch FIFO option
    public void FIFOswitch()
    {
        FIFO = !FIFO;
        ProductState.FIFO = FIFO;
        Debug.Log("FIFO switch");
    }

    //input value for demand and threshold
    public void OpenNumpad(int n)
    {
        if (!Numpad.activeSelf)
        {
            NumpadSwitch = n;
            Numpad.SetActive(true);
        }
        else
        {
            NumpadSwitch = -1;
            Numpad.SetActive(false);
        }
    }

    //Rack position index
    public void RackPosition(int n)
    {
        switch (n)
        {
            case 1:
                RackPosIndex = 1;
                RackPositionGroup.transform.Find("RackButton01").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Pressed;
                RackPositionGroup.transform.Find("RackButton02").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
                break;
            case 2:
                RackPosIndex = 2;
                RackPositionGroup.transform.Find("RackButton01").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
                RackPositionGroup.transform.Find("RackButton02").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Pressed;
                break;
            default:
                RackPosIndex = 0;
                RackPositionGroup.transform.Find("RackButton01").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
                RackPositionGroup.transform.Find("RackButton02").Find("BackPlate").Find("Quad").GetComponentInChildren<MeshRenderer>().material = Unpressed;
                break;
        }
    }
}


