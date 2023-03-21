/*
NAME: Helper.cs
DESCRIPTION: This is the file containing helper methods to be used by our Model.
*/

using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static class Helper
{
    //Used by Program Class for retrieving attribute values from XML file
    public static T Get<T>(this XElement xelem, string element, T defaultT = default)
    {
        return xelem.Attribute(element) == null 
                ? defaultT 
                : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(xelem.Attribute(element).Value);
    }

    //Used by Program Class for enumerating along the tileset information from allGenres.xml file
    public static IEnumerable<XElement> Elements(this XElement xelement, params string[] names) => 
    xelement.Elements().Where(e => names.Any(n => n == e.Name));

    //Used by Model Class for deciding a random distrubution value based on argument randomVar
    public static int Random(this double[] weights, double randomVar)
    {
        double partialSumOfWeights = 0;
        double sumOfWeights = 0;
        
        for (int i = 0; i < weights.Length; i++) 
            sumOfWeights += weights[i];
        double threshold = randomVar * sumOfWeights;

        //Reiterating through the weights again to get first highest number post comparison with calculated threshold
        for (int i = 0; i < weights.Length; i++)
        {
            partialSumOfWeights += weights[i];
            if (partialSumOfWeights >= threshold) return i;
        }
        return 0;
    }

}

//Used by SimpleTiled Class
static class BitmapHelper
{
    //Method used to extract image files/theme components from TileSets Folder for user mentioned theme in the required size format.
    public static (int[], int, int) LoadBitmap(string themeName)
    {
        using var image = Image.Load<Bgra32>(themeName);
        //Intializing the final result based on width and height of image
        int[] result = new int[image.Width * image.Height];
        //Copying the pixel data from image to Bgra32 to represent 4 bytes data using the MemoryMarshal.Cast method
        image.CopyPixelDataTo(MemoryMarshal.Cast<int, Bgra32>(result));
        return (result, image.Width, image.Height);
    }

    //Method used to generate output image post entropy calculations done in SimpleTiledModel Class
    unsafe public static void SaveBitmap(int[] imageData, int width, int height, string themeName)
    {
        //Assigning a fixed pointer to avoid it to be garbage collected.
        fixed (int* ptrImageData = imageData)
        {
            //Wrapping pdata into an image with the specified width and height, and using the Bgra32 pixel format.
            using var image = Image.WrapMemory<Bgra32>(ptrImageData, width, height);
            //Assigning theme name to the generated image.
            image.SaveAsPng(themeName);
        }
    }
}
