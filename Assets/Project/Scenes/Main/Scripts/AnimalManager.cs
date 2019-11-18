using System.Collections.Generic;
using UnityEngine;
using LowPolyAnimalPack;
using System.IO;

namespace Simulator {
	public class AnimalManager : MonoBehaviour {
		[SerializeField]
		private bool peaceTime;

		[SerializeField]
		public int wolfPopulation = 0;
		public int WolfPopulation => wolfPopulation;

		[SerializeField]
		public int rabbitPopulation = 0;
		public int RabbitPopulation => rabbitPopulation;
		public int frame = 0;
		BackgroundWorker worker;

    [SerializeField]
    string outputName;

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
			worker = new BackgroundWorker();
			worker.DoWork += DoWork;
			worker.RunWorkerCompleted += RunWorkerCompletedEventHandler;
			worker.RunWorkerAsync();

		}

		void DoWork(object sender, DoWorkEventArgs e) {
			print("Starting Work");
      string path = $"./output/run{outputName}.txt";
      Directory.CreateDirectory("./output");
      File.Delete(path);
			// Open the file to read from.
			int f = frame;
			while (frame < 10000) {
				if (f < frame) {

					List<WanderScript> list = Simulator.Animal.AllAnimals;
					int rabbits = 0;
					int wolves = 0;
					for (int i = 0; i < list.Count; i++) {
						Simulator.Animal a = (Simulator.Animal)list[i];
						if (a.SpeciesName == "Wolf" && a.IsAlive) wolves++;
						if (a.SpeciesName == "Rabbit" && a.IsAlive) rabbits++;
					}
					//print($"There are {wolves} wolves and {rabbits} rabbits.");
					wolfPopulation = wolves;
					rabbitPopulation = rabbits;
					f = frame;
          if (f % 10 == 0) {
            string s = $"{frame},{wolves},{rabbits}";
            File.AppendAllLines(path, new List<string>() { s });
          }

				}
			}


		}

		void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e) {
			print("Completed");
			print(e.Error);
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
			frame++;
			worker.Update();
		}
	}
}