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

    private static void Main(string[] args) {
        if(File.Exists(NLogConfFull)) {
            _logger = LogManager.LoadConfiguration(NLogConfFull)
                .GetCurrentClassLogger();
        } else {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Error($"NLog config file does not exist at expected directory {NLogConfFull}");
        }

        Read(out Movie[] test);
    }

    private static bool Read(out Movie[] movies) {
        StreamReader sr;
        List<Movie> movieList = new();

        try {
            FileStream fs = new(MoviesFull, FileMode.Open);
            sr = new(fs);
        } catch(Exception e) {
            _logger.Warn(e.Message);
            movies = Array.Empty<Movie>();
            return false;
        }

        int lineCount = 0;
        while(!sr.EndOfStream) {
            string? line = sr.ReadLine();

            lineCount++;

            if(line is null) {
                _logger.Warn($"Null line at line {lineCount}");
                continue;
            }

            string[] split = Regex.Split(line);

            if(int.TryParse(split[0], out int movieId)) {
                movieList.Add(new Movie(movieId,
                    split[1].Replace("\"",
                        ""), split[2].Split('|')));
                _logger.Info("Found movie {Movie}", movieList.Last().Name);
            } else {
                _logger.Error("Non-Integer Movie ID, Skipping!");
                //continue;
            }
        }

        movies = movieList.ToArray();
        return true;
    }

    private static bool Write(Movie[] movies) {
        throw new NotImplementedException("Writing to the file is not currently supported..");
    }
}