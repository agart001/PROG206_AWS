using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

class Program
{
    // Base URL for the API
    static string BaseUrl = "http://ec2-3-138-190-113.us-east-2.compute.amazonaws.com/index.php?";
    static API? api { get; set; }
    static List<Fruit>? Fruits { get; set; }

    // Entry point of the program
    static async Task Main()
    {
        // Initialize API with the base URL
        api = new API(BaseUrl);

        // Print a welcome message and start the main loop
        Print(new string[]
        {
            "------------------------------------",
            "Welcome to PROG206 AWS EC2 Database!",
        });
        await LoopAsync();
    }

    // Main loop of the program
    static async Task LoopAsync()
    {
        // Ensure API is initialized
        if (api == null) throw new NullReferenceException(nameof(api));

        // Make an asynchronous GET request to fetch fruit data
        await api.AsyncGET("get-fruit");
        // Deserialize the JSON response into a list of Fruit objects
        Fruits = JsonConvert.DeserializeObject<List<Fruit>>(api.GETResult ?? throw new NullReferenceException(nameof(api.GETResult)));

        // Display menu options and handle user input
        Print("------------------------------------");
        int answer = MultiQuestion("What would you like to do",
            new string[]
            {
                "Display Fruit",
                "Add Fruit",
                "Remove Fruit"
            });

        Print(new string[]
        {
            "------------------------------------",
            "",
            "------------------------------------"
        });

        switch (answer)
        {
            case 1:
                Console.Clear();
                Print("------------------------------------");
                // Display the list of fruits
                if (Fruits == null) throw new NullReferenceException(nameof(Fruits));
                Print(Fruits.ToArray(), "--");
                break;
            case 2:
                Console.Clear();
                // Allow the user to add a new fruit
                await AddFruitAsync();
                break;
            case 3:
                Console.Clear();
                // Allow the user to remove a fruit
                await RemoveFruitAsync();
                break;
        }

        Print("------------------------------------");

        // Ask if the user wants to perform another action
        if (BoolQuestion("Do something else", "y", "n"))
        {
            Console.Clear();
            await LoopAsync();
        }
        else
        {
            Print("Press any key to exit.");
            Console.ReadKey();
        }
    }

    // Add or subtract fruit methods
    #region Add/Sub Fruit

    /// <summary>
    /// Asynchronously adds a new fruit.
    /// </summary>
    static async Task AddFruitAsync()
    {
        string name;
        Print(new string[]
        {
            "------------------------------------",
            "Enter a new Fruit",
            "------------------------------------"
        });
        Console.Write("Name: ");
        name = Console.ReadLine() ?? "default";
        if (BoolQuestion($"Is {name} correct", "y", "n"))
        {
            if (api == null) throw new NullReferenceException(nameof(api));
            // Make an asynchronous POST request to add a new fruit
            await api.AsyncPOST(new Dictionary<string, string>
            {
                {"add-fruit", name}
            });
        }
        else
        {
            Console.Clear();
            await AddFruitAsync();
        }
    }

    /// <summary>
    /// Asynchronously removes a fruit.
    /// </summary>
    static async Task RemoveFruitAsync()
    {
        if (Fruits == null) throw new NullReferenceException(nameof(Fruits));
        string[] options = Fruits.Select(fruit => fruit.name).ToArray();
        Print("------------------------------------");
        int answer = MultiQuestion("Which entry to remove", options);
        string name = options[answer - 1];
        if (BoolQuestion($"Is {name} correct", "y", "n"))
        {
            if (api == null) throw new NullReferenceException(nameof(api));
            // Make an asynchronous POST request to remove a fruit
            await api.AsyncPOST(new Dictionary<string, string>
            {
                {"remove-fruit", name}
            });
        }
        else
        {
            Console.Clear();
            await RemoveFruitAsync();
        }
    }

    #endregion

    // Printing methods
    #region Print

    /// <summary>
    /// Prints a single line.
    /// </summary>
    static void Print(string line) => Console.WriteLine(line);

    /// <summary>
    /// Prints an array of lines.
    /// </summary>
    static void Print(string[] lines)
    {
        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }
    }

    /// <summary>
    /// Prints an array of objects with a specified header.
    /// </summary>
    static void Print(object[] entities, string header)
    {
        foreach (object entity in entities)
        {
            Console.WriteLine($"{header} {entity}");
        }
    }

    #endregion

    // Question methods
    #region Question

    /// <summary>
    /// Asks a yes/no question.
    /// </summary>
    static bool BoolQuestion(string question, string confirm, string cancel)
    {
        bool result = false;
        Print($"{question}? ({confirm}/{cancel}): ");
        string input = Console.ReadLine() ?? string.Empty;

        if (input.ToLower() == confirm)
        {
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Asks a multiple-choice question.
    /// </summary>
    static int MultiQuestion(string question, string[] answers)
    {
        int answer;
        for (int i = 0; i < answers.Length; i++)
        {
            Console.WriteLine($"#{i + 1}: {answers[i]}");
        }
        Print("------------------------------------");
        Print($"{question}? (Enter an integer #): ");
        var input = Console.ReadLine() ?? string.Empty;
        answer = int.Parse(input);

        return answer;
    }

    #endregion
}

#region API

/// <summary>
/// Represents an API for interacting with a web service.
/// </summary>
public class API
{
    private string? url;
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Sets the base URL for the API.
    /// </summary>
    public void SetUrl(string url) => this.url = url;

    /// <summary>
    /// Gets the result of the last GET request.
    /// </summary>
    public string? GETResult { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="API"/> class.
    /// </summary>
    public API()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="API"/> class with a specified URL.
    /// </summary>
    public API(string url)
    {
        this.url = url;
    }

    /// <summary>
    /// Asynchronously makes a POST request to the API.
    /// </summary>
    public async Task AsyncPOST(IDictionary<string, string> values)
    {
        var request = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(url, request);
        var asString = await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Asynchronously makes a GET request to the API.
    /// </summary>
    public async Task AsyncGET(string method)
    {
        var request = url + method;
        var response = await client.GetAsync(request);
        GETResult = await response.Content.ReadAsStringAsync();
    }
}

#endregion

#region Fruit

/// <summary>
/// Represents a fruit with an ID and a name.
/// </summary>
public class Fruit
{
    public int id { get; set; }
    public string name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Fruit"/> class.
    /// </summary>
    public Fruit()
    {
        id = 0;
        name = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Fruit"/> class with specified ID and name.
    /// </summary>
    public Fruit(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    /// <summary>
    /// Returns a string representation of the fruit.
    /// </summary>
    public override string ToString() =>
        $"| id: {id} | name: {name} |";
}

#endregion