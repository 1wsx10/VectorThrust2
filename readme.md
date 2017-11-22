# Vector Thrust
### put your thrusters on rotors!

## MAIN SETUP
1. paste the contents of VectorThrust.cs into the programmable block (click edit, not custom data, replace what is in there normally), then click 'check code' and 'save & exit'
2. ~~run the programmable block~~ it should be already running
3. setup your buttons.. either use 'Control Module' by DIGI or make your cockpit buttons with various actions. you will have to use the vanilla method for the standby action.
4. get in and hit the jetpack button!

## VANILLA BUTTONS SETUP
1. get in your cockpit, press G
2. drag the programmable block to the bar and select "RUN"
3. copy&paste the code for your controls. you will need:
* %dampeners
* %jetpack
* %standby
   
   standby completely stops the script, till you press the button again (and also safely turns off thrusters & rotors) **please use it in multiplayer**
* %raiseAccel
* %lowerAccel
* %resetAccel

the last 3 control your "Target Accel" value and are optional

## CONTROL MODULE SETUP
just install the mod and you're good to go!

### BUTTONS

* __suit jetpack key__:		engines on/off
* __inertia dampeners key__:	inertia dampenerse on/off
* __+__:			_increase target acceleration_
* __-__:			_decrease target acceleration_
* __0__:			_reset target acceleration_

there is currently no binding for standby, so you will have to set that up with the vanilla method

## INFO PANEL SETUP
#### while this is optional, i highly recommend it
1. place a text panel
2. put %VectorLCD in the name

## BUGS
unfortunately there seems to be a physics bug in space engineers at the moment which causes thrusters on rotors to spaz out in some situations. i have reproduced it on a ship without the script whatsoever, so its not my script doing it

### workaround
so far, i have narrowed it down to the thrusters being in-line with the centre of mass... that's pretty ironic because this update is supposed to remove that requirement.

just try to keep the rotors in-line with the centre of mass and it should be fine
