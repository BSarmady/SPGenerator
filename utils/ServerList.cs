using System;
using System.Collections.Generic;

namespace SPGenerator {

    public partial class ServerList {

        /// <summary>
        /// Writes Server key to registery
        /// </summary>
        /// <param name="AppRegistryKey">Root registry key for servers</param>
        /// <param name="ServerInfo">Server settings</param>
        public static void Update(string AppRegistryKey, ServerSettings ServerInfo) {
            string ServerKey = AppRegistryKey + "\\Servers\\" + ServerInfo.Server;
            Reg.Write(ServerKey, "Authentication", ServerInfo.Authentication);
            Reg.Write(ServerKey, "Username", ServerInfo.Username);
            Reg.Write(ServerKey, "Password", ServerInfo.Password);
            Reg.Write(ServerKey, "LastDatabase", ServerInfo.LastDatabaseName);
            Reg.Write(ServerKey, "LastUsed", ServerInfo.LastUsed);
        }

        public static List<ServerSettings> List(string AppRegistryKey) {
            string[] servers = Reg.SubKeys(AppRegistryKey + "\\Servers\\");

            List<ServerSettings> serverSettings = new List<ServerSettings> { };
            foreach (string server in servers) {
                string ServerKey = AppRegistryKey + "\\Servers\\" + server;
                serverSettings.Add(new ServerSettings(
                    server,
                    Reg.Read(ServerKey, "Authentication", 1),
                    Reg.Read(ServerKey, "Username", ""),
                    Reg.Read(ServerKey, "Password", ""),
                    Reg.Read(ServerKey, "LastDatabase", ""),
                    Reg.Read(ServerKey, "LastUsed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                ));
            }
            serverSettings.Sort((ServerSettings a, ServerSettings b) => {
                return b.LastUsed.CompareTo(a.LastUsed);
            });
            return serverSettings;
        }

    }

}
