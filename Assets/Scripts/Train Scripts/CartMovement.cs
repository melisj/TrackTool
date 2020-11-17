using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartMovement : MonoBehaviour
{
    FollowObject centerPoint;
    float carriageFrontTime, carriageBackTime;
    public GameObject carriageFront, carriageRear;

    public Vector3 bodyOffset;
    public float carriageOffset;

    public float speed;
    public float drag;
    public Rigidbody rb;

    public CartMovement PushedCart { get; set; }
    public List<CartMovement> cartsInRange = new List<CartMovement>();
    public List<CartMovement> connectedCart = new List<CartMovement>();

    public void Awake() {
        rb = GetComponentInChildren<Rigidbody>();
        centerPoint = GetComponentInChildren<FollowObject>();
    }

    public virtual void FixedUpdate() {
        ApplyDrag();

        UpdateSpeed(speed * Time.fixedDeltaTime);
    }

    public void ApplyDrag() {
    }

    public virtual void UpdateSpeed(float distance) {
        // Calculate new position for the centerpoint
        centerPoint.OnUpdate(distance);

        // Calculate position of the front carriage and the back
        carriageFrontTime = centerPoint.CalculateDistanceOnCurve(carriageOffset, out NodeBehaviour frontNode);
        carriageBackTime = centerPoint.CalculateDistanceOnCurve(-carriageOffset, out NodeBehaviour rearNode);

        rear = rearNode.GetCurvePoint(carriageBackTime);
        front = frontNode.GetCurvePoint(carriageFrontTime);

        carriageRear.transform.rotation = Quaternion.LookRotation(rearNode.GetCurveDirection(carriageBackTime));
        carriageFront.transform.rotation = Quaternion.LookRotation(frontNode.GetCurveDirection(carriageFrontTime));

        // Calculate rotation
        rb.MoveRotation(Quaternion.LookRotation(front - rear));

        // Calculate position
        rb.MovePosition(((front + rear) / 2) + bodyOffset);
    }

    public void GiveSpeedToConnectedCarts(CartMovement fromCart) {
        speed = fromCart.speed;
        connectedCart.ForEach((cart) => { 
            if (cart != fromCart) 
                cart.GiveSpeedToConnectedCarts(this); 
        });
    }

    public virtual void Update() {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            for (int i = connectedCart.Count - 1; i >= 0; i--) {
                connectedCart.RemoveAt(i);
            }

            foreach (CartMovement cart in cartsInRange) {
                connectedCart.Add(cart);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            for (int i = connectedCart.Count - 1; i >= 0; i--) {
                connectedCart.RemoveAt(i);
            }
        }
    }

    Vector3 rear, front;
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rear, 0.2f);
        Gizmos.DrawSphere(front, 0.2f);
    }
}
