#!/bin/bash
#################################################################################################
#
# normalize_for_unix.sh - If you edited any .sh script in Windows or Mac, you'll get errs.
#  We'll normalize the line endings here using apt `dos2unix``.
#  Supports wildcards; eg: "*.sh".
#  (!) If you edit this script, run: `dos2unix ./normalize_for_unix.sh`
#
# --------------------------------------------------------------------------------
# Created by dylan@hathora.dev @ 7/11/2023
#################################################################################################

# Check if dos2unix is installed
if ! command -v dos2unix &> /dev/null
then
    echo "dos2unix !found; installing now..."
    
    sudo apt update
    sudo apt install dos2unix -y
    
    echo "dos2unix installed successfully."
fi

# Check if a filename was supplied as the first argument to the script
if [ $# -eq 0 ]
then
    echo "Error: Supply a filename as arg"
    exit 1
fi

# Normalize the given file
dos2unix "$1"

echo "File(s) normalized."
