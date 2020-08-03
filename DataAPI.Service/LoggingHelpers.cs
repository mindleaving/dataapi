using System.Collections.Generic;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.Service.Objects;

namespace DataAPI.Service
{
    public static class LoggingHelpers
    {
        public static List<LogEntry> WriteCollectionMetadataLog(CollectionOptions collectionOptions, string username)
        {
            var logEntries = new  List<LogEntry>();
            if(collectionOptions.DisplayName != null)
            {
                logEntries.Add(new LogEntry(LogLevel.Info, $"User '{username}' changed display name " +
                                                  $"of collection '{collectionOptions.CollectionName}' " +
                                                  $"to '{collectionOptions.DisplayName}'"));
            }
            if(collectionOptions.Description != null)
            {
                logEntries.Add(new LogEntry(LogLevel.Info, $"User '{username}' changed description " +
                                                  $"of collection '{collectionOptions.CollectionName}'"));
            }
            if(collectionOptions.IsProtected.HasValue)
            {
                logEntries.Add(new LogEntry(LogLevel.Info,
                    collectionOptions.IsProtected == true
                        ? $"User '{username}' made collection '{collectionOptions.CollectionName}' protected"
                        : $"User '{username}' made collection '{collectionOptions.CollectionName}' unprotected"));
            }
            if(collectionOptions.NonAdminUsersCanOverwriteData.HasValue)
            {
                logEntries.Add(new LogEntry(LogLevel.Info,
                    collectionOptions.NonAdminUsersCanOverwriteData == true
                        ? $"User '{username}' allowed overwriting by non-admin users in collection '{collectionOptions.CollectionName}'"
                        : $"User '{username}' disabled overwriting by non-admin users in collection '{collectionOptions.CollectionName}'"));
            }
            return logEntries;
        }
    }
}
