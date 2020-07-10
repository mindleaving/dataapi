using System;
using System.IO;
using DataAPI.Client;

namespace DataProcessing.Test
{
    public static class ApiSetup
    {
        public static string ServerAddress { get; } = "";
        public static ushort ServerPort { get; } = 443;
        public static ApiConfiguration ApiConfiguration { get; } = new ApiConfiguration(ServerAddress, ServerPort);

        public static string UnitTestAdminUsername { get; } = "UnitTestAdmin";
        public static string UnitTestAdminPassword { get; } 
            = File.ReadAllText($@"C:\Users\{Environment.UserName}\AppData\Local\DataAPI\UnitTestCredentials.txt"); // SECURITY NOTE: Password MUST be read from file
                                                                                                                   // and that file MUST NOT be in source code folder
    }
}