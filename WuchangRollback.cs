using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

	string[] exePaths = {basePath, "Project_Plague", "Binaries", "Win64", "Project_Plague-Win64-Shipping.exe"};
        string expectedMainExe = Path.Combine(exePaths);
        if (!File.Exists(expectedMainExe))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: This rollback tool is not in the correct folder.");
            Console.ResetColor();
            PauseAndExit();
            return;
        }

        // Check if hash file exists in patch folder
        string patchPath = Path.Combine(basePath, "patch");
        string hashFilePath = Path.Combine(patchPath, "ProjectPlague_Hash.txt");
        if (!File.Exists(hashFilePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Missing hash file in patch folder.");
            Console.WriteLine("Cannot verify version. Rollback aborted.");
            Console.ResetColor();
            PauseAndExit();
            return;
        }

        // Compare hashes to ensure user is on Steam v1.5 of the game!
        string expectedHash = File.ReadAllText(hashFilePath).Trim().ToUpperInvariant();
        string actualHash = GetFileHash(expectedMainExe);
        if (actualHash != expectedHash)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Rollback is only supported from version 1.5 to 1.4.");
            Console.ResetColor();
            PauseAndExit();
            return;
        }

        string targetPath = basePath;
        string changedDir = Path.Combine(patchPath, "changed");
        string deletedDir = Path.Combine(patchPath, "deleted");
        string newFilesList = Path.Combine(patchPath, "new_files.txt");

        Console.WriteLine("Starting rollback...\n");

        // Restore changed files
        if (Directory.Exists(changedDir))
        {
            Console.WriteLine("Restoring changed files...");
            CopyAllWithProgress(changedDir, targetPath);
        }

        // Restore deleted files
        if (Directory.Exists(deletedDir))
        {
            Console.WriteLine("Restoring deleted files...");
            CopyAllWithProgress(deletedDir, targetPath);
        }

        // Remove new files
        if (File.Exists(newFilesList))
        {
            Console.WriteLine("Removing new files...");
            var files = File.ReadAllLines(newFilesList);
            int total = files.Length;
            for (int i = 0; i < total; i++)
            {
                string rel = files[i];
                var dest = Path.Combine(targetPath, rel);
                if (File.Exists(dest))
                {
                    try { File.Delete(dest); }
                    catch (Exception ex) { Console.WriteLine("Failed to delete: " + rel); }
                }

                int percent = (int)((i + 1) * 100.0 / total);
                if (percent % 10 == 0) Console.WriteLine("   " + percent + "% done...");
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nRollback complete!");
        Console.ResetColor();
        PauseAndExit();
    }

    static string GetFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
        }
    }

    static void CopyAllWithProgress(string sourceDir, string targetDir)
    {
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        int total = files.Length;

        for (int i = 0; i < total; i++)
        {
            string file = files[i];
            string relPath = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
            string destPath = Path.Combine(targetDir, relPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            File.Copy(file, destPath, true);

            int percent = (int)((i + 1) * 100.0 / total);
            if (percent % 10 == 0) Console.WriteLine("   " + percent + "% done...");
        }
    }

    static void PauseAndExit()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
