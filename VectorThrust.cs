// ALWAYS CHECK FOR UPDATES
// to update, simply load it from the workshop tab again (no need to actually go to the workshop page)

// weather or not dampeners are on when you start the script
public bool dampeners = true;

// weather or not thrusters are on when you start the script
public bool jetpack = false;

// weather or not cruise mode is on when you start the script
public bool cruise = false;

// make cruise mode act more like an airplane
public const bool cruisePlane = false;

// this is used to identify blocks as belonging to this programmable block.
// pass the '%applyTags' argument, and the program will spread its tag across all blocks it controls.
// the program then won't touch any blocks that don't have the tag. unless you pass the '%removeTags' argument.
// if you add or remove a tag manually, pass the '%reset' argument to force a re-check
// if you make this tag unique to this ship, it won't interfere with your other vector thrust ships
public const string myName = "VT";
// normal: |VT|
public const string activeSurround = "|";
// standby: .VT.
public const string standbySurround = ".";

// put this in custom data of a cockpit to instruct the script to use a display in that cockpit
// it has to be on a line of its own, and have an integer after the ':'
// the integer must be 0 <= integer <= total # of displays in the cockpit
// eg:
//		%Vector:0
// this would make the script use the 1st display. the 1st display is #0, 2nd #1 etc..
// if you have trouble, look in the bottom right of the PB terminal, it will print errors there
public const string textSurfaceKeyword = "%Vector:";

// standby stops all calculations and safely turns off all nacelles, good if you want to stop flying
// but dont want to turn the craft off.
public const bool startInStandby = true;
// change this is you don't want the script to start in standby... please only use this if you have permission from the server owner

// set to -1 for the fastest speed in the game (changes with mods)
public const float maxRotorRPM = 60f;

public const float defaultAccel = 1f;//this is the default target acceleration you see on the display
// if you want to change the default, change this
// note, values higher than 1 will mean your nacelles will face the ground when you want to go
// down rather than just lower thrust
// '1g' is acceleration caused by current gravity (not nessicarily 9.81m/s) although
// if current gravity is less than 0.1m/s it will ignore this setting and be 9.81m/s anyway

public const float accelBase = 1.5f;//accel = defaultAccel * g * base^exponent
// your +, - and 0 keys increment, decrement and reset the exponent respectively
// this means increasing the base will increase the amount your + and - change target acceleration

// multiplier for dampeners, higher is stronger dampeners
public const float dampenersModifier = 0.1f;

// true: only main cockpit can be used even if there is no one in the main cockpit
// false: any cockpits can be used, but if there is someone in the main cockpit, it will only obey the main cockpit
// no main cockpit: any cockpits can be used
public const bool onlyMainCockpit = true;

// choose weather you want the script to update once every frame, once every 10 frames, or once every 100 frames
// should be 1 of:
// UpdateFrequency.Update1
// UpdateFrequency.Update10
// UpdateFrequency.Update100
public const UpdateFrequency update_frequency = UpdateFrequency.Update1;

public const string LCDName = "%VectorLCD";

// arguments, you can change these to change what text you run the programmable block with
public const string standbytogArg = "%standby";
public const string standbyonArg = "%standbyenter";
public const string standbyoffArg = "%standbyexit";
public const string dampenersArg = "%dampeners";
public const string cruiseArg = "%cruise";
public const string jetpackArg = "%jetpack";
public const string raiseAccelArg = "%raiseAccel";
public const string lowerAccelArg = "%lowerAccel";
public const string resetAccelArg = "%resetAccel";
public const string resetArg = "%reset";//this one re-runs the initial setup... you probably want to use %resetAccel
public const string applyTagsArg = "%applyTags";
public const string removeTagsArg = "%removeTags";

// control module gamepad bindings
// type "/cm showinputs" into chat
// press the desired button
// put that text EXACTLY as it is in the quotes for the control you want
public const string jetpackButton = "c.thrusts";
public const string dampenersButton = "c.damping";
public const string cruiseButton = "c.cubesizemode";
public const string lowerAccel = "c.switchleft";
public const string raiseAccel = "c.switchright";
public const string resetAccel = "pipe";

// boost settings (this only works with control module)
// you can use this to set target acceleration values that you can quickly jump to by holding down the specified button
// there are defaults here:
// 	c.sprint (shift)	3g
// 	ctrl 			0.3g
public const bool useBoosts = true;
public BA[] boosts = {
	new BA("c.sprint", 3f),
	new BA("ctrl", 0.3f)
};



public struct BA {
	public string button;
	public float accel;

	public BA(string button, float accel) {
		this.button = button;
		this.accel = accel;
	}
}



// default acceleration in situations with 0 (or low) gravity
public const float zeroGAcceleration = 9.81f;
// if gravity becomes less than this, zeroGAcceleration will kick in
public const float gravCutoff = 0.1f * 9.81f;


// DEPRECATED: use the tags instead
// only use blocks that have 'show in terminal' set to true
public const bool ignoreHiddenBlocks = false;







//                              V 180 degrees
//              V 0 degrees                      V 360 degrees
// 				|-----\                    /------
// desired power|----------------------------------------- value of 0.1
// 				|       \                /
// 				|        \              /
// 				|         \            /
// no power 	|-----------------------------------------
//
//
// 				|-----\                    /------stuff above desired power gets set to desired power
// 				|      \                  /
// 				|       \                /
// desired power|----------------------------------------- value of 0.8
// 				|         \            /
// no power 	|-----------------------------------------

// the above pictures are for 'thrustModifierAbove', the same principle applies for 'thrustModifierBelow', except it goes below the 0 line, instead of above the max power line.
// the clipping value 'thrustModifier' defines how far the thruster can be away from the desired direction of thrust, and have the power still at desired power, otherwise it will be less
// these values can only be between 0 and 1
public double thrustModifierAbove = 0.1;// how close the rotor has to be to target position before the thruster gets to full power
public double thrustModifierBelow = 0.1;// how close the rotor has to be to opposite of target position before the thruster gets to 0 power


public const double thrustModifierAboveSpace = 0.01;
public const double thrustModifierBelowSpace = 0.9;

public const double thrustModifierAboveGrav = 0.1;
public const double thrustModifierBelowGrav = 0.1; 

// use control module... this can always be true
public bool controlModule = true;

// remove unreachable code warning
#pragma warning disable 0162

public Program() {
	Echo("Just Compiled");
	programCounter = 0;
	gotNacellesCount = 0;
	updateNacellesCount = 0;
	Runtime.UpdateFrequency = UpdateFrequency.Once;
	this.greedy = !hasTag(Me);
	if(Me.CustomData.Equals("")) {
		Me.CustomData = textSurfaceKeyword + 0;
	}
}
public void Save() {}


//at 60 fps this will last for 9000+ hrs before going negative
public long programCounter;
public long gotNacellesCount;
public long updateNacellesCount;

public void Main(string argument, UpdateType runType) {











	// ========== STARTUP ==========
	globalAppend = false;

	programCounter++;
	Echo($"Last Runtime {Runtime.LastRunTimeMs.Round(2)}ms");
	String spinner = "";
	switch(programCounter/10%4) {
		case 0:
			spinner = "|";
		break;
		case 1:
			spinner = "\\";
		break;
		case 2:
			spinner = "-";
		break;
		case 3:
			spinner = "/";
		break;
	}
	write($"{spinner} {Runtime.LastRunTimeMs.Round(0)}ms");


	// only accept arguments on certain update types
	UpdateType valid_argument_updates = UpdateType.None;
	valid_argument_updates |= UpdateType.Terminal;
	valid_argument_updates |= UpdateType.Trigger;
	valid_argument_updates |= UpdateType.Antenna;
	// valid_argument_updates |= UpdateType.Mod;
	valid_argument_updates |= UpdateType.Script;
	// valid_argument_updates |= UpdateType.Update1;
	// valid_argument_updates |= UpdateType.Update10;
	// valid_argument_updates |= UpdateType.Update100;
	if((runType & valid_argument_updates) == UpdateType.None) {
		// runtype is not one that is allowed to give arguments
		argument = "";
	}
	Echo("Greedy: " + this.greedy);

	// Echo("Starting Main");
	argument = argument.ToLower();
	bool togglePower = argument.Contains(standbytogArg.ToLower());

	bool anyArg =
	argument.Contains(dampenersArg.ToLower()) ||
	argument.Contains(cruiseArg.ToLower()) ||
	argument.Contains(jetpackArg.ToLower()) ||
	argument.Contains(standbytogArg.ToLower()) ||
	argument.Contains(raiseAccelArg.ToLower()) ||
	argument.Contains(lowerAccelArg.ToLower()) ||
	argument.Contains(resetAccelArg.ToLower()) ||
	argument.Contains(resetArg.ToLower()) ||
	argument.Contains(applyTagsArg.ToLower()) ||
	argument.Contains(removeTagsArg.ToLower());

	// set standby mode on
	if(argument.Contains(standbyonArg.ToLower()) || goToStandby) {
		enterStandby();
	        return;
	// set standby mode off
	} else if(argument.Contains(standbyoffArg.ToLower()) || comeFromStandby) {
		exitStandby();
		return;
	// going into standby mode toggle
	} else if((togglePower && !standby) || goToStandby) {
		enterStandby();
		return;
	// coming back from standby mode toggle
	} else if((anyArg || runType == UpdateType.Terminal) && standby || comeFromStandby) {
		exitStandby();
	} else {
		Echo("Normal Running");
	}

	if(justCompiled || controllers.Count == 0 || argument.Contains(resetArg.ToLower())) {
		if(!init()) {
			return;
		}
	}



	//tags and getting blocks
	this.applyTags = argument.Contains(Program.applyTagsArg.ToLower());
	this.removeTags = !this.applyTags && argument.Contains(Program.removeTagsArg.ToLower());
	// switch on: removeTags
	// switch off: applyTags
	this.greedy = (!this.applyTags && this.greedy) || this.removeTags;
	// this automatically calls getNacelles() as needed, and passes in previous GTS data
	if(this.applyTags) {
		addTag(Me);
	} else if(this.removeTags) {
		removeTag(Me);
	}
	if(!checkNacelles()) {
		Echo("Setup failed, stopping.");
		return;
	}
	this.applyTags = false;
	this.removeTags = false;




	if(justCompiled) {
		justCompiled = false;
		Runtime.UpdateFrequency = UpdateFrequency.Once;
		if(Storage == "" || !startInStandby) {
			Storage = "Don't Start Automatically";
			// run normally
			comeFromStandby = true;
			return;
		} else {

			// go into standby mode
			goToStandby = true;
			return;
		}
	}

	if(standby) {
		Echo("Standing By");
		write("Standing By");
		return;
	}

	// ========== END OF STARTUP ==========










	// ========== PHYSICS ==========

 	// get gravity in world space
	Vector3D worldGrav = usableControllers[0].GetNaturalGravity();

	// get velocity
	MyShipVelocities shipVelocities = usableControllers[0].GetShipVelocities();
	shipVelocity = shipVelocities.LinearVelocity;
	// Vector3D shipAngularVelocity = shipVelocities.AngularVelocity;

	// setup mass
	MyShipMass myShipMass = usableControllers[0].CalculateShipMass();
	float shipMass = myShipMass.PhysicalMass;

	if(myShipMass.BaseMass < 0.001f) {
		Echo("Can't fly a Station");
		shipMass = 0.001f;
	}

	// setup gravity
	float gravLength = (float)worldGrav.Length();
	if(gravLength < gravCutoff) {
		gravLength = zeroGAcceleration;
		thrustModifierAbove = thrustModifierAboveSpace;
		thrustModifierBelow = thrustModifierBelowSpace;
	}
	else {
		thrustModifierAbove = thrustModifierAboveGrav;
		thrustModifierBelow = thrustModifierBelowGrav;
	}

	Vector3D desiredVec = getMovementInput(argument);

	// f=ma
	Vector3D shipWeight = shipMass * worldGrav;



	if(dampeners) {
		Vector3D dampVec = Vector3D.Zero;


		if(desiredVec != Vector3D.Zero) {
			// cancel movement opposite to desired movement direction
			if(desiredVec.dot(shipVelocity) < 0) {
				//if you want to go oppisite to velocity
				dampVec += shipVelocity.project(desiredVec.normalized());
			}
			// cancel sideways movement
			dampVec += shipVelocity.reject(desiredVec.normalized());
		} else {
			dampVec += shipVelocity;
		}



		if(cruise) {

			foreach(IMyShipController cont in usableControllers) {
				if(onlyMain() && cont != mainController) continue;
				if(!cont.IsUnderControl) continue;


				if(dampVec.dot(cont.WorldMatrix.Forward) > 0 || cruisePlane) { // only front, or front+back if cruisePlane is activated
					dampVec -= dampVec.project(cont.WorldMatrix.Forward);
				}

				if(cruisePlane) {
					shipWeight -= shipWeight.project(cont.WorldMatrix.Forward);
				}
			}
		}


		desiredVec -= dampVec * dampenersModifier;
	}



	// f=ma
	desiredVec *= shipMass * (float)getAcceleration(gravLength);

	// point thrust in opposite direction, add weight. this is force, not acceleration
	Vector3D requiredVec = -desiredVec + shipWeight;

	// remove thrust done by normal thrusters
	for(int i = 0; i < normalThrusters.Count; i++) {
		requiredVec -= -1 * normalThrusters[i].WorldMatrix.Backward * normalThrusters[i].CurrentThrust;
		// Echo($"{normalThrusters[i].CustomName}: {Vector3D.TransformNormal(normalThrusters[i].CurrentThrust * normalThrusters[i].WorldMatrix.Backward, MatrixD.Invert(normalThrusters[i].WorldMatrix))}");
		// write($"{normalThrusters[i].CustomName}: \n{Vector3D.TransformNormal(normalThrusters[i].CurrentThrust * normalThrusters[i].WorldMatrix.Backward, MatrixD.Invert(normalThrusters[i].WorldMatrix))}");
	}

	Echo("Required Force: " + $"{Math.Round(requiredVec.Length(),0)}" + "N");

	// ========== END OF PHYSICS ==========









	// ========== DISTRIBUTE THE FORCE EVENLY BETWEEN NACELLES ==========

	// update thrusters on/off and re-check nacelles direction
	bool gravChanged = Math.Abs(lastGrav - gravLength) > 0.05f;
	lastGrav = gravLength;
	foreach(Nacelle n in nacelles) {
		// we want to update if the thrusters are not valid, or atmosphere has changed
		if(!n.validateThrusters(jetpack) || gravChanged) {
			n.detectThrustDirection();
		}
		// Echo($"thrusters: {n.thrusters.Count}");
		// Echo($"avaliable: {n.availableThrusters.Count}");
		// Echo($"active: {n.activeThrusters.Count}");
	}

	/* TOOD: redo this */
	// group similar nacelles (rotor axis is same direction)
	List<List<Nacelle>> nacelleGroups = new List<List<Nacelle>>();
	for(int i = 0; i < nacelles.Count; i++) {
		bool foundGroup = false;
		foreach(List<Nacelle> g in nacelleGroups) {// check each group to see if its lined up
			if(Math.Abs(Vector3D.Dot(nacelles[i].rotor.theBlock.WorldMatrix.Up, g[0].rotor.theBlock.WorldMatrix.Up)) > 0.9f) {
				g.Add(nacelles[i]);
				foundGroup = true;
				break;
			}
		}
		if(!foundGroup) {// if it never found a group, add a group
			nacelleGroups.Add(new List<Nacelle>());
			nacelleGroups[nacelleGroups.Count-1].Add(nacelles[i]);
		}
	}

	// correct for misaligned nacelles
	Vector3D asdf = Vector3D.Zero;
	// 1
	foreach(List<Nacelle> g in nacelleGroups) {
		g[0].requiredVec = requiredVec.reject(g[0].rotor.theBlock.WorldMatrix.Up);
		asdf += g[0].requiredVec;
	}
	// 2
	asdf -= requiredVec;
	// 3
	foreach(List<Nacelle> g in nacelleGroups) {
		g[0].requiredVec -= asdf;
	}
	// 4
	asdf /= nacelleGroups.Count;
	// 5
	foreach(List<Nacelle> g in nacelleGroups) {
		g[0].requiredVec += asdf;
	}
	// apply first nacelle settings to rest in each group
	double total = 0;
	foreach(List<Nacelle> g in nacelleGroups) {
		Vector3D req = g[0].requiredVec / g.Count;
		for(int i = 0; i < g.Count; i++) {
			g[i].requiredVec = req;
			g[i].thrustModifierAbove = thrustModifierAbove;
			g[i].thrustModifierBelow = thrustModifierBelow;
			// Echo(g[i].errStr);
			g[i].go(jetpack, dampeners, shipMass);
			total += req.Length();
			// write($"nacelle {i} avail: {g[i].availableThrusters.Count} updates: {g[i].detectThrustCounter}");
			// write(g[i].errStr);
			// foreach(Thruster t in g[i].activeThrusters) {
			// 	// Echo($"Thruster: {t.theBlock.CustomName}\n{t.errStr}");
			// }
		}
	}/* end of TODO */
	Echo("Total Force: " + $"{Math.Round(total,0)}" + "N");




	write("Target Accel: " + Math.Round(getAcceleration(gravLength)/gravLength, 2) + "g");
	write("Thrusters: " + jetpack);
	write("Dampeners: " + dampeners);
	write("Cruise: " + cruise);
	write("Active Nacelles: " + nacelles.Count);//TODO: make activeNacelles account for the number of nacelles that are actually active (activeThrusters.Count > 0)
	// write("Got Nacelles: " + gotNacellesCount);
	// write("Update Nacelles: " + updateNacellesCount);
	// ========== END OF MAIN ==========



	// echo the errors with surface provider
	Echo(surfaceProviderErrorStr);
}


public string surfaceProviderErrorStr = "";

public int accelExponent = 0;

public bool jetpackIsPressed = false;
public bool dampenersIsPressed = false;
public bool cruiseIsPressed = false;
public bool plusIsPressed = false;
public bool minusIsPressed = false;

public bool globalAppend = false;

public IMyShipController mainController = null;
public List<IMyShipController> controllers = new List<IMyShipController>();
public List<IMyShipController> usableControllers = new List<IMyShipController>();
public List<Nacelle> nacelles = new List<Nacelle>();
public List<IMyThrust> normalThrusters = new List<IMyThrust>();
public List<IMyTextPanel> screens = new List<IMyTextPanel>();
public List<IMyTextPanel> usableScreens = new List<IMyTextPanel>();
public HashSet<IMyTextSurface> surfaces = new HashSet<IMyTextSurface>();
public List<IMyProgrammableBlock> programBlocks = new List<IMyProgrammableBlock>();

public float oldMass = 0;

public int rotorCount = 0;
public int rotorTopCount = 0;
public int thrusterCount = 0;
public int screenCount = 0;
public int programBlockCount = 0;


public bool standby = startInStandby;
public Vector3D shipVelocity = Vector3D.Zero;

public bool justCompiled = true;
public bool goToStandby = false;
public bool comeFromStandby = false;

public const string tag = activeSurround + myName + activeSurround;
public const string offtag = standbySurround + myName + standbySurround;

public bool applyTags = false;
public bool removeTags = false;
public bool greedy = true;
public float lastGrav = 0;

public Dictionary<string, object> CMinputs = null;






public void enterStandby() {
	standby = true;
	goToStandby = false;

	//set status of blocks
	foreach(Nacelle n in nacelles) {
		n.rotor.theBlock.Enabled = false;
		standbyTag(n.rotor.theBlock);
		foreach(Thruster t in n.thrusters) {
			t.theBlock.Enabled = false;
			standbyTag(t.theBlock);
		}
	}
	foreach(IMyTextPanel screen in usableScreens) {
		standbyTag(screen);
	}
	foreach(IMyShipController cont in usableControllers) {
		standbyTag(cont);
	}
	standbyTag(Me);

	Runtime.UpdateFrequency = UpdateFrequency.None;

	Echo("Standing By");
	write("Standing By");
}

public void exitStandby() {
	standby = false;
	comeFromStandby = false;

	//set status of blocks
	foreach(Nacelle n in nacelles) {
		n.rotor.theBlock.Enabled = true;
		activeTag(n.rotor.theBlock);
		foreach(Thruster t in n.thrusters) {
			if(t.IsOn) {
				t.theBlock.Enabled = true;
			}
			activeTag(t.theBlock);
		}
	}
	foreach(IMyTextPanel screen in usableScreens) {
		activeTag(screen);
	}
	foreach(IMyShipController cont in usableControllers) {
		activeTag(cont);
	}
	activeTag(Me);

	Runtime.UpdateFrequency = update_frequency;
}

public bool hasTag(IMyTerminalBlock block) {
	return block.CustomName.Contains(tag) || block.CustomName.Contains(offtag);
}

public void addTag(IMyTerminalBlock block) {
	string name = block.CustomName;

	if(name.Contains(tag)) {
		// there is already a tag, just set it to current status
		if(standby) {
			block.CustomName = name.Replace(tag, offtag);
		}

	} else if(name.Contains(offtag)) {
		// there is already a tag, just set it to current status
		if(!standby) {
			block.CustomName = name.Replace(offtag, tag);
		}

	} else {
		// no tag found, add tag to start of string

		if(standby) {
			block.CustomName = offtag + " " + name;
		} else {
			block.CustomName = tag + " " + name;
		}
	}

}

public void removeTag(IMyTerminalBlock block) {
	block.CustomName = block.CustomName.Replace(tag, "").Trim();
	block.CustomName = block.CustomName.Replace(offtag, "").Trim();
}

public void standbyTag(IMyTerminalBlock block) {
	block.CustomName = block.CustomName.Replace(tag, offtag);
}

public void activeTag(IMyTerminalBlock block) {
	block.CustomName = block.CustomName.Replace(offtag, tag);
}


// true: only main cockpit can be used even if there is no one in the main cockpit
// false: any cockpits can be used, but if there is someone in the main cockpit, it will only obey the main cockpit
// no main cockpit: any cockpits can be used
public bool onlyMain() {
	return mainController != null && (mainController.IsUnderControl || onlyMainCockpit);
}

public void getScreens() {
	getScreens(this.screens);
}

public void getScreens(List<IMyTextPanel> screens) {
	bool greedy = this.greedy || this.applyTags || this.removeTags;
	this.screens = screens;
	usableScreens.Clear();
	foreach(IMyTextPanel screen in screens) {
		bool continue_ = false;

		if(this.removeTags) {
			removeTag(screen);
		}

		if(!greedy && !hasTag(screen)) { continue_ = true; }
		if(!screen.IsWorking) continue_ = true;
		if(!hasTag(screen) && !screen.CustomName.ToLower().Contains(LCDName.ToLower())) continue_ = true;

		if(continue_) {
			surfaces.Remove(screen);
			continue;
		}
		if(this.applyTags) {
			addTag(screen);
		}
		usableScreens.Add(screen);
		surfaces.Add(screen);
	}
	screenCount = screens.Count;
}

public void write(string str) {
	if(this.surfaces.Count > 0) {
		str += "\n";
		foreach(IMyTextSurface surface in this.surfaces) {
			surface.WriteText(str, globalAppend);
			surface.ContentType = ContentType.TEXT_AND_IMAGE;
		}
	} else if(!globalAppend) {
		Echo("No text surfaces available");
	}
	globalAppend = true;
}

double getAcceleration(double gravity) {
	// look through boosts, applies acceleration of first one found
	if(Program.useBoosts && this.controlModule) {
		for(int i = 0; i < this.boosts.Length; i++) {
			if(this.CMinputs.ContainsKey(this.boosts[i].button)) {
				return this.boosts[i].accel * gravity * defaultAccel;
			}
		}
	}

	//none found or boosts not enabled, go for normal accel
	return Math.Pow(accelBase, accelExponent) * gravity * defaultAccel;
}

public Vector3D getMovementInput(string arg) {
	Vector3D moveVec = Vector3D.Zero;

	if(controlModule) {
		// setup control module
		Dictionary<string, object> inputs = new Dictionary<string, object>();
		try {
			this.CMinputs = Me.GetValue<Dictionary<string, object>>("ControlModule.Inputs");
			Me.SetValue<string>("ControlModule.AddInput", "all");
			Me.SetValue<bool>("ControlModule.RunOnInput", true);
			Me.SetValue<int>("ControlModule.InputState", 1);
			Me.SetValue<float>("ControlModule.RepeatDelay", 0.016f);
		} catch(Exception e) {
			controlModule = false;
		}
	}

	if(controlModule) {
		// non-movement controls
		if(this.CMinputs.ContainsKey(dampenersButton) && !dampenersIsPressed) {//inertia dampener key
			dampeners = !dampeners;//toggle
			dampenersIsPressed = true;
		}
		if(!this.CMinputs.ContainsKey(dampenersButton)) {
			dampenersIsPressed = false;
		}


		if(this.CMinputs.ContainsKey(cruiseButton) && !cruiseIsPressed) {//cruise key
			cruise = !cruise;//toggle
			cruiseIsPressed = true;
		}
		if(!this.CMinputs.ContainsKey(cruiseButton)) {
			cruiseIsPressed = false;
		}

		if(this.CMinputs.ContainsKey(jetpackButton) && !jetpackIsPressed) {//jetpack key
			jetpack = !jetpack;//toggle
			jetpackIsPressed = true;
		}
		if(!this.CMinputs.ContainsKey(jetpackButton)) {
			jetpackIsPressed = false;
		}

		if(this.CMinputs.ContainsKey(raiseAccel) && !plusIsPressed) {//throttle up
			accelExponent++;
			plusIsPressed = true;
		}
		if(!this.CMinputs.ContainsKey(raiseAccel)) { //increase target acceleration
			plusIsPressed = false;
		}

		if(this.CMinputs.ContainsKey(lowerAccel) && !minusIsPressed) {//throttle down
			accelExponent--;
			minusIsPressed = true;
		}
		if(!this.CMinputs.ContainsKey(lowerAccel)) { //lower target acceleration
			minusIsPressed = false;
		}

		if(this.CMinputs.ContainsKey(resetAccel)) { //default target acceleration
			accelExponent = 0;
		}

	}

	bool changeDampeners = false;
	if(arg.Contains(dampenersArg.ToLower())) {
		dampeners = !dampeners;
		changeDampeners	= true;
	}
	if(arg.Contains(cruiseArg.ToLower())) {
		cruise = !cruise;
	}
	if(arg.Contains(jetpackArg.ToLower())) {
		jetpack = !jetpack;
	}
	if(arg.Contains(raiseAccelArg.ToLower())) {
		accelExponent++;
	}
	if(arg.Contains(lowerAccelArg.ToLower())) {
		accelExponent--;
	}
	if(arg.Contains(resetAccelArg.ToLower())) {
		accelExponent = 0;
	}

	// dampeners (if there are any normal thrusters, the dampeners control works)
	if(normalThrusters.Count != 0) {

		if(onlyMain()) {

			if(changeDampeners) {
				mainController.DampenersOverride = dampeners;
			} else {
				dampeners = mainController.DampenersOverride;
			}
		} else {

			// dampeners = false;
			foreach(IMyShipController cont in controllers) {
				if(cont.DampenersOverride && cont.IsUnderControl) {
					if(changeDampeners) {
						cont.DampenersOverride = false;
						continue;
					}
					dampeners = true;
				}
			}
		}
	}


	// movement controls
	if(onlyMain()) {
		moveVec = mainController.getWorldMoveIndicator();
	} else {
		foreach(IMyShipController cont in controllers) {
			if(cont.IsUnderControl) {
				moveVec += cont.getWorldMoveIndicator();
			}
		}
	}

	return moveVec;
}

void removeSurface(IMyTextSurface surface) {
	if(this.surfaces.Contains(surface)) {
		//need to check this, because otherwise it will reset panels
		//we aren't controlling
		this.surfaces.Remove(surface);
		surface.ContentType = ContentType.NONE;
		surface.WriteText("", false);
	}
}

bool removeSurfaceProvider(IMyTerminalBlock block) {
	if(!(block is IMyTextSurfaceProvider)) return false;
	IMyTextSurfaceProvider provider = (IMyTextSurfaceProvider)block;

	for(int i = 0; i < provider.SurfaceCount; i++) {
		if(surfaces.Contains(provider.GetSurface(i))) {
			removeSurface(provider.GetSurface(i));
		}
	}
	return true;
}
bool addSurfaceProvider(IMyTerminalBlock block) {
	if(!(block is IMyTextSurfaceProvider)) return false;
	IMyTextSurfaceProvider provider = (IMyTextSurfaceProvider)block;
	bool retval = true;

	if(block.CustomData.Length == 0) {
		return false;
	}

	bool [] to_add = new bool[provider.SurfaceCount];
	for(int i = 0; i < to_add.Length; i++) {
		to_add[i] = false;
	}

	int begin_search = 0;
	while(begin_search >= 0) {
		string data = block.CustomData;
		int start = data.IndexOf(textSurfaceKeyword, begin_search);

		if(start < 0) {
			// true if it found at least 1
			retval =  begin_search != 0;
			break;
		}
		int end = data.IndexOf("\n", start);
		begin_search = end;

		string display = "";
		if(end < 0) {
			display = data.Substring(start + textSurfaceKeyword.Length);
		} else {
			display = data.Substring(start + textSurfaceKeyword.Length, end - (start + textSurfaceKeyword.Length) );
		}

		int display_num = 0;
		if(Int32.TryParse(display, out display_num)) {
			if(display_num >= 0 && display_num < provider.SurfaceCount) {
				// it worked, add the surface
				to_add[display_num] = true;

			} else {
				// range check failed
				string err_str = "";
				if(end < 0) {
					err_str = data.Substring(start);
				} else {
					err_str = data.Substring(start, end - (start) );
				}
				surfaceProviderErrorStr += $"\nDisplay number out of range: {display_num}\nshould be: 0 <= num < {provider.SurfaceCount}\non line: ({err_str})\nin block: {block.CustomName}\n";
			}

		} else {
			//didn't parse
			string err_str = "";
			if(end < 0) {
				err_str = data.Substring(start);
			} else {
				err_str = data.Substring(start, end - (start) );
			}
			surfaceProviderErrorStr += $"\nDisplay number invalid: {display}\non line: ({err_str})\nin block: {block.CustomName}\n";
		}
	}

	for(int i = 0; i < to_add.Length; i++) {
		if(to_add[i]) {
			this.surfaces.Add(provider.GetSurface(i));
		} else {
			removeSurface(provider.GetSurface(i));
		}
	}


	return retval;
}

bool getControllers() {
	return getControllers(this.controllers);
}

bool getControllers(List<IMyShipController> blocks) {
	bool greedy = this.greedy || this.applyTags || this.removeTags;
	mainController = null;
	this.controllers = blocks;

	usableControllers.Clear();

	string reason = "";
	for(int i = 0; i < blocks.Count; i++) {
		bool canAdd = true;
		string currreason = blocks[i].CustomName + "\n";
		if(!blocks[i].ShowInTerminal && ignoreHiddenBlocks) {
			currreason += "  ShowInTerminal not set\n";
			canAdd = false;
		}
		if(!blocks[i].CanControlShip) {
			currreason += "  CanControlShip not set\n";
			canAdd = false;
		}
		if(!blocks[i].ControlThrusters) {
			currreason += "  can't ControlThrusters\n";
			canAdd = false;
		}
		if(blocks[i].IsMainCockpit) {
			mainController = blocks[i];
		}
		if(!(greedy || hasTag(blocks[i]))) {
			currreason += "  Doesn't match my tag\n";
			canAdd = false;
		}
		if(this.removeTags) {
			removeTag(blocks[i]);
		}

		if(canAdd) {
			addSurfaceProvider(blocks[i]);
			usableControllers.Add(blocks[i]);
			if(this.applyTags) {
				addTag(blocks[i]);
			}
		} else {
			removeSurfaceProvider(blocks[i]);
			reason += currreason;
		}
	}
	if(blocks.Count == 0) {
		reason += "no controllers\n";
	}

	if(usableControllers.Count == 0) {
		Echo("ERROR: no usable ship controller found. Reason: \n");
		Echo(reason);
		return false;
	}

	controllers = blocks;
	return true;
}

public IMyShipController findACockpit() {
	foreach(IMyShipController cont in usableControllers) {
		if(cont.IsWorking) {
			return cont;
		}
	}

	return null;
}

// checks to see if the nacelles have changed
public bool checkNacelles() {
	Echo("Checking Nacelles..");

	bool greedy = this.applyTags || this.removeTags;

	IMyShipController cont = findACockpit();
	if(cont == null) {
		Echo("No cockpit registered, checking everything.");
	} else if(!greedy) {
		MyShipMass shipmass = cont.CalculateShipMass();
		if(this.oldMass == shipmass.BaseMass) {
			Echo("Mass is the same, everything is good.");

			// they may have changed the screen name to be a VT one
			getControllers();
			getScreens();
			return true;
		}
		Echo("Mass is different, checking everything.");
		this.oldMass = shipmass.BaseMass;
		// surface may be exploded if mass changes, in this case, ghost surfaces my be left behind
		this.surfaces.Clear();
	}

	List<IMyShipController> conts = new List<IMyShipController>();
	List<IMyMotorStator> rots = new List<IMyMotorStator>();
	List<IMyThrust> thrs = new List<IMyThrust>();
	List<IMyTextPanel> txts = new List<IMyTextPanel>();
	List<IMyProgrammableBlock> programBlocks = new List<IMyProgrammableBlock>();

	if(true) {//artificial scope :)
		List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
		GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
		for(int i = 0; i < blocks.Count; i++) {
			if(blocks[i] is IMyShipController) {
				conts.Add((IMyShipController)blocks[i]);
			}
			if(blocks[i] is IMyMotorStator) {
				rots.Add((IMyMotorStator)blocks[i]);
			}
			if(blocks[i] is IMyThrust) {
				thrs.Add((IMyThrust)blocks[i]);
			}
			if(blocks[i] is IMyTextPanel) {
				txts.Add((IMyTextPanel)blocks[i]);
			}
			if(blocks[i] is IMyProgrammableBlock) {
				programBlocks.Add((IMyProgrammableBlock)blocks[i]);
			}
		}
	}

	if(Me.SurfaceCount > 0) {
		surfaceProviderErrorStr = "";
		Me.CustomData = textSurfaceKeyword + 0;
		addSurfaceProvider(Me);
		Me.GetSurface(0).FontSize = 2.2f;// this isn't really the right place to put this, but doing it right would be a lot more code
	}

	bool updateNacelles = false;

	// if you use the following if statement, it won't lock the non-main cockpit if someone sets the main cockpit, until a recompile or world load :/
	if(/*(mainController != null ? !mainController.IsMainCockpit : false) || */controllers.Count != conts.Count || cont == null || greedy) {
		Echo($"Controller count ({controllers.Count}) is out of whack (current: {conts.Count})");
		if(!getControllers(conts)) {
			return false;
		}
	}

	if(screenCount != txts.Count || greedy) {
		Echo($"Screen count ({screenCount}) is out of whack (current: {txts.Count})");
		getScreens(txts);
	} else {
		//probably may-aswell just getScreens either way. seems like there wouldn't be much performance hit
		foreach(IMyTextPanel screen in txts) {
			if(!screen.IsWorking) continue;
			if(!screen.CustomName.ToLower().Contains(LCDName.ToLower())) continue;
			getScreens(txts);
		}
	}

	if(rotorCount != rots.Count) {
		Echo($"Rotor count ({rotorCount}) is out of whack (current: {rots.Count})");
		updateNacelles = true;
	}

	var rotorHeads = new List<IMyAttachableTopBlock>();
	foreach(IMyMotorStator rotor in rots) {
		if(rotor.Top != null) {
			rotorHeads.Add(rotor.Top);
		}
	}
	if(rotorTopCount != rotorHeads.Count) {
		Echo($"Rotor Head count ({rotorTopCount}) is out of whack (current: {rotorHeads.Count})");
		Echo($"Rotors: {rots.Count}");
		updateNacelles = true;
	}

	if(thrusterCount != thrs.Count) {
		Echo($"Thruster count ({thrusterCount}) is out of whack (current: {thrs.Count})");
		updateNacelles = true;
	}


	if(updateNacelles || greedy) {
		Echo("Updating Nacelles");
		getNacelles(rots, thrs);
	} else {
		Echo("They seem fine.");
	}

	return true;
}

public bool init() {
	Echo("Initialising..");
	getNacelles();
	List<IMyShipController> conts = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(conts);
	if(!getControllers(conts)) {
		Echo("Init failed.");
		return false;
	}
	Echo("Init success.");
	return true;
}

//addTag(IMyTerminalBlock block)
//removeTag(IMyTerminalBlock block)
//standbyTag(IMyTerminalBlock block)
//activeTag(IMyTerminalBlock block)

// gets all the rotors and thrusters
void getNacelles() {
	var blocks = new List<IMyTerminalBlock>();

	// 1 call to GTS
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, block => (block is IMyThrust) || (block is IMyMotorStator));

	Echo("Getting Blocks for thrusters & rotors");
	// get the blocks we care about
	var rotors = new List<IMyMotorStator>();
	normalThrusters.Clear();
	// var thrusters = new List<IMyThrust>();
	for(int i = blocks.Count-1; i >= 0; i--) {
		if(blocks[i] is IMyThrust) {
			normalThrusters.Add((IMyThrust)blocks[i]);
		} else /*if(blocks[i] is IMyMotorStator) */{
			rotors.Add((IMyMotorStator)blocks[i]);
		}
		blocks.RemoveAt(i);
	}
	rotorCount = rotors.Count;
	thrusterCount = normalThrusters.Count;

	getNacelles(rotors, normalThrusters);
}
void getNacelles(List<IMyMotorStator> rotors, List<IMyThrust> thrusters) {
	bool greedy = this.applyTags || this.removeTags || this.greedy;
	gotNacellesCount++;
	this.nacelles.Clear();





	Echo("Getting Rotors");
	// make this.nacelles out of all valid rotors
	rotorTopCount = 0;
	foreach(IMyMotorStator current in rotors) {
		if(this.removeTags) {
			removeTag(current);
		} else if(this.applyTags) {
			addTag(current);
		}


		if(!(greedy || hasTag(current))) { continue; }

		if(current.Top == null) {
			continue;
		} else {
			rotorTopCount++;
		}

		//if topgrid is not programmable blocks grid
		if(current.TopGrid == Me.CubeGrid) {
			continue;
		}

		// it's not set to not be a nacelle rotor
		// it's topgrid is not the programmable blocks grid
		Rotor rotor = new Rotor(current);
		this.nacelles.Add(new Nacelle(rotor, this));
	}

	Echo("Getting Thrusters");
	// add all thrusters to their corrisponding nacelle and remove this.nacelles that have none
	for(int i = this.nacelles.Count-1; i >= 0; i--) {
		for(int j = thrusters.Count-1; j >= 0; j--) {
			if(!(greedy || hasTag(thrusters[j]))) { continue; }
			if(this.removeTags) {
				removeTag(thrusters[j]);
			}

			if(thrusters[j].CubeGrid != this.nacelles[i].rotor.theBlock.TopGrid) continue;// thruster is not for the current nacelle
			// if(!thrusters[j].IsFunctional) continue;// broken, don't add it

			if(this.applyTags) {
				addTag(thrusters[j]);
			}

			this.nacelles[i].thrusters.Add(new Thruster(thrusters[j]));
			thrusters.RemoveAt(j);// shorten the list we have to check
		}
		// remove this.nacelles (rotors) without thrusters
		if(this.nacelles[i].thrusters.Count == 0) {
			removeTag(this.nacelles[i].rotor.theBlock);
			this.nacelles.RemoveAt(i);// there is no more reference to the rotor, should be garbage collected
			continue;
		}
		// if its still there, setup the nacelle
		this.nacelles[i].validateThrusters(jetpack);
		this.nacelles[i].detectThrustDirection();
	}

}

public float lerp(float a, float b, float cutoff) {
	float percent = a/b;
	percent -= cutoff;
	percent *= 1/(1-cutoff);
	if(percent > 1) {
		percent = 1;
	}
	if(percent < 0) {
		percent = 0;
	}
	return percent;
}

void displayNacelles(List<Nacelle> nacelles) {
	foreach(Nacelle n in nacelles) {
		Echo($"\nRotor Name: {n.rotor.theBlock.CustomName}");
		// n.rotor.theBlock.SafetyLock = false;//for testing
		// n.rotor.theBlock.SafetyLockSpeed = 100;//for testing

		// Echo($@"deltaX: {Vector3D.Round(oldTranslation - km.Translation.Translation, 0)}");

		Echo("Thrusters:");
		int i = 0;
		foreach(Thruster t in n.thrusters) {
			Echo($@"{i}: {t.theBlock.CustomName}");
			i++;
		}
	}
}

public class Nacelle {
	public String errStr;
	public String DTerrStr;
	public Program program;

	// physical parts
	public Rotor rotor;
	public HashSet<Thruster> thrusters;// all the thrusters
	public HashSet<Thruster> availableThrusters;// <= thrusters: the ones the user chooses to be used (ShowInTerminal)
	public HashSet<Thruster> activeThrusters;// <= activeThrusters: the ones that are facing the direction that produces the most thrust (only recalculated if available thrusters changes)
	
	public double thrustModifierAbove = 0.1;// how close the rotor has to be to target position before the thruster gets to full power
	public double thrustModifierBelow = 0.1;// how close the rotor has to be to opposite of target position before the thruster gets to 0 power

	public bool oldJetpack = true;
	public Vector3D requiredVec = Vector3D.Zero;

	public float totalEffectiveThrust = 0;
	public int detectThrustCounter = 0;
	public Vector3D currDir = Vector3D.Zero;

	public bool thrustOn = false;


	public Nacelle() {}// don't use this if it is possible for the instance to be kept
	public Nacelle(Rotor rotor, Program program) {
		this.program = program;
		this.rotor = rotor;
		this.thrusters = new HashSet<Thruster>();
		this.availableThrusters = new HashSet<Thruster>();
		this.activeThrusters = new HashSet<Thruster>();
		errStr = "";
		DTerrStr = "";
	}

	// final calculations and setting physical components
	public void go(bool jetpack, bool dampeners, float shipMass) {
		errStr = "=======Nacelle=======";
		/*errStr += $"\nactive thrusters: {activeThrusters.Count}";
		errStr += $"\nall thrusters: {thrusters.Count}";
		errStr += $"\nrequired force: {(int)requiredVec.Length()}N\n";*/
		totalEffectiveThrust = (float)calcTotalEffectiveThrust(activeThrusters);

		double angleCos = 0;

		// TODO: fix this so that it cuts-off when the dampeners are on
		// TODO: fix the thruster on/off code

		// hysteresis
		if(requiredVec.Length() > gravCutoff * shipMass) {//TODO: this causes problems if there are many small nacelles
			thrustOn = true;
		}
		if(requiredVec.Length() < 0.1 * gravCutoff * shipMass) {
			thrustOn = false;
		}

		// errStr += $"thrustOn: {thrustOn} \n{Math.Round(requiredVec.Length()/(gravCutoff*shipMass), 2)}\n{Math.Round(requiredVec.Length()/(gravCutoff*shipMass*0.01), 2)}";

		// maybe lerp this in the future
		if(!thrustOn) {// Zero G
			// errStr += "\nnot much thrust";
			Vector3D zero_G_accel = Vector3D.Zero;
			if(program.mainController != null) {
				zero_G_accel = (program.mainController.WorldMatrix.Down + program.mainController.WorldMatrix.Backward) * zeroGAcceleration / 1.414f;
			} else {
				zero_G_accel = (program.usableControllers[0].WorldMatrix.Down + program.usableControllers[0].WorldMatrix.Backward) * zeroGAcceleration / 1.414f;
			}
			if(dampeners) {
				angleCos = rotor.setFromVec(zero_G_accel * shipMass + requiredVec);
			} else {
				angleCos = rotor.setFromVec((requiredVec - program.shipVelocity) + zero_G_accel);
			}
			// errStr += $"\n{detectThrustCounter}";
			// rotor.setFromVecOld((controller.WorldMatrix.Down * zeroGAcceleration) + requiredVec);
		} else {// In Gravity
			// errStr += "\nlots of thrust";
			angleCos = rotor.setFromVec(requiredVec);
			//angleCos = rotor.setFromVecOld(requiredVec);
		}
		/*errStr += $"\n=======rotor=======";
		errStr += $"\nname: '{rotor.theBlock.CustomName}'";
		errStr += $"\n{rotor.errStr}";
		errStr += $"\n-------rotor-------";*/


		// the clipping value 'thrustModifier' defines how far the rotor can be away from the desired direction of thrust, and have the power still at max
		// if 'thrustModifier' is at 1, the thruster will be at full desired power when it is at 90 degrees from the direction of travel
		// if 'thrustModifier' is at 0, the thruster will only be at full desired power when it is exactly at the direction of travel, (it's never exactly in-line)
		// double thrustOffset = (angleCos + 1) / (1 + (1 - Program.thrustModifierAbove));//put it in some graphing calculator software where 'angleCos' is cos(x) and adjust the thrustModifier value between 0 and 1, then you can visualise it
		double abo = thrustModifierAbove;
		double bel = thrustModifierBelow;
		if(abo > 1) { abo = 1; }
		if(abo < 0) { abo = 0; }
		if(bel > 1) { bel = 1; }
		if(bel < 0) { bel = 0; }
		// put it in some graphing calculator software where 'angleCos' is cos(x) and adjust the thrustModifier values between 0 and 1, then you can visualise it
		double thrustOffset = ((((angleCos + 1) * (1 + bel)) / 2) - bel) * (((angleCos + 1) * (1 + abo)) / 2);// the other one is simpler, but this one performs better
		// double thrustOffset = (angleCos * (1 + abo) * (1 + bel) + abo - bel + 1) / 2;
		if(thrustOffset > 1) {
			thrustOffset = 1;
		} else if(thrustOffset < 0) {
			thrustOffset = 0;
		}

		//set the thrust for each engine
		// errStr += $"\n=======thrusters=======";
		foreach(Thruster thruster in activeThrusters) {
			// errStr += thrustOffset.progressBar();
			Vector3D thrust = thrustOffset * requiredVec * thruster.theBlock.MaxEffectiveThrust / totalEffectiveThrust;
			bool noThrust = thrust.LengthSquared() < 0.001f;
			if(!jetpack || !thrustOn || noThrust) {
				thruster.setThrust(0);
				thruster.theBlock.Enabled = false;
				thruster.IsOffBecauseDampeners = !thrustOn || noThrust;
				thruster.IsOffBecauseJetpack = !jetpack;
			} else {
				thruster.setThrust(thrust);
				thruster.theBlock.Enabled = true;
				thruster.IsOffBecauseDampeners = false;
				thruster.IsOffBecauseJetpack = false;
			}

			// errStr += $"\nthruster '{thruster.theBlock.CustomName}': {thruster.errStr}\n";
		}
		// errStr += $"\n-------thrusters-------";
		// errStr += $"\n-------Nacelle-------";
		oldJetpack = jetpack;
	}

	public float calcTotalEffectiveThrust(IEnumerable<Thruster> thrusters) {
		float total = 0;
		foreach(Thruster t in thrusters) {
			total += t.theBlock.MaxEffectiveThrust;
		}
		return total;
	}


	//true if all thrusters are good
	public bool validateThrusters(bool jetpack) {
		bool needsUpdate = false;
		errStr += "validating thrusters: (jetpack {jetpack})\n";
		foreach(Thruster curr in thrusters) {

			bool shownAndFunctional = (curr.theBlock.ShowInTerminal || !ignoreHiddenBlocks) && curr.theBlock.IsFunctional;
			if(availableThrusters.Contains(curr)) {//is available
				errStr += "in available thrusters\n";

				bool wasOnAndIsNowOff = curr.IsOn && !curr.theBlock.Enabled && !curr.IsOffBecauseJetpack && !curr.IsOffBecauseDampeners;

				if((!shownAndFunctional || wasOnAndIsNowOff) && (jetpack && oldJetpack)) {
					// if jetpack is on, the thruster has been turned off
					// if jetpack is off, the thruster should still be in the group

					curr.IsOn = false;
					//remove the thruster
					availableThrusters.Remove(curr);
					needsUpdate = true;
				}

			} else {//not available
				errStr += "not in available thrusters\n";
				if(ignoreHiddenBlocks) {
					errStr += $"ShowInTerminal {curr.theBlock.ShowInTerminal}\n";
				}
				errStr += $"IsWorking {curr.theBlock.IsWorking}\n";
				errStr += $"IsFunctional {curr.theBlock.IsFunctional}\n";

				bool wasOffAndIsNowOn = !curr.IsOn && curr.theBlock.Enabled;
				if(shownAndFunctional && wasOffAndIsNowOn) {
					availableThrusters.Add(curr);
					needsUpdate = true;
					curr.IsOn = true;
				}
			}
		}
		return !needsUpdate;
	}

	public void detectThrustDirection() {
		// DTerrStr = "";
		detectThrustCounter++;
		Vector3D engineDirection = Vector3D.Zero;
		Vector3D engineDirectionNeg = Vector3D.Zero;
		Vector3I thrustDir = Vector3I.Zero;
		Base6Directions.Direction rotTopUp = rotor.theBlock.Top.Orientation.Up;

		// add all the thrusters effective power
		foreach(Thruster t in availableThrusters) {
			// Base6Directions.Direction thrustForward = t.theBlock.Orientation.TransformDirection(Base6Directions.Direction.Forward); // Exhaust goes this way
			Base6Directions.Direction thrustForward = t.theBlock.Orientation.Forward; // Exhaust goes this way

			//if its not facing rotor up or rotor down
			if(!(thrustForward == rotTopUp || thrustForward == Base6Directions.GetFlippedDirection(rotTopUp))) {
				// add it in
				var thrustForwardVec = Base6Directions.GetVector(thrustForward);
				if(thrustForwardVec.X < 0 || thrustForwardVec.Y < 0 || thrustForwardVec.Z < 0) {
					engineDirectionNeg += Base6Directions.GetVector(thrustForward) * t.theBlock.MaxEffectiveThrust;
				} else {
					engineDirection += Base6Directions.GetVector(thrustForward) * t.theBlock.MaxEffectiveThrust;
				}
			}
		}

		// get single most powerful direction
		double max = Math.Max(engineDirection.Z, Math.Max(engineDirection.X, engineDirection.Y));
		double min = Math.Min(engineDirectionNeg.Z, Math.Min(engineDirectionNeg.X, engineDirectionNeg.Y));
		// DTerrStr += $"\nmax:\n{Math.Round(max, 2)}";
		// DTerrStr += $"\nmin:\n{Math.Round(min, 2)}";
		double maxAbs = 0;
		if(max > -1*min) {
			maxAbs = max;
		} else {
			maxAbs = min;
		}
		// DTerrStr += $"\nmaxAbs:\n{Math.Round(maxAbs, 2)}";

		// TODO: swap onbool for each thruster that isn't in this
		float DELTA = 0.1f;
		if(Math.Abs(maxAbs - engineDirection.X) < DELTA) {
			// DTerrStr += $"\nengineDirection.X";
			thrustDir.X = 1;
		} else if(Math.Abs(maxAbs - engineDirection.Y) < DELTA) {
			// DTerrStr += $"\nengineDirection.Y";
			thrustDir.Y = 1;
		} else if(Math.Abs(maxAbs - engineDirection.Z) < DELTA) {
			// DTerrStr += $"\nengineDirection.Z";
			thrustDir.Z = 1;
		} else if(Math.Abs(maxAbs - engineDirectionNeg.X) < DELTA) {
			// DTerrStr += $"\nengineDirectionNeg.X";
			thrustDir.X = -1;
		} else if(Math.Abs(maxAbs - engineDirectionNeg.Y) < DELTA) {
			// DTerrStr += $"\nengineDirectionNeg.Y";
			thrustDir.Y = -1;
		} else if(Math.Abs(maxAbs - engineDirectionNeg.Z) < DELTA) {
			// DTerrStr += $"\nengineDirectionNeg.Z";
			thrustDir.Z = -1;
		} else {
			// DTerrStr += $"\nERROR (detectThrustDirection):\nmaxAbs doesn't match any engineDirection\n{maxAbs}\n{engineDirection}\n{engineDirectionNeg}";
			return;
		}

		// use thrustDir to set rotor offset
		rotor.setPointDir((Vector3D)thrustDir);
		// Base6Directions.Direction rotTopForward = rotor.theBlock.Top.Orientation.TransformDirection(Base6Directions.Direction.Forward);
		// Base6Directions.Direction rotTopLeft = rotor.theBlock.Top.Orientation.TransformDirection(Base6Directions.Direction.Left);
		// rotor.offset = (float)Math.Acos(rotor.angleBetweenCos(Base6Directions.GetVector(rotTopForward), (Vector3D)thrustDir));

		// disambiguate
		// if(false && Math.Acos(rotor.angleBetweenCos(Base6Directions.GetVector(rotTopLeft), (Vector3D)thrustDir)) > Math.PI/2) {
			// rotor.offset += (float)Math.PI;
		// 	rotor.offset = (float)(2*Math.PI - rotor.offset);
		// }

		foreach(Thruster t in thrusters) {
			t.theBlock.Enabled = false;
			t.IsOn = false;
		}
		activeThrusters.Clear();

		// put thrusters into the active list
		Base6Directions.Direction thrDir = Base6Directions.GetDirection(thrustDir);
		foreach(Thruster t in availableThrusters) {
			Base6Directions.Direction thrustForward = t.theBlock.Orientation.Forward; // Exhaust goes this way

			if(thrDir == thrustForward) {
				t.theBlock.Enabled = true;
				t.IsOn = true;
				activeThrusters.Add(t);
			}
		}
	}

}

public class Thruster {
	public IMyThrust theBlock;

	// stays the same when in standby, if not in standby, this gets updated to weather or not the thruster is on
	public bool IsOn;

	// these 2 indicate the thruster was turned off from the script, and should be kept in the active list
	public bool IsOffBecauseDampeners = true;
	public bool IsOffBecauseJetpack = true;

	public string errStr = "";

	public Thruster(IMyThrust thruster) {
		this.theBlock = thruster;
		// this.IsOn = theBlock.Enabled;
		this.IsOn = false;
		this.theBlock.Enabled = true;
	}

	// sets the thrust in newtons (N)
	// thrustVec is in worldspace, who'se length is the desired thrust
	public void setThrust(Vector3D thrustVec) {
		setThrust(thrustVec.Length());
	}

	// sets the thrust in newtons (N)
	public void setThrust(double thrust) {
		errStr = "";
		/*errStr += $"\ntheBlock.Enabled: {theBlock.Enabled.toString()}";
		errStr += $"\nIsOffBecauseDampeners: {IsOffBecauseDampeners.toString()}";
		errStr += $"\nIsOffBecauseJetpack: {IsOffBecauseJetpack.toString()}";*/

		if(thrust > theBlock.MaxThrust) {
			thrust = theBlock.MaxThrust;
			// errStr += $"\nExceeding max thrust";
		} else if(thrust < 0) {
			// errStr += $"\nNegative Thrust";
			thrust = 0;
		}

		theBlock.ThrustOverride = (float)(thrust * theBlock.MaxThrust / theBlock.MaxEffectiveThrust);
		/*errStr += $"\nEffective {(100*theBlock.MaxEffectiveThrust / theBlock.MaxThrust).Round(1)}%";
		errStr += $"\nOverride {theBlock.ThrustOverride}N";*/
	}
}

public class Rotor {
	public IMyMotorStator theBlock;
	// don't want IMyMotorBase, that includes wheels

	// Depreciated, this is for the old setFromVec
	public float offset = 0;// radians

	public Vector3D direction = Vector3D.Zero;//offset relative to the head

	public string errStr = "";
	float maxRPM = maxRotorRPM;

	public Rotor(IMyMotorStator rotor) {
		this.theBlock = rotor;


		if(maxRotorRPM <= 0) {
			maxRPM = rotor.GetMaximum<float>("Velocity");
		} else {
			maxRPM = maxRotorRPM;
		}
	}

	public void setPointDir(Vector3D dir) {
		// MatrixD inv = MatrixD.Invert(theBlock.Top.WorldMatrix);
		// direction = Vector3D.TransformNormal(dir, inv);
		this.direction = dir;
		//TODO: for some reason, this is equal to rotor.worldmatrix.up
	}

	/*===| Part of Rotation By Equinox on the KSH discord channel. |===*/
	private void PointRotorAtVector(IMyMotorStator rotor, Vector3D targetDirection, Vector3D currentDirection, float multiplier) {
		double errorScale = Math.PI * maxRPM;

		Vector3D angle = Vector3D.Cross(targetDirection, currentDirection);
		// Project onto rotor
		double err = Vector3D.Dot(angle, rotor.WorldMatrix.Up);
		double err2 = Vector3D.Dot(angle.normalized(), rotor.WorldMatrix.Up);
		double diff = (rotor.WorldMatrix.Up - angle.normalized()).Length();

		/*this.errStr += $"\nrotor.WorldMatrix.Up: {rotor.WorldMatrix.Up}";
		this.errStr += $"\nangle: {Math.Acos(angleBetweenCos(angle, rotor.WorldMatrix.Up)) * 180.0 / Math.PI}";
		this.errStr += $"\nerr: {err}";
		this.errStr += $"\ndirection difference: {diff}";

		this.errStr += $"\ncurrDir vs Up: {currentDirection.Dot(rotor.WorldMatrix.Up)}";
		this.errStr += $"\ntargetDir vs Up: {targetDirection.Dot(rotor.WorldMatrix.Up)}";

		this.errStr += $"\nmaxRPM: {maxRPM}";
		this.errStr += $"\nerrorScale: {errorScale}";
		this.errStr += $"\nmultiplier: {multiplier}";*/


		double rpm = err * errorScale * multiplier;
		//double rpm = err2 * errorScale * multiplier;
		// errStr += $"\nSETTING ROTOR TO {err:N2}";
		if (rpm > maxRPM) {
			rotor.TargetVelocityRPM = maxRPM;
			// this.errStr += $"\nRPM Exceedes Max";
		} else if ((rpm*-1) > maxRPM) {
			rotor.TargetVelocityRPM = maxRPM * -1;
			// this.errStr += $"\nRPM Exceedes -Max";
		} else {
			rotor.TargetVelocityRPM = (float)rpm;
		}
		// this.errStr += $"\nRPM: {(rotor.TargetVelocityRPM).Round(5)}";
	}

	// this sets the rotor to face the desired direction in worldspace
	// desiredVec doesn't have to be in-line with the rotors plane of rotation
	public double setFromVec(Vector3D desiredVec, float multiplier) {
		errStr = "";
		//desiredVec = desiredVec.reject(theBlock.WorldMatrix.Up);
		desiredVec.Normalize();
		//Vector3D currentDir = Vector3D.TransformNormal(this.direction, theBlock.Top.WorldMatrix);
		//                                                                 ^ only correct if it was built from the head
		//                                                                   it needs to be based on the grid
		Vector3D currentDir = Vector3D.TransformNormal(this.direction, theBlock.Top.CubeGrid.WorldMatrix);
		PointRotorAtVector(theBlock, desiredVec, currentDir/*theBlock.Top.WorldMatrix.Forward*/, multiplier);

		//this.errStr += $"\ncurrent dir: {currentDir}\ntarget dir: {desiredVec}\ndiff: {currentDir - desiredVec}";


		return angleBetweenCos(currentDir, desiredVec, desiredVec.Length());
	}

	public double setFromVec(Vector3D desiredVec) {
		return setFromVec(desiredVec, 1);
	}

	// this sets the rotor to face the desired direction in worldspace
	// desiredVec doesn't have to be in-line with the rotors plane of rotation
	public double setFromVecOld(Vector3D desiredVec) {
		desiredVec = desiredVec.reject(theBlock.WorldMatrix.Up);
		if(Vector3D.IsZero(desiredVec) || !desiredVec.IsValid()) {
			errStr = $"\nERROR (setFromVec()):\n\tdesiredVec is invalid\n\t{desiredVec}";
			return -1;
		}

		double des_vec_len = desiredVec.Length();
		double angleCos = angleBetweenCos(theBlock.WorldMatrix.Forward, desiredVec, des_vec_len);

		// angle between vectors
		float angle = -(float)Math.Acos(angleCos);

		//disambiguate
		if(Math.Acos(angleBetweenCos(theBlock.WorldMatrix.Left, desiredVec, des_vec_len)) > Math.PI/2) {
			angle = (float)(2*Math.PI - angle);
		}

		setPos(angle + (float)(offset/* * Math.PI / 180*/));
		return angleCos;
	}

	// gets cos(angle between 2 vectors)
	// cos returns a number between 0 and 1
	// use Acos to get the angle
	public double angleBetweenCos(Vector3D a, Vector3D b) {
		double dot = Vector3D.Dot(a, b);
		double Length = a.Length() * b.Length();
		return dot/Length;
	}

	// gets cos(angle between 2 vectors)
	// cos returns a number between 0 and 1
	// use Acos to get the angle
	// doesn't calculate length because thats expensive
	public double angleBetweenCos(Vector3D a, Vector3D b, double len_a_times_len_b) {
		double dot = Vector3D.Dot(a, b);
		return dot/len_a_times_len_b;
	}

	// set the angle to be between 0 and 2pi radians (0 and 360 degrees)
	// this takes and returns radians
	float cutAngle(float angle) {
		while(angle > Math.PI) {
			angle -= 2*(float)Math.PI;
		}
		while(angle < -Math.PI) {
			angle += 2*(float)Math.PI;
		}
		return angle;
	}

	// move rotor to the angle (radians), make it go the shortest way possible
	public void setPos(float x)
	{
		theBlock.Enabled = true;
		x = cutAngle(x);
		float velocity = maxRPM;
		float x2 = cutAngle(theBlock.Angle);
		if(Math.Abs(x - x2) < Math.PI) {
			//dont cross origin
			if(x2 < x) {
				theBlock.SetValue<float>("Velocity", velocity * Math.Abs(x - x2));
			} else {
				theBlock.SetValue<float>("Velocity", -velocity * Math.Abs(x - x2));
			}
		} else {
			//cross origin
			if(x2 < x) {
				theBlock.SetValue<float>("Velocity", -velocity * Math.Abs(x - x2));
			} else {
				theBlock.SetValue<float>("Velocity", velocity * Math.Abs(x - x2));
			}
		}
	}

}

}
public static class CustomProgramExtensions {

	public static bool IsAlive(this IMyTerminalBlock block) {
		return block.CubeGrid.GetCubeBlock(block.Position)?.FatBlock == block;
	}

	// projects a onto b
	public static Vector3D project(this Vector3D a, Vector3D b) {
		double aDotB = Vector3D.Dot(a, b);
		double bDotB = Vector3D.Dot(b, b);
		return b * aDotB / bDotB;
	}

	public static Vector3D reject(this Vector3D a, Vector3D b) {
		return Vector3D.Reject(a, b);
	}

	public static Vector3D normalized(this Vector3D vec) {
		return Vector3D.Normalize(vec);
	}

	public static double dot(this Vector3D a, Vector3D b) {
		return Vector3D.Dot(a, b);
	}

	// get movement and turn it into worldspace
	public static Vector3D getWorldMoveIndicator(this IMyShipController cont) {
		return Vector3D.TransformNormal(cont.MoveIndicator, cont.WorldMatrix);
	}


	public static string progressBar(this double val) {
		char[] bar = {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '};
		for(int i = 0; i < 10; i++) {
			if(i <= val * 10) {
				bar[i] = '|';
			}
		}
		var str_build = new StringBuilder("[");
		for(int i = 0; i < 10; i++) {
			str_build.Append(bar[i]);
		}
		str_build.Append("]");
		return str_build.ToString();
	}

	public static string progressBar(this float val) {
		return ((double)val).progressBar();
	}

	public static string progressBar(this Vector3D val) {
		return val.Length().progressBar();
	}


	public static Vector3D Round(this Vector3D vec, int num) {
		return Vector3D.Round(vec, num);
	}

	public static double Round(this double val, int num) {
		return Math.Round(val, num);
	}

	public static float Round(this float val, int num) {
		return (float)Math.Round(val, num);
	}

	public static String toString(this Vector3D val) {
		return $"X:{val.X} Y:{val.Y} Z:{val.Z}";
	}

	public static String toString(this Vector3D val, bool pretty) {
		if(!pretty)
			return val.toString();
		else
			return $"X:{val.X}\nY:{val.Y}\nZ:{val.Z}\n";
	}

	public static String toString(this bool val) {
		if(val) {
			return "true";
		}
		return "false";
	}