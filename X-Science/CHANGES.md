[x] Science!
============


4.6
---
24-June-2015 Gui bug for KSP V1.0.4
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
