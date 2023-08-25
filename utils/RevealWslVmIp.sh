#!/bin/bash
################################################################
# If `localhost` doesn't work for you when testing a localhost`
#   Linux server via WSL2 in Windows, then use this script
#   to get the local IP of the WSL VM.
# v1.0.1
################################################################
ip addr show eth0 | grep 'inet ' | awk '{ print $2; }' | sed 's/\/.*$//'
