﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : AdvCard {

	protected int bids;
	
	public Test(string _name, int _BP, int _bids, Sprite _sprite)
	{
		name = _name;
		type = "test";
		BP = _BP;
		bids = _bids;
		sprite = _sprite;
	}
	
	public override int getMinBid() {return bids;}
}
