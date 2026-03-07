Hey! Welcome to the README of the Dedicated Server Window! If you're wanting to know about the special integrated commands, you came to the right place!

Around the bottom of the server window, right above all the information such as FPS, Level, and so on is text box you can type in. There are certain commands you can input!
Here are a list of special server commands available for Garden Warfare 1 and 2!

NOTE: These commands are case sensitive, meaning you have to type them out exactly as they're displayed here, otherwise it will not work. These will

Garden Warfare:
Server.RestartLevel
Server.LoadLevel
Server.KickPlayer
Server.KickPlayerById
Server.BanPlayer
Server.BanPlayerById

Garden Warfare 2: 
Server.RestartLevel
Server.LoadLevel
Server.KickPlayer
Server.KickPlayerById
Server.BanPlayer
Server.BanPlayerById
Server.Say


Here's a cheat sheet dictionary of what all of these commands do, and their needed indexes.

Server.RestartLevel - Restarts the current level the server is hosting
Server.LoadLevel - Loads the level of a server, requires the level and inclusion - EXAMPLE: Server.LoadLevel Level_FE_Hub GameMode=FreeRoam;TOD=Day;HostedMode=ServerHosted - More info can be found within the Level Info readmes.
Server.KickPlayer - Kicks a player out of the server based on name - EXAMPLE: Server.KickPlayer Jim
Server.KickPlayerById - Kicks a player out of the server based on the Player ID the server assigned them - EXAMPLE: Server.KickPlayerById 4
Server.BanPlayer - Bans a player out of the server based on name - EXAMPLE: Server.BanPlayer Jim
Server.BanPlayerById - Bans a player out of the server based on the Player ID the server assigned them - EXAMPLE: Server.BanPlayerById 4
Server.Say - Displays a message to everyone's screen for a certain amount of time. - EXAMPLE: Server.Say "Hello World!" 5


You can also initiate settings found within settings_Win32! - EXAMPLE: GameMode.ModeTeamId 2