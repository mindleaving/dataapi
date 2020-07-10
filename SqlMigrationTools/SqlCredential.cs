namespace SqlMigrationTools
{
    public class SqlCredential
    {
        public SqlCredential(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}