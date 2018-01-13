using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICatcher : IOccupant {

	Vector3 BallOffset { get; }

	Action<Ball> OnCatch { get; }
}
