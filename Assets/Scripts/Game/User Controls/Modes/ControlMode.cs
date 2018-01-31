using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMode {

	//probably fix this at some point. dont need both
	protected ControlModeEnum type;
	protected Action<Hex> onMouseOver;
	protected Action onMouseNotOverMap;
	protected Action<Hex> onLeftClick;
	protected Action<Hex> onRightClick;
	protected Action onTabPressed;
	protected Action onLeavingMode;
	protected Action onEnteringMode;

	public Action<Hex> OnMouseOver { get { return onMouseOver; } }
	public Action OnMouseNotOverMap { get { return onMouseNotOverMap; } } 
	public Action<Hex> OnLeftClick { get { return onLeftClick; } }
	public Action<Hex> OnRightClick  { get { return onRightClick; } }
	public Action OnTabPressed  { get { return onTabPressed; } }
	public Action OnEnteringMode { get { return onEnteringMode; } }
	public Action OnLeavingMode { get { return onLeavingMode; } }

	public bool AutoOnly { get; protected set;}
	public Func<bool> CheckValidity { get; protected set;}

	public ControlModeEnum ModeType { get; }

	public ControlMode(ControlModeEnum type, Action<Hex> onMouseOver, Action onMouseNotOverMap, Action<Hex> onLeftClick, Action<Hex> onRightClick, Action onTabPressed, Action onEnteringMode, Action onLeavingMode, bool autoOnly, Func<bool> checkValidity = null){
		this.type = type;
		this.onMouseOver = onMouseOver;
		this.onMouseNotOverMap = onMouseNotOverMap;
		this.onLeftClick = onLeftClick;
		this.onRightClick = onRightClick;
		this.onTabPressed = onTabPressed;
		this.onEnteringMode = onEnteringMode;
		this.onLeavingMode = onLeavingMode;
		this.AutoOnly = autoOnly;
		this.CheckValidity = checkValidity;
	}


}
