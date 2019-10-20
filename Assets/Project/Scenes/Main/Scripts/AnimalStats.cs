using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    [CreateAssetMenu(fileName = "New Simulation Animal Stats", menuName = "Simulation/NewAnimalStats", order = 1)]

    public class AnimalStats : global::AnimalStats
    {
        [SerializeField, Tooltip("Chance of this animal reproducing with another animal."), Range(0f, 100f)]
        public float reproduction = 0f;

        [SerializeField, Tooltip("Maximum age of animal."), Range(0f, 1000f)]
        public float maxAge = 1000;
    }

}
