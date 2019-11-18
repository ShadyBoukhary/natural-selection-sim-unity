using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Simulator {
	[ExecuteInEditMode]
	public class Spawner : MonoBehaviour {
		[SerializeField] float spawnSize;
		[SerializeField] int spawnAmmount;

    [SerializeField, Range(10, 100)]
    private int rabbitPercentage = 50;

		[SerializeField] Animal[] animals;

		[ContextMenu("Spawn Animals")]

		private void Start() {
			SpawnAnimals();
		}
		void SpawnAnimals() {
			var parent = new GameObject("SpawnedAnimals");
			for (int i = 0; i < spawnAmmount; i++) {
        int value = 1;

        if (Random.Range(0, 100) < rabbitPercentage) {
          value = 0;
        } 
				print($"Instantiating {animals[value].gameObject.name}. Value: {value}");
				var animal = Instantiate(animals[value], RandomNavmeshLocation(spawnSize), Quaternion.identity, parent.transform);
        animal.SetProperties(Reproduction.DecideIfFemale(), animals[value].MateAwareness, 100);
			}
		}

		public Vector3 RandomNavmeshLocation(float radius) {
			Vector3 randomDirection = Random.insideUnitSphere * radius;
			randomDirection += transform.position;
			NavMeshHit hit;
			Vector3 finalPosition = Vector3.zero;
			if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
				finalPosition = hit.position;
			}
			return finalPosition;
		}

		private void OnDrawGizmosSelected() {
			Gizmos.DrawWireSphere(transform.position, spawnSize);
		}
	}

}
