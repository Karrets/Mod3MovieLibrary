using System.Diagnostics;
using System.Text.RegularExpressions;
using NLog;

namespace MovieLibrary;

static class Program {
    private const string NlogConf = "nlog.config";
    private static readonly string NLogConfFull = Directory.GetCurrentDirectory() + '/' + NlogConf;
    private const string Movies = "movies.csv";
    private static readonly string MoviesFull = Directory.GetCurrentDirectory() + '/' + Movies;
    private static readonly Regex Regex = new("""(?!\B"[^"]*),(?![^"] * "\B)""");
    private static Logger _logger = null!;
    
    //TODO, Cache read results, and only read when user asks.

    private static void Main(string[] args) {
        if(File.Exists(NLogConfFull)) {
            _logger = LogManager.LoadConfiguration(NLogConfFull)
                .GetCurrentClassLogger();
        } else {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Error($"NLog config file does not exist at expected directory {NLogConfFull}");
        }

        bool run = true;
        while(run) {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. (R)ead from file");
            Console.WriteLine("2. (W)rite to file");
            Console.WriteLine("3. (E)xit");
            
            switch(Console.ReadLine()?[0]) {
                case '1':
                case 'r':
                case 'R':
                    if(Read(out Movie[] movies))
                        foreach(var movie in movies)
                            Console.WriteLine(movie.UserToString());
                    else
                        _logger.Error("Read failed... See above for reason.");
                    break;
                case '2':
                case 'w':
                case 'W':
                    if(!CreateRecords(out Movie[] moviesToAdd)) {
                        _logger.Error("Failed to create records.");
                        break;
                    }

                    if(!WriteAppend(moviesToAdd)) {
                        _logger.Error("Failed to append records to database.");
                        break;
                    }
                    
                    Console.WriteLine("Added the following movies:");
                    foreach(var movie in moviesToAdd)
                        Console.WriteLine($" {movie.UserToString()}");
                    
                    break;
                case '3':
                case 'e':
                case 'E':
                    run = false;
                    break;
                default:
                    _logger.Warn("Invalid user input supplied. (Not 123rweRWE)");
                    break;
            }
        }
    }

    private static bool Read(out Movie[] movies) {
        Stopwatch sw = new();
        FileStream fs;
        StreamReader sr;

        List<Movie> movieList = new();

        sw.Start();

        if(File.Exists(MoviesFull)) {
            fs = new(MoviesFull, FileMode.Open);
            sr = new(fs);
        } else {
            _logger.Warn("File");
            movies = Array.Empty<Movie>();
            return false;
        }

        int lineCount = 0;
        while(!sr.EndOfStream) {
            string? line = sr.ReadLine();

            lineCount++;

            if(line is null) {
                _logger.Warn("Null line at line {lineNum}", lineCount);
                continue;
            }

            string[] split = Regex.Split(line);

            if(int.TryParse(split[0], out int movieId)) {
                Movie toAdd = new(
                    movieId,
                    split[1].Replace("\"", ""),
                    split[2].Split('|'));

                if(movieList.Contains(toAdd)) {
                    _logger.Warn("Found duplicate of movie {Movie} at {lineNum}", toAdd.Name, lineCount);
                } else {
                    movieList.Add(toAdd);
                    _logger.Info("Found movie {Movie}", toAdd.Name);
                }
            } else {
                _logger.Error("Non-Integer Movie ID, Skipping!");
                //continue;
            }
        }

        sw.Stop();

        _logger.Debug("Movie parsing took {Time}ms.", sw.ElapsedMilliseconds);

        sr.Close();
        fs.Close();

        movies = movieList.ToArray();
        return true;
    }

    private static bool Write(params Movie[] movies) {
        FileStream fs;
        StreamWriter sw;

        if(File.Exists(MoviesFull)) {
            fs = new(MoviesFull, FileMode.Create);
            sw = new(fs);
        } else {
            _logger.Warn("File");
            return false;
        }

        foreach(var movie in movies) {
            sw.WriteLine(movie);
        }

        sw.Close();
        fs.Close();

        return true;
    }

    private static bool WriteAppend(params Movie[] movies) {
        Movie[] newList;

        if(Read(out Movie[] existing)) {
            newList = new Movie[existing.Length + movies.Length];

            existing.CopyTo(newList, 0);
            movies.CopyTo(newList, existing.Length);
        } else {
            _logger.Warn("Unable to write-append due to issue in file read.");
            return false;
        }

        return Write(newList);
    }

    private static bool CreateRecords(out Movie[] movies) {
        List<Movie> movieList = new();
        int nextId = -1;

        if(Read(out Movie[] curMovies)) {
            foreach(var movie in curMovies) {
                nextId = Math.Max(nextId, movie.Id);
            }
        } else {
            _logger.Error("Unable to find a suitable ID for new record due to failed read.");
            movies = Array.Empty<Movie>();
            return false;
        }

        while(true) {
            List<string> genres = new();
            
            Console.Write("Enter the name:\n> ");
            string name = Console.ReadLine() ?? "";

            while(true) {
                Console.Write("Enter a genre:\n> ");
                genres.Add(Console.ReadLine() ?? "");
                
                Console.Write("Would you like to add another genre (y/n)\n> ");
                if(Console.ReadLine()?.ToUpper()[0] != 'Y')
                    break;
            }
            
            movieList.Add(new(nextId++, name, genres.ToArray()));
            
            Console.Write("Would you like to add another movie (y/n)\n> ");
            if(Console.ReadLine()?.ToUpper()[0] != 'Y')
                break;
        }

        movies = movieList.ToArray();
        return true;
    } 
}