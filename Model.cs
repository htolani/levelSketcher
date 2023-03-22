/*
NAME: Model.cs
DESCRIPTION: The file where WaveFunctionCollapse is implemented.
*/
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

abstract class Model
{   protected bool[][] wave;
    //2D array of Booleans to represent the superposition of possible states of each cell in a grid (can be thought of as the solution space)
    protected int[][][] propagator;
    //3D array of integers to store info about the possible patterns that can be used for neighboring tiles based on the currently selected pattern
    protected int[] observed;
    //1D array of integers to store the state of the cells that have been measured or observed so far

    int[][][] adjacency_relations;
    //3D array of integers to hold info about the compatibility of each pair of adjacent cells in each possible pair of states (comes from Adjacency Matrix- storing information about the adjacency of cells in different states and State Relations- storing information about the relationship between different states)

    (int, int)[] stack;
    //Stack of integer pairs to hold the coordinates of the cells to be updated after a cell is measured (or observed)
    int stacksize, observed_so_far;
    //Maintaining counts of the sizes of stack and measured 

    protected int MX, MY, T, N;
    //MX represents the width of the game level in (the number of) tiles.
    //MY represents the height of the game level in (the number of) tiles.
    //T represents the number of unique tiles in the tileset.
    //N represents the size of the tileset that is used to generate the game level (NxN).

    protected bool periodic, ground;
    //periodic indicates whether the grid has periodic boundary conditions or not
    //ground specifies whether the wave should be grounded or not

    protected double[] weights;
    //1D array to store the weights of each tile pattern (also indicative of probability)
    double[] weight_logWeight, distribution;
    //distribution describes the weight/probability distribution

    protected int[] sumsOfOnes;
    double weights_sum, weights_logWeights_sum, entropy_initial;
    protected double[] sumsOfWeights, weights_logWeights_sums_set, tile_entropies;

    public enum Heuristic {Entropy, MRV, Scanline};
    //Specifies three heuristics used to select the next node to observe: Entropy, MRV (minimum remaining values), and Scanline
    Heuristic heuristic;

    protected Model(int width, int height, int N, bool periodic, Heuristic heuristic)
    {   MX = width;
        MY = height;
        this.N = N;
        this.periodic = periodic;
        this.heuristic = heuristic;
    }

    void Init() //Initializes all the variable values
    {   wave = new bool[MX * MY][];
        adjacency_relations = new int[wave.Length][][];
        for (int i=0; i<wave.Length; i++)
        {   wave[i] = new bool[T];
            adjacency_relations[i] = new int[T][];
            for (int t=0; t<T; t++)
                adjacency_relations[i][t] = new int[4];
        }
        distribution = new double[T];
        observed = new int[MX * MY];

        weight_logWeight = new double[T];
        weights_sum = 0;
        weights_logWeights_sum = 0;

        for (int t=0; t<T; t++)
        {   weight_logWeight[t] = weights[t] * Math.Log(weights[t]);
            weights_sum += weights[t];
            weights_logWeights_sum += weight_logWeight[t];
        }

        entropy_initial = Math.Log(weights_sum) - weights_logWeights_sum / weights_sum;

        sumsOfOnes = new int[MX * MY];
        sumsOfWeights = new double[MX * MY];
        weights_logWeights_sums_set = new double[MX * MY];
        tile_entropies = new double[MX * MY];

        stack = new (int, int)[wave.Length * T];
        stacksize = 0;
    }

    public bool Run(int seed, int limit) //The main function of WFC, takes a seed value and a limit and returns a boolean value indicating whether the algorithm has succeeded in generating a game level or not.
    {   if (wave == null)
            Init();

        Clear();
        Random random = new(seed);

        for (int l=0; l<limit || limit<0; l++)
        {   int node = NextUnobservedNode(random);
            if (node >= 0)
            {   Observe(node, random);
                bool success = Propagate();
                if (!success)
                    return false;
            }

            else
            {   for (int i=0; i<wave.Length; i++)
                    for (int t = 0; t < T; t++)
                        if (wave[i][t])
                        {   observed[i] = t;
                            break;
                        }
                return true;
            }
        }

        return true;
    }

    int NextUnobservedNode(Random random) //Used to select the next unobserved node in the game level, based on the chosen heuristic
    {   if (heuristic == Heuristic.Scanline)
        {   for (int i=observed_so_far; i<wave.Length; i++)
            {   if (!periodic && (i % MX + N > MX || i / MX + N > MY))
                    continue;
                if (sumsOfOnes[i] > 1)
                {   observed_so_far = i + 1;
                    return i;
                }
            }
            return -1;
        }

        double min = 1E+4;
        int argmin = -1;
        for (int i=0; i<wave.Length; i++)
        {   if (!periodic && (i % MX + N > MX || i / MX + N > MY))
                continue;
            int remainingValues = sumsOfOnes[i];
            double entropy = heuristic == Heuristic.Entropy ? tile_entropies[i] : remainingValues;
            if (remainingValues > 1 && entropy <= min)
            {   double noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {   min = entropy + noise;
                    argmin = i;
                }
            }
        }
        return argmin;
    }

    void Observe(int node, Random random) //Selects a value for the node to be observed based on the (weight) distribution
    {   bool[] w = wave[node];
        for (int t=0; t<T; t++)
            distribution[t] = w[t] ? weights[t] : 0.0;
        int r = distribution.Random(random.NextDouble());
        for (int t=0; t<T; t++)
            if (w[t] != (t==r))
                Ban(node, t);
    }

    bool Propagate() //Propagates the constraints that result from the observed values and bans any values violating these constraints
    {   while (stacksize>0)
        {   (int i1, int t1) = stack[stacksize - 1];
            stacksize--;

            int x1 = i1 % MX;
            int y1 = i1 / MX;

            for (int d=0; d<4; d++)
            {   int x2 = x1 + dx[d];
                int y2 = y1 + dy[d];
                if (!periodic && (x2 < 0 || y2 < 0 || x2 + N > MX || y2 + N > MY))
                    continue;

                if (x2 < 0)
                    x2 += MX;
                else if (x2 >= MX)
                    x2 -= MX;
                if (y2 < 0)
                    y2 += MY;
                else if (y2 >= MY)
                    y2 -= MY;

                int i2 = x2 + y2 * MX;
                int[] p = propagator[d][t1];
                int[][] adj_rel = adjacency_relations[i2];

                for (int l=0; l<p.Length; l++)
                {   int t2 = p[l];
                    int[] rel = adj_rel[t2];

                    rel[d]--;
                    if (rel[d] == 0)
                        Ban(i2, t2);
                }
            }
        }

        return sumsOfOnes[0]>0;
    }

    void Ban(int i, int t) //Rejects a tile that will increase the entropy on being placed at the current location,  by setting all of its adjacency_relations elements to zero
    {   wave[i][t] = false;

        int[] rel = adjacency_relations[i][t];
        for (int d=0; d<4; d++)
            rel[d] = 0;
        stack[stacksize] = (i, t);
        stacksize++;

        sumsOfOnes[i] -= 1;
        sumsOfWeights[i] -= weights[t];
        weights_logWeights_sums_set[i] -= weight_logWeight[t];

        double sum = sumsOfWeights[i];
        tile_entropies[i] = Math.Log(sum) - weights_logWeights_sums_set[i] / sum;
    }

    void Clear() //Resets properties
    {   for (int i=0; i<wave.Length; i++)
        {   for (int t=0; t<T; t++)
            {   wave[i][t] = true;
                for (int d=0; d<4; d++)
                    adjacency_relations[i][t][d] = propagator[opposite[d]][t].Length;
            }

            sumsOfOnes[i] = weights.Length;
            sumsOfWeights[i] = weights_sum;
            weights_logWeights_sums_set[i] = weights_logWeights_sum;
            tile_entropies[i] = entropy_initial;
            observed[i] = -1;
        }
        observed_so_far = 0;

        if (ground)
        {   for (int x=0; x<MX; x++)
            {   for (int t=0; t<T - 1; t++)
                    Ban(x + (MY - 1) * MX, t);
                for (int y=0; y<MY - 1; y++)
                    Ban(x + y * MX, T - 1);
            }
            Propagate();
        }
    }

    public abstract void Save(string filename);

    static int[] opposite = {2, 3, 0, 1};
    protected static int[] dx = {-1, 0, 1, 0};
    protected static int[] dy = {0, 1, 0, -1};
}

// Reference: Maxim Gumin's WaveFunction GitHub Project (https://github.com/mxgmn/WaveFunctionCollapse)
