--DISCLAIMER--
It is advised you have EA Desktop opened before starting or joining a server. This is due to the game having internal checks. If it doesn't detect EA Desktop opened, with an account that legitimately owns the game, none of the game's content will load, and you will end up in a black screen. 

Hiya! Welcome to the ReadMe for setting up a private server! In this ReadMe, we are going to go over two methods of setting servers up.



1: Port Forwarding
2: Radmin (or anything similar)


-- 1 - PORT FORTWARING --
For this method, players will connect to you through your router's IP Address.

1. In Command Prompt, enter the command, ipconfig. This will display current your current IP configuration. You'll have to find your router's Default Gateway, The correct one will be listed under what you're currently connected to, whether it's through Ethernet or WiFi. Copy the gateway into your preferred browser. 

2. In most cases, the gateway will ask for a password before you continue. This password is normally somewhere labeled on your router. Enter the password, and enter through

3. This varies from router to router, and how the gateway's set up, but normally, it is usually stored under Security settings. You might have to search where Port Forward settings are in your router's gateway.

4. Once you access the settings to Port Forward, here are some configurations you'll need to set up.

Give the configuration an application name.
Anything relating to ports, set them to 25200, as that's the default port for all PvZ Shooters. Unless you want to host multiple servers, then make a set range, with 25200 being the start port.
If there's anything regarding Protocols, set it to UDP.
If there's anything regarding Device IP, or IP Address, or anything similar, that is your IPv4 address, again, found via ipconfig in command prompt. Under the same category you found the Default Gateway.
If there's an option to enable the setup, enable it, of course.

5. For the Device IP in the launcher, that is where your IPv4 address goes. 
NOTE: Every so often, the IPv4 will change, so don't fret if your dedicated server closes moments after launching! You'll have to check it every so often to ensure the server's listening to the right address!

Save once you're done. 

On the right side of the 
For information on setting up the Level and Inclusion, check out the LevelInfo Readmes.
If you wish to add a password, set up a password in the Set Server Password box. Leave it blank if you don't want a password.
If you wish to launch a server with mods, head to the furthest left of the launcher, click Use Mods, and select the ModPack.
Once everything is set, click Start Server! If everything's been done right, you should see a big white box, that is the thin client of the game that acts as your server!
Be sure to allow any Firewall permissions if there are any.

Congratulations, if done properly, you should be able to host dedicated servers! For people to join you, you'll have to provide them your router's public IP address. Quickest way to find what your public IP is through IP Chicken at https://ipchicken.com/ If you want your IP to have a unique hostname, you can use something like no-ip at https://www.noip.com/


-- 2 - RADMIN --
NOTE: If you plan to host a server using Radmin, anyone who wants to join you must also install Radmin, and join your network.

1. Install Radmin if you haven't. You can get it at https://www.radmin-vpn.com/

2. After Radmin is installed and opened, click on Network tab, and click Create Network. You'll need to make a Network Name and password that you'll share with other people who wish to join you.

3. After making a Network, Copy the IP Address that is at the top, below your Desktop name, and in the launcher set that as your Device IP.

For information on setting up the Level and Inclusion, check out the LevelInfo ReadMes.
If you wish to add a password, set up a password in the Set Server Password box. Leave it blank if you don't want a password.
If you wish to launch a server with mods, head to the furthest left of the launcher, click Use Mods, and select the ModPack.
Once everything is set, click Start Server! If everything's been done right, you should see a big white box, that is the thin client of the game that acts as your server!
Be sure to allow any Firewall permissions if there are any.

If you want others to join you, Make sure they are using Radmin as well, have them join your network, and provide them the IP Radmin assigned to you.


If you want to join servers, Check out the JoiningReadMe!