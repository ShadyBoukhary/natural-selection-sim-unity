using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using LowPolyAnimalPack;

namespace Simulator {
	[RequireComponent(typeof(Animator)), RequireComponent(typeof(CharacterController))]
	public class Animal : WanderScript {

		protected override void Awake() {
			if (idleStates.Length == 0 && movementStates.Length == 0) {

				Debug.LogError(string.Format("{0} has no idle or movement states state.", gameObject.name));
				enabled = false;
				return;
			}

			foreach (IdleState state in idleStates) {
				totalIdleStateWeight += state.stateWeight;
			}

			origin = transform.position;
			animator = GetComponent<Animator>();
			animator.applyRootMotion = false;
			characterController = GetComponent<CharacterController>();
			navMeshAgent = GetComponent<NavMeshAgent>();
			originalDominance = ScriptableAnimalStats.dominance;
			originalScent = scent;
			originalAgression = ScriptableAnimalStats.agression;

			if (navMeshAgent) {
				useNavMesh = true;
				navMeshAgent.stoppingDistance = contingencyDistance;
			}

			if (matchSurfaceRotation && transform.childCount > 0) {
				transform.GetChild(0).gameObject.AddComponent<SurfaceRotation>().SetRotationSpeed(surfaceRotationSpeed);
			}

			allAnimals.Add(this);
		}

		private Animal GetAnimal(int i) {
			return (Animal)allAnimals[i];
		}

		protected override void DecideNextState(bool wasIdle, bool firstState = false) {
			attacking = false;

			// Look for a predator.
			if (awareness > 0) {
				for (int i = 0; i < allAnimals.Count; i++) {
					if (GetAnimal(i).dead == true || GetAnimal(i) == this || GetAnimal(i).species == species || GetAnimal(i).ScriptableAnimalStats.dominance <= ScriptableAnimalStats.dominance || GetAnimal(i).ScriptableAnimalStats.stealthy) {
						continue;
					}

					if (Vector3.Distance(transform.position, GetAnimal(i).transform.position) > awareness) {
						continue;
					}

					if (useNavMesh) {
						RunAwayFromAnimal(GetAnimal(i));
					} else {
						NonNavMeshRunAwayFromAnimal(GetAnimal(i));
					}

					if (logChanges) {
						Debug.Log(string.Format("{0}: Found predator ({1}), running away.", gameObject.name, GetAnimal(i).gameObject.name));
					}

					return;
				}
			}

			// Look for pray.
			if (ScriptableAnimalStats.dominance > 0) {
				for (int i = 0; i < allAnimals.Count; i++) {
					if (GetAnimal(i).dead == true || GetAnimal(i) == this || (GetAnimal(i).species == species && !ScriptableAnimalStats.territorial) || GetAnimal(i).ScriptableAnimalStats.dominance > ScriptableAnimalStats.dominance || GetAnimal(i).ScriptableAnimalStats.stealthy) {
						continue;
					}

					int p = System.Array.IndexOf(nonAgressiveTowards, GetAnimal(i).species);
					if (p > -1) {
						continue;
					}

					if (Vector3.Distance(transform.position, GetAnimal(i).transform.position) > scent) {
						continue;
					}

					if (Random.Range(0, 100) > ScriptableAnimalStats.agression) {
						continue;
					}

					if (logChanges) {
						Debug.Log(string.Format("{0}: Found prey ({1}), chasing.", gameObject.name, GetAnimal(i).gameObject.name));
					}

					ChaseAnimal(GetAnimal(i));
					return;
				}
			}

			if (wasIdle && movementStates.Length > 0) {
				if (logChanges) {
					Debug.Log(string.Format("{0}: Wandering.", gameObject.name));
				}
				BeginWanderState();
				return;
			} else if (idleStates.Length > 0) {
				if (logChanges) {
					Debug.Log(string.Format("{0}: Idling.", gameObject.name));
				}
				BeginIdleState(firstState);
				return;
			}

			// Backup selection.
			if (idleStates.Length == 0) {
				BeginWanderState();
			} else if (movementStates.Length == 0) {
				BeginIdleState();
			}
		}
		protected override void OnDestroy() {
			allAnimals.Remove(this);
		}
	}

}
