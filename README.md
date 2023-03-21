<h2>Level Sketcher</h2>


Application to generate level for your favorite game genre and as per your theme.


<b>Methodology:</b>


<img src="https://github.com/htolani/levelSketcher/blob/main/Methodology.png" width="850" height="450">


<b>Data Set:</b>


<img src="https://github.com/htolani/levelSketcher/blob/main/DataSet.png" width="400" height="450">


<b>Pre-requisites:</b>
1. .NET Run Time
   Note: As per your latest version, update the version number in LevelSkecther.csproj


<b>Steps to run this project: </b>


1. Clone this Repository
2. In the project directory, run following commands:
   #dotnet restore
   #dotnet build
3. For running the project you need to pass two arguments based on the above table data, first argument representing Genre  and second genre representing theme for your genre
   for eg., dotnet run platformer summer
   #dotnet run arg1 arg2




<b>Project Understanding </b>
1. The Genre and Theme constraints defined in this application are mentioned in allGenres.xml file.
2. All Theme Tilesets and their constraints are specified in tilesets folder.
3. All final Levels are stored in genreOutput folder along with a text file describing entropy calculations.
