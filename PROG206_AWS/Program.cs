using System;
using System.IO;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;

class Program
{
    static string BaseUrl = "http://ec2-3-138-190-113.us-east-2.compute.amazonaws.com/index.php?";
    static string[] Entries;
    static async Task Main()
    {
        await GetDBEntries();
        Print(new string[]
        {
            "------------------------------------",
            "Welcome to PROG206 AWS EC2 Database!",
        });
        await LoopAsync();
    }

    static async Task LoopAsync()
    {
        await GetDBEntries();

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
                Print(Entries, "--"); 
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
            LoopAsync();
        }
        else
        {
            Print("Press any key to exit.");
            Console.ReadKey();
        }
    }

    static async Task CallUrlAsync(string request)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}{request}");

                if (response.IsSuccessStatusCode)
                {
                    string urlText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(urlText);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    #region DB Entries
    static async Task GetDBEntries()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}print-fruit=true");

                if (response.IsSuccessStatusCode)
                {
                    string urlText = await response.Content.ReadAsStringAsync();
                    string[] entries = ParseUrlContent(urlText);
                    SetEntries(entries);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static string[] ParseUrlContent(string content)
    {
        List<string> entries = content.Split("<br>").ToList();
        entries.Remove(entries.Last());
        return entries.ToArray();
    }

    static void SetEntries(string[] entries) => Entries = entries;
    #endregion

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
        name = Console.ReadLine();
        if(BoolQuestion($"Is {name} correct","y","n"))
        {
            await CallUrlAsync($"add-fruit={name}");
        }
        else
        {
            Console.Clear();
            await AddFruitAsync();
        }
    }

    static async Task RemoveFruitAsync()
    {
        string name;
        Print("------------------------------------");
        int answer = MultiQuestion("Which entry to remove", Entries);
        name = Entries[answer - 1];
        if (BoolQuestion($"Is {name} correct", "y", "n"))
        {
            await CallUrlAsync($"remove-fruit={name}");
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

    static void Print(string[] lines, string header)
    {
        foreach (string line in lines)
        {
            Console.WriteLine($"{header} {line}");
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
