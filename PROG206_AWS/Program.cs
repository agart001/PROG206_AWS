using System;
using System.IO;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static string BaseUrl = "http://ec2-3-138-190-113.us-east-2.compute.amazonaws.com/index.php?";
    static API? api { get; set; }
    static List<Fruit>? Fruits {  get; set; }
    static async Task Main()
    {
        api = new API(BaseUrl);

        Print(new string[]
        {
            "------------------------------------",
            "Welcome to PROG206 AWS EC2 Database!",
        });
        await LoopAsync();
    }

    static async Task LoopAsync()
    {
        if (api == null) throw new NullReferenceException(nameof(api));
        await api.AsyncGET("get-fruit");
        Fruits = JsonConvert.DeserializeObject<List<Fruit>>(api.GETResult ?? throw new NullReferenceException(nameof(api.GETResult)));

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
                if(Fruits == null) throw new NullReferenceException(nameof(Fruits));
                Print(Fruits.ToArray(), "--");
            break;
            case 2:
                Console.Clear();
                await AddFruitAsync();
            break;
            case 3: 
                Console.Clear();
                await RemoveFruitAsync();
            break;
        }

        Print("------------------------------------");

        if(BoolQuestion("Do something else", "y", "n"))
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

    #region Add/Sub Fruit
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
        if(BoolQuestion($"Is {name} correct","y","n"))
        {
            if(api == null) throw new NullReferenceException(nameof(api));
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

    static async Task RemoveFruitAsync()
    {
        if(Fruits == null) throw new NullReferenceException(nameof(Fruits));
        string[] options = Fruits.Select(fruit => fruit.name).ToArray();
        Print("------------------------------------");
        int answer = MultiQuestion("Which entry to remove", options);
        string name = options[answer - 1];
        if (BoolQuestion($"Is {name} correct", "y", "n"))
        {
            if (api == null) throw new NullReferenceException(nameof(api));
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

    #region Print
    static void Print(string line) => Console.WriteLine(line);

    static void Print(string[] lines)
    {
        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }
    }

    static void Print(object[] entities, string header)
    {
        foreach (object entity in entities)
        {
            Console.WriteLine($"{header} {entity}");
        }
    }
    #endregion

    #region Question
    static bool BoolQuestion(string question, string confirm, string cancel) 
    {
        bool result = false;
        Print($"{question}? ({confirm}/{cancel}): ");
        string input = Console.ReadLine() ?? string.Empty;

        if( input.ToLower() == confirm)
        {
            result = true;
        }

        return result;
    }

    static int MultiQuestion(string question, string[] answers)
    {
        int answer;
        for(int i = 0; i < answers.Length; i++)
        {
            Console.WriteLine($"#{i + 1}: {answers[i]}");
        }
        Print("------------------------------------");
        Print($"{question}? (Enter an integer #): ");
        answer = int.Parse(Console.ReadLine());

        return answer;
    }
    #endregion
}

public class API
{
    private string? url;
    private static readonly HttpClient client = new HttpClient();
    public void SetUrl(string url) => this.url = url;

    public string? GETResult { get; internal set; }

    public API()
    {

    }

    public API(string url)
    {
        this.url = url;
    }

    public async Task AsyncPOST(IDictionary<string, string> values)
    {
        var request = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(url, request);
        var asString = await response.Content.ReadAsStringAsync();
    }

    public async Task AsyncGET(string method)
    {
        var request = url + method;
        var response = await client.GetAsync(request);
        GETResult = await response.Content.ReadAsStringAsync();
    }
}

public class Fruit
{
    public int id { get; set; }

    public string name { get; set; }

    public Fruit() 
    {
        id = 0;
        name = "";
    }

    public Fruit(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public override string ToString() =>
        $"| id: {id} | name: {name} |";
}