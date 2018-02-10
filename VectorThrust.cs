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

// this is used to identify programmable blocks as instances of vector thrust
// if you change this, you should probably change all those that are going to connect to the same ship, otherwise they will fight for control.
public const string myName = "|VT|";
public const string myNameStandby = ".VT.";


// default acceleration in situations with 0 (or low) gravity
public const float zeroGAcceleration = 9.81f;
// if gravity becomes less than this, zeroGAcceleration will kick in
public const float gravCutoff = 0.1f * 9.81f;


// PID settings, these effect how the rotor moves to position
public float pmul = 1f;
public float imul = 0f;
public float dmul = 0f;
public const bool enablePID = true;

// experimental PID setting: I is multiplied by this each tick, so it shouldn't build up as much
public float idecay = 0.9f;







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
		enterStandby();
		return;
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


	checkNacelles();
	if(updateNacelles) {
		nacelles.Clear();
		nacelles = getNacelles();
		updateNacelles = false;
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

	if(myShipMass.BaseMass == 0) {
		Echo("Can't fly a Station");
		enterStandby();
		return;
	}

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









	// ========== OTHER ==========
	if(enablePID) {

		bool hasChanged = false;

		if(argument.Contains("pup")) {
			hasChanged = true;
			pmul += 0.1f;
		}
		if(argument.Contains("pdn")) {
			hasChanged = true;
			pmul -= 0.1f;
		}
		if(argument.Contains("iup")) {
			hasChanged = true;
			imul += 0.1f;
		}
		if(argument.Contains("idn")) {
			hasChanged = true;
			imul -= 0.1f;
		}
		if(argument.Contains("dup")) {
			hasChanged = true;
			dmul += 0.1f;
		}
		if(argument.Contains("ddn")) {
			hasChanged = true;
			dmul -= 0.1f;
		}

		if(pmul < 0) {
			pmul *= -1;
		}
		if(imul < 0) {
			imul *= -1;
		}
		if(dmul < 0) {
			dmul *= -1;
		}

		PIDContainer myPid = null;

		if(hasChanged) {
			myPid = checkPID(Me.CustomData, pmul, imul, dmul);
		} else {
			myPid = checkPID(Me.CustomData);
		}

		if(myPid != null) {
			pmul = myPid.p;
			imul = myPid.i;
			dmul = myPid.d;
			if(myPid.newText != null) {
				Me.CustomData = myPid.newText;
			}
		}

		if(Me.CustomData.Contains("ShowOnScreen")) {
			write($"p: {pmul}");
			write($"i: {imul}");
			write($"d: {dmul}");
		}
	}
	// ========== END OF OTHER ==========









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
public List<IMyProgrammableBlock> programBlocks = new List<IMyProgrammableBlock>();

public float oldMass = 0;

public int rotorCount = 0;
public int rotorTopCount = 0;
public int thrusterCount = 0;
public int screenCount = 0;
public int programBlockCount = 0;


public bool updateNacelles = false;
public bool standby = false;
public Vector3D shipVelocity = Vector3D.Zero;

public bool justCompiled = true;
public bool goToStandby = false;
public bool comeFromStandby = false;




public class PIDContainer {
	public string newText;
	public float p;
	public float i;
	public float d;
}


// public PIDContainer checkPID(string customData) {
// 	return checkPID(customData, null, null, null);
// }

public PIDContainer checkPID(string customData, float setp = -1, float seti = -1, float setd = -1) {

	string[] lines = customData.Split('\n');

	int markerLine = -1;
	int pLine = -1;
	int iLine = -1;
	int dLine = -1;
	bool reset_marker = false;
	bool fixPCaps = false;
	bool fixICaps = false;
	bool fixDCaps = false;
	string first2 = null;

	string pidMarker = "|PID|";
	string pMarker = "P:";
	string iMarker = "I:";
	string dMarker = "D:";

	// this is still O(lines.Length)
	for(int i = 0; i < lines.Length; i++) {
		if(lines[i].ToUpper().Contains(pidMarker)) {
			markerLine = i;

			if(!(lines[i] == pidMarker)) {
				// reset PID marker
				reset_marker = true;
			}
			// stop looking for "|PID|" and start looking for "P:"
			for(int j = i + 1; j < lines.Length; j++) {
				if(lines[j].Length >= 2) {
					first2 = lines[j].Substring(0, 2);
				} else {
					first2 = "";
				}
				bool gop = false;
				if(first2 == pMarker) {
					gop = true;
				} else if(first2.ToUpper() == pMarker) {
					// make p caps
					fixPCaps = true;
					gop = true;
				}

				if(gop) {
					pLine = j;

					// stop looking for "P:" and start looking for "I:"
					for(int k = j + 1; k < lines.Length; k++) {
						if(lines[k].Length >= 2) {
							first2 = lines[k].Substring(0, 2);
						} else {
							first2 = "";
						}
						bool goi = false;
						if(first2 == iMarker) {
							goi = true;
						} else if(first2.ToUpper() == iMarker) {
							// make i caps
							fixICaps = true;
							goi = true;
						}

						if(goi) {
							iLine = k;

							// stop looking for "I:" and start looking for "D:"
							for(int l = k + 1; l < lines.Length; l++) {
								if(lines[l].Length >= 2) {
									first2 = lines[l].Substring(0, 2);
								} else {
									first2 = "";
								}
								bool god = false;
								if(first2 == dMarker) {
									god = true;
								} else if(first2.ToUpper() == dMarker) {
									// make d caps
									fixDCaps = true;
									god = true;
								}

								if(god) {
									dLine = l;

									// stop looking for "D:"
									break;
								}
							}
							// stop looking for "I:"
							break;
						}
					}
					// stop looking for "P:"
					break;
				}
			}
			// stop looking for "|PID|"
			break;
		}
	}

	if(markerLine == -1) {
		// no pid settings
		if(setp == -1 && seti == -1 && setd == -1) {
			return null;
		}
	}




	float p_val = -1;
	bool reset_p = false;
	float i_val = -1;
	bool reset_i = false;
	float d_val = -1;
	bool reset_d = false;


	try {
		p_val = (float)Convert.ToDouble(lines[pLine].Substring(2));
		// no negatives
		if(p_val < 0) {
			reset_p = true;
			p_val *= -1;
		}

	} catch(Exception e) {
		// either wrong format or out of float domain or pLine is null
		reset_p = true;
		p_val = -1;
	}
	try {
		i_val = (float)Convert.ToDouble(lines[iLine].Substring(2));
		// no negatives
		if(i_val < 0) {
			reset_i = true;
			i_val *= -1;
		}

	} catch(Exception e) {
		// either wrong format or out of float domain or iLine is null
		reset_i = true;
		i_val = -1;
	}
	try {
		d_val = (float)Convert.ToDouble(lines[dLine].Substring(2));
		// no negatives
		if(d_val < 0) {
			reset_d = true;
			d_val *= -1;
		}

	} catch(Exception e) {
		// either wrong format or out of float domain or dLine is null
		reset_d = true;
		d_val = -1;
	}


	if(setp != -1) {
		p_val = setp;
		reset_p = true;
	}
	if(seti != -1) {
		i_val = seti;
		reset_i = true;
	}
	if(setd != -1) {
		d_val = setd;
		reset_d = true;
	}




	// woohoo, don't need regex... here it is anyway
	// string pattern = "[0-9]*.?[0-9]*";
	// var matches = System.Text.RegularExpressions.Regex.Matches("10.2 3.7 3", pattern);




	// Echo("End of reading, start writing");
	// end of reading, now to write





	bool consecutive =
		pLine == markerLine + 1 &&
		iLine == pLine + 1 &&
		dLine == iLine + 1;


	bool updateText =
		!consecutive ||
		fixPCaps ||
		fixICaps ||
		fixDCaps ||
		markerLine == -1 ||
		pLine == -1 ||
		iLine == -1 ||
		dLine == -1 ||
		reset_marker ||
		reset_p ||
		reset_i ||
		reset_d ||
		setp != -1 ||
		seti != -1 ||
		setd != -1 ||
		p_val == -1 ||
		i_val == -1 ||
		d_val == -1;


	PIDContainer outval = new PIDContainer();

	if(updateText) {

		// convert to linked list since we will probably add things in
		// Echo("convert to linked list since we will probably add things in");
		List<string> linesList = new List<string>();
		linesList.Capacity = lines.Length;
		for(int i = 0; i < lines.Length; i++) {
			linesList.Add(lines[i]);
		}


		// reset null ones
		// Echo("reset null ones");
		if(p_val == -1) {
			p_val = pmul;
		}
		if(i_val == -1) {
			i_val = imul;
		}
		if(d_val == -1) {
			d_val = dmul;
		}


		// insert missing lines
		// Echo("insert missing lines");
		if(markerLine == -1) {
			markerLine = linesList.Count;
			linesList.Add(pidMarker);
		}
		if(pLine == -1) {
			pLine = markerLine + 1;
			linesList.Insert(pLine, $"{pMarker} {p_val}");
			reset_p = false;
			fixPCaps = false;
		}
		if(iLine == -1) {
			iLine = pLine + 1;
			linesList.Insert(iLine, $"{iMarker} {i_val}");
			reset_i = false;
			fixICaps = false;
		}
		if(dLine == -1) {
			dLine = iLine + 1;
			linesList.Insert(dLine, $"{dMarker} {d_val}");
			reset_d = false;
			fixDCaps = false;
		}


		// reset syntax errors
		// Echo("reset syntax errors");
		if(reset_marker) {
			linesList[markerLine] = pidMarker;
			reset_marker = false;
		}
		if(reset_p) {
			linesList[pLine] = $"{pMarker} {p_val}";
			fixPCaps = false;
		}
		if(reset_i) {
			linesList[iLine] = $"{iMarker} {i_val}";
			fixICaps = false;
		}
		if(reset_d) {
			linesList[dLine] = $"{dMarker} {d_val}";
			fixDCaps = false;
		}

		// fix caps
		// Echo("fix caps");
		if(fixPCaps) {
			linesList[pLine] = linesList[pLine].ToUpper();
		}
		if(fixICaps) {
			linesList[iLine] = linesList[iLine].ToUpper();
		}
		if(fixDCaps) {
			linesList[dLine] = linesList[dLine].ToUpper();
		}






		// append all lines into the stringbuilder
		// Echo("append all lines into the stringbuilder");
		StringBuilder finalLines = new StringBuilder();
		string lastLine = null;
		for(int i = 0; i < linesList.Count; i++) {

			// remove the in-betweeners
			if(i > markerLine && i < pLine) continue;
			if(i > pLine && i < iLine) continue;
			if(i > iLine && i < dLine) continue;

			// add newlines between
			if(lastLine != null && lastLine.Length != 0 && lastLine[lastLine.Length - 1] != '\n') {
				finalLines.Append('\n');
			}

			// append the line
			finalLines.Append(linesList[i]);

			lastLine = linesList[i];
		}

		outval.newText = finalLines.ToString();
	}

	outval.p = p_val;
	outval.i = i_val;
	outval.d = d_val;

	return outval;
}

public void enterStandby() {
	standby = true;
	goToStandby = false;
	foreach(Nacelle n in nacelles) {
		n.rotor.theBlock.ApplyAction("OnOff_Off");
		foreach(Thruster t in n.thrusters) {
			t.theBlock.ApplyAction("OnOff_Off");
		}
	}
	Runtime.UpdateFrequency = UpdateFrequency.None;

	Echo("Standing By");
	write("Standing By");
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

	bool changeDampeners = false;
	if(arg.Contains(dampenersArg.ToLower())) {
		dampeners = !dampeners;
		changeDampeners	= true;
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
		if(onlyMainCockpit || mainController != null && mainController.IsUnderControl) {
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
	if(mainController != null && (mainController.IsUnderControl || onlyMainCockpit)) {
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

bool getControllers() {
	var blocks = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);

	return getControllers(blocks);
}

bool getControllers(List<IMyShipController> blocks) {
	mainController = null;

	usableControllers.Clear();

	string reason = "";
	for(int i = 0; i < blocks.Count; i++) {
		bool canAdd = true;
		reason += blocks[i].CustomName + "\n";
		if(!blocks[i].ShowInTerminal) {
			reason += "  ShowInTerminal not set\n";
			canAdd = false;
		}
		if(!blocks[i].CanControlShip) {
			reason += "  CanControlShip not set\n";
			canAdd = false;
		}
		if(!blocks[i].ControlThrusters) {
			reason += "  can't ControlThrusters\n";
			canAdd = false;
		}
		if(blocks[i].IsMainCockpit) {
			mainController = blocks[i];
		}
		if(canAdd) {
			usableControllers.Add(blocks[i]);
		}
	}

	if(usableControllers.Count == 0) {
		Echo("ERROR: no usable ship controller found");
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
public void checkNacelles() {
	var blocks = new List<IMyTerminalBlock>();
	Echo("Checking Nacelles..");

	IMyShipController cont = findACockpit();
	if(cont != null) {
		MyShipMass shipmass = cont.CalculateShipMass();
		if(oldMass == shipmass.BaseMass) {
			Echo("Mass is the same, everything is good.");

			// they may have changed the screen name to be a VT one
			getScreens(screens);
			return;
		}
		Echo("Mass is different, checking everything.");
		oldMass = shipmass.BaseMass;
	} else {
		Echo("No cockpit registered, checking everything.");
	}

	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, block => (block is IMyShipController || block is IMyThrust || block is IMyMotorStator || block is IMyTextPanel || block is IMyProgrammableBlock));
	List<IMyShipController> conts = new List<IMyShipController>();
	List<IMyMotorStator> rots = new List<IMyMotorStator>();
	List<IMyThrust> thrs = new List<IMyThrust>();
	List<IMyTextPanel> txts = new List<IMyTextPanel>();
	List<IMyProgrammableBlock> programBlocks = new List<IMyProgrammableBlock>();

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


	// if you use the following if statement, it won't lock the non-main cockpit if someone sets the main cockpit, until a recompile or world load :/
	if(/*(mainController != null ? !mainController.IsMainCockpit : false) || */controllers.Count != conts.Count) {
		Echo($"Controller count ({controllers.Count}) is out of whack (current: {conts.Count})");
		getControllers(conts);
	}

	if(screenCount != txts.Count) {
		Echo($"Screen count ({screenCount}) is out of whack (current: {txts.Count})");
		getScreens(txts);
	} else {
		foreach(IMyTextPanel screen in txts) {
			if(!screen.IsWorking) continue;
			if(!screen.CustomName.ToLower().Contains(LCDName.ToLower())) continue;
			getScreens(txts);
		}
	}

	if(rotorCount != rots.Count) {
		Echo($"Rotor count ({rotorCount}) is out of whack (current: {rots.Count})");
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
		Echo($"Rotor Head count ({rotorTopCount}) is out of whack (current: {rotorHeads.Count})");
		Echo($"Rotors: {rots.Count}");
		updateNacelles = true;
		return;
	}

	if(thrusterCount != thrs.Count) {
		Echo($"Thruster count ({thrusterCount}) is out of whack (current: {thrs.Count})");
		updateNacelles = true;
		return;
	}

	Echo("They seem fine.");
}

public bool init() {
	Echo("Initialising..");
	nacelles.Clear();
	nacelles = getNacelles();
	if(!getControllers()) {
		Echo("Init failed.");
		return false;
	}
	Echo("Init success.");
	return true;
}

// G(thrusters * rotors)
// gets all the rotors and thrusters
List<Nacelle> getNacelles() {
	gotNacellesCount++;
	var blocks = new List<IMyTerminalBlock>();
	var nacelles = new List<Nacelle>();
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
		} else/* if(blocks[i] is IMyMotorStator) */{
			rotors.Add((IMyMotorStator)blocks[i]);
		}
		blocks.RemoveAt(i);
	}
	rotorCount = rotors.Count;
	thrusterCount = normalThrusters.Count;
	blocks.Clear();



	Echo("Getting Rotors");
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
		Rotor rotor = new Rotor(current, this);
		nacelles.Add(new Nacelle(rotor, this));
	}

	Echo("Getting Thrusters");
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

public class PID {

	public Program prog;

	public float pmul = 1;
	public float imul = 0;
	public float dmul = 0;

	private double lasterror = 0;
	private double integral = 0;

	public PID(Program prog) {
		this.prog = prog;
	}

	public PID(float proportional, float integral, float derivative, Program prog) : this(prog) {
		this.pmul = proportional;
		this.imul = integral;
		this.dmul = derivative;
	}

	public double update(double setpoint, double measured) {
		double error = setpoint - measured;
		return update(error);
	}

	public double update(double error) {
		double deltaT = prog.Runtime.TimeSinceLastRun.TotalMilliseconds;
		deltaT = (deltaT == 0 ? 1 : deltaT);

		integral *= prog.idecay;
		integral += error/deltaT;
		double derivative = (error - lasterror) / deltaT;

		// return error * pmul + integral * imul + derivative * dmul;
		if(!enablePID) {
			return error;
		}
		return error * pmul + integral * imul + -1 * derivative * dmul;
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

	public Program prog;

	public Vector3D direction = Vector3D.Zero;//offset relative to the head

	public string errStr = "";

	public PID positionController;

	public Rotor(IMyMotorStator rotor, Program prog) {
		this.theBlock = rotor;
		this.positionController = new PID(1, 0, 0, prog);
		this.prog = prog;
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
		double err = angle.Dot(rotor.WorldMatrix.Up) * errorScale * multiplier;


		PIDContainer vals = prog.checkPID(theBlock.CustomData);
		if(vals != null) {
			positionController.pmul = vals.p;
			positionController.imul = vals.i;
			positionController.dmul = vals.d;
			if(vals.newText != null) {
				theBlock.CustomData = vals.newText;
			}
		} else {
			positionController.pmul = prog.pmul;
			positionController.imul = prog.imul;
			positionController.dmul = prog.dmul;
		}
		err = positionController.update(err);

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