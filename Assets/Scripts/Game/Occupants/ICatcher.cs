using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICatcher : IOccupant, IStats {

	Transform BallHolderObject { get; }

	Action<Ball> OnCatch { get; }

	Ball Ball { get; }
}
