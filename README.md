# ManiaModGamepad

An unofficial osu!mania conversion mod for gamepad players.

## Install

This mod is only available for osu!lazer. Please also check [Limitations](#limitations)
to see if this mod suits your play style.

[![Demo](https://img.youtube.com/vi/NcdVmPx8OHY/hqdefault.jpg)](https://youtu.be/NcdVmPx8OHY?si=Jev-6CwarAEA-75K)

###### Harmony

Download the latest release of [Harmony](https://github.com/pardeike/Harmony/releases/latest)
and copy `0Harmony.dll` to the `rulesets` folder, in your osu!lazer installation.  
The folder can be found by clicking `Open osu! folder` in the setings sidebar.

This only needs to be downloaded once, unless this mod has issues with the current version of
Harmony being used.

###### Installing the mod

Download the latest [release](https://github.com/Desdaemon/ManiaModGamepad/releases/latest) and copy `osu.Game.Rulesets.ManiaModGamepad.dll` to
the same `rulesets` folder as above.

## Limitations

- This mod is a work in progress! Things may be broken, please don't hesitate to open a new issue/PR if you see one.  
  This includes issues with non-conformance to the [Rule of Two](docs/RULE_OF_TWO.md) or Autoplay failing to get 100%.
- This mod loads itself as a non-functioning custom ruleset, so ignore the warning about the ruleset not being able to load
  when first opening osu!.
- This mod is not yet sanctioned for use by the official osu!lazer client. As such, you will encounter issues with attempting
  to register scores or replays. If you need replays, log out before using this mod.
- This mod currently only works for 6K. If you play converted maps, be sure to also enable the 6K mod alongside this mod.
  More key modes will be supported in the future, or help us by contributing a PR!
- This mod was designed with *bumper layout* in mind; i.e. <kbd>⬅️ LB ➡️ X RB B</kbd> instead of the *face-button layout*
  <kbd>⬅️ ⬆️ ➡️ X Y B</kbd>.  
  While both are playable with this mod, you might find some patterns much more difficult than would be done with the
  bumper layout.

## Development

.NET Framework 8.0 is required. Follow instructions in [lib/README.md](lib/README.md) and run `dotnet restore`.
