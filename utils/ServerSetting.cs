using System;

namespace SPGenerator {

    public class ServerSettings {

        public string Server;
        public int Authentication;
        public string Username;
        public string Password;
        public string LastDatabaseName;
        public string LastUsed;

        public ServerSettings(string Server, int Authentication, string Username, string Password, string LastDatabaseName = "", string LastUsed = "") {
            this.Server = Server;
            this.Authentication = Authentication;
            this.Username = Username;
            this.Password = Password;
            this.LastDatabaseName = LastDatabaseName;
            this.LastUsed = LastUsed;
            if (this.LastUsed == "")
                this.LastUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

}


