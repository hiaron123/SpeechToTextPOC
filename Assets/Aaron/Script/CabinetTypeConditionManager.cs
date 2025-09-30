
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class CabinetTypeConditionManager : MonoBehaviour
    {

         // side by side and back to back can generate in runtime, only pod need to use prefab
        [Header("Cabinet Type")]
        public List<CabinetTypeCondition> cabinetTypeConditions;


    }
