Welcome to the Playlist Readme! Before we get started, here's a reminder!

--REMINDER--
This README is heavily WIP. I could be wrong with the following on this matter, but from my research, and as of right now, playlists are only functional in Garden Warfare 2. In Garden Warfare 1, you can load a playlist, however, it'll only load the first level, and when the match concludes, it'll loop to the first level listed in the playlist, completely ignoring all other levels below it.
IsMixed, the option to randomize maps and gamemodes, doesn't work too. Any attempts I've tried, I'd get the server running, however, it would immediately crash. 

Now that you're aware, lets get started! 
Within the Playlist folder, You'll find 1 playlist that was made to recreate the multiplayer rotation of Turf Takeover.

Playlists, as of now, that use PlaylistRotation, start with the following:
LevelName,
GameMode,
TOD,
SettingsToApply,

LevelName is the Level you want to load into. HOWEVER, unlike the command, where you can have just a Level name, like Level_Rush_Snow. You'll need the entire level's path, which is equivalent to Levels/Level_Rush_Snow/Level_Rush_Snow. You'll find all the full level paths at the bottom of this Readme.

GameMode is the Game Mode the level loads in to.
TOD is the Time of Day, whether it's Day or Night
SettingsToApply are game changing settings/modifiers that'll be applied to the server.

Remember, If you want settings, like lets say for this example, crazy settings! Say you want them only enabled for one match, then disabled. You'll throw them inside the SettingsToApply of the level. Each setting is separated by a Vertical Bar, or |

GameMode.CrazyOption8 true|GameMode.CrazyOption3 true

If you want to disable them the next round or further, make sure to reference them, but to set them false.