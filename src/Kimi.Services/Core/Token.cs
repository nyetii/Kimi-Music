using Kimi.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kimi.Services.Core
{
    public class Token
    {
        public static string GetToken()
        {
            try
            {
                Log.Write("Fetching token!");
                var token = Environment.GetEnvironmentVariable("TOKEN");

                if (token != null)
                    return token;

                var path = @$"{Info.AppDataPath}\token.kimi";
                
                string[] tokens = File.ReadAllLines(path);

                if (!Info.IsDebug)
                    return tokens[0];
                else
                    return tokens[1];

            }
            catch (IndexOutOfRangeException ex)
            {
                Log.Write(ex.Message, Severity.Error);
                Log.Write("Probably the token for this instance hasn't been defined, using default token as fallback...", Severity.Warning);
                Info.IsDebug = false;
                return GetToken();
            }
            catch(Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                Log.Write(ex.Message, Severity.Error);
                Log.Write("The file and directory will be automatically created", Severity.Warning);
                Log.Write("Creating directory...");
                CreateDirectory();

                string path = $@"{Info.AppDataPath}\token.kimi";

                Console.Write("Insert your token: ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                string? token = Console.ReadLine();
                Console.ForegroundColor = default;
                if(string.IsNullOrWhiteSpace(token))
                    Environment.Exit(1);

                File.WriteAllText(path, token);

                return GetToken();
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message, Severity.Fatal);
                Console.ReadKey();
                Environment.Exit(1);
                return ex.Message.ToString();
            }
        }

        public static void SetToken(string token)
        {
            var path = @$"{Info.AppDataPath}\token.kimi";

            if (string.IsNullOrWhiteSpace(token))
                Environment.Exit(1);

            try
            {
                File.WriteAllText(path, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(1);
            }
        }
        

        private static void CreateDirectory()
        {
            
            Directory.CreateDirectory(@$"{Info.AppDataPath}");
        }
    }
}
