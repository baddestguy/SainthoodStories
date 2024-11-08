# How To Deploy to local XBOX using SDK

## 1. Setup
Get access to the [Unity for Game Downloads Unity Forum](https://discussions.unity.com/t/unity-for-game-core-downloads/778704). The Add-ons we will be downloading here give you the ability to deploy native Xbox applications to an xbox rather than going through the [UWP scheme](./Xbox%20UWP.md).
1. ### Add-ons:
    1. Find the release stream that matches your version of Unity. You need to find the exact version for your unity build. Note that the GDKX has an expiry date (typically a year).
    1. Download and install the **Unity Game Core Series (Scarlett) Add-on**. This will give you the ability to deploy to the Xbox series S and X.
    1. Download and install the **Unity Game Core Xbox One Add-on**. This will give you the ability to deploy to the Xbox One and Xbox One X. 
    1. Download and install the **GDKX** associated with the release stream. This will give you the ability to access the XBOX native APIs.
2. ### Tools:
    1. Back on the Main forum page, find and download the latest version of the **Microsoft GDK Tools Xbox Package**. This extends the functionality provided by the [Packages](#packages) below. Install this to *{rootFolder}\Packages*
    1. Download the latest versino of the **GXDK Input System**. This allows us to treat the Xbox Controller as a native Gamepad.
3. ### Packages:
    1. Launch Unity and navigate to the Package manager. We'll need the following packages to work with the Unity SDK from within Unity.
    1. Microsoft GDK API Package: This is a Unity wrapper class that gives you access to the native APIS. It has decent documentation and sample projects that show you how to implement features.
    1. Microsoft GDK Tools Package: Not to be confused with the [tools xbox package](#tools), this makes it easy to manage assets and configs for your depoys.

## 2. Build
1. Double click the Assets -> _Scripts -> Xbox -> Sign-in -> GdkSignInSample.gdksettings file to make it the current active settings.
    1. Note that the Game Version set here supercedes whatever is set in the player settings.
1. Change Playform to Game Core - [Xbox version you want]
1. Ensure the following settings:
    1. Build Type - Development Player
    1. Deploy Method - Push
    1. Deploy Drive - Default
    1. Script Debugging - Checked off
    1. **Build**. Do not Build and run.


## 3. Deploy
1. I'm going to assume your xbox is already in dev mode. If not, look it up.
1. Get the remote access url
    1. If you set up a username and password in the remote access settings, well you're gonna need them. Or you can easily change it on the xbox if you can't remember it.
1. Back on your PC, navigate to the remote access url
    1. And surprise, enter the credentials (if they exist?) to gain access
1. Under the My Games & Apps section select Add a new game
1. GDK/XDK DEployment
1. Source should point to *{buildOutputDirectory}\Loose*
1. Destination will be the desired folder name it gets installed to on the xbox. I use Sainthood
1. On clicking next, a terminal window should open letting you know the status of the files being transferred. Once that's done, you're ready to play your game.

## 4. Debug
After the game has launched (typically once you see the Unity log) you can attach to the Xbox to see log messages. On the Console window, select the dropdown beside "Error Pause" and select Remote -> GameCoreXbox -> Console. You should now start seeing log messages in the console.
    