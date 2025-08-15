# Wuchang-Steam-Rollback
Rollback Wuchang: Fallen Feathers steam patch 1.5 to 1.4

# How to use the diff generator:
- Have the 1.5 (or any future) version of the game and the older 1.4 version of the game in two separate directories
- Provide this info in the WuchangRollback_DiffGenerator.ps1 file variables and a location to put the patch files that will be generated
- Run the patch generator using Windows Terminal.
- Because the game files are large, this will take a while (upto ~5 minutes) depending on your PC.

# Compile the Wuchang Rollback application
- Compile it using C#
- csc /t:exe /out:WuchangRollback.exe WuchangRollback.cs
- If your csc.exe is not added to env path, its located in C:\Windows\Microsoft.NET\Framework64\<.NET version>\csc.exe - You can run it from there

# Package the mod for Gamepass / Future updates
- Put the Patch folder and the EXE you generated in one folder and zip it. This can be provided as a single release to the user.
