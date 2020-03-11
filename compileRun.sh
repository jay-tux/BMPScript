# !/bin/bash

compile='false'
run='false'

while getopts 'crh' opt; do
	case $opt in
		'c')
			compile='true'
			;;
		'r')
			run='true'
			;;
		'h')
			echo " **** MONO COMPILER FOR BMPSCRIPT ****"
			echo "This script uses Mono to build and/or"
			echo "run the Jay.BMPScript parser."
			echo ""
			echo "Use the -c option to compile, and the"
			echo "-r option to run."
			echo ""
			echo "When compiling, the output file is"
			echo "./bin/bmpscript.exe, which can be run "
			echo "with the mono command, or with Wine."
			echo ""
			echo "The included runner/parser expects a "
			echo "./bin/0.bmp file as input for the run."
			echo "This file can be a copy of any file in "
			echo "the ./examples/ directory (we reccomend"
			echo "intro.bmp or fib.bmp as first runs)."
			echo " **** MONO COMPILER FOR BMPSCRIPT ****"
			;;
		*)
			echo "Unknown option $opt"
			;;
	esac
done

mkdir -p bin

if [[ $compile == 'true' ]]; then
	mcs -r:System.Drawing -out:bin/bmpscript.exe -main:Jay.BMPScript.Program *.cs 
	if [[ $? != 0 ]]; then
		>&2 echo "Failed to compile." 
		exit -1
	fi
fi
if [[ $run == 'true' ]]; then
	mono bin/bmpscript.exe
fi
