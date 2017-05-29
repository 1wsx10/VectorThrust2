
public const string resetArg = "%reset";

public const float maxRotorRPM = 60;

public Program() {
    init();
}
public void Save() {}

public List<Nacelle> nacelles;
public IMyShipController controller;

public void Main(string argument) {
	if(argument.Equals(resetArg)) {
		init();
	}

	displayNacelles(nacelles);

	foreach(Nacelle n in nacelles) {
		n.rotor.setFromVec(controller.GetNaturalGravity());
	}
}

public void init() {
	nacelles = getNacelles();
	controller = getController();
}

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

// G(thrusters * rotors)
// gets all the rotors and thrusters
List<Nacelle> getNacelles() {
	var blocks = new List<IMyTerminalBlock>();
	var nacelles = new List<Nacelle>();
	bool flag;

	// get rotors
	GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(blocks);
	foreach(IMyMotorStator r in blocks) {
		if(false/* TODO: set to not be in a nacelle */) {
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
		if(false/* TODO: set to not be in a nacelle */) {
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
		Echo($"\nRotor Name: {n.rotor.theBlock.CustomName}");
		// n.rotor.theBlock.SafetyLock = false;//for testing
		// n.rotor.theBlock.SafetyLockSpeed = 100;//for testing

		n.rotor.getAxis();
		// Echo($@"rotor axis: {Math.Round(n.rotor.wsAxis.Length(), 3)}");
		// Echo($@"rotor axis: {n.rotor.wsAxis.Length()}");
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

	public Vector3D wsAxis;// axis it rotates around in worldspace
	public float offset = 0;

	public string errStr = "";

	public Rotor(IMyMotorStator rotor) {
		this.theBlock = rotor;
		getAxis();
	}

	// gets the rotor axis (worldmatrix.up)
	public void getAxis() {
		this.wsAxis = theBlock.WorldMatrix.Up;//this should be normalized already
		if(Math.Round(this.wsAxis.Length(), 6) != 1.000000) {
			errStr += $"\nERROR (getAxis()):\n\trotor up isn't normalized\n\t{Math.Round(this.wsAxis.Length(), 2)}";
			this.wsAxis.Normalize();
		}
	}

	// this sets the rotor to face the desired direction in worldspace
	// desiredVec doesn't have to be in-line with the rotors plane of rotation
	public void setFromVec(Vector3D desiredVec) {
		desiredVec = Vector3D.Reject(desiredVec, wsAxis);
		if(Vector3D.IsZero(desiredVec) || !desiredVec.IsValid()) {
			errStr += $"\nERROR (setFromVec()):\n\tdesiredVec is invalid\n\t{desiredVec}";
			return;
		}

		// angle between vectors
		float angle = -(float)Math.Acos(angleBetweenCos(theBlock.WorldMatrix.Forward, desiredVec));

		if(Math.Acos(angleBetweenCos(theBlock.WorldMatrix.Left, desiredVec)) > Math.PI/2) {
			angle = (float)(2*Math.PI - angle);
		}

		setPos(angle + (float)(offset * Math.PI / 180));
	}

	// gets cos(angle between 2 vectors)
	// use Acos to get the angle
	public double angleBetweenCos(Vector3D a, Vector3D b) {
		double dot = Vector3D.Dot(a, b);
		double Length = a.Length() * b.Length();
		return dot/Length;
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
	void setPos(float x)
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