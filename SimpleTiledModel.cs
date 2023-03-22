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
    List<int[]> tilesList;
    List<string> tileNamesList;
    int tilelen;
    bool blackBckgrnd;
    // This is a constructor for SimpleTiledModel that takes in parameters for the name of the model, 
// the name of a subset, the width and height of the model, whether it's periodic or not, 
// whether the background is black or not, and a heuristic object.
// It initializes the base class with the width, height, and heuristic object.

    public SimpleTiledModel(string name, int width, int height, bool periodic, bool blackBg, Heuristic heuristic) : base(width, height, 1, periodic, heuristic)
    {
        // It also loads an XML file based on the name of the model, gets a boolean flag for whether the tiles are unique,
        this.blackBckgrnd = blackBg;
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


        tilesList = new List<int[]>();
        tileNamesList = new List<string>();
        var wtList = new List<double>();

        var action = new List<int[]>();
        var firstOccurr = new Dictionary<string, int>();
        // The program loops through each tile and extracts its name using the "Get" method of the "xtile" object.
        foreach (XElement xtile in xroot.Element("tiles").Elements("tile"))
        {
            string tilename = xtile.Get<string>("name");

            Func<int, int> a, b;
            int cardnltycnt;
            // The program then determines the tile's symmetry and sets some variables based on the symmetry.
            char sym = xtile.Get("symmetry", 'X');
            // The variables include the number of possible rotations ("cardinality") and two functions ("a" and "b") used to calculate the rotated tile's coordinates.
            if (sym == 'L')
            {
                cardnltycnt = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i + 1 : i - 1;
            }
            else if (sym == 'T')
            {
                cardnltycnt = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i : 4 - i;
            }
            else if (sym == 'I')
            {
                cardnltycnt = 2;
                a = i => 1 - i;
                b = i => i;
            }
            else if (sym == '\\')
            {
                cardnltycnt = 2;
                a = i => 1 - i;
                b = i => 1 - i;
            }
            else if (sym == 'F')
            {
                cardnltycnt = 8;
                a = i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4;
                b = i => i < 4 ? i + 4 : i - 4;
            }
            else
            {
                cardnltycnt = 1;
                a = i => i;
                b = i => i;
            }
            // The program then adds the current tile's name and the number of actions performed so far ("T") to a dictionary called "firstOccurrence".
            // The dictionary keeps track of the first time each tile was encountered, allowing the program to avoid processing duplicates later on.
            T = action.Count;
            firstOccurr.Add(tilename, T);

            int[][] cardMap = new int[cardnltycnt][];
            // This code block creates a 2D array called "map" with "cardinality" rows and 8 columns.
            // The "a" and "b" functions calculated earlier are used to determine the coordinates of the rotated tile.
            for (int t = 0; t < cardnltycnt; t++)
            {
                cardMap[t] = new int[8];

                cardMap[t][0] = t;
                cardMap[t][1] = a(t);
                cardMap[t][2] = a(a(t));
                cardMap[t][3] = a(a(a(t)));
                cardMap[t][4] = b(t);
                cardMap[t][5] = b(a(t));
                cardMap[t][6] = b(a(a(t)));
                cardMap[t][7] = b(a(a(a(t))));

                for (int s = 0; s < 8; s++) cardMap[t][s] += T;

                action.Add(cardMap[t]);
            }

           if (unique)
{
    // If "unique" is true, load bitmaps for each unique tile.
    for (int t = 0; t < cardnltycnt; t++)
    {
        int[] bitmap;
        // Load bitmap from file path.
        (bitmap, tilelen, tilelen) = BitmapHelper.LoadBitmap($"tilesets/{name}/{tilename} {t}.png");
        // Add loaded bitmap to "tiles" list.
        tilesList.Add(bitmap);
        // Add tile name to "tilenames" list.
        tileNamesList.Add($"{tilename} {t}");
    }
}
else
{
    // If "unique" is false, load a single bitmap and create all its rotated and reflected versions.
    int[] bitmap;
    // Load bitmap from file path.
    (bitmap, tilelen, tilelen) = BitmapHelper.LoadBitmap($"tilesets/{name}/{tilename}.png");
    // Add loaded bitmap to "tiles" list.
    tilesList.Add(bitmap);
    // Add tile name to "tilenames" list.
    tileNamesList.Add($"{tilename} 0");
    // Create rotated and reflected versions of the loaded bitmap and add them to the "tiles" list.
    for (int t = 1; t < cardnltycnt; t++)
    {   
        if (t <= 3) tilesList.Add(rotate(tilesList[T + t - 1], tilelen));
        if (t >= 4) tilesList.Add(reflect(tilesList[T + t - 4], tilelen));
        // Add tile name to "tilenames" list.
        tileNamesList.Add($"{tilename} {t}");
    }
}

// Add weights to each tile.
for (int t = 0; t < cardnltycnt; t++) wtList.Add(xtile.Get("weight", 1.0));

        }
        // This block of code sets up the propagator which represents the compatibility between pairs of tiles in the puzzle.

        T = action.Count;
weights = wtList.ToArray();

// The propagator is stored in both dense and sparse forms.
propagator = new int[4][][];
var denseProp = new bool[4][][];

// In the dense form, a 4xTxT array is created, where T is the number of distinct tiles 
// and 4 represents the 4 possible directions of the neighbor relationship.
for (int d = 0; d < 4; d++)
{
    denseProp[d] = new bool[T][];
    propagator[d] = new int[T][];
    for (int t = 0; t < T; t++) 
    {
        // Initialize each cell in the densePropagator array to false.
        denseProp[d][t] = new bool[T];
    }
}

foreach (XElement xneighbor in xroot.Element("neighbors").Elements("neighbor"))
{
    string[] left = xneighbor.Get<string>("left").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    string[] right = xneighbor.Get<string>("right").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

  
    // Find the indices of the tiles in the action list.
    int L = action[firstOccurr[left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])];
    int D = action[L][1];
    int R = action[firstOccurr[right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])];
    int U = action[R][1];

    // Set the corresponding cells in the densePropagator array to true to indicate compatibility between the tiles.
    denseProp[0][R][L] = true;
    denseProp[0][action[R][6]][action[L][6]] = true;
    denseProp[0][action[L][4]][action[R][4]] = true;
    denseProp[0][action[L][2]][action[R][2]] = true;

    denseProp[1][U][D] = true;
    denseProp[1][action[D][6]][action[U][6]] = true;
    denseProp[1][action[U][4]][action[D][4]] = true;
    denseProp[1][action[D][2]][action[U][2]] = true;
}

// Copy the values from densePropagator to propagator for the 2nd and 3rd dimensions (down and right) 
// to save memory by using a sparse representation.
for (int t2 = 0; t2 < T; t2++) 
{
    for (int t1 = 0; t1 < T; t1++)
    {
        denseProp[2][t2][t1] = denseProp[0][t1][t2];
        denseProp[3][t2][t1] = denseProp[1][t1][t2];
    }
}


    // Create a sparse version of the propagator, where each cell in the array is a list of integers representing the indices of compatible tiles.
List<int>[][] sparseProp = new List<int>[4][];
for (int d = 0; d < 4; d++)
{
sparseProp[d] = new List<int>[T];
for (int t = 0; t < T; t++) sparseProp[d][t] = new List<int>();
}

// Populate the sparsePropagator array by iterating through the densePropagator array.
for (int d = 0; d < 4; d++) for (int t1 = 0; t1 < T; t1++)
{
List<int> sp = sparseProp[d][t1];
bool[] tp = denseProp[d][t1];

    // For each t2 where tp[t2] is true, add t2 to the corresponding list in sparsePropagator.
    for (int t2 = 0; t2 < T; t2++) if (tp[t2]) sp.Add(t2);

    int ST = sp.Count;
    // If a tile has no neighbors in a particular direction, print an error message.
    if (ST == 0) Console.WriteLine($"ERROR: tile {tileNamesList[t1]} has no neghbrs along {d}");
    // Convert the list in sparsePropagator to an array in propagator.
    propagator[d][t1] = new int[ST];
    for (int st = 0; st < ST; st++) propagator[d][t1][st] = sp[st];

            }
    }

public override void Save(string filename)
{
    // Create an array to hold the bitmap data for the output image
    int[] bitmap = new int[MX * MY * tilelen * tilelen];

    // If the wave has collapsed into a solution, use the observed tiles to create the output image
    if (observed[0] >= 0)
    {
        for (int x = 0; x < MX; x++)
        {
            for (int y = 0; y < MY; y++)
            {
                // Get the tile data for the observed tile at the current location
                int[] tile = tilesList[observed[x + y * MX]];

                // Copy the tile data into the appropriate position in the bitmap data array
                for (int dy = 0; dy < tilelen; dy++)
                {
                    for (int dx = 0; dx < tilelen; dx++)
                    {
                        bitmap[x * tilelen + dx + (y * tilelen + dy) * MX * tilelen] = tile[dx + dy * tilelen];
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
            if (blackBckgrnd && sumsOfOnes[i] == T)
            {
                for (int yt = 0; yt < tilelen; yt++)
                {
                    for (int xt = 0; xt < tilelen; xt++)
                    {
                        bitmap[x * tilelen + xt + (y * tilelen + yt) * MX * tilelen] = 255 << 24;
                    }
                }
            }
            // Otherwise, set the pixel color based on the weighted average of the possible tiles at the current location
            else
            {
                bool[] w = wave[i];
                double normalization = 1.0 / sumsOfWeights[i];
                for (int yt = 0; yt < tilelen; yt++)
                {
                    for (int xt = 0; xt < tilelen; xt++)
                    {
                        // Calculate the weighted average color of the possible tiles at the current location
                        int idi = x * tilelen + xt + (y * tilelen + yt) * MX * tilelen;
                        double r = 0, g = 0, b = 0;
                        for (int t = 0; t < T; t++)
                        {
                            if (w[t])
                            {
                                int argb = tilesList[t][xt + yt * tilelen];
                                r += ((argb & 0xff0000) >> 16) * weights[t] * normalization;
                                g += ((argb & 0xff00) >> 8) * weights[t] * normalization;
                                b += (argb & 0xff) * weights[t] * normalization;
                            }
                        }

                        // Convert the color to an integer ARGB value and store it in the bitmap data array
                        bitmap[idi] = unchecked((int)0xff000000 | ((int)r << 16) | ((int)g << 8) | (int)b);
                    }
                }
            }
        }
    }

    // Save the bitmap data as an image file using the BitmapHelper class
     BitmapHelper.SaveBitmap(bitmap, MX * tilelen, MY * tilelen, filename);
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
            result.Append($"{tileNamesList[observed[x + y * MX]]}, ");
        }
        // add a new line to the result string to separate each row of observed tiles
        result.Append(Environment.NewLine);
    }

    // return the result string as a regular string
    return result.ToString();
}

}
