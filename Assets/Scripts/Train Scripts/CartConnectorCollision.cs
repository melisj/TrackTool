using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartConnectorCollision : MonoBehaviour
{
    BoxCollider colliderInstance;
    CartMovement cartInstance;

    CartMovement currentCart;

    void Start()
    {
        colliderInstance = GetComponent<BoxCollider>();
        cartInstance = GetComponentInParent<CartMovement>();
        //GameManager.cartManager.AddReference(colliderInstance.GetInstanceID(), cartInstance);
    }

    void OnDestroy()
    {
        //GameManager.cartManager.RemoveReference(colliderInstance.GetInstanceID());
    }

    public void UpdateCollision(CartMovement thisCart, CartMovement interactedCart) {
        //GameManager.cartManager.CollisionThisFrame(colliderInstance.GetInstanceID());

        thisCart.PushedCart = interactedCart;
        interactedCart = thisCart.PushedCart;
        float speedDifference = thisCart.speed - interactedCart.speed;

        if (speedDifference > 0) {
            thisCart.speed += speedDifference / 2;
            interactedCart.speed -= speedDifference / 2;
        } else {
            thisCart.speed -= speedDifference / 2;
            interactedCart.speed += speedDifference / 2;
        }
    }

    public void OnTriggerEnter(Collider collider) {
        //currentCart = GameManager.cartManager.SearchFollowObject(collider.GetInstanceID());
        if (currentCart != null) {
            //UpdateCollision(cartInstance, currentCart);
        } 
    }

    public void OnTriggerStay(Collider collider) {
        if (currentCart != null) {
            /*
            if (cartInstance.speed > 0) {
                if(cartInstance.speed < currentCart.speed)
                    cartInstance.speed = currentCart.speed;
            } else {
                if (cartInstance.speed > currentCart.speed)
                    cartInstance.speed = currentCart.speed;
            }
            */
            //cartInstance.speed = currentCart.speed;
        }
    }

    public void OnTriggerExit(Collider collider) {
        //currentCart = GameManager.cartManager.SearchFollowObject(collider.GetInstanceID());
        //if (currentCart == null) {
        //    cartInstance.PushedCart = null;
        //}
    }
}
