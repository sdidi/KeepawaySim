# KeepawaySim


This simulator is used to run an experiment to test policy transfer with behavioral diversity for a multi-agent setting using Keepaway Soccer task as a multi-agent benchmark task. This software adapted from RoboCupSharp benchmark task by Phillip Verbancsics , http://eplex.cs.ucf.edu. 

The simulator contains two packages:
1) RoboCup Server C# Implementation 
	-This is the Server origionally designed by Peter Stone and modified by Phillip Verbancsics. 
2) RoboCup Keepaway Experiment 
	-This is the Keepaway task implemented on the above RoboCup Server. It has been modified to suit our experiment by adding parameters and methods for policy transfer, behavior and 
	genotype characterisation. 
	
The following shows files with parameters for the experiment :

-mutation.xml, initialisation.xml, reproduction.xml, saving.xml, speciation.xml and KeepawayConfig.xml.

To run the simulation, open the Executables folder and run Keepaway.exe file or using Visual Studio open RoboCup.sln file and run Keepaway project.

-In windows c:> Keepaway.exe
-Linux c:> mono Keepaway.exe

You need MonoDeveloper to run the experiment in Linux platform.




