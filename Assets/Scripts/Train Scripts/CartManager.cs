using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartManager
{
    public Dictionary<int, CartMovement> references = new Dictionary<int, CartMovement>();
    public List<int> currentCollisions = new List<int>();

    public void AddReference(int colliderId, CartMovement cartReference) {
        if(!references.ContainsKey(colliderId))
            references.Add(colliderId, cartReference);
    }

    public void RemoveReference(int colliderId) {
        references.Remove(colliderId);
    }

    public CartMovement SearchFollowObject(int colliderId) {
        if (!currentCollisions.Contains(colliderId)) {
            references.TryGetValue(colliderId, out CartMovement value);
            return value;
        }

        return null;
    }
}
