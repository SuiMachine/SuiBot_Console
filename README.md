﻿SuiBot
=====================
SuiBot is simplistic IRC bot written in C#.

Compilation
-------
Just download the code, open TwitchBotConsole.csproj with a either Visual Studios 2013 (or newer) or Xamarin Studio / Mono Develop.
First start may seem like a crash, but it should create a config file, where you can type in your username, password, channel the bots should join etc.
Once you've edited the config file, you should be good to go.

Note: Running ask module with CleverBot via Mono, may require Mono Beta, due to issues with SSL certificates.


Credits
-------
  * [SuicideMachine](http://twitch.tv/suicidemachine)
  * [HardlySober](https://www.youtube.com/watch?v=Ss-OzV9aUZg) - who provided a base on which this bot was built.
  * Bot uses [SpeedrunComSharp](https://github.com/LiveSplit/SpeedrunComSharp) libary, written by CryZe.
  * Thanks to 5paceToast for cleaning up my main method.
  * Since version 1.1 (Horace Update) the bot uses [SmartIrc4Net](https://github.com/meebey/SmartIrc4net) for its IRC connection.
  
