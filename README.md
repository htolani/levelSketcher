<h2>Level Sketcher</h2>

Application to generate level for your favorite game genre and as per your theme.


<b>Methodology:</b>

<img src="https://github.com/htolani/levelSketcher/blob/main/Methodology.png" width="850" height="450">

<a href="https://drive.google.com/file/d/1PYO7XIU0cwZPof-Q_CvcsLVVpXCH0ssK/view?usp=share_link">Project Demo Link</a>

<b>Data Set:</b>

<img src="https://github.com/htolani/levelSketcher/blob/main/Data%20Set.png" width="400" height="500">


<b>Pre-requisites:</b>
1. .NET Run Time
   Note: As per your latest version, update the version number in LevelSkecther.csproj

<b>Steps to run this project: </b>
1. Clone this Repository
2. In the project directory, run following commands: <br /> 
   #dotnet restore <br /> 
   #dotnet build 
3. For running the project you need to pass two arguments based on the above table data, first argument representing Genre  and second genre representing theme for your genre. <br /> 
   for eg., dotnet run platformer summer <br /> 
   #dotnet run arg1 arg2


<b>Project Understanding :</b>
1. The Genre and Theme constraints defined in this application are mentioned in allGenres.xml file.
2. All Theme Tilesets and their constraints are specified in tilesets folder.
3. All final Levels are stored in genreOutput folder along with a text file describing entropy calculations.

