#Vector Thrust
###put your thrusters on rotors!

##MAIN SETUP
1. put %VectorTim in the name of a timer block and set it to run the programmable block with default argument
2. paste the contents of VectorThrust.cs into the programmable block, then click 'check code' and 'save & exit'
~~3. run the programmable block~~ it should be already running
4. put thrusters on rotors
5. setup your buttons.. either use 'Control Module' by DIGI or make your cockpit buttons with various actions
6. get in and hit the jetpack button!

##VANILLA BUTTONS SETUP
1. get in your cockpit, press G
2. drag the programmable block to the bar and select "RUN"
3. copy&paste the code for your controls. you will need:
%dampeners
%jetpack
%standby			this completely stops the script, till you press the button again (and also safely turns off thrusters & rotors)
%raiseAccel
%lowerAccel
%resetAccel
the last 3 control your "Target Accel" value and are optional

##CONTROL MODULE SETUP
make sure the mod is installed: just subscribe then add it to the mod list in world options
this will add settings to the programmable block. set these:
1. Monitored inputs: read all inputs
2. Trigger on state: pressed and released
3. Repeat interval: 0.016 (you can type it by ctrl+click the slider bar)

this sets the following buttons:
suit jetpack key: engines on/off
inertia dampeners key: inertia dampenerse on/off
+:	increase target acceleration
-:	decrease target acceleration
0:	reset target acceleration

##INFO PANEL SETUP:
1. place a text panel
2. put %VectorLCD in the name
3. set it to show text on screen
