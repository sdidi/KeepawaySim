This is mainly based on RoboCupSharp by Phillip Verbancsics and to get more details go to: http://eplex.cs.ucf.edu

This software supports experiments for :RLKeepaway used for my desertation. The code modifies RoboCupSharp
so as to test impact of behavior transfer on collective behavior tasks. 

The following explains how the experiments run and what is included in the package:

There is a RoboCupSharp Server implementation, SharpNEAT and RoboCup Keepaway domain implementation. This experiment adds the search methods to the existing RoboCup Implementation of Keepaway Domain. In the software there is code to implement SARSA and Q-Learning.

For a detailed explanation of the experiments and parameters used here read : my desertation : Neuro-Evolution and Behavior Transfer in Collective Behavior Tasks  

For the introduction and full explanation of RoboCup and Keepaway Soccer domain, visit http://eplex.cs.ucf.edu on the paper 
by Phillip Verbancsics : Evolving Static Representations for Task Transfer Journal of Machine Learning Research 11:  pages 1737-1769. Available at: http://eplex.cs.ucf.edu/publications/2010/verbancsics.jmlr10.html   


Usage :

There are three main projects: 

	- TD methods (SARSA and Q-Learning) implementation that specifies the policy follwed by the learning agent   
	- Keepaway Project, that contains the Keepaway Domain as specified by cited author above. 
          The specification and implementation of our learning algorithm is defined in this project.
	- RoboCup Project, that contains the RoboCup Server implementation as specified by Peter Stone, 
	  the server defines the agents and environment of interaction.
	
	
To view and open this software you need at least Microsoft Visual Studio 2010 or MonoDevelop depending on 
the operating systems you are using.To open the solution on the Source code folder, select the RoboCup.sln
solution. 

Execution: 

To run the executable the project, run Keepaway.exe file which is compiled for multi-platform. 

If windows run C:\ Keepaway.exe , if Linux $mono Keepaway.exe

Settings for the experiments :

TD methods : - RLConfig.xml , specifies the Keepaway domain parameters, the policy selection
	       and other essential parameters necessary for the experiment. Read the description portion to gain
	       more understanding.
       
The rest of the support setup files are used unchanged as in http://eplex.cs.ucf.edu. 

For more details contact: sabredd0@gmail.com.
		
