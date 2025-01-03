using Newtonsoft.Json;

namespace OpenRestClient.ApiController
{
    public static class DebugMiddleware
    {
        public static async void Middleware(HttpClient _, HttpResponseMessage httpResponse, HttpRequest httpRequest)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("{0} {1}", httpRequest.Method.ToString(), (int)httpResponse.StatusCode);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(httpRequest.Url);
            Console.WriteLine();

           

            if (!string.IsNullOrWhiteSpace(httpRequest.Content))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"REQUEST");
                ParseJson(httpRequest.Content);
            }

            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseString))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"RESPONSE");
                ParseJson(responseString);
            }

        }

        private static void ParseJson(string json)
        {
            json = JsonPrettify(json);
            StringReader reader = new StringReader(json);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    ParseLine(line);
                }
                catch { }

                Console.WriteLine();
            }
        }

        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        private static void ParseLine(string line)
        {
            bool alternarColor = true;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '{' || c == '}' || c == ':' || c == ' ')
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(c.ToString());
                }

                if (c == '[' || c == ']')
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(c.ToString());
                }

                if (c == ',')
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(c.ToString());
                    alternarColor = false;
                }

                if (c == '"')
                {
                    if (alternarColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(c.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(c.ToString());
                    }

                    bool b = false;
                    while (true)
                    {
                        i++;
                        c = line[i];
                        if (c == '\\')
                            b = true;

                        if (c == '"')
                        {
                            if (!b)
                                break;
                            else
                                b = false;
                        }

                        if (alternarColor)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(c.ToString());
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(c.ToString());
                        }
                    }

                    if (alternarColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(c.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(c.ToString());
                    }
                    alternarColor = !alternarColor;
                }

                if (char.IsNumber(c))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(c.ToString());
                }

                if (char.IsLetter(c))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(c.ToString());
                }
            }
        }
    }
}
