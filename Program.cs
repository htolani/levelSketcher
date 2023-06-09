﻿/*
NAME: Program.cs
DESCRIPTION: This is the main file which runs post the project is ran.
             Its takes user inputs and calls SimpleTiled Class for further processing.
*/

using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Program
{
    public static void Main(string[] args)
    {
        //String representing Regex of special characters 
        string specialChars = @"[$&+,:;=?@#|'<>.^*()%!-]";
        //TextInfo used to captialize first letter only of a word
        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

        //Checking if required args are passed
        if(args.Length == 2){ 
            bool isAllOkay = true;
            bool foundGenre = false;
            args[0]= args[0].ToLower();
            args[1]= args[1].ToLower();

            //Checking the total list of Genres
            XDocument xdoc = XDocument.Load("allGenres.xml");
            foreach (XElement xelem in xdoc.Root.Element("genres").Elements("genre")){
                if(xelem.Get<string>("name").Contains(args[0])){
                    foundGenre = true;
                    break;
                }
            }

            //Checking if special characters are added in any of the args
            if (Regex.IsMatch(args[0], specialChars) || Regex.IsMatch(args[1], specialChars)) {
                Console.WriteLine($"Argument contains special characters.");
                isAllOkay = false;
            }else if(!foundGenre){
                //Checking if genre specified is present in the dataset
                isAllOkay = false;
                Console.WriteLine($"String '{args[0]}' is not in the list of genres.");
            }
            
            //2 Args representing Genre name and image tileset combination passed to function for further processing
            if(isAllOkay){ 
                tileSpecificExecution(ti.ToTitleCase(args[0]),ti.ToTitleCase(args[1]));
            }
        }else{
            Console.WriteLine("Insufficient Arguments. 2 Arguments Required: 1) Represting Genre 2) Representing TileSet");
        }
        
        //Used for starting localhost
        //CreateHostBuilder().Build().Run();    
    }

    //This function is responsible for processing selected tileset combination  
    public static void tileSpecificExecution(String genre, String tileSelected){
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("genreOutput");
        foreach (var file in folder.GetFiles()) file.Delete();

        Random random = new();
        bool foundTileTheme = false;
        //Getting the constraints file 
        XDocument xdoc = XDocument.Load("allGenres.xml");

        //Running through theme/tilesets information
        foreach (XElement xelem in xdoc.Root.Element("tileset").Elements("simpletiled"))
        {
            //Checking the tileset with same theme and genre mentioned in the arguments
            if(xelem.Get<string>("name") == tileSelected && xelem.Get<string>("genre")==genre){
                foundTileTheme = true;

                Model model;
                string name = xelem.Get<string>("name");
                Console.WriteLine($"Generating Level for Genre : {xelem.Get<string>("genre")}");
                Console.WriteLine($"Generating Level for Theme : {name}");

                int size = xelem.Get("size", 24);
                bool periodic = xelem.Get("periodic", false);
                string heuristicString = xelem.Get<string>("heuristic");
                var heuristic = heuristicString == "Scanline" ? Model.Heuristic.Scanline : (heuristicString == "MRV" ? Model.Heuristic.MRV : Model.Heuristic.Entropy);
                
                //Calling SimpleTiledModel for performing entropy based calculations
                model = new SimpleTiledModel(xelem.Get<string>("name"), 
                xelem.Get("width", size), xelem.Get("height", size), 
                xelem.Get("periodic", false), xelem.Get("blackBackground", false), heuristic);
            
                //Setting the output screenshots limit to default for 2
                for (int i = 0; i < xelem.Get("screenshots", 2); i++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        int seed = random.Next();
                        //Checking for contradictions
                        bool success = model.Run(seed, xelem.Get("limit", -1));
                        if (success)
                        {
                            model.Save($"genreOutput/{name}{seed}.png");
                            Console.WriteLine($"Generated Level Output Path : /genreOutput/{name}{seed}.png");
                            if (model is SimpleTiledModel stmodel && xelem.Get("textOutput", false)){ 
                                System.IO.File.WriteAllText($"genreOutput/{name}{seed}.txt", stmodel.TextOutput());
                                Console.WriteLine($"Generated Level Output's text file describing entropy calculations at each step: /genreOutput/{name}{seed}.txt");
                            }
                            break;
                        }
                        else Console.WriteLine("Found Contradiction in the generated image");
                    }
                }
            }
        }

        if(!foundTileTheme){
            //Checking if the theme exists for the specified genre
            Console.WriteLine($"String '{tileSelected}' is not the part of the mentioned genre.");
        }

        Console.WriteLine($"Total time taken in milliseconds = {sw.ElapsedMilliseconds}");
    }

    //Method to call localhost 
    public static IHostBuilder CreateHostBuilder() =>
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<HomeController>();
            webBuilder.UseUrls("https://localhost:8080");
        });

    //Method called from the Action Listener when Genre is selected     
    public static void tileSetExecution(String tileSelectedName){
           Console.WriteLine("tileSelectedName :",tileSelectedName);
    }
}
