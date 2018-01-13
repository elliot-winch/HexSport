using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMode {

	protected Action<Hex> onMouseOver;
	protected Action onMouseNotOverMap;
	protected Action<Hex> onLeftClick;
	protected Action<Hex> onRightClick;
	protected Action onTabPressed;
	protected Action onModeChanged;

	public Action<Hex> OnMouseOver { get { return onMouseOver; } }
	public Action OnMouseNotOverMap { get { return onMouseNotOverMap; } } 
	public Action<Hex> OnLeftClick { get { return onLeftClick; } }
	public Action<Hex> OnRightClick  { get { return onRightClick; } }
	public Action OnTabPressed  { get { return onTabPressed; } }
	public Action OnModeChanged { get { return onModeChanged; } }

	public ControlMode(Action<Hex> onMouseOver, Action onMouseNotOverMap, Action<Hex> onLeftClick, Action<Hex> onRightClick, Action onTabPressed, Action onModeChanged){
		this.onMouseOver = onMouseOver;
		this.onMouseNotOverMap = onMouseNotOverMap;
		this.onLeftClick = onLeftClick;
		this.onRightClick = onRightClick;
		this.onTabPressed = onTabPressed;
		this.onModeChanged = onModeChanged;
	}
}
