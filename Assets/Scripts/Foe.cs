﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foe : AdvCard {

	protected int altBP;

	public Foe(string _name, int _BP, int _altBP, Sprite _sprite)
	{
		name = _name;
		type = "foe";
		BP = _BP;
		altBP = _altBP;
		sprite = _sprite;
	}
	public override int getAltBP() {
		return altBP;
	}
}
