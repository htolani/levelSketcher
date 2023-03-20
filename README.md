#Level Sketcher

Application to generate level for your favorite game genre and as per your theme.

Data Available:
|---------------X--------------------------------------------|
|    GENRE      X                     THEME                  |
|---------------X--------------------------------------------|
| Platformer    X   1. Summer                                |
|               X         - Cliff                            |
|               X         - Grass                            |
|               X         - Road                             |
|               X         - Water                            |
|               X   2. Haunted                               |
|               X         - Cliff                            |
|               X         - Grass                            |
|               X         - Road                             |
|               X         - Water                            |
|---------------X--------------------------------------------|
| RogueLike     X   1. Summer                                |
|               X         - Cliff                            |
|               X         - Grass                            |
|               X         - Road                             |
|               X         - Water                            |
|               X   2. Haunted                               |
|               X         - Cliff                            |
|               X         - Grass                            |
|               X         - Road                             |
|               X         - Water                            |
|---------------X--------------------------------------------|
| Puzzle        X   1. Circuit                               |
|               X         - Bridge                           |
|               X         - Block Component                  |
|               X         - Track                            |
|               X         - Wire                             |
|               X   2. Knots                                 |
|               X         - Corner                           |
|               X         - Cross                            |
|               X         - Line Tracks                      |
|---------------X--------------------------------------------|

Pre-requisites:
1. .NET Run Time
    Note: As per your latest version, update the version number in LevelSkecther.csproj


Steps to run this project: 

1. Clone this Repository 
2. In the project directory, run following commands: 
    #dotnet restore 
    #dotnet build 
3. For running the project you need to pass two arguments based on the above table data, first argument representing Genre  and second genre representing theme for your genre
    for eg., dotnet run platformer summer 
    #dotnet run arg1 arg2
