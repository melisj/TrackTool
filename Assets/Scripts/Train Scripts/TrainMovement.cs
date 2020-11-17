using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMovement : CartMovement
{
    public override void FixedUpdate() {
        connectedCart.ForEach((cart) => cart.GiveSpeedToConnectedCarts(this));

        base.FixedUpdate();
    }

    float multiplier = 1;
    public override void Update() {
        base.Update();

        if(Input.GetKey(KeyCode.RightShift)) {
            multiplier = 50;
        }
        else {
            multiplier = 1;
        }

        if (Input.GetKey(KeyCode.N)) {
            speed += Time.deltaTime * multiplier;
        }

        if (Input.GetKey(KeyCode.M)) {
            speed -= Time.deltaTime * multiplier;
        }
    }
}
