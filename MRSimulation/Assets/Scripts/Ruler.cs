using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using TMPro;



public class Ruler : MonoBehaviour
{
    public GameObject Sphere;
    public GameObject Tip;
    static public List<GameObject> Spheres = new List<GameObject>();
    static public List<GameObject> LengthText = new List<GameObject>();
    public Vector3 EndPoint = new Vector3(0, 0, 0);
    public Transform IndexTrans;
    protected LineRenderer line;
    public Material LineColor;
    public Vector3 PlacePos;
    static public bool raypointer = true;
    static public int i = 1;
    static public int LineCount = 0;
    public bool handbool;
    public bool HitSphere;
    static public bool ManipBool;

    //When the pointer of hand is on an equipment the ManipBool set true
    public void ManipStartDontPlace()
    {
        ManipBool = true;
    }

    //When the pointer of hand left an equipment the ManipBool set false
    public void ManipStopCanPlace()
    {
        ManipBool = false;
    }

    //Selecting spheres placing method
    public void RayPointer()
    {
        raypointer = true;
    }
    public void IndexPointer()
    {
        raypointer = false;
    }

    //Create Lines
    void CreateLine()
    {
        line = new GameObject("Lines(" + LineCount+1 + ")").AddComponent<LineRenderer>();
        line.tag = "Lines";
        line.positionCount = 2;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.useWorldSpace = true;
        line.material = LineColor;
        line.receiveShadows = false;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.startColor = Color.white;
        line.endColor = Color.white;
    }

    //Tap to place spheres
    public void TapPlaceSphere()
    {
        //Check if the pointer is on any equipment
        if(ManipBool)
        {
            return;
        }

        //Far pointer placement
        else if (raypointer == true)
        {
            if (handbool != false)
            {
                Spheres.Add(Instantiate(Sphere, PlacePos, Quaternion.identity));
            }
            else
            {
                return;
            }
        }
        //Index pointer placement
        else 
        {
            Spheres.Add(Instantiate(Sphere, PlacePos, Quaternion.identity));
        }
    }

    void Update()
    {
        //Get index position
        var HandJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
        IndexTrans = HandJointService.RequestJointTransform(TrackedHandJoint.IndexTip,Handedness.Right);
        //Get pointer position and check if pointer hit spheres
        foreach (var source in CoreServices.InputSystem.DetectedInputSources)
        {
            if (source.SourceType == Microsoft.MixedReality.Toolkit.Input.InputSourceType.Hand)
            {
                foreach (var p in source.Pointers)
                {
                    if (p is IMixedRealityNearPointer)
                    {
                        continue;
                    }
                    if (p.Result != null)
                    {
                        //Gets far pointer postition and check if hits sphere
                        var hitObject = p.Result.Details.Object;
                        if (!hitObject)
                        {
                            handbool = false;
                        }
                        else
                        {
                            handbool = true;
                            EndPoint = p.Result.Details.Point;
                        }
                    }
                }
            }
        }
        //raypointer true means currently select placing method is far pointer
        if (raypointer == true)
        {
            PlacePos = EndPoint;
        }
        //Currently selected method is index placing
        else 
        {
            PlacePos = IndexTrans.position;
        }

        //Create line between two spheres
        if (Spheres.Count == i+1 && LineCount == i-1 )
        {
            Debug.Log("CreateLine");
            CreateLine();
            line.SetPosition(0, Spheres[i-1].GetComponent<Transform>().position);
            line.SetPosition(1, Spheres[i].GetComponent<Transform>().position);
            //Get distance between two points and mid point
            float Dis = Vector3.Distance(Spheres[i - 1].GetComponent<Transform>().position, Spheres[i].GetComponent<Transform>().position);
            Vector3 MidPoint = Vector3.Lerp(Spheres[i - 1].GetComponent<Transform>().position, Spheres[i].GetComponent<Transform>().position, 0.5f);
            LengthText.Add(Instantiate(Tip, MidPoint, Quaternion.identity));
            LengthText[i - 1].GetComponent<ToolTip>().ToolTipText = string.Format("{0:0.###}", Dis) + "m";
            i++;
            LineCount++;
        }

        int m = 0;
        //Check if spheres were moved to update line and distance text
        while(m < LineCount)
        {
            float Dis = Vector3.Distance(Spheres[m].GetComponent<Transform>().position, Spheres[m+1].GetComponent<Transform>().position);
            Vector3 MidPoint = Vector3.Lerp(Spheres[m].GetComponent<Transform>().position, Spheres[m+1].GetComponent<Transform>().position, 0.5f);
            GameObject MovingLine = GameObject.Find("Lines(" + m+1 +")");
            MovingLine.GetComponent<LineRenderer>().SetPosition(0, Spheres[m].GetComponent<Transform>().position);
            MovingLine.GetComponent<LineRenderer>().SetPosition(1, Spheres[m+1].GetComponent<Transform>().position);
            LengthText[m].GetComponent<Transform>().position = MidPoint;
            LengthText[m].GetComponent<ToolTip>().ToolTipText = string.Format("{0:0.###}", Dis) + "m";
            m++;
        }

        

        
    }
}
