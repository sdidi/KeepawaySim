using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    public abstract class SMDPAgent
    {
        public static int rl_memory_Size = 1048576;
        public static int rl_max_nonzero_Traces = 100000;
        public static int rl_max_num_Tilings = 6000;
        public static int max_States = 31;
        public static int max_Actions = 10;
        public static int max_Tiles = 31 * 32;
        public static int sourceTiles = 13 * 32;
        public static int sourceActions = 3;
        public static int sourceFeatures = 13;
        public static int tilesPerTiling = 32;

        int m_numFeatures; /* number of state features <= MAX_STATE_VARS */
        int m_numActions;  /* number of possible actions <= MAX_ACTIONS */

        protected int getNumFeatures() { return m_numFeatures; }
        protected int getNumActions()  { return m_numActions;  }

  
        public SMDPAgent( int numFeatures, int numActions ) 
         { 
            m_numFeatures = numFeatures; 
            m_numActions = numActions; 
        }
         
        public SMDPAgent() {}

        // abstract methods to be supplied by implementing class
        public abstract int  startEpisode( double [] state);
        public abstract int  step( double reward, double [] state );
        public abstract void endEpisode( double reward ) ;
        public abstract void setEpsilon(double epsilon);
        //public abstract void setParams(int iCutoffEpisodes, int iStopLearningEpisodes) ; //*met 8/16/05

        // Optional customization point.
       // public virtual void shutDown() {}
    }
}
