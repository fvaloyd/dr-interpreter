namespace Interpreter;

public record Repl
{

    public const string SHARK_ASCII = """"""
                                         ,-
                                       ,'::|
                                      /::::|
                                    ,'::::o\                                      _..
                 ____........-------,..::?88b                                  ,-' /
         _.--"""". . . .      .   .  .  .  ""`-._                           ,-' .;'
        <. - :::::o......  ...   . . .. . .  .  .""--._                  ,-'. .;'
         `-._  ` `":`:`:`::||||:::::::::::::::::.:. .  ""--._ ,'|     ,-'.  .;'
             """_=--       //'doo.. ````:`:`::::::::::.:.:.:. .`-`._-'.   .;'
                 ""--.__     P(       \               ` ``:`:``:::: .   .;'
                        "\""--.:-.     `.                             .:/
                          \. /    `-._   `.""-----.,-..::(--"".\""`.  `:\
                           `P         `-._ \          `-:\          `. `:\
                                           ""            "            `-._)
        """""";

    public static void Start()
    {
        Console.WriteLine("Type in commands");
        while (true)
        {
            Console.Write(">> ");
            string line = Console.ReadLine() ?? throw new Exception();

            Lexer l = Lexer.Create(line);
            Parser p = new Parser(l);
            Program pr = p.ParseProgram();

            if (p.Errors.Any())
            {
                PrintParserErros(p.Errors);
                continue;
            }

            Console.WriteLine(pr.String());
        }
    }

    static void PrintParserErros(List<string> errors)
    {
        Console.WriteLine(SHARK_ASCII);
        Console.WriteLine("Woops! We ran into some shark business here!");
        Console.WriteLine(" parser errors:");
        errors.ForEach(Console.WriteLine);
    }
}
