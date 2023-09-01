# Hathora Cloud + Photon Fusion Dedicated Server

[![UnityVersion](https://img.shields.io/badge/Unity-2021.3%20LTS-57b9d3.svg?logo=unity&color=2196F3)](https://unity.com/releases/editor/qa/lts-releases)
[![HathoraSdkVersion](https://img.shields.io/badge/Hathora%20SDK-1.5.1-57b9d3.svg?logo=none&color=AF64EE)](https://hathora.dev/docs)
<br>

### About

This minimalist technical demo was started from Photon's [Fusion Dedicated Server Sample](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server) and assumes familiarity with the associated Photon Fusion [Dedicated Server Docs](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server).


### Fork Changes

We have added integration to deploy a Photon Fusion dedicated server on Hathora Cloud. By deploying your Photon Fusion game on Hathora Cloud you unlock all the benefits of dedicated servers (cheat protection, improved latency, more stable connections), while also gaining access to [10+ global regions](https://hathora.dev/docs/faq/scale-globally) that map nicely with Photon Cloud's regions. 

The original repo inits in Photon **Relay** mode, but we will have a **Direct** connection in this fork for best performance:

- Hathora SDK assets at: `/Hathora/Hathora.Cloud.Sdk`
- Hathora high-level wrapper assets at: `/Hathora/Core`
- Photon-specific tools at: `/HathoraPhoton` for *direct connection* support.
- `HathoraServerConfig` build, deployment and room management utils

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

 - Select [Code](https://github.com/hathora/hathora-photon-fusion-dedicated-server) and select the 'Download Zip' option.  Please note that this will download the branch you're currently viewing on Github.

 ![Code - Zip](resources/git-code-zip.png)
<br><br>

## Requirements

- This sample game is compatible with the latest Unity Long Term Support (LTS) editor version, currently [2021 LTS](https://unity.com/releases/2021-lts). Please include **Linux Dedicated Server Build Support** in your installation, as well as **Linux Build Support (Mono)**.

- [Photon account](https://www.photonengine.com/fusion) with an active app created (for `AppId`).

- [Hathora Cloud account](https://console.hathora.dev) with an active app created (for `AppId`).
<br><br>

## Steps

1. If building your Linux headless server via `HathoraServerConfig`, the Dockerfile will automatically add the `-args` necessary to start "as a server".
    - To see the default args, see [./hathora/Dockerfile](https://github.com/hathora/hathora-photon-fusion-dedicated-server/blob/main/src/.hathora/Dockerfile) - or the official [Photon docs](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server)

2. Use the Hathora Unity plugin to configure, build, and deploy your server on Hathora Cloud via `Assets/Hathora/HathoraServerConfig`. For detailed steps, check out our guide on getting started with [Hathora's Unity Plugin](https://hathora.dev/docs/engines/unity/beginners-tutorial).
    - After setting up, serialize your HathoraServerConfig to the 1st scnee (`0.Launch_Default`)'s `HathoraManager.HathoraServerConfig` GameObject component.
    - Since this demo is not using any Hathora-specific Client calls, you don't need to do anything with `HathoraClientConfig`.

3. Once deployed, create a room in Hathora Cloud via any method:
    - via `Hathora ServerConfig`: Click "Create Room" button in the "Create Room" dropdown group
    - via [Hathora Console](https://console.hathora.dev) in your browser at the top-right

4. Play the `Menu` scene (in your local Editor or a standalone Client build) click the `Client` button in the loaded Lobby scene.
    - If you are in the Editor, ensure your `ServerNetworkManager.HathoraPhotonServerMgr` GameObject component script is set to `Client` (default).

## Altered Photon Files

Within this repro, we have already made changes to support Hathora for direct connections. However, if you are interested in the core **logic changes** from the original, in order of importance:

1. [Asssets/HathoraPhoton/HathoraPhotonServerMgr.cs](https://github.com/hathora/hathora-photon-fusion-dedicated-server/blob/main/src/Assets/HathoraPhoton/HathoraPhotonServerMgr.cs)
    - At `Awake()`, if deployed headless Server, subscribe to `HathoraServerMgr.OnInitialized` event, returning [HathoraServerContext](https://github.com/hathora/hathora-photon-fusion-dedicated-server/blob/46ce7b20c71b91c5debca50d2de390c595a96752/src/Assets/Hathora/Core/Scripts/Runtime/Server/Models/HathoraServerContext.cs#L16).
    - At `OnInitialized()`:
        * Set Photon's Config `containerPort` (renamed from `port`) to HathoraServerContext's `Port` (default `7777`).
        * Set Photon's Config `PublicIp` and `PublicPort` from `HathoraServerContext`.
        * Continue as normal to `base.StartSimulation()`.

2. Assets/Scenes/0.Launch_Default.unity
    - Added `HathoraManager` GameObject with `HathoraServerManager` script component to get ip:port info at `OnInitialized()` (mentioned above).
        * **(!)** Serialize your selected `HathoraServerConfig` ScriptableObject here!

### Region Mapping

Although unused the vanilla demo, we have included [HathoraRegionMap.cs](https://github.com/hathora/hathora-photon-fusion-dedicated-server/blob/main/src/Assets/HathoraPhoton/HathoraRegionMap.cs) to map the following Photon<>Hathora regions:

**<< Photon : Hathora >>**
- "us" : Washingington DC
- "usw" : Seattle
- "asia" : Singapore210
- "jp" : Tokyo
- "eu" : Frankfurt
- "sa" : SaoPaulo
- "kr" : Singapore

Should you choose to touch Region specifics later, this should prove useful!

## Default Dockerfile launch args

From the [official docs](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server):

### Unity Required Args
- `-batchmode` | Unity arg to run as headlesss server.
- `-nographics` | Unity arg to skips shaders/GUI; requires `-batchmode`.

### Photon Optional Args
- `-session <custom session name>` | Start a Game Session with name my-custom-session. Default is a random GUID.
- `-region <region ID>` | Connect to Region US. Default is Best Region.
- `-lobby <custom lobby name>` | Publish the Game Session on the Lobby named my-custom-lobby. Default is ClientServer Lobby.
- `-port <custom port number>` | Bind to port 30001. Default is 27015.
- `-P <property name> <value>` | Setup the custom properties map = city and type = free-for-all. Default is an empty list.

### Hathora Optional Args
- `-mode server` | Legacy from other projects to automatically start in `Server` mode; does not natively affect anything since Photon handles this for you.

## Troubleshooting
### Bug Reporting
- Report bugs in the sample game using Github [issues](https://github.com/hathora/hathora-photon-fusion-dedicated-server/issues).

### Known Issues
1. **Issue:** `InvalidOperationException: Failed to load the global config from "NetworkProjectConfig"`
    - **Caught:** Known Photon issue - your `NetworkProjectConfig` is desync'd. 
    - **Resolution:** Open this ScriptableObject -> click "Rebuild Object Table" button at top.

2. **Issue:** `Crashes in the editor often` / `TypeLoadExceptions on play (error spam in logs)`
    - **Caught:** Known Photon issue - likely for the use of native *Cecil* dlls injecting/disposing net code back and forth: Going too fast can cause permanent desyncs (for the session) with Unity.
    - **Resolution:** Simply restart your Editor.

3. **Issue:** `When debugging with breakpoints after a connection is started, my IDE just suddenly stops debugging`
    - **Caught:** known Photon issue - due to the way timeouts are handled (classes are seemingly just disposed, interrupting the debugging threads).
    - **Resolution:** TODO: There is likely a way to lower or disable this timeout, which would be more-ideal for development builds.

4. **Issue:** `When I try to test as a Client in the Editor, I simply get No 'Cameras Rendered'`
    - **Caught:** You were probably mocking a Server in the `0.Launch_Default` scene, where `ServerNetworkManager.HathoraPhotonServerMgr` is set to `Server`.
    - **Resolution:** In `0.Launch_Default` scene, set `ServerNetworkManager.HathoraPhotonServerMgr` GameObject component script to `Client` (default).
  
### Documentation
- For a deep dive into Hathora Cloud, visit our [documentation site](https://hathora.dev/docs).
- Learn more the [Photon Dedicated Server Demo](https://doc.photonengine.com/fusion/current/technical-samples/dedicated-server/fusion-dedicated-server).

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
