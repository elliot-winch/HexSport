using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICatcher : IOccupant {

	Transform BallHolderObject { get; }

	Action<Ball> OnCatch { get; }
}
