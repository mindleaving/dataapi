using System.Reflection;
using DataAPI.DataStructures.PostBodies;

namespace DataAPI.Service
{
    public static class ApiVersion
    {
        public static string Current => Assembly.GetAssembly(typeof(SubmitDataBody)).GetName().Version.ToString(3);
    }
}
