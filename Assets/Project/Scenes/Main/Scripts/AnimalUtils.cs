namespace Simulator {
	public static class AnimalUtils {
		public static int GetFastestMovementState(Animal animal) {
			int fastestMovementState = 0;
			for (int i = 0; i < animal.MovementStates.Length; i++) {
				if (animal.MovementStates[i].moveSpeed > animal.MovementStates[fastestMovementState].moveSpeed) {
					fastestMovementState = i;
				}
			}
			return fastestMovementState;
		}

    public static bool AreOppositeSex(Animal a, Animal b) {
      return a.IsFemale != b.IsFemale;
    }
	}
}

