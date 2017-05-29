
public const string resetArg = "%reset";

public Program() {
    init();
}
public void Save() {}

public List<Nacelle> nacelles;
public void Main(string argument) {
	if(argument.Equals(resetArg)) {
		init();
	}

	displayNacelles(nacelles);
}

public void init() {
	nacelles = getNacelles();
}

// G(thrusters * rotors)
// gets all the rotors and thrusters
List<Nacelle> getNacelles() {
	var blocks = new List<IMyTerminalBlock>();
	var nacelles = new List<Nacelle>();
	bool flag;

	// get rotors
	GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(blocks);
	foreach(IMyMotorStator r in blocks) {
		if(false/* set to not be in a nacelle */) {
			continue;
		}

		//if topgrid is not programmable blocks grid
		if(r.TopGrid.EntityId == Me.CubeGrid.EntityId) {
			continue;
		}

		// it's not set to not be a nacelle rotor
		// it's topgrid is not the programmable blocks grid
		Rotor rotor = new Rotor(r);
		nacelles.Add(new Nacelle(rotor));
	}
	blocks.Clear();

	// get thrusters
	GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
	foreach(IMyThrust t in blocks) {
		if(false/* set to not be in a nacelle */) {
			continue;
		}

		// get rotor it belongs to
		Nacelle nacelle = new Nacelle();// its impossible for the instance to be kept, this just shuts up the compiler
		IMyCubeGrid grid = t.CubeGrid;
		flag = false;
		foreach(Nacelle n in nacelles) {
			IMyCubeGrid rotorGrid = n.rotor.theBlock.TopGrid;
			if(rotorGrid.EntityId == grid.EntityId) {
				flag = true;// flag = 'it is on a rotor'
				nacelle = n;
				break;
			}
		}
		if(!flag) {// not on any rotor
			continue;
		}

		// it's not set to not be a nacelle thruster
		// it's on the end of a rotor
		Thruster thruster = new Thruster(t);
		nacelle.thrusters.Add(thruster);

	}
	blocks.Clear();

	return nacelles;
}

void displayNacelles(List<Nacelle> nacelles) {
	foreach(Nacelle n in nacelles) {
		Echo("\nRotor Name: "+n.rotor.theBlock.CustomName);

		Echo("Thrusters:");
		int i = 0;
		foreach(Thruster t in n.thrusters) {
			Echo(i + ": " + t.theBlock.CustomName);
			i++;
		}
	}
}

public class Nacelle {

	// physical parts
	public Rotor rotor;
	public List<Thruster> thrusters;

	public Nacelle() {}// don't use this if it is possible for the instance to be kept
	public Nacelle(Rotor rotor) {
		this.rotor = rotor;
		this.thrusters = new List<Thruster>();
	}
}

public class Thruster {
	public IMyThrust theBlock;

	public Thruster(IMyThrust thruster) {
		this.theBlock = thruster;
	}
}

public class Rotor {
	public IMyMotorStator theBlock;
	// don't want IMyMotorBase, that includes wheels

	public Rotor(IMyMotorStator rotor) {
		this.theBlock = rotor;
	}
}