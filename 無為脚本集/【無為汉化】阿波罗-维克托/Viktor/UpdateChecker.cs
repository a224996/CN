using System;
using System.Net;
using System.Reflection;
using System.Threading;
using LeagueSharp.Common;

namespace Viktor
{
    public class UpdateChecker
    {
        public static void Initialize(string path)
        {
            using (var client = new WebClient())
            {
                new Thread(async () =>
                {
                    try
                    {
                        var data = await client.DownloadStringTaskAsync(string.Format("https://raw.github.com/{0}/Properties/AssemblyInfo.cs", path));
                        foreach (var line in data.Split('\n'))
                        {
                            // Skip comments
                            if (line.StartsWith("//"))
                            {
                                continue;
                            }

                            // Search for AssemblyVersion
                            if (line.StartsWith("[assembly: AssemblyVersion"))
                            {
                                // TODO: Use Regex for this...
                                var serverVersion = new System.Version(line.Substring(28, (line.Length - 4) - 28 + 1));

                                // Compare both versions
                                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                                if (serverVersion > assemblyName.Version)
                                {
                                        var msg = assemblyName.Name + " Update available: " + assemblyName.Version + " => "+ serverVersion +"!";
                                        Notifications.AddNotification(msg, 6000, false);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occured while trying to check for an update:\n{0}", e.Message);
                    }
                }).Start();
            }
        }
    }
}
