using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HeuristicLab.Problems;
using HeuristicLab.Problems.Instances.TSPLIB;

namespace Experimenter
{
    internal class Ant_Colony_Optim_TSP
    {
        private Random random = new Random(0);

        public  double alpha{get; set;}
        
        public double beta{get; set;}

        public  double rho{ get; set; }
        public double Q{ get; set; }

        public  int numCities{ get; set; }
        
        public int numAnts{ get; set; } 
        public int  maxTime{ get; set; }

        public int  maxRand;

        public FileInfo Actualfile;
        



        
        

        

        public double BestInitial {get; set;}
        public double Best { get; set; }
        public int Time {get; set;}
        public int[] Trail { get; set; }

     

        
        public Ant_Colony_Optim_TSP(string ProblemName, double Alpha, double Beta, double Rho, double Q_ , int NumCities, int NumAnts, int MaxTime)
        {
            Actualfile = new FileInfo(ProblemName);

            alpha = Alpha;
            beta = Beta;
            rho = Rho;
            Q = Q_ ;
            numCities = NumCities;
            numAnts = NumAnts;
            maxTime = MaxTime;

           

           
        try
            {
               
                int[][]  dists = ReadProblem(ProblemName);

                int[][]  ants = InitAnts(numAnts, numCities); // initialize ants to random trails

                int[]  bestTrail = BestTrail(ants, dists); // determine the best initial trail

            

                double bestLength = Length(bestTrail, dists); // the length of the best initial trail

                BestInitial = bestLength;
                     
                double[][]  pheromones = InitPheromones(numCities);

                int time = 0;

                while (time < maxTime)
                {
                    UpdateAnts(ants, pheromones, dists);
                    UpdatePheromones(pheromones, ants, dists);

                    int[]  currBestTrail = BestTrail(ants, dists);

                    double currBestLength = Length(currBestTrail, dists);
                    if (currBestLength < bestLength)
                    {
                        bestLength = currBestLength;
                        bestTrail = currBestTrail;
                        Time = time; 
                    }

                    ++time;
                }
                
                Trail = bestTrail; Best = bestLength;

            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message,"Runtime error", MessageBoxButton.OK) == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        int[][] ReadProblem(string p)
        {

            int size = 0;
            List<double[]> coordinates = new List<double[]>();
            using (StreamReader sr = new StreamReader(Actualfile.FullName))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {


                        if (line.Contains("DIMENSION: "))
                        {
                            size = Convert.ToInt32(line.Replace("DIMENSION: ", ""));

                        }
                        if (line.Contains("DIMENSION : "))
                        {
                            size = Convert.ToInt32(line.Replace("DIMENSION : ", ""));

                        }

                        char[] c = line.ToCharArray();





                        if (Char.IsDigit(c[0]))
                        {
                            string[] coodiny = line.Split(' ');
                            double[] coor = new double[2];
                            coor[0] = Convert.ToDouble(coodiny[1].Replace('.', ','));
                            coor[1] = Convert.ToDouble(coodiny[2].Replace('.', ','));



                            coordinates.Add(coor);

                        }

                        else if (Char.IsWhiteSpace(c[0]))
                        {

                            string[] coodiny = line.Split(' ');
                            double[] coor = new double[2];
                            coor[0] = Convert.ToDouble(coodiny[1].Replace('.', ','));
                            coor[1] = Convert.ToDouble(coodiny[2].Replace('.', ','));

                            coordinates.Add(coor);
                        }





                    }


                }




                int[][] distances = new int[size][];
                for (int i = 0; i < distances.Length; ++i)
                    distances[i] = new int[size];


                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (i == j)
                        {
                            distances[i][j] = distances[j][i] = 0;

                        }

                        else
                        {
                            double[] x = coordinates[i];
                            double[] y = coordinates[j];

                            double dx = x[0] - y[0];
                            double dy = x[1] - y[1];



                            distances[i][j] = Convert.ToInt32(Math.Sqrt(dx * dx + dy * dy));
                        }
                    }



                }
                return distances;

            }
            
            
        }


         int[][] InitAnts(int numAnts, int numCities)
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k < numAnts; ++k)
            {
                int start = random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }

         int[] RandomTrail(int start, int numCities) // helper for InitAnts
        {
            int[] trail = new int[numCities];

            for (int i = 0; i < numCities; ++i) { trail[i] = i; } // sequential

            for (int i = 0; i < numCities; ++i) // Fisher-Yates shuffle
            {
                int r = random.Next(i, numCities);
                int tmp = trail[r]; trail[r] = trail[i]; trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start); // put start at [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

         int IndexOfTarget(int[] trail, int target) // helper for RandomTrail
        {
            for (int i = 0; i < trail.Length; ++i)
            {
                if (trail[i] == target)
                    return i;
            }
            throw new Exception("Target not found in IndexOfTarget");
        }

         double Length(int[] trail, int[][] dists) // total length of a trail
        {
            double result = 0.0;
            for (int i = 0; i < trail.Length - 1; ++i)
                result += Distance(trail[i], trail[i + 1], dists);
            result += Distance(trail[trail.Length - 1], trail[0], dists);
            return result;
        }

        // --------------------------------------------------------------------------------------------

         int[] BestTrail(int[][] ants, int[][] dists) // best trail has shortest total length
        {
            double bestLength = Length(ants[0], dists);
            int idxBestLength = 0;
            for (int k = 1; k < ants.Length; ++k)
            {
                double len = Length(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numCities = ants[0].Length;
            int[] bestTrail = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrail, 0);
            return bestTrail;
        }

        // --------------------------------------------------------------------------------------------

        double[][] InitPheromones(int numCities)
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i < numCities; ++i)
                pheromones[i] = new double[numCities];
            for (int i = 0; i < pheromones.Length; ++i)
                for (int j = 0; j < pheromones[i].Length; ++j)
                    pheromones[i][j] = 0.00000000000001; // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

         void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            for (int k = 0; k < ants.Length; ++k)
            {
                int start = random.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

         int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i < numCities - 1; ++i)
            {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

         int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i < probs.Length; ++i)
                cumul[i + 1] = cumul[i] + probs[i]; // consider setting cumul[cuml.Length-1] to 1.00

            double p = random.NextDouble();

            for (int i = 0; i < cumul.Length - 1; ++i)
                if (p >= cumul[i] && p < cumul[i + 1])
                    return i;
            throw new Exception("Failure to return valid city in NextCity");
        }

         double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities]; // inclues cityX and visited cities
            double sum = 0.0; // sum of all tauetas
            for (int i = 0; i < taueta.Length; ++i) // i is the adjacent city
            {
                if (i == cityX)
                    taueta[i] = 0.0; // prob of moving to self is 0
                else if (visited[i] == true)
                    taueta[i] = 0.0; // prob of moving to a visited city is 0
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta); // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.00000000000001)
                        taueta[i] = 0.00000000000001;
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                        taueta[i] = double.MaxValue / (numCities * 100);
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i < probs.Length; ++i)
                probs[i] = taueta[i] / sum; // big trouble if sum = 0.0
            return probs;
        }

        // --------------------------------------------------------------------------------------------

         void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (int i = 0; i < pheromones.Length; ++i)
            {
                for (int j = i + 1; j < pheromones[i].Length; ++j)
                {
                    for (int k = 0; k < ants.Length; ++k)
                    {
                        double length = Length(ants[k], dists); // length of ant k trail
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true) increase = (Q / length);

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0000001)
                            pheromones[i][j] = 0.0000001;
                        else if (pheromones[i][j] > 100000.0)
                            pheromones[i][j] = 100000.0;

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

         bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY) return true;
            else if (idx == 0 && trail[lastIndex] == cityY) return true;
            else if (idx == 0) return false;
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY) return true;
            else if (idx == lastIndex && trail[0] == cityY) return true;
            else if (idx == lastIndex) return false;
            else if (trail[idx - 1] == cityY) return true;
            else if (trail[idx + 1] == cityY) return true;
            else return false;
        }

        // --------------------------------------------------------------------------------------------

         int Distance(int cityX, int cityY, int[][] dists)
        {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------
 
       
    }
}