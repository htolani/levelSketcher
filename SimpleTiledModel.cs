/*
NAME: SimpleTiledModel.cs
DESCRIPTION: This is the file where entropy based calculations are made for tilesets.
*/
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

class SimpleTiledModel : Model
{
    List<int[]> tiles;
    List<string> tilenames;
    int tilesize;
    bool blackBackground;

    // This is a constructor for SimpleTiledModel that takes in parameters for the name of the model, 
    // the width and height of the model, whether it's periodic or not, 
    // whether the background is black or not, and a heuristic object.
    // It initializes the base class with the width, height, and heuristic object.
    public SimpleTiledModel(string name, int width, int height, bool periodic, bool blackBg, Heuristic heuristic) : base(width, height, 1, periodic, heuristic)
    {
        // It also loads an XML file based on the name of the model, gets a boolean flag for whether the tiles are unique,
        this.blackBackground = blackBg;
        XElement xroot = XDocument.Load($"tilesets/{name}.xml").Root;
        bool unique = xroot.Get("unique", false);

        static int[] tile(Func<int, int, int> f, int size)
        {
            int[] result = new int[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) result[x + y * size] = f(x, y);
            return result;
        };
        // It defines three static methods for rotating, reflecting, and tiling an array of integers.

        static int[] rotate(int[] array, int size) => tile((x, y) => array[size - 1 - y + x * size], size);
        static int[] reflect(int[] array, int size) => tile((x, y) => array[size - 1 - x + y * size], size);
        // It initializes empty lists for tiles, tile names, and tile weights, 
        // as well as a dictionary for the first occurrence of each tile name.


        tiles = new List<int[]>();
        tilenames = new List<string>();
        var weightList = new List<double>();

        var action = new List<int[]>();
        var firstOccurrence = new Dictionary<string, int>();
        // The program loops through each tile and extracts its name using the "Get" method of the "xtile" object.
        foreach (XElement xtile in xroot.Element("tiles").Elements("tile"))
        {
            string tilename = xtile.Get<string>("name");
            
            Func<int, int> a, b;
            int cardinality;
            // The program then determines the tile's symmetry and sets some variables based on the symmetry.
            char sym = xtile.Get("symmetry", 'X');
            // The variables include the number of possible rotations ("cardinality") and two functions ("a" and "b") used to calculate the rotated tile's coordinates.
            if (sym == 'L')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i + 1 : i - 1;
            }
            else if (sym == 'T')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i : 4 - i;
            }
            else if (sym == 'I')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => i;
            }
            else if (sym == '\\')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => 1 - i;
            }
            else if (sym == 'F')
            {
                cardinality = 8;
                a = i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4;
                b = i => i < 4 ? i + 4 : i - 4;
            }
            else
            {
                cardinality = 1;
                a = i => i;
                b = i => i;
            }
            // The program then adds the current tile's name and the number of actions performed so far ("T") to a dictionary called "firstOccurrence".
            // The dictionary keeps track of the first time each tile was encountered, allowing the program to avoid processing duplicates later on.
            T = action.Count;
            firstOccurrence.Add(tilename, T);

            int[][] map = new int[cardinality][];
            // This code block creates a 2D array called "map" with "cardinality" rows and 8 columns.
            // The "a" and "b" functions calculated earlier are used to determine the coordinates of the rotated tile.
            for (int t = 0; t < cardinality; t++)
            {
                map[t] = new int[8];

                map[t][0] = t;
                map[t][1] = a(t);
                map[t][2] = a(a(t));
                map[t][3] = a(a(a(t)));
                map[t][4] = b(t);
                map[t][5] = b(a(t));
                map[t][6] = b(a(a(t)));
                map[t][7] = b(a(a(a(t))));

                for (int s = 0; s < 8; s++) map[t][s] += T;

                action.Add(map[t]);
            }

           if (unique)
{
    // If "unique" is true, load bitmaps for each unique tile.
    for (int t = 0; t < cardinality; t++)
    {
        int[] bitmap;
        // Load bitmap from file path.
        (bitmap, tilesize, tilesize) = BitmapHelper.LoadBitmap($"tilesets/{name}/{tilename} {t}.png");
        // Add loaded bitmap to "tiles" list.
        tiles.Add(bitmap);
        // Add tile name to "tilenames" list.
        tilenames.Add($"{tilename} {t}");
    }
}
else
{
    // If "unique" is false, load a single bitmap and create all its rotated and reflected versions.
    int[] bitmap;
    // Load bitmap from file path.
    (bitmap, tilesize, tilesize) = BitmapHelper.LoadBitmap($"tilesets/{name}/{tilename}.png");
    // Add loaded bitmap to "tiles" list.
    tiles.Add(bitmap);
    // Add tile name to "tilenames" list.
    tilenames.Add($"{tilename} 0");
    // Create rotated and reflected versions of the loaded bitmap and add them to the "tiles" list.
    for (int t = 1; t < cardinality; t++)
    {   
        if (t <= 3) tiles.Add(rotate(tiles[T + t - 1], tilesize));
        if (t >= 4) tiles.Add(reflect(tiles[T + t - 4], tilesize));
        // Add tile name to "tilenames" list.
        tilenames.Add($"{tilename} {t}");
    }
}

// Add weights to each tile.
for (int t = 0; t < cardinality; t++) weightList.Add(xtile.Get("weight", 1.0));

        }
        // This block of code sets up the propagator which represents the compatibility between pairs of tiles in the puzzle.

        T = action.Count;
weights = weightList.ToArray();

// The propagator is stored in both dense and sparse forms.
propagator = new int[4][][];
var densePropagator = new bool[4][][];

// In the dense form, a 4xTxT array is created, where T is the number of distinct tiles 
// and 4 represents the 4 possible directions of the neighbor relationship.
for (int d = 0; d < 4; d++)
{
    densePropagator[d] = new bool[T][];
    propagator[d] = new int[T][];
    for (int t = 0; t < T; t++) 
    {
        // Initialize each cell in the densePropagator array to false.
        densePropagator[d][t] = new bool[T];
    }
}

foreach (XElement xneighbor in xroot.Element("neighbors").Elements("neighbor"))
{
    string[] left = xneighbor.Get<string>("left").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    string[] right = xneighbor.Get<string>("right").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

    // Find the indices of the tiles in the action list.
    int L = action[firstOccurrence[left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])];
    int D = action[L][1];
    int R = action[firstOccurrence[right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])];
    int U = action[R][1];

    // Set the corresponding cells in the densePropagator array to true to indicate compatibility between the tiles.
    densePropagator[0][R][L] = true;
    densePropagator[0][action[R][6]][action[L][6]] = true;
    densePropagator[0][action[L][4]][action[R][4]] = true;
    densePropagator[0][action[L][2]][action[R][2]] = true;

    densePropagator[1][U][D] = true;
    densePropagator[1][action[D][6]][action[U][6]] = true;
    densePropagator[1][action[U][4]][action[D][4]] = true;
    densePropagator[1][action[D][2]][action[U][2]] = true;
}

// Copy the values from densePropagator to propagator for the 2nd and 3rd dimensions (down and right) 
// to save memory by using a sparse representation.
for (int t2 = 0; t2 < T; t2++) 
{
    for (int t1 = 0; t1 < T; t1++)
    {
        densePropagator[2][t2][t1] = densePropagator[0][t1][t2];
        densePropagator[3][t2][t1] = densePropagator[1][t1][t2];
    }
}


    // Create a sparse version of the propagator, where each cell in the array is a list of integers representing the indices of compatible tiles.
List<int>[][] sparsePropagator = new List<int>[4][];
for (int d = 0; d < 4; d++)
{
sparsePropagator[d] = new List<int>[T];
for (int t = 0; t < T; t++) sparsePropagator[d][t] = new List<int>();
}

// Populate the sparsePropagator array by iterating through the densePropagator array.
for (int d = 0; d < 4; d++) for (int t1 = 0; t1 < T; t1++)
{
List<int> sp = sparsePropagator[d][t1];
bool[] tp = densePropagator[d][t1];

    // For each t2 where tp[t2] is true, add t2 to the corresponding list in sparsePropagator.
    for (int t2 = 0; t2 < T; t2++) if (tp[t2]) sp.Add(t2);

    int ST = sp.Count;
    // If a tile has no neighbors in a particular direction, print an error message.
    if (ST == 0) Console.WriteLine($"ERROR: tile {tilenames[t1]} has no neighbors in direction {d}");
    // Convert the list in sparsePropagator to an array in propagator.
    propagator[d][t1] = new int[ST];
    for (int st = 0; st < ST; st++) propagator[d][t1][st] = sp[st];

            }
    }

public override void Save(string filename)
{
    // Create an array to hold the bitmap data for the output image
    int[] bitmapData = new int[MX * MY * tilesize * tilesize];

    // If the wave has collapsed into a solution, use the observed tiles to create the output image
    if (observed[0] >= 0)
    {
        for (int x = 0; x < MX; x++)
        {
            for (int y = 0; y < MY; y++)
            {
                // Get the tile data for the observed tile at the current location
                int[] tile = tiles[observed[x + y * MX]];

                // Copy the tile data into the appropriate position in the bitmap data array
                for (int dy = 0; dy < tilesize; dy++)
                {
                    for (int dx = 0; dx < tilesize; dx++)
                    {
                        bitmapData[x * tilesize + dx + (y * tilesize + dy) * MX * tilesize] = tile[dx + dy * tilesize];
                    }
                }
            }
        }
    }
    // If the wave has not yet collapsed, use the wave to create the output image
    else
    {
        for (int i = 0; i < wave.Length; i++)
        {
            int x = i % MX, y = i / MX;

            // If blackBackground is enabled and the current cell can only contain one tile, set the pixel to black
            if (blackBackground && sumsOfOnes[i] == T)
            {
                for (int yt = 0; yt < tilesize; yt++)
                {
                    for (int xt = 0; xt < tilesize; xt++)
                    {
                        bitmapData[x * tilesize + xt + (y * tilesize + yt) * MX * tilesize] = 255 << 24;
                    }
                }
            }
            // Otherwise, set the pixel color based on the weighted average of the possible tiles at the current location
            else
            {
                bool[] w = wave[i];
                double normalization = 1.0 / sumsOfWeights[i];
                for (int yt = 0; yt < tilesize; yt++)
                {
                    for (int xt = 0; xt < tilesize; xt++)
                    {
                        // Calculate the weighted average color of the possible tiles at the current location
                        int idi = x * tilesize + xt + (y * tilesize + yt) * MX * tilesize;
                        double r = 0, g = 0, b = 0;
                        for (int t = 0; t < T; t++)
                        {
                            if (w[t])
                            {
                                int argb = tiles[t][xt + yt * tilesize];
                                r += ((argb & 0xff0000) >> 16) * weights[t] * normalization;
                                g += ((argb & 0xff00) >> 8) * weights[t] * normalization;
                                b += (argb & 0xff) * weights[t] * normalization;
                            }
                        }

                        // Convert the color to an integer ARGB value and store it in the bitmap data array
                        bitmapData[idi] = unchecked((int)0xff000000 | ((int)r << 16) | ((int)g << 8) | (int)b);
                    }
                }
            }
        }
    }

    // Save the bitmap data as an image file using the BitmapHelper class
     BitmapHelper.SaveBitmap(bitmapData, MX * tilesize, MY * tilesize, filename);
    }
public string TextOutput()
{
    // create a new StringBuilder object to store the output
    var result = new System.Text.StringBuilder();

    // loop through each row of the observed tiles
    for (int y = 0; y < MY; y++)
    {
        // loop through each column of the observed tiles in the current row
        for (int x = 0; x < MX; x++)
        {
            // append the name of the current observed tile to the result string, followed by a comma and a space
            result.Append($"{tilenames[observed[x + y * MX]]}, ");
        }
        // add a new line to the result string to separate each row of observed tiles
        result.Append(Environment.NewLine);
    }

    // return the result string as a regular string
    return result.ToString();
}

}
