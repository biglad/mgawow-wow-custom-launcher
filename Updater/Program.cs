using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;

internal class Program
{
    private static DateTimeOffset currentTime;
    //DateTime currentTime = DateTime.UtcNow;
    static long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    // end to the end of all downloads
    //"?timestamp="+unixTime.toString();
    // Program running flag

    // private static readonly string unixTime = "1";
    private static readonly Random getrandom = new Random();

    public static int GetRandomNumber(int min, int max)
    {
        lock (getrandom) // synchronize
        {
            return getrandom.Next(min, max);
        }
    }
    //static long unixTime = GetRandomNumber(0, 10000);
    // Program running flag
    private static bool m_Running;
    private static string m_ClientUpdateUri = "http://mgawow.online/Patch/client1.zip?timestamp=" + unixTime.ToString();//  unixTime.toString();  // set to no caching on php

    /*
     * HOW TO ORGANIZE YOUR PATCH SERVER
     * 

        patch-folder (e.g www.example.com/Patch/) 
            |
            |- Patch
                |--- plist.txt       <== your list of patch files (each filename on seperate line)
                |--- realm.txt       <== contains the IP address of your game server
                |--- update.txt      <== version number of latest launcher
                |--- client.zip      <== latest launcher files as zip

                |--- Patch-4.MPQ     <== list of patch files, can be any name (for WoW must start with "Patch-"
                |--- Patch-C.MPQ         and filenames must not contain spaces
                |--- ... etc

     *
     *
     */

    /// <summary>
    /// Program entry
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        m_Running = true;
        Console.WriteLine("*** Welcome to the Launcher Updater! ***");
        Console.WriteLine("\n\r");
        Console.WriteLine("> Fetching update...");

        // prepare folders
        if (!Directory.Exists("Cache/L"))
            Directory.CreateDirectory("Cache/L");

        // fetch zip
        using (WebClient wc = new())
        {
            HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            wc.CachePolicy = noCachePolicy;
            wc.DownloadProgressChanged += update_Progress;
            wc.DownloadFileAsync(new Uri(m_ClientUpdateUri), "Cache/L/launcher.zip");
            wc.DownloadFileCompleted += update_Completed;
        }

        // wait to finish before exiting
        while (m_Running)
        {
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Unpacks update when download task finishes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void update_Completed(object? sender, AsyncCompletedEventArgs e)
    {
        Console.WriteLine("\r\n");
        Console.WriteLine("> Downloading complete!");
        Console.WriteLine("> Unpacking...");
        Unpack();
    }

    /// <summary>
    /// Progress while downloading update.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void update_Progress(object sender, DownloadProgressChangedEventArgs e)
    {
        Console.Write($"\r> Downloading... {e.ProgressPercentage}%");
    }

    /// <summary>
    /// Starts unpacking process before finishing.
    /// </summary>
    private async static void Unpack()
    {
        await Task.Run(() => Unzip());
        Finish();
    }

    /// <summary>
    /// Unpacks update using ZIP.
    /// </summary>
    private static void Unzip()
    {
        try
        {
            ZipFile.ExtractToDirectory("Cache/L/launcher.zip", Directory.GetCurrentDirectory(), true);
            Console.WriteLine($"> ALL DONE");
            Thread.Sleep(1000);
        }
        catch (Exception e)
        {
            //Console.WriteLine($"> Can't unzip update. Try again... ");
            Console.WriteLine($" ");
            //Console.WriteLine(e.Message);
            //Console.WriteLine("https://mgawow.online/Patch/client.php?timestamp="+unixTime.ToString());
            //Thread.Sleep(5000);
        };
    }

    /// <summary>
    /// Finish up and re-launch original application.
    /// </summary>
    private static void Finish()
    {
        // Clean up files
        if (File.Exists("Cache/L/launcher.zip"))
            File.Delete("Cache/L/launcher.zip");

        // If the launcher is here (it should be), launch it again
        if (File.Exists("update.bat"))
        {
            try
            {
                Process.Start(new ProcessStartInfo("update.bat")
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                Console.WriteLine($"YOU CLICK NO!");
                Thread.Sleep(5000);
                m_Running = false;
            }
        }

        // Exit
        m_Running = false;
    }
}