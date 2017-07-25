using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using RoboCup.Geometry;
using SharpNeat.Phenomes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Keepaway
{
    public class RLKeepawayPlayer : FixedKeepawayPlayer 
    {

        
        #region Variables
        int lastAction = 0;
        //int lastActionTime = 0;
        double[] inputs;//= new double[19];
        int max = 0;
        Dictionary<int, int> layerToIndex = new Dictionary<int, int>();
        #endregion
        public static RLConfig config = new RLConfig();
        /***
         * Toggle between object definitions of SARSA and Q_Learning to solve problem of interference
         * Yet to solve the problem
         ***/
        
        public SMDPAgent saPolicy = new SarsaAgent(); //turn this on when running SARSA and of when running Q_Learning
        //public Q_LearningAgent qlPolicy = new Q_LearningAgent(); // turn on - Q_Learning and turn off when its SARSA

        //public SMDPAgent saPolicy; // turn off - when running SARSA otherwise on when running Q_Learning
        public SMDPAgent qlPolicy; //turn off - when running Q_Learning otherwise on when running SARSA
        
        
        int numK, numT; RoboCup.Stadium std; string team, side; int unum;
        #region Constructor
       /* public RLKeepawayPlayer(RoboCup.Stadium std, string team, int unum, string side, int numK, int numT, SarsaAgent policy)
            : base(std, team, unum, side, numK, numT)
        {
            this.numK = numK;
            this.numT = numT;
            this.std = std;
            this.team = team;
            this.side = side;
            this.unum = unum;
            this.saPolicy = policy; // to pass the brain controller to an agent with ball
        } */
        
        public RLKeepawayPlayer(RoboCup.Stadium std, string team, int unum, string side, int numK, int numT)
            : base(std, team, unum, side, numK, numT)
        {
            this.numK = numK;
            this.numT = numT;
            this.std = std;
            this.team = team;
            this.side = side;
            this.unum = unum;
        }
        
        #endregion

        private void setInputIsignalArray(ISignalArray inputArr, double[] inp)
        {
            for (int i = 0; i < inp.Length; i++)
            {
                inputArr[i] = inp[i];
            }

        }
        int episodesCount;
        public override void keeper()
        {
            config = RLConfig.Load("RLConfig.xml");
            if (config.visualize == 1)
            {
                if (RLVisualisation.epi != 0 && RLVisualisation.epi != episodesCount)
                {
                    episodesCount = RLVisualisation.epi;
                    if (config.learningMethod == "SARSA")
                        saPolicy.endEpisode(-1);//(RLVisualisation.reward);
                    else
                        qlPolicy.endEpisode(-1);// (RLVisualisation.reward);
                    //set time start episode
                    //last_action to be set to -1

                }
            }
            else
            {
                if (RLProgram.epi != 0 && RLProgram.epi != episodesCount)
                {
                    episodesCount = RLProgram.epi;
                    if (config.learningMethod == "SARSA")
                        saPolicy.endEpisode(-1);//(RLProgram.reward);
                    else
                        qlPolicy.endEpisode(-1);// (RLProgram.reward);
                    //set time start episode
                    //last_action to be set to -1
                }


            }
            // If the ball is kickable,
            // call main action selection routine.
            if (isBallKickable())
            {
                turns = 0;
                trajectoryChanges = 0;
                trajectorySame = 2;
                keeperWithBall();
            }

            // Get fastest to ball
            int iTmp = 0;
            int fastest = getFastestInSetTo(teammates, ballp, ref iTmp);


            // If fastest, intercept the ball.
            if (fastest == Unum)
            {
                intercept();
                return;
            }

            // Not fastest, get open
            turns = 0;
            keeperSupport(fastest);
        }

        /*
         the method creates the network input-output structure  
         */
        #region Mapping helper method
        public Vector[] myTeammatesPlayers(Vector[] vector, int num)
       {
          Vector[] playerListp = new Vector[num];

          
              for (int i = 0; i < num; i++)
              {
                  playerListp[i] = new Vector(0, 0);
              }
              double close = 1000; int val = 0, temp = 0; ;

              for (int j = 0; j < num-1; j++)
              {
                  for (int i = 0; i < vector.Length; i++)
                  {
                      if (i != Unum - 1)
                      {
                          if (j == 1 && i == temp) continue;
                          if (vector[i].distance(mySelfp) < close)
                          {
                              close = vector[i].distance(mySelfp);
                              val = i;
                          }
                      }
                  }
                  temp = val;
                  playerListp[j] = vector[val];
              }
          
           return playerListp;
       }
        #endregion 


        double[] keeperstateVars(int stateCount)
        {
            config = RLConfig.Load("RLConfig.xml");
            Vector[] tmatesp = null;
            Vector[] oppPlayersp = null;
            Vector posToPass = new Vector(0, 0);
            Command soc = new Command();
            inputs = new double[stateCount];
            soc.Com = "illegal";
            int j=0,h = 0, v = 0, g = 0; double ang1, ang2, ang3;
            tmatesp = myTeammatesPlayers(teammatesp, config.num_Keepers);
            oppPlayersp = myTeammatesPlayers(opp, config.num_Takers);
            tmatesp[config.num_Keepers-1] = mySelfp;
            double[] closestmateOpp = new double[teammatesp.Length - 1];
            // Brain.ResetState();

            double[] widthArray = new double[config.numFeatures];
            for (int i = 0; i < config.numFeatures; i++)
                widthArray[i] = 1 / config.numFeatures;

           if (inputs == null)
            {
                inputs = new double[stateCount];
            }

            for (int i = 0; i < closestmateOpp.Length; i++)
            {
                closestmateOpp[i] = 1000;
            }


            for (int i = 0; i < tmatesp.Length; i++)
            {
                if (i != Unum - 1)
                {
                    inputs[j++] = tmatesp[i].distance(mySelfp);
                    getClosestInSetTo(oppPlayersp, tmatesp[i], ref closestmateOpp[h]);
                    inputs[j++] = closestmateOpp[h++];
                    inputs[j++] = DistFromCenter(tmatesp[i]);
                    ang1 = mySelfp.angle(tmatesp[i]);
                    ang2 = mySelfp.angle(oppPlayersp[0]);
                    ang3 = mySelfp.angle(oppPlayersp[1]);
                    inputs[j++] = Math.Min(Magnitude(ang1 - ang2), Magnitude(ang1 - ang3));
                }
            }

            for (int i = 0; i < oppPlayersp.Length; i++)
            {
                inputs[j++] = oppPlayersp[i].distance(mySelfp);
                inputs[j++] = DistFromCenter(oppPlayersp[i]);

            }
           // RoboCup.Server srv = new RoboCup.Server(std);

            inputs[j] = DistFromCenter(mySelfp);

            Console.Write("The state inputs are: ");
            for (int i = 0; i < inputs.Length; i++)
                Console.Write("\t {0:f}", inputs[i]);
           
            return inputs;
        }
        //double temp_reward;
        static int count;
        #region Reinforcement controlled Keeper With Ball action
        public override void keeperWithBall()
        {
            config = RLConfig.Load("RLConfig.xml");
           count++;
           // Console.WriteLine("Trial : {0}", count);
          // base.keeperWithBall();
            
            Vector[] tmatesp = null;
            Vector[] oppPlayersp = null;
            Vector posToPass = new Vector(0, 0);
            Command soc = new Command();
            soc.Com = "illegal";
            int action = 0, j = 0, h = 0, v = 0, g = 0; double ang1, ang2, ang3;
            tmatesp = myTeammatesPlayers(teammatesp, config.num_Keepers);
            oppPlayersp = myTeammatesPlayers(opp, config.num_Takers);
            tmatesp[config.num_Keepers-1] = mySelfp;

            int possibleStates = config.numFeatures; 
            double[] statesVector = new double[possibleStates]; 
            double[] prevStateVector = new double[possibleStates];
            //statesVector = keeperstateVars(possibleStates);
            double[] closestmateOpp = new double[teammatesp.Length - 1];
            try
            {
                if (inputs == null)
                {
                    inputs = new double[config.numFeatures];
                }


                for (int i = 0; i < closestmateOpp.Length; i++)
                {
                    closestmateOpp[i] = 1000;
                }


                for (int i = 0; i < tmatesp.Length; i++)
                {
                    if (i != Unum - 1)
                    {
                        inputs[j++] = tmatesp[i].distance(mySelfp);
                        getClosestInSetTo(oppPlayersp, tmatesp[i], ref closestmateOpp[h]);
                        inputs[j++] = closestmateOpp[h++];
                        inputs[j++] = DistFromCenter(tmatesp[i]);
                        ang1 = mySelfp.angle(tmatesp[i]);
                        ang2 = mySelfp.angle(oppPlayersp[0]);
                        ang3 = mySelfp.angle(oppPlayersp[1]);
                        inputs[j++] = Math.Min(Magnitude(ang1 - ang2), Magnitude(ang1 - ang3));
                    }
                }

                for (int i = 0; i < oppPlayersp.Length; i++)
                {
                    inputs[v++] = oppPlayersp[i].distance(mySelfp);
                    inputs[v++] = DistFromCenter(oppPlayersp[i]);

                }


                inputs[v] = DistFromCenter(mySelfp);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Exception:Array index out of range :-> Increase the num of Features");
                throw e;
            }


            statesVector = inputs;
           
            switch(config.learningMethod){
                case "Q_Learning":
                    {
                       // qlPolicy = new Q_LearningAgent(); // turn on when running SARSA otherwise off
                        if (qlPolicy == null)
                        {
                            Console.WriteLine("The Policy object is null");
                            base.keeperWithBall();

                        }
                        //Reading the saved xml file so as to use it in the next episode step
                        /*
                        XDocument loadVector = XDocument.Load("Previous_State.xml");
                        var varVector = loadVector.Element("state-variables").Elements("variable");
                        prevStateVector = varVector.Select(x => Double.Parse(x.Value)).ToArray();
                        //do the same with last action
                        XDocument loadValue = XDocument.Load("lactAction.xml");
                        string data = loadValue.Root.Element("lastAction").Value;
                        lastAction = int.Parse(data);


                        //verify by printing
                        Console.WriteLine("!!!!!! ---- last action is = {0}", lastAction);
                        */
                        //testing if it has read the xml file back to an array by saving it again as different file
                        XDocument testState = new XDocument();
                        testState.Add(new XElement("state-variables", statesVector.Select(x => new XElement("variable", x))));
                        testState.Save("Test_Previous_State.xml");
                        ///////////
                        if (statesVector.Length > 0)
                        { // if we can calculate state vars
                            // Call startEpisode() on the first SMDP step
                            if (timeLastAction == 0)
                            {
                                //temp_reward = 0;
                                action = qlPolicy.startEpisode(statesVector);

                            }
                            else if (timeLastAction == std.Time - 1 && lastAction > 0)
                            {   // if we were in the middle of a pass last cycle
                                temp_reward = body.Time - timeLastAction;
                                action = lastAction;         // then we follow through with it
                            }    // Call step() on all but first SMDP step
                            else
                            {
                                temp_reward = body.Time - timeLastAction;
                                //action = saPolicy.step(std.Time - timeLastAction, statesVector);
                                action = saPolicy.step(temp_reward, statesVector);
                               
                               // action = qlPolicy.step(temp_reward, statesVector);
                            }
                            lastAction = action;
                            // temp_reward += 1;
                            //Save the previous state to an xml file
                            XDocument prevState = new XDocument();
                            prevState.Add(new XElement("state-variables", statesVector.Select(x => new XElement("variable", x))));
                            prevState.Save("Previous_State.xml");
                            //Save the last action to xml file
                            XDocument prevAction = new XDocument();
                            prevAction.Add(new XElement("Actions", new XElement("lastAction", lastAction)));
                            prevAction.Save("lactAction.xml");
                        }
                        else
                        { // if we don't have enough info to calculate state vars
                            action = 1;  // hold ball 
                            temp_reward = body.Time - timeLastAction;
                            // return;
                        }
                        
                        //action = saPolicy.GetAction(inputs.Length);
                        // action = saPolicy.; 
                        //temp_reward += 1;      
                        max = Unum - 1;
                        posToPass = mySelfp;
                        if (action < 0)
                            lastAction = 0;
                        else lastAction = action;
                        timeLastAction = body.Time;
                        
                            break;
                    }
                case "SARSA":
                    {

                       //saPolicy = new SarsaAgent(); // Turn on when running Q_Learning otherwise off
                        if (saPolicy == null)
                        {
                            Console.WriteLine("The Policy object is null");
                            base.keeperWithBall();

                        }
                        //Reading the saved xml file so as to use it in the next episode step
                        /*
                        XDocument loadVector = XDocument.Load("Previous_State.xml");
                        var varVector = loadVector.Element("state-variables").Elements("variable");
                        prevStateVector = varVector.Select(x => Double.Parse(x.Value)).ToArray();
                        //do the same with last action
                        XDocument loadValue = XDocument.Load("lactAction.xml");
                        string data = loadValue.Root.Element("lastAction").Value;
                        lastAction = int.Parse(data);
                       
                       
                        //verify by printing
                        Console.WriteLine("!!!!!! ---- last action is = {0}", lastAction);
                        */
                        //testing if it has read the xml file back to an array by saving it again as different file
                        //XDocument testState = new XDocument();
                        //testState.Add(new XElement("state-variables", statesVector.Select(x => new XElement("variable", x))));
                       // testState.Save("Test_Previous_State.xml");
                        ///////////
                        if (statesVector.Length > 0)
                        { // if we can calculate state vars
                            // Call startEpisode() on the first SMDP step
                            if (timeLastAction == 0)
                            {
                                //temp_reward = 0;
                                action = saPolicy.startEpisode(statesVector);

                            }
                            else if (timeLastAction == std.Time - 1 && lastAction > 0)
                            {   // if we were in the middle of a pass last cycle
                                temp_reward = body.Time - timeLastAction;
                                action = lastAction;         // then we follow through with it
                            }    // Call step() on all but first SMDP step
                            else
                            {
                                //action = saPolicy.step(std.Time - timeLastAction, statesVector);
                                //action = saPolicy.step(saPolicy.lastReward, statesVector);
                                temp_reward = body.Time - timeLastAction;
                                action = saPolicy.step(temp_reward, statesVector);
                            }
                            lastAction = action;
                           // temp_reward += 1;
                            //Save the previous state to an xml file
                           // XDocument prevState = new XDocument();
                           // prevState.Add(new XElement("state-variables", statesVector.Select(x => new XElement("variable", x))));
                           // prevState.Save("Previous_State.xml");
                            //Save the last action to xml file
                           // XDocument prevAction = new XDocument();                            
                           // prevAction.Add( new XElement("Actions", new XElement("lastAction",lastAction)));
                           // prevAction.Save("lactAction.xml");
                        }
                        else
                        { // if we don't have enough info to calculate state vars
                            action = 1;  // hold ball 
                            temp_reward = body.Time - timeLastAction;
                            // return;
                        }


                        //action = saPolicy.GetAction(inputs.Length);
                        // action = saPolicy.; 
                        //temp_reward += 1; 
                        max = Unum - 1;
                        posToPass = mySelfp;
                        if (action < 0)
                            lastAction = 0;
                        else lastAction = action;
                        timeLastAction = body.Time;
                        
                        break;
                    }

                default:
                    Console.WriteLine("Default Case does not exist");
                    break;
        } //end of a switch
            if (action == Unum - 1)
            {
                holdBall(0.7);
                return;
            }
            else
            {
                Vector pos = new Vector(0, 0);
                pos.assign(teammates[action + 1].X, teammates[action + 1].Y);
                //pos.assign(predictPlayerPosAfterNrCycles(teammates[action + 1], 4, 30, null, null, false)); //Sabre commented out this to check if it can solve issue of passing wrongly
                pos.assign((float)Math.Min(20 / 2, Math.Max(-20 / 2, pos.X)), (float)Math.Min(20 / 2, Math.Max(-20 / 2, pos.Y)));
                directPass(pos, "fast");
                /* To be added on this RL algorithm is the estimate of Q and update of Qvalues */
               // get a new state using keeperStates()
               // then update the Q-Value
                
                return;
            }

          // int possibleStates = 13; double[] vec = new double[possibleStates];
            //vec = keeperstateVars(possibleStates);
           //Console.WriteLine("States are : {0:f} {1:f} {2:f} {3:f} {4:f} {5:f} {6:f} {7:f} {8:f} {9:f} {10:f} {11:f} {12:f} ", vec[0], vec[1], vec[2], vec[3], vec[4], vec[5], vec[6], vec[7], vec[8], vec[9], vec[10], vec[11], vec[12]);
           // base.keeperWithBall();

        }
        #endregion

        #region Vector Helper methods
        private double DistFromCenter(Vector pos)
        {
            return Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y);
        }

        private double Magnitude(double val)
        {
            return Math.Sqrt(val * val);
        }

        #endregion
    }
}
