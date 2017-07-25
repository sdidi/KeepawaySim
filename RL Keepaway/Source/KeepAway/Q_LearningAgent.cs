using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace Keepaway
{
   public class Q_LearningAgent : SMDPAgent
    {
        
        //char []weightsFile=new char[256];
  string weightsFile;
  bool bLearning;
  bool bSaveWeights;

  public double lastReward;
  /// Hive mind indicator and file descriptor.
  bool hiveMind;
  int hiveFile;
  
  int epochNum;
  int lastAction;
  public static RLConfig config = new RLConfig();
  double alpha;
  double gamma;
  double lambda;
  double epsilon;
  int numActions;
  int numFeatures;
  static int numDimTiles = 32;
  int numTiles;// = 800 // 25 * 32
  double []tileWidths;//= new double[max_States];
  public double[] Q =new double[10];
  double [,]weights = new double[max_Actions,max_Tiles];
  double [,]weightsRaw;//=new double[max_Actions,numTiles];
  double [,]traces;//=new double[max_Actions,numTiles];
  static double[] traceAll;// = new double[numTiles];

   // number of tiles in each tiling
  int [,]tile ;// = new int[max_States,numDimTiles];
  int numTilings ;
  int[] tiles= new int[rl_memory_Size];
  int [,] actionTiles ;//= new int[max_Actions,rl_memory_Size];
  Tile[,] tiless;// = new Tile[max_States, numDimTiles];
  double minimumTrace;
  int []nonzeroTraces;//= new int[rl_max_nonzero_Traces];
  int numNonzeroTraces;
  int []nonzeroTracesInverse;//= new int[rl_memory_Size];
  int [,]currentState;// = new int[max_Actions,numDimTiles];
  int [,] previousState;// = new int[max_Actions,numDimTiles];
  collision_table colTab;
  
 public struct CollisionTableHeader {
  long m;
  int safe;
  long calls;
  long clearhits;
  long collisions;
};

 public Q_LearningAgent()
 {
 }


 public Q_LearningAgent(int numFeatures, int numActions, bool bLearn, double rewards,
                                    double []widths, 
                                    string loadWeightsFile, string saveWeightsFile ): base( numFeatures, numActions )
{
    this.numActions = numActions;
    this.numFeatures = numFeatures;
    numTiles = numFeatures*numDimTiles;
    tileWidths= new double[numFeatures];
   Q = new double[numActions];
  //weights = new double[numActions,numTiles];
  //weightsRaw=new double[numActions,numTiles];
  //weights = weightsRaw;
  traces=new double[numActions,numTiles];
  traceAll = new double[numTiles];

   // number of tiles in each tiling
  tile  = new int[numFeatures,numDimTiles];
  actionTiles = new int[numActions ,rl_memory_Size];
  tiless = new Tile[numFeatures, numDimTiles];
  nonzeroTraces= new int[rl_max_nonzero_Traces];
  nonzeroTracesInverse= new int[rl_memory_Size];
  currentState = new int[numActions ,numDimTiles];
  previousState = new int[numActions ,numDimTiles];
  weightsFile = saveWeightsFile;
     
     bLearning = bLearn;
     
  lastReward = rewards;
  for ( int i = 0; i < getNumFeatures(); i++ ) {
    tileWidths[ i ] = widths[ i ];
  }


  config = RLConfig.Load("RLConfig.xml");
  alpha = config.alpha;
  gamma = config.gamma;
  lambda = config.lambda;
  epsilon = config.epsilon;
  minimumTrace = config.traceability;
  //for (int i=0; i < rl_memory_Size;i++)
  for (int j = 0; j < numActions; j++)  //yet to do ... each Q(s,a) should represent  
        Q[j] =0;

  epochNum = 0;
  lastAction = -1;
  

  numNonzeroTraces = 0;
    //Sabre changed this to initial weights for all tiles per action
     //need to check if this yields to weights being always zero when ever you call a SarsaAgent object
     //if it is remove this initialisation and put it in the start-episode method  
  if (config.transfer == 1)
  {
      weights = loadWeights(loadWeightsFile);
      //load traces too
      //at the moment initial the traces but need to explore saving them too
      for (int j = 0; j < numActions; j++)
               for (int i = 0; i < numTiles; i++)
                       traces[j, i] = 0;
      
  }
  else
  {
      for (int j = 0; j < numActions; j++)
      {
          for (int i = 0; i < numTiles; i++)
          {
              weights[j, i] = 0;
              traces[j, i] = 0;
          }
      }
  }
 
  int []tmp= new int[ 2 ];
  float []tmpf=new float[ 2 ];
  colTab = new collision_table( rl_memory_Size, 1 );
  tiles obj_tiles = new tiles();
  
}

public double getQ(int action, int state) {
  if (action < 0 || action > getNumActions()) {
    Console.WriteLine("either actions is less than 0 or actions more that specified");
  }
    //to do code to compute the action
  return Q[action];
}

public override void setEpsilon(double epsilon) {
  this.epsilon = epsilon;
}

       
public override int startEpisode( double []state )
{
  //if (hiveMind) loadColTabHeader(colTab, weights);
  epochNum++;
    for(int t=0;t< getNumActions() ;t++)
    decayTraces( t,0 );
  
  currentState = makeTiles( state );
  //to do call - capture an array to pass to computeQ
  for ( int a = 0; a < getNumActions(); a++ ) {
    Q[ a] = computeQ( a , currentState);
  }
  
  lastAction = selectAction();
  previousState = currentState;
  
  Console.WriteLine("Start Q[{0}] = {1:f}", lastAction, Q[lastAction] );
  
  for ( int j = 0; j < numTilings; j++ )
    setTrace(lastAction, actionTiles[lastAction, j ], 1.0 );
      //saveWeights(weightsFile);
  return lastAction;
}

public override int step( double reward, double []state )
{
    Console.WriteLine("Episode Steps");
      if (lastAction < 0)
        lastAction = 0;
  
  double delta = reward - Q[ lastAction];
  currentState = makeTiles( state );
  for ( int a = 0; a < getNumActions(); a++ ) {
    Q[ a ] = computeQ( a , currentState);
  }

  lastAction = selectAction();
  previousState = currentState;
 
  Console.WriteLine("Steps Q[{0}] = {1:f}", lastAction, Q[lastAction]);
  

  if ( !bLearning )
    return lastAction;

  
  Console.WriteLine("reward: {0:f}", reward ); 

  delta += Q[ lastAction];
  updateWeights( lastAction,delta );
  Q[ lastAction] = computeQ( lastAction , previousState); // need to redo because weights changed
  decayTraces(lastAction, gamma * lambda );

  for ( int a = 0; a < getNumActions(); a++ ) {  //clear other than F[a]
    if ( a != lastAction ) {
      for ( int j = 0; j < numTilings; j++ )
        clearTrace(lastAction, actionTiles[ a,j ] );
    }
  }
  for ( int j = 0; j < numTilings; j++ )      //replace/set traces F[a]
    setTrace( lastAction, actionTiles[ lastAction, j ], 1.0 );
   
  //check if loop is endless
  if (reward > 500)
      RLGameWorld.kref.episodeEnded = true;


  return lastAction;
}

public override void endEpisode( double reward)
{
    saveWeights(weightsFile);
  if ( lastAction != -1 ) {
    
    Console.WriteLine("reward: {0:f}", reward ); 
   
    if ( gamma != 1.0)
     Console.WriteLine("We're assuming gamma's 1");
    double delta = reward - Q[ lastAction]; //deltaV(s)
    updateWeights(lastAction, delta );
    
  }
  
  //saveWeights(weightsFile);
  lastAction = -1;
}


public void  shutDown()
{
  // We usually only save weights at random intervals.
  // Always save at shutdown (if we are in saving mode).
  if ( bLearning && bSaveWeights ) {
    Console.WriteLine("Saving weights at shutdown.");
    saveWeights( weightsFile );
  }
  
}

public int selectAction()
{
  int action;
        action = argmaxQ();
  
  return action;
}

public bool saveWeights( string filename )
{
    
   // Console.WriteLine("Requested to save weights");
    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
    BinaryFormatter formatter = new BinaryFormatter();
    try
    {
        formatter.Serialize(fs, weights);
        fs.Close();
    }
    catch (SerializationException e)
    {
        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
        throw;
    }
    //Console.WriteLine("Saving weights here");
     
  //  XDocument prevState = new XDocument();
  //  prevState.Add(new XElement("weight-variables", weights.Select(x => new XElement("variable", x))));
  //  prevState.Save("weightsFile.xml");
    return true;
   }
//inter-task mapping
public double[,] loadWeights(string fileToLoad)
{
    config = RLConfig.Load("RLConfig.xml");
    double[,] currentWeight = new double[numActions, numTiles];
    if (File.Exists(fileToLoad))
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Open(fileToLoad, FileMode.Open);
        weights = (double[,])bf.Deserialize(file);
        file.Close();
        Console.WriteLine("Loading weights here-----------------------");
       // weights;
    }
    else { return null; }
     
    //XDocument loadVector = XDocument.Load("weightsFile.xml");
   // var varVector = loadVector.Element("weight-variables").Elements("variable");
   // weights = varVector.Select(x => Double.Parse(x.Value)).ToArray();


    for (int jj = 0; jj < numActions; jj++)
    {
        int j = 0; int v=0;
        if (jj < sourceActions) //check for direct transfer w_source = w_target for actions 
        { //number of actions 
            for (int k = 0; k < config.num_Keepers - 1; k++)
            {
                if (k < config.source_Keepers-1)
                {
                    for (int s = 0; s < 4; s++)
                        for (int tl = 0; tl < tilesPerTiling; tl++)
                            currentWeight[jj, v++] = weights[jj, j++];

                }
                else
                {
                    j -= (4*tilesPerTiling);
                    for (int s = 0; s < 4; s++) 
                        for (int tl = 0; tl < tilesPerTiling; tl++)
                            currentWeight[jj, v++] = weights[jj, j++];

                }
            }

            for (int ii = 0; ii < config.num_Takers; ii++)
            {
                if (ii < config.source_Takers)
                {
                    for (int s = 0; s < 2; s++)
                        for (int tl = 0; tl < tilesPerTiling; tl++)
                            currentWeight[jj, v++] = weights[jj, j++];

                }
                else
                {
                    j -= (2*tilesPerTiling);
                    for (int s = 0; s < 2; s++)
                        for (int tl = 0; tl < tilesPerTiling; tl++)
                            currentWeight[jj, v++] = weights[jj, j++];

                }

            }

            for (int tl = 0; tl < tilesPerTiling; tl++)
                currentWeight[jj, v] = weights[jj, j];
        }
        else //its an added action not available in the source task
        {
            for (int i = 0; i < numTiles; i++)
            {
                currentWeight[jj, i] = currentWeight[jj, i];
                
            }
        }
    }
    
    
    return currentWeight;
}

// Compute an action value from current F and theta    
public double computeQ( int a, int[,] tiles )
{
 // int val;
  double q = 0;
    numTiles = numDimTiles*getNumFeatures();
  int[] linearTiles = new int[numTiles];
    int h=0;
    for (int f = 0; f < getNumFeatures(); f++)
        for (int t = 0; t < numDimTiles; t++)
            linearTiles[h++] = tiles[f, t];
  for ( int j = 0; j < numTiles; j++ ) 
      q += weights[a, j] * linearTiles[j];// to change
      
   return q;
}

// Returns index (action) of largest entry in Q array, breaking ties randomly 
public int argmaxQ()
{
    numTiles = numDimTiles * getNumFeatures();
  int bestAction = 0;
  Console.WriteLine("Q1 is {0:f} , Q2 is {1:f} , Q3 is {2:f}", Q[0], Q[1], Q[2]); 
  double bestValue = Q[ bestAction ];
  int numTies = 0;
  for ( int a = bestAction + 1; a < getNumActions(); a++ ) {
    double value = Q[ a ];
    if ( value > bestValue ) {
      bestValue = value;
      bestAction = a;
    }
    else if ( value == bestValue ) {
      numTies++;
        Random rand = new Random();
      if ( rand.Next() % ( numTies + 1 ) == 0 ) {
        bestValue = value;
        bestAction = a;
      }
    }
  }

  return bestAction;
}

public void updateWeights( int a, double delta )
{
    numTiles = numDimTiles * getNumFeatures();
  double tmp = delta * alpha / numTiles; //alpha/m*bi(s)*deltaV(s) i.e adjusting each weight to reduce deltaV(s)
  int g=0;
  //Console.WriteLine("The parameters for calculating weight:Traces-{0:f}, alpha-{1:f}, numTiles-{2:f} ", tiles[0],alpha,numTiles);
  for ( int i = 0; i < numTiles; i++ ) {
    //int f = nonzeroTraces[ i ];
      weights[a, i] += tmp * tiles[i];// traceAll[i]; // adjust the weight w <- w + tmp*traces[f]
    
    }
 //Console.WriteLine("One of the weight not equal to zero is ==== {0:f} ", weights[a, g]);
}

public int[,] makeTiles(double[] state)
{
    Tile[,] tiless = new Tile[getNumFeatures(), numDimTiles];
    int tilingsPack = numDimTiles;
    int maxDim = 22; // maximum possible value of each state variable;
    int numTilings = 0, tileCount = 0, tileIndex = 0;
    for (int f = 0; f < getNumActions(); f++)
    {
        for (int t = 0; t < tilingsPack; t++)
        {
            int val = (int)Math.Floor(state[f] / maxDim * tilingsPack);
            if (val == t)
            {
                tile[f,t] = 1;
                tiless[f, t] = new Tile(1);
                tiles[tileCount] = 1;
                traceAll[tileCount] = 1/config.numFeatures; //Since only one tile per tilling has a value 1, and there are 13 tilings and need a total of trace value 1 per state
                tileCount++;
            }
            else
            {
                tile[f,t] = 0;
                tiless[f, t] = new Tile(0);
                tiles[tileCount] = 0;
                traceAll[tileCount] = 0;
                tileCount++;
            }

        }
        numTilings += tilingsPack;

    }
      if (tileIndex < 0)
        tileIndex = 0;
    return tile;
}


// Clear any trace for feature f      
public void clearTrace( int a, int f)
{
  if ( f > rl_memory_Size || f < 0 )
    Console.WriteLine("ClearTrace: f out of range  : {0}",f);
  if ( traces[a, f ] != 0 )
    clearExistentTrace( a, f, nonzeroTracesInverse[ f ] );
}

// Clear the trace for feature f at location loc in the list of nonzero traces 
public void clearExistentTrace( int a, int f, int loc )
{
  if ( f > rl_memory_Size || f < 0 )
    Console.WriteLine("ClearExistentTrace: f out of range : {0} ",f);
  traces[ a, f ] = 0.0;
  numNonzeroTraces--;
  nonzeroTraces[ loc ] = nonzeroTraces[ numNonzeroTraces ];
  nonzeroTracesInverse[ nonzeroTraces[ loc ] ] = loc;
}

// Decays all the (nonzero) traces by decay_rate, removing those below minimum_trace 
public void decayTraces( int a, double decayRate )
{
  int f;
  for ( int loc = numNonzeroTraces - 1; loc >= 0; loc-- ) {
    f = nonzeroTraces[ loc ];
    if ( f > rl_memory_Size || f < 0 )
      Console.WriteLine("DecayTraces: f out of range : {0}",f);
    traces[ a, f ] *= decayRate;
    if ( traces[a, f ] < minimumTrace )
      clearExistentTrace( a, f, loc );
  }
}

// Set the trace for feature f to the given value, which must be positive   
public void setTrace( int a, int f, double newTraceValue )
{
  if ( f > rl_memory_Size || f < 0 )
    Console.WriteLine("SetTraces: f out of range :{0}");
  if ( traces[a, f ] >= minimumTrace )
    traces[ a, f ] = newTraceValue;         // trace already exists              
  else {
    while ( numNonzeroTraces >= rl_max_nonzero_Traces)
      increaseMinTrace(a); // ensure room for new trace              
    traces[a, f ] = newTraceValue;
    nonzeroTraces[ numNonzeroTraces ] = f;
    nonzeroTracesInverse[ f ] = numNonzeroTraces;
    numNonzeroTraces++;
  }
}

// Try to make room for more traces by incrementing minimum_trace by 10%,
// culling any traces that fall below the new minimum                      
public void increaseMinTrace(int a)
{
  minimumTrace *= 1.1;
  Console.WriteLine("Changing minimum_trace to :{0}",minimumTrace);
  for ( int loc = numNonzeroTraces - 1; loc >= 0; loc-- ) { // necessary to loop downwards    
    int f = nonzeroTraces[ loc ];
    if ( traces[ a, f ] < minimumTrace )
      clearExistentTrace( a, f, loc );
  }
}

public void setParams(int iCutoffEpisodes, int iStopLearningEpisodes)
{
  /* set learning parameters */
}

    }
}
