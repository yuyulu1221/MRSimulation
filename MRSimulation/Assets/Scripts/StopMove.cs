using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using Photon.Pun;

public class StopMove : MonoBehaviour
{
    public Error error;
    public Menu menu;
    private bool Collided = false;
    private Vector3 old;
   
    //Enters when enters collision
    void OnCollisionEnter(Collision collision)
    {
        //If collided is true means the product already collided
        if(Collided == true)
        {
            return;
        }

        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        else if (collision.gameObject.tag == "Stopper" || collision.gameObject.tag == "Product01" || collision.gameObject.tag == "Product02")
        {
            //If it's the manager then handle the gameobject queues(I mean data structure queue not the equipment) in ProductState script
            if (ChooseMenu.Manager || ChooseMenu.LonelyManager)
            {
                
                //ProductState.InQueue.Enqueue(ProductState.InConveyor.ElementAt(0));
                ProductState.InQueue.AddLast(ProductState.InConveyor.ElementAt(0));
                ProductState.InConveyor.RemoveFirst();

                //If the product entering the queue is the number queue limit + 1 then over limit count plus one
                if(ProductState.InQueue.Count == ProductState.QueueSize + 1)
                {
                    ProductState.ErrorCounts++;                                                   
                    PhotonView photonViewE = PhotonView.Find(3);
                    photonViewE.RPC("SpawnDialogforTarget", RpcTarget.All, "ERROR", "Queue is full!", true);
                               
                }
            }
            //Stop the product by destroying the rigidbody
            //this.GetComponent<Rigidbody>().useGravity = false;
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            this.GetComponent<Rigidbody>().isKinematic = true;
            //Destroy(GetComponent<Rigidbody>());
            Collided = true;
            //If the product is not the first to enter queue
            if(collision.transform.tag == "Product01" || collision.transform.tag == "Product02")
            {
                //trying to deal with the physics of unity by setting the position
                this.transform.localPosition = new Vector3(collision.transform.localPosition.x, collision.transform.localPosition.y + 0.1f, this.transform.localPosition.z);
            }
        }
    }

    //To update if any collided of product
    private void Update()
    {
        if(Collided == true)
        {
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
