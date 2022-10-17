using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CartHitboxPos : MonoBehaviour
{
    private float p1_hitbox_x;
    private float p1_hitbox_y;
    private float p1_hitbox_z;
    private float p2_hitbox_y;
    private float p2_hitbox_z;

    //Move cart hit box product 1
    public Vector3 MoveP1Box(int num_of_p1) // number of p1 in cart
    {
        GameObject cart = GameObject.Find("Cart1");
        p1_hitbox_x = num_of_p1 % 2 == 0 ? 0.05f : 0.15f;
        if (num_of_p1 <= 7)
        {
            p1_hitbox_z = 0.25f - (num_of_p1) / 2 * 0.1f;
            p1_hitbox_y = 0.1f;
        }
        else if (num_of_p1 <= 15)
        {
            p1_hitbox_z = 0.25f - (num_of_p1 - 8) / 2 * 0.1f;
            p1_hitbox_y = 0.2f;
        }
        else if (num_of_p1 <= 23)
        {
            p1_hitbox_z = 0.25f - (num_of_p1 - 16) / 2 * 0.1f;
            p1_hitbox_y = 0.3f;
        }
        else if (num_of_p1 <= 31)
        {
            p1_hitbox_z = 0.25f - (num_of_p1 - 24) / 2 * 0.1f;
            p1_hitbox_y = 0.4f;
        }
        else if (num_of_p1 <= 39)
        {
            p1_hitbox_z = 0.25f - (num_of_p1 - 32) / 2 * 0.1f;
            p1_hitbox_y = 0.5f;
        }
        else if (num_of_p1 <= 47)
        {
            p1_hitbox_z = 0.25f - (num_of_p1 - 40) / 2 * 0.1f;
            p1_hitbox_y = 0.6f;
        }
        foreach(Transform child in cart.transform)
        {
            if (child.tag == "CartDetection1")
            {
                child.transform.localPosition = new Vector3(p1_hitbox_x, p1_hitbox_y, p1_hitbox_z);
            }
        }
        return new Vector3(p1_hitbox_x, p1_hitbox_y, p1_hitbox_z);
    }

    ////Move cart hit box product 2
    public Vector3 MoveP2Box(int num_of_p2)
    {
        GameObject cart = GameObject.Find("Cart1"); 
        //GameObject C = GameObject.FindGameObjectWithTag("Cart");
        if (num_of_p2 <= 3)
        {
            p2_hitbox_z = 0.25f - (num_of_p2) * 0.1f;
            p2_hitbox_y = 0.1f;

        }
        else if (num_of_p2 <= 7)
        {
            p2_hitbox_z = 0.25f - (num_of_p2 - 4) * 0.1f;
            p2_hitbox_y = 0.2f;
        }
        else if (num_of_p2 <= 11)
        {
            p2_hitbox_z = 0.25f - (num_of_p2 - 8) * 0.1f;
            p2_hitbox_y = 0.3f;
        }
        else if (num_of_p2 <= 15)
        {
            p2_hitbox_z = 0.25f - (num_of_p2 - 12) * 0.1f;
            p2_hitbox_y = 0.4f;
        }
        else if (num_of_p2 <= 19)
        {
            p2_hitbox_z = 0.25f - (num_of_p2 - 16) * 0.1f;
            p2_hitbox_y = 0.5f;
        }
        else if (num_of_p2 <= 23)
        {
            p2_hitbox_z = 0.25f - (num_of_p2 - 20) * 0.1f;
            p2_hitbox_y = 0.6f;
        }
        foreach (Transform child in cart.transform)
        {
            if (child.tag == "CartDetection2")
            {
                child.transform.localPosition = new Vector3(-0.1f, p2_hitbox_y, p2_hitbox_z);
            }
        }
        return new Vector3(-0.1f, p2_hitbox_y, p2_hitbox_z);
    }
}
