using System.Text.Json;
using System.Text.Json.Serialization;


class Task
{
    public int Id { get; set; }
    public string Description {get; set;}
    public Status Status { get; set; } = global::Status.TODO;

}

enum Status
{
    TODO,IN_PROGRESS, DONE
}

class Program
{
    private static string basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".task-cli"
    );
    
    private static string data_path = Path.Combine(basePath, "data.json");
    private static string id_path = Path.Combine(basePath, "id.json");
    private static List<Task> tasks = GetTasks();
    private static int lastId = GetLastId();
    static void Main(string[] args)
    {
        Directory.CreateDirectory(basePath);
        if (args.Length == 0)
        {
            Console.WriteLine("Add arguments");
            return;
        }

        string command = args[0];
        switch (command)
        {
            case "add":
            {
                var task = new Task
                {
                    Id = ++lastId,
                    Description = args[1]
                };
                add(task);
                SaveLastId();
                break;
            }

            case "delete":
            {
                var id = int.Parse(args[1]);
                delete(id);
                break;
            }

            case "update":
            {
                var id = int.Parse(args[1]);
                var new_description = args[2];
                update(id, new_description);
                break;
            }

            case "mark-in-progress":
            {
                var id = int.Parse(args[1]);
                update_progress(id);
                break;
            }

            case "mark-done":
            {
                var id = int.Parse(args[1]);
                update_done(id);
                break;
            }

            case "list":
            {
                var list_type = args.Length > 1 ? args[1] : null;;
                switch (list_type)
                {
                    case "todo":
                    {
                        list_todo();
                        break;
                    }
                    case "in-progress":
                    {
                        list_inprogress();
                        break;
                    }
                    case "done":
                    {
                        list_done();
                        break;
                    }

                    default:
                    {
                        list();
                        break;
                    }
                        
                }
                break;
            }
            case "help":
            {
                ShowHelp();
                break;
            }
            
        }
        
    }

    static void add(Task task)
    {
        tasks.Add(task);
        Serialize();
    }

    static void delete(int id)
    {
        tasks.Remove(tasks.Find(t => t.Id == id));
        Serialize();
    }

    static void update(int id, string description)
    {
        var task = tasks.Find(t => t.Id == id);
        task.Description = description;
        Serialize();
    }

    static void update_progress(int id)
    {
        var task = tasks.Find(t => t.Id == id);
        task.Status = Status.IN_PROGRESS;
        Serialize();
    }

    static void update_done(int id)
    {
        var task = tasks.Find(t => t.Id == id);
        task.Status = Status.DONE;
        Serialize();
    }

    static void list()
    {
        printer(tasks);
    }

    static void list_done()
    {
        var filtered = tasks.Where(t => t.Status == Status.DONE);
        printer(filtered);
    }

    static void list_todo()
    {
        var filtered = tasks.Where(t => t.Status == Status.TODO);
        printer(filtered);
    }

    static void list_inprogress()
    {
        var filtered = tasks.Where(t => t.Status == Status.IN_PROGRESS);
        printer(filtered);
    }

    static void printer(IEnumerable<Task> tasks)
    {
        foreach (var task in tasks)
        {
            Console.WriteLine($"ID: {task.Id}");
            Console.WriteLine(task.Description);
            Console.WriteLine(task.Status);
            Console.WriteLine("////////////////////////");
        }
    }
    static List<Task> GetTasks()
    {
        // Check if file exists
        if (File.Exists(data_path))
        {
            string content = File.ReadAllText(data_path);
            if (!string.IsNullOrWhiteSpace(content))
            {
                return JsonSerializer.Deserialize<List<Task>>(content) ?? new List<Task>();
            }
        }
        return [];
    }

    static int GetLastId()
    {
        // Check if file exists
        if (File.Exists(id_path))
        {
            var content = File.ReadAllText(id_path);
            int.TryParse(content, out int id);
            return id;
        }
        
        return -1;
        
    }
    
    public static void SaveLastId()
    {
        File.WriteAllText(id_path, lastId.ToString());
    }
    
    static void Serialize()
    {
        
        // Serialize to JSON
        var options = new JsonSerializerOptions { WriteIndented = true };
        // options.Converters.Add(new JsonStringEnumConverter());
        string json = JsonSerializer.Serialize(tasks, options);
        

        // Save file
        File.WriteAllText(data_path, json);
    }
    
    public static void ShowHelp()
    {
        Console.WriteLine("Use: task-cli <command> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Available commands:");
        Console.WriteLine("  add <description>           Add a new task");
        Console.WriteLine("  delete <id>                 Delete a task by Id");
        Console.WriteLine("  update <id> <description>   Update task's description");
        Console.WriteLine("  make-in-progress <id>       Tag task as 'InProgress'");
        Console.WriteLine("  make-done <id>              Tag task as 'Done'");
        Console.WriteLine("  list                        List pending tasks");
        Console.WriteLine("  list done                   List completed tasks");
        Console.WriteLine("  list all                    List all tasks");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  task-cli add \"Buy milk\"");
        Console.WriteLine("  task-cli make-done 2");
    }
}

