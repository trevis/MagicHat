﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Launcher {
    internal class LaunchManager {
        private readonly string CLIENT_EXE = @"C:\Turbine\Asheron's Call\acclient.exe";

        public LaunchManager() {
        }

        internal unsafe void Launch(string server, string username, string password) {
            var host = server.Split(":").First();
            var port = server.Split(":").Last();
            var clientArgs = $"-h {host} -p {port} -a {username} -v {password} -rodat off";

            var dllsToInject = new List<string>() {
                Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "MagicHat.Bootstrapper.dll") + ";Bootstrap",
                //DecalHelpers.GetDecalLocation() + ";DecalStartup"
            };

            var injectorPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "MagicHat.Injector.exe");
            var injectorArgs = new StringBuilder();
            injectorArgs.Append($"--client-path=\"{CLIENT_EXE}\" --client-args=\"{clientArgs}\"");

            if (dllsToInject.Count > 0) {
                injectorArgs.Append($" --inject ");
                foreach (var dll in dllsToInject) {
                    injectorArgs.Append($"\"{dll}\"");
                }
            }

            Process.Start(injectorPath, injectorArgs.ToString());
        }
    }
}