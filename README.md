## Synopsis

AndCEC is .NET program that is used to control Windows machine through my Android Television's remote controller (2k14 Philips 48pfs8109 Android 4.2.2).
Since it's rare to HTPC or desktop PC's have Consumer Electronics Control (CEC, Easylink) feature over HDMI this program solves the problem over Android Debug Bridge (ADB).
ADB is connected to television and remote controller's input events are captured to PC.

##Features ATM

Full qwerty on remote's backside, SIFT and ALT meta keys need be still implemented to get all special chars. Somewhat working mouse control with left click.Fully usable but not ideal because the relative data from TV.
volume and other media keys. One click ignore button to stop/start interacting with PC if doing something else with TV. Color coded buttons to launch applications.
In my case yellow Launches Plex HT and green Switches/extends/dublicates image between my monitor and TV
Every key is programmable to needed purpose. 

## Installation

Full VS solution/project. ADB from android SDK must be installed and path set and debug mode enabled from TV. Some key mappings (mainly qwerty) are stored in layout.txt
TV's IP address and other spesific info are not generic and must be changed to proper values to work. 

## License

MIT