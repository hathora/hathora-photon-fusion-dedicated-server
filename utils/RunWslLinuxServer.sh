#!/bin/bash
###################################################################################################
#
# RunWslLinuxServer.sh - Runs 1 LinuxServer (via bash shell).
#  By default, this works with default Unity `HathoraServerConfig` settings.
#  See 'Networking.cs' for the main init.
#
# -------------------------------------------------------------------------------------
# 
# EXPORTED ENV VARS:
# - HATHORA_PROCESS_ID
# - SERVER_IP # Fallback if !HATHORA_PROCESS_ID
# - SERVER_PORT # Fallback if !HATHORA_PROCESS_ID
#
# (!) Get these via Unity's `Environment.GetEnvironmentVariable()`
#
# -------------------------------------------------------------------------------------
#
# ARGS WHEN CALLING THIS SCRIPT, ITSELF
#  -p {hathoraProcessId} # Optional - Gets ip:port from a Hathora Cloud Process Id
#
# -------------------------------------------------------------------------------------
#
# UNITY ARGS: 
# -batchmode runs as a headless (dedicated) server
# -nographics (requires -batchmode) runs without a GUI
# -> More @ https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html 
#
# -------------------------------------------------------------------------------------
#
# HATHORA ARGS: 
# -scene {sceneName} - Change scene before doing any other args
# -mode {client | server | host} - Autostart a NetCode Transport
#
# -------------------------------------------------------------------------------------
# Created by dylan@hathora.dev @ 7/11/2023
# v1.0.1
###################################################################################################
# CUSTOMIZABLE >>
export SERVER_PORT=7777 # Arbitrary, but client must match server
exe_name="Hathora-Unity_LinuxServer.x86_64"
path_to_linux_server="../src/Build-Server/$exe_name"
extra_unity_args=""
hathora_args="-mode server"
###################################################################################################

# Check for -p HathoraProcessId arg
while getopts ":p:" opt; do
  case ${opt} in
    p )
      HATHORA_PROCESS_ID=$OPTARG
      ;;
    \? )
      echo "Invalid option: $OPTARG" 1>&2
      ;;
    : )
      echo "Option $OPTARG requires an argument" 1>&2
      ;;
  esac
done

export HATHORA_PROCESS_ID

# ====================================================================
# Set local WSL2 server fallbacks, if !HATHORA_PROCESS_ID from -p arg
# Then log results
# ====================================================================

# Get the "real" IP of the Wsl2 IP (essentially 'localhost' ported through)
echo "Current directory: $(pwd)"
export LOCAL_SERVER_IP=$(./RevealWslVmIp.sh)

# ====================================================================
# Set the expected args + cmd early so we may log it
# ====================================================================
unity_args="-batchmode -nographics $extra_unity_args"
linux_cmd="$path_to_linux_server $unity_args $hathora_args"

# ====================================================================
# Colored logs: ip:port, exe path, cmd, args
# ====================================================================
# Print the output of the Wsl2 IP (essentially `localhost` for WSL2)
# Make it stand out with color
COLOR='\033[38;5;213m' # Magenta
NC='\033[0m' # No Color

clear
echo -e "${COLOR}-----------------------------${NC}"
echo -e "${COLOR}Starting dedicated server: \`$LOCAL_SERVER_IP:$SERVER_PORT\`${NC}"
echo -e "${COLOR}HATHORA_PROCESS_ID: \`$HATHORA_PROCESS_ID\`${NC}"
echo -e "${COLOR}Starting instance @ \`$path_to_linux_server\`${NC}"
echo -e "${COLOR}unity_args: \`$unity_args\`${NC}"
echo -e "${COLOR}hathora_args: \`$hathora_args\`${NC}"
echo -e "${COLOR}-----------------------------${NC}"
echo ""

# ====================================================================
# START LINUX SERVER (from Windows -> via wsl2)
# By default, works with default Unity `HathoraServerConfig` settings
# ====================================================================
$linux_cmd
