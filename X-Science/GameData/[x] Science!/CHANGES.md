[x] Science!
============

5.9
---
20-July-2017 Built against KSP V1.3.0
* Rewrote window settings
* Added selected object information window to tracking station scene
* Added body information to selected object window
* Added vehicle information to selected object window - based on "Resource Details in Tracking Center" by avivey
* Setting to turn off selected object window
* Added StationScience experiments to science.cfg as requested
* Bug fix - Checklist window didn't open on KSC screen after loading saved game.
  Calling GameSceneSwitch event after mod becomes "_active=true" to enforce visibility



5.8
---
28-June-2017 Built against KSP V1.3.0
* Save windows settings more often
* Window settings for Here&Now
* New icons on Here&Now window
* Changed button code to allow right clicks
* Right click to mute music
* Right click to open Here&Now
* Hide Here&Now icon when using right click
* Option for mute music at start



5.7
---
26-May-2017 Built against KSP V1.3.0



5.6
---
04-Apr-2017 Built against KSP V1.2.2
* Situation bug on checklist window in KSC scene, now correctly becomes blank.
* Reading stored setting for checklist filter button correctly
* Orbital Science support for Here&Now trigger buttons
* Only using stock calls for triggering science if we can't use DMagic ones (seems to be best)



5.5
---
09-Mar-2017 Built against KSP V1.2.2
* Ui Scaling contributed by jsmcconn
* Text Filters in science.cfg
* Difficult science filter
* Updated help



5.4
---
07-Dec-2016 Built against KSP V1.2.2



5.3
---
03-Nov-2016 Built against KSP V1.2.1
* Bug fix - Icons invisible after going to MAIN MENU
* Bug fix - science.cfg - changing "scopeScan" into "dmReconScan", as the radial telescope uses normal biomes. 
* Display situation on Here&Now Window + MouseOver - number of experiments stored
* Progress bars for experiments on Here&Now window.
* Message when entering a new biome.


5.2
---
24-Oct-2016 Built against KSP V1.2.0
* Fix for Contract Configurator
* Don't rely on KSC being in Kerbin+Shores
* Works with LTech

5.1
---
13-Oct-2016 Built against KSP V1.2.0
* Rebuilt for KSP 1.2
* Bug fixes
* New settings, without Gui

5.0
---
11-Sept-2016 Built against KSP V1.1.3
* New window a bit like Science Alert

4.20
---
22-June-2016 Built against KSP V1.1.3
* Fix for science.cfg not applying to KSC baby biomes
* Fix for remaining science tooltip.

4.19
---
20-June-2016 Built against KSP V1.1.2
* New button code to handle multiple buttons - ready for V5
* New artwork for icons
* New title bar icons
* Compact mode use same icons as maximised mode
* Separate settings window
* New help window
* New resizable window class - not on main window
* Science totals tool tip contains onboard science
* Filter text + state in settings

4.18
---
24-May-2016 Built against KSP V1.1.2
* Remembers window positions
* Changed location of settings file
* New tool tip for experiment count and value
* Current situation text word-wraps
* Removed coroutines - refreshes from Update()
* New event handler class

4.17
---
03-May-2016 Built against KSP V1.1.2
* Built against KSP V1.1.2

4.16
---
29-Apr-2016 Built against KSP V1.1.1
* Built against KSP V1.1.1

4.15
---
20-Apr-2016 Built against KSP V1.1.0
* Built against KSP V1.1.0


4.14
---
26-Jan-2016 Minor GUI changes KSP V1.0.5
* Supports F2
* More tooltips
* Close button z-sorts correctly


4.12
---
26-Nov-2015 Bug fix version KSP V1.0.5
* Only initialise if we need to.  Avoids an exception when moving scene from settings to main menu


4.11
---
10-Nov-2015 Updated for KSP V1.0.5
* New situation filters
* Recompiled for KSP V1.0.5


4.10
---
4-Oct-2015 Bug fix version for KSP V1.0.4
* Fix bug where situation filters aren't applied to KSC baby biomes


4.9
---
29-Sept-2015 Celestial Body filters for KSP V1.0.4
* A fairly major code reorganisation
* A new config file for filtering science according to the properties of celestial bodies


4.8
---
03-Aug-2015 Bug fixes + modified filters for KSP V1.0.4
* Loading settings is a bit more robust
* Fixed an exception if Blizzy78's toolbar is missing
* Better analysis of planet's properties, should work better with modified solar system.
* Filtered results includes only biomes of visited bodies
* Option to hide the "Show All" button to prevent discovery of biomes on unexplored bodies


4.7
---
3-July-2015 New settings for KSP V1.0.4
* Option to check for science in debris
* Option to consider science "completed" before recovery


4.6
---
24-June-2015 Gui bug fix for KSP V1.0.4
* Completed experiments total is updated correctly when vehicles are recovered
* Window is closed on main menu and certain other scenes
* Added close button to window


4.5
---
12-June-2015 Speed increase and bug fix for KSP V1.0.2
* Handles building upgrades in career mode - surface samples listed as appropriate, baby biomes appear and disappear as appropriate
* Better use of indexed lists means it is much faster 


4.4
---
15-May-2015 Speed increase for KSP V1.0.2
* No longer checks for science stored in debris.
* Only gets the science subjects once and indexes the list for efficient queries.


4.3
---
5-May-2015 Fix and rebuild for KSP V1.0.2
* Changed blizzy icon text - doesn't say 'test' any more!


4.2
---
1-May-2015 Fix and rebuild for KSP V1.0.1 by Z-Key Aerospace


4.1
---
28-April-2015 Fix and rebuild for KSP V1.0 by Z-Key Aerospace


4.0
---
* Added support for blizzy's toolbar - find the toggle in the settings menu if you've got it installed.
* Changes to the settings are now saved across games.

3.1
---
* Can now use the - symbol to do NOT searches e.g. "goo -Minmus".
* Added compact mode.
* Moved the complete experiment filter option to the settings panel.
* Now only updates when the window is open.
* Fixed ArgumentNullException when running alongside Karbonite (thanks Jaxx).
* Fixed experiments not being detected in biomes with spaces in them (thanks Tahib).
* Now refreshes the experiment cache far less frequently - only once per scene instead of whenever science is obtained or the active vessel is changed.

3.0
---
* Added onboard science detection.
* Added support for CustomBiomes.
* Fixed experiments not showing up as complete correctly due to a floating point rounding error.

2.0
---
* Implemented current situation detection and filter.
* Added tooltips to buttons.
* Completed experiment progress bars are now rendered in a different colour.
* Replaced text on filter buttons with icons.
* Fixed several memory leaks.
* Fixed addon being loaded multiple times.

1.1
---
* Mod now correctly disables itself in Sandbox mode (stops the VAB lockup bug).
* Can now use the | symbol to do OR searches e.g. "goo Mun|Minmus".
* Optimized experiment list rendering.
* Will automatically detect OrbitalScience if installed, and add its experiments to the list.
* Biomes are now displayed in the experiment list correctly.
* Fixed some experiments not appearing in the list if they took biomes into account.
* Fixed "while landed at sun" experiments appearing in the list.
* Fixed science levels not taking game difficulty settings into account.

1.0
---
* Added search bar.
* Added science values to experiment progress bars.
* Fixed window remaining hidden when returning to the main menu.

0.3
---
* Recompiled for KSP 0.25.0.

0.2
---
* Implemented active vessel detection.

0.1
---
* Initial commit.
