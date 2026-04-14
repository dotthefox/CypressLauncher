Hiiyaa! What's good?! Welcome to the AntiCheat Readme for GW2! 

Due to the current state of Garden Warfare 2's online multiplayer, dylannws has worked on a server-sided anti-cheat system for GW2 Private Servers! I'm really, really proud of his work on this.

From my understanding, here's the following measures that've been implemented.
- Client Buff Blocking <-- Prevent buffs initiated from a player's client being applied to themselves or other players, completely breaking a lot of MP Ruining mods
- EventSync Blacklisting <-- Prevents EventSyncs skids use to immediately end rounds, pause timers, and so on from initiating
- Invalid Loadout Blocking <-- Prevents skids from using invalid kit loadouts (no more boss mods)
- Client Level Load Blocking <-- Prevents a level from being loaded to the server off a player's client
- Player Swap Blocking <-- Prevents Player Swap from being initiated, Player Swap being the gimmick in Solo Ops allowing you to take control of your AI Teammates.
- Self Revive Blocking <-- Prevents skids from reviving themselves upon death
- Server Crash Blocking <-- Prevents methods skids use to crash the server
- Alive Weapon Change Blocking <-- Prevents skids from being able to change loadout while alive.

Here are commands to enable/disable these measures. If you want to disable them at any time, be sure to add False after the command, if you want to enable them, type the command exactly how it is below, DO NOT ADD TRUE

Anticheat.Enabled - Enables the Anticheat
Anticheat.Verbose - Enables Anticheat Verbose, good for debugging, and also tells you who exactly is attempting to do what
Anticheat.PreventClientBuffs - Enables measure to prevent Buffs from a player's client being initiated
Anticheat.PreventBlacklistedEventSyncs - Enables measures to prevent Blacklisted Event Syncs (Pausing Timer, Ending Round for example) from being initatied
Anticheat.PreventInvalidLoadouts - Enables measures to prevent players from using an invalid loadout compared to the server's initial loadout (no more boss mods)
Anticheat.PreventPlayerSwap - Enables measures to block Player Swap.
Anticheat.PreventAliveWeaponChange - Enables measures to prevent players from changing their loadout while being alive
Anticheat.PreventSelfRevive - Enables measures to prevent players from using self revive after death
Anticheat.PreventServerCrash - Enables measures to prevent methods skids use to crash the server.

Good luck, and have fun hosting servers!
