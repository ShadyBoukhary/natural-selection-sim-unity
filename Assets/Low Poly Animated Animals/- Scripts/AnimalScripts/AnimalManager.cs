using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LowPolyAnimalPack {
	public class AnimalManager : MonoBehaviour {
		[SerializeField]
		private bool peaceTime;

		[SerializeField]
		public int wolfPopulation = 0;
    public int WolfPopulation => wolfPopulation;

		[SerializeField]
		public int rabbitPopulation = 0;
    public int RabbitPopulation => rabbitPopulation;

		public bool PeaceTime {
			get {
				return peaceTime;
			}
			set {
				SwitchPeaceTime(value);
			}
		}

		private static AnimalManager instance;
		public static AnimalManager Instance {
			get {
				return instance;
			}
		}

		private void Awake() {
			if (instance != null && instance != this) {
				Destroy(gameObject);
				return;
			}

			instance = this;
		}

		private void Start() {
			if (peaceTime) {
				Debug.Log("AnimalManager: Peacetime is enabled, all animals are non-agressive.");
				SwitchPeaceTime(true);
			}
		}

		public void SwitchPeaceTime(bool enabled) {
			if (enabled == peaceTime) {
				return;
			}

			peaceTime = enabled;

			Debug.Log(string.Format("AnimalManager: Peace time is now {0}.", enabled ? "On" : "Off"));
			foreach (WanderScript animal in WanderScript.AllAnimals) {
				animal.SetPeaceTime(enabled);
			}
		}

		public void Nuke() {
			foreach (WanderScript animal in WanderScript.AllAnimals) {
				animal.Die();
			}
		}

		private void Update() {
      List<WanderScript> list = Simulator.Animal.AllAnimals;
      int rabbits = 0;
      int wolves = 0;
      foreach(var animal in list) {
        Simulator.Animal a = (Simulator.Animal)animal;
        if (a.Species == "Wolf" && a.IsAlive) wolves++;
        if (a.Species == "Rabbit" && a.IsAlive) rabbits++;
      }
      //print($"There are {wolves} wolves and {rabbits} rabbits.");
      wolfPopulation = wolves;
      rabbitPopulation = rabbits;
		}
	}
}