using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Linq;
using System.Text;
using TMPro;
using Vuforia;
using Photon.Pun;
using UnityEngine.SceneManagement;


public class OperatorTraining : MonoBehaviour
{
    //PERFORMANCE is true for "Good"
    public bool Performance()
    {
        if (Assessment.cyTime <= Assessment.ThresholdCT)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Quantity of cart is true for "More than 15"
    public bool Quantity()
    {
        if (ProductState.SpawnTime >= 15 && ProductState.Batch >= 15)
        {
            return true;
        }
        else if (!Performance() && ProductState.InCart.Count() >= 15)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Time process is true for "Continue"
    public bool Time()
    {
        if (ProductState.HumanErrorCounts == 0 && ProductState.ErrorCounts == 0)
        {
            return true;
        }
        else if ((Performance() && ProductState.HumanErrorCounts > 5) || (ProductState.HumanErrorCounts != 0 && ProductState.ErrorCounts == 0))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Route transport is true for "Clockwise"
    public bool Route()
    {
        if (Factors.RackPosIndex == 1)
        {
            return true;
        }
        else if (!Performance() || Factors.RackPosIndex == 2)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
