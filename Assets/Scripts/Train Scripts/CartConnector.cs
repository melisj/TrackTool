using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartConnector : MonoBehaviour
{
    BoxCollider colliderInstance;
    CartMovement cartInstance;

    CartMovement cartBeingChecked;

    void Start() {
        colliderInstance = GetComponent<BoxCollider>();
        cartInstance = GetComponentInParent<CartMovement>();
        GameManager.cartManager.AddReference(colliderInstance.GetInstanceID(), cartInstance);
    }

    void OnDestroy() {
        GameManager.cartManager.RemoveReference(colliderInstance.GetInstanceID());
    }

    public void OnTriggerEnter(Collider collider) {
        cartBeingChecked = GameManager.cartManager.SearchFollowObject(collider.GetInstanceID());
        if (cartBeingChecked)
            cartInstance.cartsInRange.Add(cartBeingChecked);
        cartBeingChecked = null;
    }

    public void OnTriggerExit(Collider collider) {
        cartBeingChecked = GameManager.cartManager.SearchFollowObject(collider.GetInstanceID());
        if (cartBeingChecked)
            cartInstance.cartsInRange.Remove(cartBeingChecked);
        cartBeingChecked = null;
    }
}
