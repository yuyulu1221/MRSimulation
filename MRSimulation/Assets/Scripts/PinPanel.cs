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

public class PinPanel : MonoBehaviour
{
    public Material UnPinned;
    public Material Pinned;
    public void PinthePanel(GameObject Panel)
    {
        if (Panel.GetComponent<SolverHandler>().UpdateSolvers)
        {
            Panel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = Pinned;
            Panel.GetComponent<SolverHandler>().UpdateSolvers = false;
        }
        else
        {
            Panel.transform.Find("PinButtonHoloLens2").Find("BackPlate").GetComponentInChildren<MeshRenderer>().material = UnPinned;
            Panel.GetComponent<SolverHandler>().UpdateSolvers = true;
        }
    }
}
