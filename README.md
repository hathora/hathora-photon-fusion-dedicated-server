# Hathora Cloud + Photon Fusion Dedicated Server

[![UnityVersion](https://img.shields.io/badge/Unity%20Version:-2021.3%20LTS-57b9d3.svg?logo=unity&color=2196F3)](https://unity.com/releases/editor/qa/lts-releases)
[![HathoraSdkVersion](https://img.shields.io/badge/Hathora%20SDK-1.5.1-57b9d3.svg?logo=none&color=AF64EE)](https://hathora.dev/docs)
![image](https://assetstorev1-prd-cdn.unity3d.com/key-image/44946285-5088-4f57-b51b-a996184da940.webp)
<br><br>

This sample minimalist technical demo was started from Photon's [Fusion Dedicated Server Sample](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server)
Minimal example; see [Photon Docs]()
<br><br>

## (!) TODO

Everything below this point is a template from [hathora/photon-fusion-br](https://github.com/hathora/photon-fusion-br) - This will later be updated to reflect the new sample game.

____________________

### Overview

The original [Photon Fusion BR200](https://doc.photonengine.com/fusion/current/game-samples/fusion-br/quickstart) was a AAA and production-ready third-person battle royale shooter game sample. We have added integration to deploy a Photon Fusion dedicated server on Hathora Cloud. By deploying your Photon Fusion game on Hathora Cloud you unlock all the benefits of dedicated servers (cheat protection, improved latency, more stable connections), while also gaining access to [10+ global regions](https://hathora.dev/docs/faq/scale-globally) that map nicely with Photon Cloud's regions.

* Hathora SDK
* Hathora high-level wrappers
* HathoraServerConfig build, deployment and room management utils

Check it out to learn more about hosting Photon Fusion on Hathora Cloud and accessing the Hathora SDK. We have made a few modifications to make this game easily deployable with a dedicated server on [Hathora Cloud](https://hathora.dev/docs).
<br><br>

---
## Readme Contents and Quick Links
<details open> <summary> Click to expand/collapse contents </summary>

- ### [Getting the Project](#getting-the-project-1)
- ### [Requirements](#requirements-1)
- ### [Troubleshooting](#troubleshooting-1)
  - [Bugs](#bugs)
  - [Documentation](#documentation)

</details>

---
<br>

## Getting the project
### Direct download

 - select `Code` and select the 'Download Zip' option.  Please note that this will download the branch you're currently viewing on Github
<br><br>

## Requirements

- This sample game is compatible with the latest Unity Long Term Support (LTS) editor version, currently [2021 LTS](https://unity.com/releases/2021-lts). Please include **Linux Dedicated Server Build Support** in your installation, as well as **Linux Build Support (Mono)**.

- [Photon account](https://www.photonengine.com/fusion) with an active app created (for `AppId`).

- [Hathora Cloud account](https://console.hathora.dev) with an active app created (for `AppId`).

- 
<br><br>

## Steps

1. If building your Linux headless server via `HathoraServerConfig`, the Dockerfile will automatically add the `-args` necessary to start "as a server
    - To see the default args, see [./hathora/Dockerfile](https://github.com/hathora/photon-fusion-br/blob/main/.hathora/Dockerfile) - or the official [Photon docs](https://doc.photonengine.com/fusion/current/game-samples/fusion-br/quickstart)

2. Use the Hathora Unity plugin to configure, build, and deploy your server on Hathora Cloud via `Assets/Hathora/HathoraServerConfig`. See [Hathora Unity Plugin](https://github.com/hathora/unity-plugin)
    - After setting up, serialize @ `Menu` scene's `HathoraManager` GameObject's `HathoraServerConfig`.
    - At the same spot, also serialize `HathoraClientConfig` next to the same file.

4. Once deployed, create a room in Hathora Cloud via any method:
  - Create via `Menu` scene (as a Client): Click "Create" button at the bottom-right (adds a browsable Lobby)
  - via `Hathora ServerConfig`: Click "Create Room" button in the "Create Room" dropdown group
  - via [Hathora Console](https://console.hathora.dev) in your browser at the top-right

5. Play the `Menu` scene and join the new Room that appeared in a browsable Lobby list.

## Altered Photon Files

Within this repro, we have already made changes to support Hathora. However, if you are interested in the core changes, in order of importance:

1. [Asssets/TPSBR/Scripts/Networking/Networking.cs](https://github.com/hathora/photon-fusion-br/blob/main/Assets/TPSBR/Scripts/Networking/Networking.cs)
    * At `ConnectPeerCoroutine()`, if **Server** GameMode: Get Hathora Cloud Proccess/Room info to set Photon `startGameArgs`. Server name, max # of players, etc.

2. [Assets/HathoraPhoton/HathoraMatchmaking.cs](https://github.com/hathora/photon-fusion-br/blob/main/Assets/HathoraPhoton/HathoraMatchmaking.cs)
    * We override Photon's `Matchmaking.cs` to check if the user created a new Lobby from the `Menu` scene. Pure Photon will have the user be both a Client and Server. In Hathora, we want to create a dedicated server in the cloud (rather than have the player host as a server).

3. Assets/TPSBR/Scenes/Menu.unity
    * Added HathoraManager GameObject with manager scripts. 
      * **(!)** Serialize your selected `HathoraServerConfig` & `HathoraClientConfig` files here!
      * Added `HathoraCreateDoneTxt` & `HathoraCCreateStatusTxt` for status texts.
      * Renamed `Matchmaking` prefab to `HathoraMatchmaking` -> Swapped `Matchmaking.cs` with `HathoraMatchmaking.cs`.
          * When the users clicks "Create": instead of locally hosting as a server, we'll create a Hathora Cloud dedicated server (Room with Lobby) and show the Lobby list.

### Region Mapping

Within the demo, we have included [HathoraRegionMap.cs](https://github.com/hathora/photon-fusion-br/blob/main/Assets/HathoraPhoton/HathoraRegionMap.cs) to map the following Photon<>Hathora regions:

**<< Photon : Hathora >>**
- "us" : Washingington DC
- "usw" : Seattle
- "asia" : Singapore210
- "jp" : Tokyo
- "eu" : Frankfurt
- "sa" : SaoPaulo
- "kr" : Singapore

## Default Dockerfile launch argss

- `-batchmode` | Unity arg to run as headlesss server
- `-nographics` | Unity arg to skips shaders/GUI; requires `-batchmode`
- `-dedicatedServer` | Photon arg to automatically start as dedicated server
- `-deathmatch` | Photon arg to automatically start `deathmatch` game mode
- `-maxPlayers 5` | Photon arg
- `-scene GenArea2` | Photon arg
- `-region us` | Photon arg; see Photon<>Hathora mapping in the section below
- `-serverName hathoraDeployedServer` | Shows up in the lobby List under name
- `-sessionName hathoraDeployedServerSession` | Photon arg; unknown use (arbitrary?)
- `-port 7777` | The default and recommended Docker container port
- `-mode server` | Hathora arg to start as Server when using a `HathoraArgHandler` script; unnecessary in Photon.

## Troubleshooting
### Bugs
- Report bugs in the sample game using Github [issues](https://github.com/hathora/photon-fusion-br/issues)
  
### Documentation
- For a deep dive into Hathora Cloud, visit our [documentation site](https://hathora.dev/docs).
- Learn more the [Photon Fusion BR Demo](https://doc.photonengine.com/fusion/current/game-samples/fusion-br).

<br><br>

## Community
For help, questions, advice, or discussions about Hathora Cloud and its samples, please join our [Discord Community](https://discord.gg/hathora). We have a growing community of multiplayer developers and our team is very responsive.
<br><br>

## Other samples
### Hathora Unity Plugin
[Hathora Unity Plugin (with FishNet and Mirror demos)](https://github.com/hathora/hathora-unity) is our Unity Plugin that comes with FishNet and Mirror networking demos. It allows you to deploy your game on Hathora Cloud directly from our editor plugin.

### Unity NGO Sample
[@hathora/unity-ngo-sample](https://github.com/hathora/unity-ngo-sample) takes Unity's 2D Space Shooter sample game with *Unity NetCode for Game Objects* (NGO) and modifies it to be easily deployable on Hathora Cloud.
<br><br>
