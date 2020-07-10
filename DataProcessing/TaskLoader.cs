using System.Collections.Generic;
using System.IO;
using DataProcessing.GenericTasks;
using Newtonsoft.Json;

namespace DataProcessing
{
    public class TaskLoader
    {
        private readonly string taskDefinitionDirectory;

        public TaskLoader(string taskDefinitionDirectory)
        {
            this.taskDefinitionDirectory = taskDefinitionDirectory;
        }

        public IList<ITask> Load()
        {
            if (!Directory.Exists(taskDefinitionDirectory))
            {
                try
                {
                    Directory.CreateDirectory(taskDefinitionDirectory);
                }
                catch
                {
                    // ignore
                }
                return new List<ITask>();
            }
            var jsonFiles = Directory.EnumerateFiles(taskDefinitionDirectory, "*.json");
            var tasks = new List<ITask>();
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var taskDefinition = JsonConvert.DeserializeObject<ScriptPeriodicTaskDefinition>(json);
                    var task = new ScriptPeriodTask(taskDefinition);
                    tasks.Add(task);
                }
                catch
                {
                    // ignored
                }
            }
            return tasks;
        }
    }
}
