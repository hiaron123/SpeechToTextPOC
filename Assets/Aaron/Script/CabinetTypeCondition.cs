using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CabinetTypeCondition
    {
        public string cabinetType;
        public int[] quantity;
        public GameObject[] prefabForThisQuantity;
        public bool CheckIfCanBePlaced(int currentQuantity)
        {
            if (quantity.Contains(currentQuantity))
            {
                return true;
            }

            return false;
        }
    }
