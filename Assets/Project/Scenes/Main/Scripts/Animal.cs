using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using LowPolyAnimalPack;

namespace Simulator {
	[RequireComponent(typeof(Animator)), RequireComponent(typeof(CharacterController))]
	public class Animal : WanderScript {

		public MovementState[] MovementStates => movementStates;
		public bool IsFemale => isFemale;
    public float MateAwareness => mateAwareness;
    public float Hunger => hunger;
		[Space(), Header("Reproductive AI"), Space(5)]
		[SerializeField, Tooltip("How far this animal can find a mate.")]
		protected float mateAwareness = 30f;

		[SerializeField, Tooltip("Whether the animal is a female.")]
		protected bool isFemale;

		[SerializeField]
		private bool drawMateAwarenesRange;

		private float hunger = 100;
		protected Color mateAwarenessColor = new Color(1f, 0.5f, 0.75f, 1f);

		public override void OnDrawGizmosSelected() {
			base.OnDrawGizmosSelected();
			if (drawMateAwarenesRange) {
				//Draw circle radius for Mate Awareness.
				Gizmos.color = mateAwarenessColor;
				Gizmos.DrawWireSphere(transform.position, mateAwareness);


				Vector3 IconAwareness = new Vector3(transform.position.x, transform.position.y + mateAwareness, transform.position.z);
				Gizmos.DrawIcon(IconAwareness, "ico-awareness", true);
			}
		}
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


		private static Animal GetAnimal(int i) {
			return (Animal)allAnimals[i];
		}


		protected override void DecideNextState(bool wasIdle, bool firstState = false) {
			attacking = false;

			SearchReport report = SearchForAnimals();

			// Run away from predator if found
			if (report.FoundPredator) {
				if (logChanges) {
					Debug.Log($"{gameObject.name}: Found predator ({GetAnimal(report.PredatorIndex).gameObject.name}), running away.");
				}
				if (useNavMesh) {
					RunAwayFromAnimal(GetAnimal(report.PredatorIndex));

				} else {
					NonNavMeshRunAwayFromAnimal(GetAnimal(report.PredatorIndex));
				}

				// Chase prey if found
			} else if (report.FoundPrey) {
				if (logChanges) {
					Debug.Log($"{gameObject.name}: Found prey ({GetAnimal(report.PreyIndex).gameObject.name}), chasing.");
				}
				ChaseAnimal(GetAnimal(report.PreyIndex));

				// Approach animal to mate if found
			} else if (report.FoundMate) {
				if (logChanges) {
					Debug.Log($"{gameObject.name}: Found mate ({GetAnimal(report.MateIndex).gameObject.name}), approaching.");
				}
				ApproachMate(GetAnimal(report.MateIndex));
				// TODO: implement

				// Start wandering if previously idle
			} else if (wasIdle && movementStates.Length > 0) {
				if (logChanges) {
					Debug.Log(string.Format("{0}: Wandering.", gameObject.name));
				}
				BeginWanderState();

				// Idle otherwise
			} else if (idleStates.Length > 0) {
				if (logChanges) {
					Debug.Log(string.Format("{0}: Idling.", gameObject.name));
				}
				BeginIdleState(firstState);

				// backup selection
			} else if (idleStates.Length == 0) {
				BeginWanderState();

			} else if (movementStates.Length == 0) {
				BeginIdleState();
			} else {
				Debug.LogError($"{gameObject.name}: Unknown state when deciding next state.");
			}

		}
		protected override void OnDestroy() {
			allAnimals.Remove(this);
		}

		private void ApproachMate(Animal animal) {
			Vector3 target = animal.transform.position;
			StartCoroutine(animal.BeApproachedBy(this));
			currentState = AnimalUtils.GetFastestMovementState(this);
			SetMovementAnimation();
			StartCoroutine(MateState(animal));

		}

		private IEnumerator MateState(Animal mate) {
			moving = true;
			navMeshAgent.speed = movementStates[currentState].moveSpeed;
			navMeshAgent.angularSpeed = movementStates[currentState].turnSpeed;
			navMeshAgent.SetDestination(mate.transform.position);
			float timeMoving = 0f;
			bool gotAway = false;

			while ((navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance || timeMoving < 0.1f) && timeMoving < ScriptableAnimalStats.stamina) {
				navMeshAgent.SetDestination(mate.transform.position);

				timeMoving += Time.deltaTime;


				if (WithinMatingRange(mate)) {
					if (logChanges) {
						Debug.Log($"{gameObject.name}: Reached mate ({mate.gameObject.name})!");
					}
					SetMovementAnimation(false);
					moving = false;

					if (!isFemale) {
						MateWith(mate);
						//DecideNextState(false);
					} else {
						mate.MateWith(this);
					}

					//AttackAnimal(prey);
					yield break;
				}

				if (constainedToWanderZone && Vector3.Distance(transform.position, origin) > wanderZone) {
					gotAway = true;
					navMeshAgent.SetDestination(transform.position);
					break;
				}

				yield return null;
			}

			navMeshAgent.SetDestination(transform.position);

			SetMovementAnimation(false);

			if (timeMoving > ScriptableAnimalStats.stamina || mate.dead || gotAway) {
				BeginIdleState();
			} else {
				ApproachMate(mate);
			}
		}

		private bool WithinMatingRange(Animal mate) {
			return Vector3.Distance(transform.position, mate.transform.position) < 1f;
		}

		private void SetMovementAnimation(bool state = true) {
			if (!string.IsNullOrEmpty(movementStates[currentState].animationBool)) {
				animator.SetBool(movementStates[currentState].animationBool, state);
			}
		}

		private void MateWith(Animal mate) {
			StartCoroutine(TurnToLookAtTarget(mate.transform));
			StartCoroutine(mate.TurnToLookAtTarget(this.transform));
			if (logChanges) {
				Debug.Log($"{gameObject.name}: Mating with ({mate.gameObject.name})!");
			}
			mate.UpdateStateAfterMating();
			UpdateStateAfterMating();
			Animal animal = Instantiate(this, new Vector3(this.transform.position.x + 2, transform.position.y, transform.position.z + 2), Quaternion.identity);
			animal.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
		}

		private void UpdateStateAfterMating() {
			hunger -= 35;
			StopAllCoroutines();
			StopMoving();
			DecideNextState(false);

		}

		private void StopMoving() {
			if (moving) {
				if (useNavMesh) {
					navMeshAgent.SetDestination(transform.position);
				} else {
					targetLocation = transform.position;
				}

				SetMovementAnimation(false);
				moving = false;
			} else {
				if (idleStates.Length > 0 && !string.IsNullOrEmpty(idleStates[currentState].animationBool)) {
					animator.SetBool(idleStates[currentState].animationBool, false);
				}
			}
		}

		private IEnumerator BeApproachedBy(Animal mate) {
			Debug.Log($"{gameObject.name}: being approached by ({mate.gameObject.name})");
			while (Vector3.Distance(transform.position, mate.transform.position) > mateAwareness) {
				yield return new WaitForSeconds(0.5f);
				Debug.Log($"{gameObject.name}: waiting for ({mate.gameObject.name})");
			}

			StopAllCoroutines();
			StartCoroutine(TurnToLookAtTarget(mate.transform));
			StopMoving();

		}

		private SearchReport SearchForAnimals() {

			bool foundPredator = false, foundPrey = false, foundMate = false;
			int predatorIndex = -1, preyIndex = -1, mateIndex = -1;

			for (int i = 0; i < allAnimals.Count; i++) {
				// Check if it's a predator
				if (awareness > 0 && !foundPredator && IsPredator(GetAnimal(i))) {
					foundPredator = true;
					predatorIndex = i;

					// If a predator is found there is no need to continue
					return new SearchReport(i);

					// Check if it's a prey
				} else if (ScriptableAnimalStats.dominance > 0 && !foundPrey && IsPrey(GetAnimal(i))) {
					foundPrey = true;
					preyIndex = i;

					// Check if it's a mate
				} else if (ScriptableAnimalStats.reproduction > 0 && !foundMate && IsMate(GetAnimal(i))) {
					foundMate = true;
					mateIndex = i;
				}
			}
			return new SearchReport(predatorIndex, preyIndex, mateIndex);
		}

		private bool IsPredator(Animal potentialPredator) {
			return IsWithinRange(potentialPredator, awareness) && potentialPredator.CanAttack(this);
		}

		private bool IsPrey(Animal potentialPrey) {
			return CanAttack(potentialPrey)
				&& IsAggressiveTowards(potentialPrey)
				&& IsWithinRange(potentialPrey, scent)
				&& WillAttackDueToChance();
		}

		private bool CanAttack(Animal potentialPrey) {
			return !potentialPrey.dead && potentialPrey != this
				&& (potentialPrey.species != species || ScriptableAnimalStats.territorial)
				&& potentialPrey.ScriptableAnimalStats.dominance <= ScriptableAnimalStats.dominance
				&& !potentialPrey.ScriptableAnimalStats.stealthy;
		}

		private bool IsAggressiveTowards(Animal other) {
			int p = System.Array.IndexOf(nonAgressiveTowards, other.species);
			return p < 0;
		}

		private bool IsWithinRange(Animal animal, float range) {
			return Vector3.Distance(transform.position, animal.transform.position) <= range;
		}

		private bool WillAttackDueToChance() {
			return ScriptableAnimalStats.agression >= Random.Range(0, 100);
		}

		private bool CanMate() {
			return !dead && !attacking && hunger >= 80;
		}

		private bool IsMate(Animal potentialMate) {
			return CanMate() && potentialMate != this && species == potentialMate.species
				&& AnimalUtils.AreOppositeSex(this, potentialMate) && potentialMate.CanMate()
				&& IsWithinRange(potentialMate, mateAwareness) && WillMateDueToChance()
				&& potentialMate.WillMateDueToChance(); // because consent is mandatory
		}

		private bool WillMateDueToChance() {
			return ScriptableAnimalStats.reproduction >= Random.Range(0, 100);
		}

    public void SetProperties(bool isFemale, float mateAwareness, float hunger) {

    }

		class SearchReport {
			public int PredatorIndex { get; set; }
			public int PreyIndex { get; set; }
			public int MateIndex { get; set; }
			public bool FoundPredator => PredatorIndex > -1;
			public bool FoundPrey => PreyIndex > -1;
			public bool FoundMate => MateIndex > -1;

			public SearchReport(int predatorIndex = -1, int preyIndex = -1, int mateIndex = -1) {
				PredatorIndex = predatorIndex;
				PreyIndex = preyIndex;
				MateIndex = mateIndex;
			}

		}
	}



}
