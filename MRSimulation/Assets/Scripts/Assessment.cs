using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class Assessment : MonoBehaviour
{

    public GameObject FactorPanel;
    public GameObject FollowAssessmentMenu;
    
    static public float cyTime = 0f;
    static public float ThresholdCT;
    //static public float RoundedOverflowTime; in sec
    //static public float RoundedOldOverflowTime; in sec
    static public float RoundedBlockingTime;
    static public float RoundedOldBlockingTime;
    static public float AVQ = 0;
    public string MinuteOver;
    public string OldMinuteOver;
    public string Minute;
    public string OldMinute;
    
    private void OnEnable()
    {
        //Get the entered threshold cycle time 
        StringBuilder TempThres = Numpad.EnteredNum;
        TempThres.Remove(TempThres.Length - 3, 3);
        ThresholdCT = float.Parse(TempThres.ToString());
        Numpad.EnteredNum.Insert(TempThres.Length, "sec");
    }

    

    private void OnDisable()
    {

        if (ProductState.CYTotalProducts == 0)
        {
            cyTime = 0;
            //blockPercent = 0;
        }
        else
        {
            cyTime = ProductState.TempFullTime / ProductState.CYTotalProducts;
            
        }
        
        //Change text of current assessment
        ChangeCurrentAssessmentText();
        
        //Get the entered threshold cycle time 
        StringBuilder TempThres = Numpad.EnteredNum;
        TempThres.Remove(TempThres.Length - 3, 3);
        ThresholdCT = float.Parse(TempThres.ToString());
        Numpad.EnteredNum.Insert(TempThres.Length, "sec");
        
        //Check if exceed threshold, CT not zero and no blocking
        ChangeSuggestionText(cyTime < ThresholdCT && cyTime != 0 && RoundedBlockingTime == 0);
    }


    void Update()
    {
        if (GameObject.Find("ProductStateManager") != null && FollowAssessmentMenu)
        {
            //Calculating Cycle Time
            if((ProductState.TotalP1 + ProductState.TotalP2) == 0)
            {
                cyTime = 0;
                //blockPercent = 0;

            }
            else
            {
                cyTime = ProductState.PassedTime / (ProductState.TotalP1 + ProductState.TotalP2);
                                                                                                              
            }
            cyTime = Mathf.Round(cyTime* 100.0f) * 0.01f;
            //blockPercent = Mathf.Round(blockPercent* 100.0f) * 0.01f;
            //Change text of current assessment
            ChangeCurrentAssessmentText();

            
        }
    }

    //Change the text of current assessment result
    public void ChangeCurrentAssessmentText()
    {
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(0).GetComponent<TextMeshPro>().text = (cyTime).ToString() + "sec";

        //Queue overflow count
        //FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(1).GetComponent<TextMeshPro>().text = ProductState.ErrorCounts.ToString();
        //Queue overflow time in min
        //RoundedOverflowTime = Mathf.Round(ProductState.OverflowTime * 100) / 100;
        MinuteOver = ((int)ProductState.OverflowTime / 60) > 0 ? ((int)ProductState.OverflowTime / 60).ToString() + "min" : "";
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(1).GetComponent<TextMeshPro>().text = MinuteOver + ((int)ProductState.OverflowTime % 60f).ToString() + "sec";

        //carrycounts and Human error count
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(2).GetComponent<TextMeshPro>().text = ProductState.CarryCounts.ToString();
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(3).GetComponent<TextMeshPro>().text = ProductState.HumanErrorCounts.ToString();

        //blocking time  
        RoundedBlockingTime = Mathf.Round((ProductState.BlockingTime / ProductState.PassedTime) * 100);
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(4).GetComponent<TextMeshPro>().text = RoundedBlockingTime.ToString();

        //Missed demand
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(5).GetComponent<TextMeshPro>().text = ProductState.MissedProduct.ToString();     

        //Updating Time Passed on assessment panel
        Minute = ((int)ProductState.PassedTime / 60) > 0 ? ((int)ProductState.PassedTime / 60).ToString() + "min" : "";
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(6).GetComponent<TextMeshPro>().text = Minute + ((int)ProductState.PassedTime % 60f).ToString() + "sec";

    }

    //Change the text of last simulation's assessment result
    public void ChangeOldAssessmentText()
    {

        if(!FollowAssessmentMenu)
        {
            return;
        }
        //Change text of assessment last time
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(0).GetComponent<TextMeshPro>().text = (Mathf.Round(ProductState.OldCyTime * 100.0f) * 0.01f).ToString() + "sec";

        ////Queue overflow count old
        //FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(1).GetComponent<TextMeshPro>().text = ProductState.OldErrorCount.ToString();
        //Queue overflow time in min
        //RoundedOldOverflowTime = Mathf.Round(ProductState.OldOverflowTime * 100) / 100;
        //FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(1).GetComponent<TextMeshPro>().text = RoundedOldOverflowTime.ToString();
        OldMinuteOver = ((int)ProductState.OldOverflowTime / 60) > 0 ? ((int)ProductState.OldOverflowTime / 60).ToString() + "min" : "";
        FollowAssessmentMenu.transform.Find("ResultTextCollection").GetChild(1).GetComponent<TextMeshPro>().text = OldMinuteOver + ((int)ProductState.OldOverflowTime % 60f).ToString() + "sec";

        //Old carry count and human error count
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(2).GetComponent<TextMeshPro>().text = ProductState.OldCarryCount.ToString();
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(3).GetComponent<TextMeshPro>().text = ProductState.OldHumanErrorCounts.ToString();

        // old blocking time
        RoundedOldBlockingTime = Mathf.Round((ProductState.OldBlockingTime / ProductState.OldTimePassed) * 100);
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(4).GetComponent<TextMeshPro>().text = RoundedOldBlockingTime.ToString();

        //old Missed demand
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(5).GetComponent<TextMeshPro>().text = ProductState.OldMissedProduct.ToString();

        //old passed time
        OldMinute = ((int)ProductState.OldTimePassed / 60) > 0 ? ((int)ProductState.OldTimePassed / 60).ToString() + "min" : "";
        FollowAssessmentMenu.transform.Find("OldResultTextCollection").GetChild(6).GetComponent<TextMeshPro>().text = OldMinute + ((int)ProductState.OldTimePassed % 60f).ToString() + "sec";
    }
    //Change the text on the suggestion panel
    public void ChangeSuggestionText(bool Performance)
    {
        if (!FactorPanel)
        {
            Debug.Log("No factor panel");
            return;
        }
        if (Performance)
        {
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("Performance").GetComponent<TextMeshPro>().text = "Good Performance";
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("Performance").GetComponent<TextMeshPro>().color = Color.green;
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("SuggestionText").GetComponent<TextMeshPro>().text = "No suggestion.";

        }
        else
        {
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("Performance").GetComponent<TextMeshPro>().text = "Bad Performance";
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("Performance").GetComponent<TextMeshPro>().color = Color.red;
            FactorPanel.transform.Find("Suggestion").Find("SuggestionPlate").Find("SuggestionText").GetComponent<TextMeshPro>().text = DetermineSuggestion();

        }
    }
    //Determine the suggestion based on the currently set factors
    public string DetermineSuggestion()
    {
        if (ProductState.Batch <= 10) // 8
        {
            return "Higher batch size is recommended.";
        }
        else if(ProductState.SpawnTime > 15) //13
        {
            return "Lower inter-arrival rate is recommended.";
        }
        else if(ProductState.FullTimer <= 15) //13
        {
            return "Higher simulation time is recommended.";
        }
        else if (RoundedBlockingTime <= 5)
        {
            return "Increase inter-arrival rate is recommended.";
        }
        else
        {
            return "Cycle time threshold might be too high to accomplish, a lower target is recommended.";
        }
    }

}
