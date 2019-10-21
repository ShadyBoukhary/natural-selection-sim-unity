using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace Simulator {
	public static class Reproduction {
		public static void Reproduce(Animal father, Animal mother) {
			Animal animal = Animal.Instantiate(father, new Vector3(father.transform.position.x + 2, father.transform.position.y, father.transform.position.z + 2), Quaternion.identity);
			animal.transform.localScale = DecideSize(father, mother);
      animal.SetProperties(DecideIfFemale(), father.MateAwareness, 100);
		}

    private static Vector3 DecideSize(Animal father, Animal mother) {
      return new Vector3(0.7f, 0.7f, 0.7f);
    }

    private static bool DecideIfFemale() {
      return Random.Range(0, 100) > 50;
    }
	}
}

