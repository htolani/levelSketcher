using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


    [AllowAnonymous]
    public class userInput
    {
        // holds the user response from the index page
        public string tileSet { get; set; }
    }
    

