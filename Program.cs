// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
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
            args[0]= args[0].ToLower();
            args[1]= args[1].ToLower();

            //Maintaining Genre based theme list
            Dictionary<string, List<string>> genreTileSetCombination = new Dictionary<string, List<string>>();
            genreTileSetCombination.Add("platformer", new List<string>{"summer", "haunted","castle","coins"});
            genreTileSetCombination.Add("puzzle", new List<string>{"knots", "circuit"});
            genreTileSetCombination.Add("roguelike", new List<string>{"floorplan"});

            //Checking if special characters are added in any of the args
            if (Regex.IsMatch(args[0], specialChars) || Regex.IsMatch(args[1], specialChars)) {
                Console.WriteLine($"Argument '{ti.ToTitleCase(args[1])}' contains special characters.");
                isAllOkay = false;
            }else if(!genreTileSetCombination.ContainsKey(args[0])){
                //Checking if genre specified is present in the dataset
                isAllOkay = false;
                Console.WriteLine($"String '{args[0]}' is not in the list of genres.");
            }else if(!genreTileSetCombination[args[0]].Contains(args[1])){
                //Checking if the theme exists for the specified genre
                isAllOkay = false;
                Console.WriteLine($"String '{args[1]}' is not the part of the mentioned genre.");
            }
            
            //2 Args representing Genre name and image tileset combination passed to function for further processing
            if(isAllOkay){ 
                tileSpecificExecution(ti.ToTitleCase(args[0]),ti.ToTitleCase(args[1]));
            }
        }else{
            Console.WriteLine("Insufficient Arguments. 2 Arguments Required: 1) Represting Genre 2)Representing TileSet");
        }
        
        //Used for starting localhost
        //CreateHostBuilder().Build().Run();    
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

    //This function is responsible for processing selected tileset combination  
    public static void tileSpecificExecution(String genre, String tileSelected){
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("genreOutput");
        foreach (var file in folder.GetFiles()) file.Delete();

        Random random = new();
        XDocument xdoc = XDocument.Load("allGenres.xml");

        foreach (XElement xelem in xdoc.Root.Elements("simpletiled"))
        {
            if(xelem.Get<string>("name") == tileSelected){

                Model model;
                string name = xelem.Get<string>("name");
                Console.WriteLine($"< {name}");

                bool isOverlapping = xelem.Name == "overlapping";
                int size = xelem.Get("size", isOverlapping ? 48 : 24);
                bool periodic = xelem.Get("periodic", false);
                string heuristicString = xelem.Get<string>("heuristic");
                var heuristic = heuristicString == "Scanline" ? Model.Heuristic.Scanline : (heuristicString == "MRV" ? Model.Heuristic.MRV : Model.Heuristic.Entropy);
                
                model = new SimpleTiledModel(xelem.Get<string>("name"), 
                null, xelem.Get("width", size), xelem.Get("height", size), 
                xelem.Get("periodic", false), xelem.Get("blackBackground", false), heuristic);
            
                for (int i = 0; i < xelem.Get("screenshots", 2); i++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        //Console.Write("> ");
                        int seed = random.Next();
                        bool success = model.Run(seed, xelem.Get("limit", -1));
                        if (success)
                        {
                            //Console.WriteLine("DONE");
                            model.Save($"genreOutput/{name}{seed}.png");
                            Console.WriteLine($"Generated Output Path : /genreOutput/{name}{seed}.png");
                            if (model is SimpleTiledModel stmodel && xelem.Get("textOutput", false))
                                System.IO.File.WriteAllText($"genreOutput/{name}{seed}.txt", stmodel.TextOutput());
                            break;
                        }
                        else Console.WriteLine("Found Contradiction");
                    }
                }
            }
        }
        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}
