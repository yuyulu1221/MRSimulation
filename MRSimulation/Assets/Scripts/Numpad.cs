using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class Numpad : MonoBehaviour
{
    static public StringBuilder EnteredNum = new StringBuilder("0sec");
    static public StringBuilder P1Sold = new StringBuilder("0");
    static public StringBuilder P2Sold = new StringBuilder("0");
    public GameObject FactorPanel;

    //To close the numpad
    public void ClosePad()
    {
        this.gameObject.SetActive(false);
    }

    //The script for all number buttons
    public void NumButtonEnter(string n)
    {
        switch (Factors.NumpadSwitch)
        {
            case 0:
                if (n == "-1")
                {

                    //When there is only one digit remove the digit and change it to 0
                    if (EnteredNum.Length == 4)
                    {
                        EnteredNum.Remove(EnteredNum.Length - 4, 1);
                        EnteredNum.Insert(EnteredNum.Length - 3, "0");
                    }
                    else
                    {
                        EnteredNum.Remove(EnteredNum.Length - 4, 1);
                    }
                }

                //For all the other buttons
                else
                {
                    //When the only digit is 0 then replace it with the entered number
                    if (EnteredNum.ToString() == "0sec" && n != ".")
                    {
                        EnteredNum.Remove(EnteredNum.Length - 4, 1);
                        EnteredNum.Insert(EnteredNum.Length - 3, n);
                    }
                    else if (n != ".")
                    {
                        EnteredNum.Insert(EnteredNum.Length - 3, n);
                    }
                    else if (!EnteredNum.ToString().Contains("."))
                    {
                        EnteredNum.Insert(EnteredNum.Length - 3, n);
                    }
                }
                FactorPanel.transform.Find("Suggestion").Find("EnterNumberButton").Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = EnteredNum.ToString();
                break;

            case 1:
                //This is for the delete button
                if (n == "-1")
                {
                    //When there is only one digit remove the digit and change it to 0
                    if (P1Sold.Length == 1)
                    {
                        P1Sold.Remove(0, 1);
                        P1Sold.Insert(0, "0");
                    }
                    else
                    {
                        P1Sold.Remove(P1Sold.Length - 1, 1);
                    }
                }

                //For all the other buttons
                else
                {
                    //When the only digit is 0 then replace it with the entered number
                    if (n == ".")
                    {
                        return;
                    }
                    if (P1Sold.ToString() == "0")
                    {
                        P1Sold.Remove(0, 1);
                        P1Sold.Insert(0, n);
                    }
                    else
                    {
                        P1Sold.Insert(P1Sold.Length, n);
                    }
                }
                FactorPanel.transform.Find("Product Sold").Find("P1EnterNumberButton").Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = P1Sold.ToString();
                break;

            case 2:
                //This is for the delete button
                if (n == "-1")
                {
                    //When there is only one digit remove the digit and change it to 0
                    if (P2Sold.Length == 1)
                    {
                        P2Sold.Remove(0, 1);
                        P2Sold.Insert(0, "0");
                    }
                    else
                    {
                        P2Sold.Remove(P2Sold.Length, 1);
                    }
                }

                //For all the other buttons
                else
                {
                    //When the only digit is 0 then replace it with the entered number
                    if (n == ".")
                    {
                        return;
                    }
                    if (P2Sold.ToString() == "0")
                    {
                        P2Sold.Remove(0, 1);
                        P2Sold.Insert(0, n);
                    }
                    else
                    {
                        P2Sold.Insert(P2Sold.Length - 1, n);
                    }
                }
                FactorPanel.transform.Find("Product Sold").Find("P2EnterNumberButton").Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().text = P2Sold.ToString();
                break;
        }
    }
}
