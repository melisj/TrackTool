using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static CartManager cartManager;

    public void Awake() {
        cartManager = new CartManager();
    }
}
