// ALWAYS CHECK FOR UPDATES
// to update, simply load it from the workshop tab again (no need to actually go to the workshop page)

// weather or not dampeners or thrusters are on when you start the script
public bool dampeners = true;
public bool jetpack = false;

public bool controlModule = true;

// standby stops all calculations and safely turns off all nacelles, good if you want to stop flying
// but dont want to turn the craft off.
public bool startInStandby = true;
// change this is you don't want the script to start in standby... please only use this if you have permission from the server owner

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
public const bool onlyMainCockpit = false;

// choose weather you want the script to update once every frame, once every 10 frames, or once every 100 frames
// should be 1 of:
// UpdateFrequency.Update1
// UpdateFrequency.Update10
// UpdateFrequency.Update100
public const UpdateFrequency update_frequency = UpdateFrequency.Update1;

public const string LCDName = "%VectorLCD";

// arguments, you can change these to change what text you run the programmable block with
public const string standbyArg = "%standby";
public const string dampenersArg = "%dampeners";
public const string jetpackArg = "%jetpack";
public const string raiseAccelArg = "%raiseAccel";
public const string lowerAccelArg = "%lowerAccel";
public const string resetAccelArg = "%resetAccel";
public const string resetArg = "%reset";//this one re-runs the initial setup... you probably want to use %resetAccel

// control module gamepad bindings
// type "/cm showinputs" into chat
// press the desired button
// put that text EXACTLY as it is in the quotes for the control you want
public const string jetpackButton = "c.thrusts";
public const string dampenersButton = "c.damping";
public const string lowerAccel = "minus";
public const string raiseAccel = "plus";
public const string resetAccel = "0";

public const float maxRotorRPM = 60;

// default acceleration in situations with 0 (or low) gravity
public const float zeroGAcceleration = 9.81f;
// if gravity becomes less than this, zeroGAcceleration will kick in
public const float gravCutoff = 0.1f * 9.81f;







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
public const double thrustModifierAbove = 0.1;// how close the rotor has to be to target position before the thruster gets to full power
public const double thrustModifierBelow = 0.1;// how close the rotor has to be to opposite of target position before the thruster gets to 0 power


// debugging prints
public const bool debug = true;

public Program() {
	Echo("Just Compiled");
	programCounter = 0;
	gotNacellesCount = 0;
	updateNacellesCount = 0;
	Runtime.UpdateFrequency = UpdateFrequency.Once;
}
public void Save() {}


//at 60 fps this will last for 9000+ hrs before going negative
public long programCounter;
public long gotNacellesCount;
public long updateNacellesCount;

public void Main(string argument, UpdateType runType) {
	// ========== STARTUP ==========
	globalAppend = false;

	Echo("Running "+ programCounter++);
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
	write(spinner);

	// write(runType.ToString());

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
		argument = "";
	}

	// Echo("Starting Main");
	argument = argument.ToLower();
	bool togglePower = argument.Contains(standbyArg.ToLower());

	bool anyArg =
	argument.Contains(dampenersArg.ToLower()) ||
	argument.Contains(jetpackArg.ToLower()) ||
	argument.Contains(standbyArg.ToLower()) ||
	argument.Contains(raiseAccelArg.ToLower()) ||
	argument.Contains(lowerAccelArg.ToLower()) ||
	argument.Contains(resetAccelArg.ToLower()) ||
	argument.Contains(resetArg.ToLower());

	// going into standby mode
	if((togglePower && !standby) || goToStandby) {
		standby = true;
		goToStandby = false;
		foreach(Nacelle n in nacelles) {
			n.rotor.theBlock.ApplyAction("OnOff_Off");
			foreach(Thruster t in n.thrusters) {
				t.theBlock.ApplyAction("OnOff_Off");
			}
		}
		Runtime.UpdateFrequency = UpdateFrequency.None;
	// coming back from standby mode
	} else if((anyArg || runType == UpdateType.Terminal) && standby || comeFromStandby) {
		standby = false;
		comeFromStandby = false;
		foreach(Nacelle n in nacelles) {
			n.rotor.theBlock.ApplyAction("OnOff_On");
			foreach(Thruster t in n.thrusters) {
				if(t.IsOn) {
					t.theBlock.ApplyAction("OnOff_On");
				}
			}
		}
		Runtime.UpdateFrequency = update_frequency;
	}

	if(argument.Contains(resetArg.ToLower()) || controllers.Count == 0 /*|| timer == null*/ || justCompiled) {
		if(!init()) {
			return;
		}
	}

	checkNacelles(debug);
	if(updateNacelles) {
		nacelles.Clear();
		nacelles = getNacelles();
		updateNacelles = false;
	}

	if(standby) {
		Echo("Standing By");
		write("Standing By");
		return;
	} else {
		// timer.Trigger();
		// timer.StartCountdown();
		// write($"dampers={dampeners}");
	}

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

	// setup gravity
	float gravLength = (float)worldGrav.Length();
	if(gravLength < gravCutoff) {
		gravLength = zeroGAcceleration;
	}

	Vector3D desiredVec = getMovementInput(argument);

	//safety, dont go over max speed DEPRECATED (SE no longer has safety-lock so this is no longer needed)
	/*if(shipVelocity.Length() > speedLimit) {
		desiredVec -= shipVelocity;
	}*/

	if(dampeners) {
		Vector3D dampVec = Vector3D.Zero;
		if(desiredVec != Vector3D.Zero) {
			// cancel backwards movement
			if(desiredVec.dot(shipVelocity) < 0) {
				//if you want to go oppisite to velocity
				dampVec = shipVelocity.project(desiredVec.normalized());
			}
			// cancel sideways movement
			dampVec += shipVelocity.reject(desiredVec.normalized());
		} else {
			// no desiredVec, just use shipVelocity
			dampVec = shipVelocity;
		}
		desiredVec -= dampVec * dampenersModifier;
	}


	// f=ma
	Vector3D shipWeight = shipMass * worldGrav;
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
	foreach(Nacelle n in nacelles) {
		if(!n.validateThrusters(jetpack)) {
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
	write("Active Nacelles: " + nacelles.Count);//TODO: make activeNacelles account for the number of nacelles that are actually active (activeThrusters.Count > 0)
	// write("Got Nacelles: " + gotNacellesCount);
	// write("Update Nacelles: " + updateNacellesCount);
	Echo(deBug);
	write(deBug);
	deBug = "";
	// ========== END OF MAIN ==========
}


public int accelExponent = 0;

public bool jetpackIsPressed = false;
public bool dampenersIsPressed = false;
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
public int rotorCount = 0;
public int rotorTopCount = 0;
public int thrusterCount = 0;
public int screenCount = 0;
public bool updateNacelles = false;
public bool standby = false;
public Vector3D shipVelocity = Vector3D.Zero;

public bool justCompiled = true;
public bool goToStandby = false;
public bool comeFromStandby = false;

public string deBug = "";




// DEPRECATED
//public float speedLimit = 90000;//speed limit of your game
//this is because you cant disable rotor safety lock
//the best you can do is set the safety lock as max speed of the game.
//default is 100m/s i recommend subtract 10 and set it as that.
//make sure all your rotors safety lock is at max speed

public const string TimName = "%VectorTim";

// DEPRECATED
// TODO: remove this
public IMyTimerBlock getTimer() {
	List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(blocks);

	for(int i = blocks.Count-1; i >= 0; i--) {
		if(!blocks[i].CustomName.ToLower().Contains(TimName.ToLower())) {//not named as the vector timer
			// remove it from the list
			blocks.Remove(blocks[i]);
		}
	}

	if(blocks.Count > 0) {
		//use first one
		return (IMyTimerBlock)blocks[0];
	} else {
		Echo($"ERROR: no timer found\nYou need to set a timer block to run the programmable block and put '{TimName}' in its name");
		return null;
	}
}

public void getScreens(List<IMyTextPanel> screens) {
	this.screens = screens;
	usableScreens.Clear();
	foreach(IMyTextPanel screen in screens) {
		if(!screen.IsWorking) continue;
		if(!screen.CustomName.ToLower().Contains(LCDName.ToLower())) continue;
		usableScreens.Add(screen);
	}
	screenCount = screens.Count;
}

public void write(string str) {
	if(usableScreens.Count > 0) {
		str += "\n";
		foreach(IMyTextPanel screen in usableScreens) {
			screen.WritePublicText(str, globalAppend);
			screen.ShowPublicTextOnScreen();
		}
		globalAppend = true;
	} else {
		if(globalAppend) return;
		Echo("No screens available");
		globalAppend = true;
	}
}

double getAcceleration(double gravity) {
	return Math.Pow(accelBase, accelExponent) * gravity * defaultAccel;
}

public Vector3D getMovementInput(string arg) {
	Vector3D moveVec = Vector3D.Zero;

	if(controlModule) {
		// setup control module
		Dictionary<string, object> inputs = new Dictionary<string, object>();
		try {
			inputs = Me.GetValue<Dictionary<string, object>>("ControlModule.Inputs");
			Me.SetValue<string>("ControlModule.AddInput", "all");
			Me.SetValue<bool>("ControlModule.RunOnInput", true);
			Me.SetValue<int>("ControlModule.InputState", 1);
			Me.SetValue<float>("ControlModule.RepeatDelay", 0.016f);
		} catch(Exception e) {
			controlModule = false;
		}

		// non-movement controls
		if(inputs.ContainsKey(dampenersButton) && !dampenersIsPressed) {//inertia dampener key
			dampeners = !dampeners;//toggle
			dampenersIsPressed = true;
		}
		if(!inputs.ContainsKey(dampenersButton)) {
			dampenersIsPressed = false;
		}
		if(inputs.ContainsKey(jetpackButton) && !jetpackIsPressed) {//jetpack key
			jetpack = !jetpack;//toggle
			jetpackIsPressed = true;
		}
		if(!inputs.ContainsKey(jetpackButton)) {
			jetpackIsPressed = false;
		}
		if(inputs.ContainsKey(raiseAccel) && !plusIsPressed) {//throttle up
			accelExponent++;
			plusIsPressed = true;
		}
		if(!inputs.ContainsKey(raiseAccel)) { //increase target acceleration
			plusIsPressed = false;
		}

		if(inputs.ContainsKey(lowerAccel) && !minusIsPressed) {//throttle down
			accelExponent--;
			minusIsPressed = true;
		}
		if(!inputs.ContainsKey(lowerAccel)) { //lower target acceleration
			minusIsPressed = false;
		}
		if(inputs.ContainsKey(resetAccel)) { //default throttle
			accelExponent = 0;
		}

	}

	// dampeners (if there are any normal thrusters, the dampeners control works)
	if(normalThrusters.Count != 0) {
		if(onlyMainCockpit || mainController != null && mainController.IsUnderControl) {
			dampeners = mainController.DampenersOverride;
		} else {
			// dampeners = false;
			foreach(IMyShipController cont in controllers) {
				if(cont.DampenersOverride && cont.IsUnderControl) {
					dampeners = true;
				}
			}
		}
	}

	// movement controls
	if(mainController != null && (mainController.IsUnderControl || onlyMainCockpit)) {
		moveVec = mainController.getWorldMoveIndicator();
	} else {
		foreach(IMyShipController cont in controllers) {
			if(cont.IsUnderControl) {
				moveVec += cont.getWorldMoveIndicator();
			}
		}
	}

	if(arg.Contains(dampenersArg.ToLower())) {
		dampeners = !dampeners;
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

	return moveVec;
}

bool getControllers() {
	var blocks = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
	mainController = null;

	usableControllers.Clear();

	for(int i = 0; i < blocks.Count; i++) {
		if(!blocks[i].ShowInTerminal) continue;
		if(!blocks[i].CanControlShip) continue;
		if(!blocks[i].ControlThrusters) continue;
		if(blocks[i].IsMainCockpit) {
			mainController = blocks[i];
		}
		usableControllers.Add(blocks[i]);
	}

	if(usableControllers.Count == 0) {
		Echo("ERROR: no ship controller found");
		return false;
	}

	controllers = blocks;
	return true;
}

bool getControllers(List<IMyShipController> blocks) {
	mainController = null;

	usableControllers.Clear();

	for(int i = 0; i < blocks.Count; i++) {
		if(!blocks[i].ShowInTerminal) continue;
		if(!blocks[i].CanControlShip) continue;
		if(!blocks[i].ControlThrusters) continue;
		if(blocks[i].IsMainCockpit) {
			mainController = blocks[i];
		}
		usableControllers.Add(blocks[i]);
	}

	if(usableControllers.Count == 0) {
		Echo("ERROR: no ship controller found");
		return false;
	}

	controllers = blocks;
	return true;
}

/*
IMyShipController getController() {
	var blocks = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
	if(blocks.Count < 1) {
		Echo("ERROR: no ship controller found");
		return null;
	}

	IMyShipController cont = blocks[0];
	int lvl = 0;
	bool allCockpitsAreFree = true;
	int prevLvl = 0;
	IMyShipController prevController = cont;
	bool hasReverted = false;
	for(int i = 0; i < blocks.Count; i++) {
		// only one of them is being controlled
		if(((IMyShipController)blocks[i]).IsUnderControl && allCockpitsAreFree) {
			prevController = cont;
			prevLvl = lvl;
			cont = ((IMyShipController)blocks[i]);
			lvl = 5;
		}//more than one is being controlled, it reverts to previous setting
		else if(((IMyShipController)blocks[i]).IsUnderControl && !allCockpitsAreFree && !hasReverted) {
			lvl = prevLvl;
			cont = prevController;
			hasReverted = true;
		}//has %Main in the name
		else if(((IMyShipController)blocks[i]).CustomName.IndexOf("%Main") != -1 && lvl < 4) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 4;
		}//is ticked as a main cockpit
		else if(((IMyShipController)blocks[i]).GetValue<bool>("MainCockpit") && lvl < 3) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 3;
		}//is set to control thrusters
		else if(((IMyShipController)blocks[i]).ControlThrusters && lvl < 2) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 2;
		}
		else {
			cont = ((IMyShipController)blocks[i]);
			lvl = 1;
		}
	}
	return cont;
}//*/

/*
IMyShipController getController() {
	var blocks = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
	if(blocks.Count < 1) {
		Echo("ERROR: no ship controller found");
		return null;
	}

	IMyShipController cont = blocks[0];
	int undercontrol = 0;
	// bool allCockpitsAreFree = true;
	int cockpitID = 0;
	IMyShipController prevController = cont;
	// bool hasReverted = false;
	/* not working as intended
	for(int i = 0; i < blocks.Count; i++) {
		// only one of them is being controlled
		if(((IMyShipController)blocks[i]).IsUnderControl && allCockpitsAreFree) {
			prevController = cont;
			prevLvl = lvl;
			cont = ((IMyShipController)blocks[i]);
			lvl = 5;
		}//more than one is being controlled, it reverts to previous setting
		else if(((IMyShipController)blocks[i]).IsUnderControl && !allCockpitsAreFree && !hasReverted) {
			lvl = prevLvl;
			cont = prevController;
			hasReverted = true;
		}//has %Main in the name
		else if(((IMyShipController)blocks[i]).CustomName.IndexOf("%Main") != -1 && lvl < 4) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 4;
		}//is ticked as a main cockpit
		else if(((IMyShipController)blocks[i]).GetValue<bool>("MainCockpit") && lvl < 3) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 3;
		}//is set to control thrusters
		else if(((IMyShipController)blocks[i]).ControlThrusters && lvl < 2) {
			cont = ((IMyShipController)blocks[i]);
			lvl = 2;
		}
		else {
			cont = ((IMyShipController)blocks[i]);
			lvl = 1;
		}
	}*//*
	for(int i = 0; i < blocks.Count; i++) {
		//keep track of all the cockpits under control
		if (((IMyShipController)blocks[i]).IsUnderControl)
		{
			undercontrol++;
			cockpitID = i;
		}
		if (undercontrol > 1)
			Echo("Too many pilots, select a main cockpit using the G screen!");

		if(((IMyShipController)blocks[i]).GetValue<bool>("MainCockpit")) //if a main cockpit is checked, then there is no need to check for any other cockpits
		{
			cont = ((IMyShipController)blocks[i]);
			break;
		}
	}

	if (undercontrol == 0)
	{
		Echo("No main cockpit and no pilot, using first cockpit found as main cockpit!");
		cont = ((IMyShipController)blocks[0]);
	}
	else if (undercontrol == 1)
	{
		cont = ((IMyShipController)blocks[cockpitID]);
	}
	return cont;
}*/

// checks to see if the nacelles have changed
public void checkNacelles(bool verbose) {
	var blocks = new List<IMyTerminalBlock>();
	echoV("Checking Nacelles...", verbose);

	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, block => (block is IMyShipController || block is IMyThrust || block is IMyMotorStator || block is IMyTextPanel));
	List<IMyShipController> conts = new List<IMyShipController>();
	List<IMyMotorStator> rots = new List<IMyMotorStator>();
	List<IMyThrust> thrs = new List<IMyThrust>();
	List<IMyTextPanel> txts = new List<IMyTextPanel>();

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
	}


	// if you use the following if statement, it won't lock the non-main cockpit if someone sets the main cockpit, until a recompile or world load :/
	if(/*(mainController != null ? !mainController.IsMainCockpit : false) || */controllers.Count != conts.Count) {
		echoV($"Controller count ({controllers.Count}) is out of whack (current: {conts.Count})", verbose);
		getControllers(conts);
	}

	if(screenCount != txts.Count) {
		echoV($"Screen count ({screenCount}) is out of whack (current: {txts.Count})", verbose);
		getScreens(txts);
	} else {
		foreach(IMyTextPanel screen in txts) {
			if(!screen.IsWorking) continue;
			if(!screen.CustomName.ToLower().Contains(LCDName.ToLower())) continue;
			getScreens(txts);
		}
	}

	if(rotorCount != rots.Count) {
		echoV($"Rotor count ({rotorCount}) is out of whack (current: {rots.Count})", verbose);
		updateNacelles = true;
		return;
	}

	var rotorHeads = new List<IMyAttachableTopBlock>();
	foreach(IMyMotorStator rotor in rots) {
		if(rotor.Top != null) {
			rotorHeads.Add(rotor.Top);
		}
	}
	if(rotorTopCount != rotorHeads.Count) {
		echoV($"Rotor Head count ({rotorTopCount}) is out of whack (current: {rotorHeads.Count})", verbose);
		echoV($"Rotors: {rots.Count}", verbose);
		updateNacelles = true;
		return;
	}

	if(thrusterCount != thrs.Count) {
		echoV($"Thruster count ({thrusterCount}) is out of whack (current: {thrs.Count})", verbose);
		updateNacelles = true;
		return;
	}

	echoV("They seem fine.", verbose);
}

void echoV(string s, bool verbose) {
	if(verbose) {
		Echo(s);
	}
}

public bool init() {
	Echo("init");
	nacelles.Clear();
	nacelles = getNacelles();
	getControllers();
	if(usableControllers.Count == 0) {
		return false;
	}
	/*
	if(false && timer == null) {//TODO: remove this
		timer = getTimer();
		if(timer == null) {
			return false;
		}
	}/**/
	return true;
}
/*
IMyShipController getController() {
	var blocks = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
	IMyShipController cont = blocks[0];

	foreach(IMyShipController c in blocks) {
		if(c.IsUnderControl) {
			return c;
		}
	}

	return cont;
}
*/
// G(thrusters * rotors)
// gets all the rotors and thrusters
List<Nacelle> getNacelles() {
	gotNacellesCount++;
	var blocks = new List<IMyTerminalBlock>();
	var nacelles = new List<Nacelle>();
	// 1 call to GTS
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, block => (block is IMyThrust) || (block is IMyMotorStator));

	echoV("Getting Blocks for thrusters & rotors", debug);
	// get the blocks we care about
	var rotors = new List<IMyMotorStator>();
	normalThrusters.Clear();
	// var thrusters = new List<IMyThrust>();
	for(int i = blocks.Count-1; i >= 0; i--) {
		if(blocks[i] is IMyThrust) {
			normalThrusters.Add((IMyThrust)blocks[i]);
		} else/* if(blocks[i] is IMyMotorStator) */{
			rotors.Add((IMyMotorStator)blocks[i]);
		}
		blocks.RemoveAt(i);
	}
	rotorCount = rotors.Count;
	thrusterCount = normalThrusters.Count;
	blocks.Clear();



	echoV("Getting Rotors", debug);
	// make nacelles out of all valid rotors
	rotorTopCount = 0;
	foreach(IMyMotorStator current in rotors) {

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
		nacelles.Add(new Nacelle(rotor, this));
	}

	echoV("Getting Thrusters", debug);
	// add all thrusters to their corrisponding nacelle and remove nacelles that have none
	for(int i = nacelles.Count-1; i >= 0; i--) {
		for(int j = normalThrusters.Count-1; j >= 0; j--) {
			if(normalThrusters[j].CubeGrid != nacelles[i].rotor.theBlock.TopGrid) continue;// thruster is not for the current nacelle
			// if(!thrusters[j].IsFunctional) continue;// broken, don't add it

			nacelles[i].thrusters.Add(new Thruster(normalThrusters[j]));
			normalThrusters.RemoveAt(j);// shorten the list we have to check
		}
		// remove nacelles (rotors) without thrusters
		if(nacelles[i].thrusters.Count == 0) {
			nacelles.RemoveAt(i);
			continue;
		}
		// if its still there, setup the nacelle
		nacelles[i].validateThrusters(jetpack);
		nacelles[i].detectThrustDirection();
	}

	return nacelles;
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
	public Program program;

	// physical parts
	public Rotor rotor;
	public HashSet<Thruster> thrusters;// all the thrusters
	public HashSet<Thruster> availableThrusters;// <= thrusters: the ones the user chooses to be used (ShowInTerminal)
	public HashSet<Thruster> activeThrusters;// <= activeThrusters: the ones that are facing the direction that produces the most thrust (only recalculated if available thrusters changes)

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
	}

	// final calculations and setting physical components
	public void go(bool jetpack, bool dampeners, float shipMass) {
		errStr = "";
		// errStr += $"\nactive thrusters: {activeThrusters.Count}";
		// errStr += $"\nall thrusters: {thrusters.Count}";
		totalEffectiveThrust = (float)calcTotalEffectiveThrust(activeThrusters);

		double angleCos = 0;

		// TODO: fix this so that it cuts-off when the dampeners are on
		// TODO: fix the thruster on/off code

		// hysteresis
		if(requiredVec.Length() > gravCutoff * shipMass) {
			thrustOn = true;
		}
		if(requiredVec.Length() < 0.1 * gravCutoff * shipMass) {
			thrustOn = false;
		}

		// errStr += $"thrustOn: {thrustOn} \n{Math.Round(requiredVec.Length()/(gravCutoff*shipMass), 2)}\n{Math.Round(requiredVec.Length()/(gravCutoff*shipMass*0.01), 2)}";

		// maybe lerp this in the future
		if(!thrustOn) {// Zero G
			// errStr += "\nnot much thrust";
			Vector3D direction = Vector3D.Zero;
			if(program.mainController != null) {
				direction = (program.mainController.WorldMatrix.Down + program.mainController.WorldMatrix.Backward) * zeroGAcceleration / 1.414f;
			} else {
				direction = (program.usableControllers[0].WorldMatrix.Down + program.usableControllers[0].WorldMatrix.Backward) * zeroGAcceleration / 1.414f;
			}
			if(dampeners) {
				angleCos = rotor.setFromVec(direction * shipMass + requiredVec);
			} else {
				angleCos = rotor.setFromVec((requiredVec - program.shipVelocity) + direction);
			}
			// errStr += $"\n{detectThrustCounter}";
			// rotor.setFromVecOld((controller.WorldMatrix.Down * zeroGAcceleration) + requiredVec);
		} else {// In Gravity
			// errStr += "\nlots of thrust";
			angleCos = rotor.setFromVec(requiredVec);
			// rotor.setFromVecOld(requiredVec);
		}
		// errStr += "\n" + rotor.errStr;
		rotor.errStr = "";


		// the clipping value 'thrustModifier' defines how far the rotor can be away from the desired direction of thrust, and have the power still at max
		// if 'thrustModifier' is at 1, the thruster will be at full desired power when it is at 90 degrees from the direction of travel
		// if 'thrustModifier' is at 0, the thruster will only be at full desired power when it is exactly at the direction of travel, (it's never exactly in-line)
		// double thrustOffset = (angleCos + 1) / (1 + (1 - Program.thrustModifierAbove));//put it in some graphing calculator software where 'angleCos' is cos(x) and adjust the thrustModifier value between 0 and 1, then you can visualise it
		double abo = Program.thrustModifierAbove;
		double bel = Program.thrustModifierBelow;
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
		foreach(Thruster thruster in activeThrusters) {
			// errStr += "\n" + activeThrusters[i].errStr;
			thruster.errStr = "";
			// errStr += thrustOffset.progressBar();
			Vector3D thrust = thrustOffset * requiredVec * thruster.theBlock.MaxEffectiveThrust / totalEffectiveThrust;
			bool noThrust = thrust.LengthSquared() == 0;
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
		}
		oldJetpack = jetpack;
	}

	public float calcTotalEffectiveThrust(List<Thruster> thrusters) {
		float total = 0;
		foreach(Thruster t in thrusters) {
			total += t.theBlock.MaxEffectiveThrust;
		}
		return total;
	}

	public float calcTotalEffectiveThrust(HashSet<Thruster> thrusters) {
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

			bool shownAndFunctional = curr.theBlock.ShowInTerminal && curr.theBlock.IsFunctional;
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
				errStr += $"ShowInTerminal {curr.theBlock.ShowInTerminal}\n";
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
		detectThrustCounter++;
		Vector3D engineDirection = Vector3D.Zero;
		Vector3D engineDirectionNeg = Vector3D.Zero;
		Vector3I thrustDir = Vector3I.Zero;
		// Base6Directions.Direction rotTopUp = rotor.theBlock.Top.Orientation.TransformDirection(Base6Directions.Direction.Up);
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
		// errStr += $"\nmax:\n{Math.Round(max, 2)}";
		// errStr += $"\nmin:\n{Math.Round(min, 2)}";
		double maxAbs = 0;
		if(max > -1*min) {
			maxAbs = max;
		} else {
			maxAbs = min;
		}
		// errStr += $"\nmaxAbs:\n{Math.Round(maxAbs, 2)}";

		// TODO: swap onbool for each thruster that isn't in this
		if(Math.Abs(maxAbs - engineDirection.X) < 0.1) {
			errStr += $"\nengineDirection.X";
			if(engineDirection.X > 0) {
				thrustDir.X = 1;
			} else {
				thrustDir.X = -1;
			}
		} else if(Math.Abs(maxAbs - engineDirection.Y) < 0.1) {
			errStr += $"\nengineDirection.Y";
			if(engineDirection.Y > 0) {
				thrustDir.Y = 1;
			} else {
				thrustDir.Y = -1;
			}
		} else if(Math.Abs(maxAbs - engineDirection.Z) < 0.1) {
			errStr += $"\nengineDirection.Z";
			if(engineDirection.Z > 0) {
				thrustDir.Z = 1;
			} else {
				thrustDir.Z = -1;
			}
		} else if(Math.Abs(maxAbs - engineDirectionNeg.X) < 0.1) {
			errStr += $"\nengineDirectionNeg.X";
			if(engineDirectionNeg.X < 0) {
				thrustDir.X = -1;
			} else {
				thrustDir.X = 1;
			}
		} else if(Math.Abs(maxAbs - engineDirectionNeg.Y) < 0.1) {
			errStr += $"\nengineDirectionNeg.Y";
			if(engineDirectionNeg.Y < 0) {
				thrustDir.Y = -1;
			} else {
				thrustDir.Y = 1;
			}
		} else if(Math.Abs(maxAbs - engineDirectionNeg.Z) < 0.1) {
			errStr += $"\nengineDirectionNeg.Z";
			if(engineDirectionNeg.Z < 0) {
				thrustDir.Z = -1;
			} else {
				thrustDir.Z = 1;
			}
		} else {
			errStr += $"\nERROR (detectThrustDirection):\nmaxAbs doesn't match any engineDirection\n{maxAbs}\n{engineDirection}\n{engineDirectionNeg}";
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
			t.theBlock.ApplyAction("OnOff_Off");
			t.IsOn = false;
		}
		activeThrusters.Clear();

		// put thrusters into the active list
		Base6Directions.Direction thrDir = Base6Directions.GetDirection(thrustDir);
		foreach(Thruster t in availableThrusters) {
			Base6Directions.Direction thrustForward = t.theBlock.Orientation.TransformDirection(Base6Directions.Direction.Forward); // Exhaust goes this way

			if(thrDir == thrustForward) {
				t.theBlock.ApplyAction("OnOff_On");
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
		double thrust = thrustVec.Length();
		if(thrust > theBlock.MaxThrust) {
			thrust = theBlock.MaxThrust;
		} else if(thrust < 0) {
			thrust = 0;
		}
		theBlock.ThrustOverride = (float)(thrust * theBlock.MaxThrust / theBlock.MaxEffectiveThrust);
	}

	// sets the thrust in newtons (N)
	public void setThrust(double thrust) {
		if(thrust > theBlock.MaxThrust) {
			thrust = theBlock.MaxThrust;
		} else if(thrust < 0) {
			thrust = 0;
		}
		theBlock.ThrustOverride = (float)(thrust * theBlock.MaxThrust / theBlock.MaxEffectiveThrust);
	}
}

public class Rotor {
	public IMyMotorStator theBlock;
	// don't want IMyMotorBase, that includes wheels

	// Depreciated, this is for the old setFromVec
	public float offset = 0;// radians

	public Vector3D direction = Vector3D.Zero;//offset relative to the head

	public string errStr = "";

	public Rotor(IMyMotorStator rotor) {
		this.theBlock = rotor;
	}

	public void setPointDir(Vector3D dir) {
		// MatrixD inv = MatrixD.Invert(theBlock.Top.WorldMatrix);
		// direction = Vector3D.TransformNormal(dir, inv);
		this.direction = dir;
	}

	/*===| Part of Rotation By Equinox on the KSH discord channel. |===*/
	private void PointRotorAtVector(IMyMotorStator rotor, Vector3D targetDirection, Vector3D currentDirection, float multiplier) {
		double errorScale = Math.PI * maxRotorRPM;

		Vector3D angle = Vector3D.Cross(targetDirection, currentDirection);
		// Project onto rotor
		double err = angle.Dot(rotor.WorldMatrix.Up);

		err *= errorScale * multiplier;
		// errStr += $"\nSETTING ROTOR TO {err:N2}";
		if (err > maxRotorRPM) {
			rotor.TargetVelocityRPM = (float)maxRotorRPM;
		} else if ((err*-1) > maxRotorRPM) {
			rotor.TargetVelocityRPM = (float)(maxRotorRPM * -1);
		} else {
			rotor.TargetVelocityRPM = (float)err;
		}
	}

	// this sets the rotor to face the desired direction in worldspace
	// desiredVec doesn't have to be in-line with the rotors plane of rotation
	public double setFromVec(Vector3D desiredVec, float multiplier) {
		desiredVec = desiredVec.reject(theBlock.WorldMatrix.Up);
		desiredVec.Normalize();
		Vector3D currentDir = Vector3D.TransformNormal(this.direction, theBlock.Top.WorldMatrix);
		PointRotorAtVector(theBlock, desiredVec, currentDir/*theBlock.Top.WorldMatrix.Forward*/, multiplier);

		return angleBetweenCos(currentDir, desiredVec, desiredVec.Length());
	}

	public double setFromVec(Vector3D desiredVec) {
		return setFromVec(desiredVec, 1);
	}

	// this sets the rotor to face the desired direction in worldspace
	// desiredVec doesn't have to be in-line with the rotors plane of rotation
	public void setFromVecOld(Vector3D desiredVec) {
		desiredVec = desiredVec.reject(theBlock.WorldMatrix.Up);
		if(Vector3D.IsZero(desiredVec) || !desiredVec.IsValid()) {
			errStr += $"\nERROR (setFromVec()):\n\tdesiredVec is invalid\n\t{desiredVec}";
			return;
		}

		// angle between vectors
		float angle = -(float)Math.Acos(angleBetweenCos(theBlock.WorldMatrix.Forward, desiredVec));

		//disambiguate
		if(Math.Acos(angleBetweenCos(theBlock.WorldMatrix.Left, desiredVec)) > Math.PI/2) {
			angle = (float)(2*Math.PI - angle);
		}

		setPos(angle + (float)(offset/* * Math.PI / 180*/));
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
	public double angleBetweenCos(Vector3D a, Vector3D b, double len) {
		double dot = Vector3D.Dot(a, b);
		return dot/len;
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
		theBlock.ApplyAction("OnOff_On");
		x = cutAngle(x);
		float velocity = maxRotorRPM;
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