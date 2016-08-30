This is mainly based on RoboCupSharp by Phillip Verbancsics and to get more details go to: http://eplex.cs.ucf.edu

This software supports experiments for :Multi-Agent Behavior-Based Policy transfer. The code modifies RoboCupSharp
so as to test impact of incorporating behavioral diversity and Genotypic diversity search as a means to boost the task performance of transferred policies. 

The following explains how the experiments run and what is included in the package:

There is a NEAT Keepaway and HyperNEAT pack that combines RoboCupSharp Server implementation, SharpNEAT and RoboCup Keepaway domain implementation. This experiment adds the search methods to the existing RoboCup Implementation of Keepaway Domain. In the software there is code to implement Behavioral Diversity Search and Genotypic Diversity Search.

For a detailed explanation of the experiments and parameters used here read : Multi-Agent Behavior-Based Policy transfer found at:  http://people.cs.uct.ac.za/~gnitschke/ 

For the introduction and full explanation of RoboCup and Keepaway Soccer domain, visit http://eplex.cs.ucf.edu on the paper 
by Phillip Verbancsics : Evolving Static Representations for Task Transfer Journal of Machine Learning Research 11:  pages 1737-1769. Available at: http://eplex.cs.ucf.edu/publications/2010/verbancsics.jmlr10.html. The SharpNEAT, the C# implementation of NEAT Algorithm by Colin Green is found on http://nn.cs.utexas.edu/?SharpNEAT. 


Usage :

There are two packs: 1. NEAT Keepaway and 2. HyperNEAT both containing, the following: 

	- Keepaway Project, that contains the Keepaway Domain as specified by cited author above. 
          The specification and implementation of our learning algorithm is defined in this project.
	- RoboCup Project, that contains the RoboCup Server implementation as specified by Peter Stone, 
	  c# version adapted from Phillip, the server defines the agents and environment of interaction.
	
To view and open this software you need at least Microsoft Visual Studio 2010 or MonoDevelop depending on 
the operating systems you are using.To open the solution on the Source code folder, select the RoboCup.sln
solution. 

Execution: 

To run the executable the project, run Keepaway.exe file which is compiled for multi-platform. 

If windows run C:\ Keepaway.exe , if Linux $mono Keepaway.exe

Settings for the experiments :

NEAT : - KPExperimentConfig.xml , specifies the Keepaway domain parameters, the search methods selected
	 and other essential parameters necessary for the experiment. Read the description portion to gain
	 more understanding.
       - Keepaway.config.xml, specifies the parameters for the NEAT algorithm that could be changed during 
	 the experiment. It also enables you to switch between training from scratch and policy transfer.
HyperNEAT :- KeepawayConfig.xml, contains specific settings for the HyperNEAT version of Keepaway domain.
             It contains Keepaway domain settings, and search method option.
	  :- Initialization.xml, specifies the experiment setup parameters such as number of generations,
             evaluations and option to switch between training from scratch and policy transfer.
The rest of the support setup files are used unchanged as in http://eplex.cs.ucf.edu. 

For more details contact: sabredd0@gmail.com.
		