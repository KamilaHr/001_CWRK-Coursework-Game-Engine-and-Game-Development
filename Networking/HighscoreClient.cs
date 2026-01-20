using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenGL_Game.Networking
{
    public class HighscoreEntry
    {
        public string Initials { get; set; } = "???";
        public int Score { get; set; } = 0;
        public DateTime WhenUtc { get; set; } = DateTime.UtcNow;
    }

    public class HighscoreResponse
    {
        public List<HighscoreEntry> Scores { get; set; } = new();
    }

    public class PostScoreRequest
    {
        public string Initials { get; set; } = "???";
        public int Score { get; set; } = 0;
    }

    public class HighscoreClient
    {
        private readonly string host;
        private readonly int port;

        public HighscoreClient(string host = "127.0.0.1", int port = 7777)
        {
            this.host = host;
            this.port = port;
        }

        public List<HighscoreEntry> GetScores()
        {
            using var client = new TcpClient(host, port);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            writer.WriteLine("GET");
            string json = reader.ReadLine() ?? "{\"scores\":[]}";
            var resp = JsonSerializer.Deserialize<HighscoreResponse>(json);
            return resp?.Scores ?? new List<HighscoreEntry>();
        }

        public bool PostScore(string initials, int score)
        {
            using var client = new TcpClient(host, port);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            writer.WriteLine("POST");
            var req = new PostScoreRequest { Initials = initials, Score = score };
            writer.WriteLine(JsonSerializer.Serialize(req));

            string resp = reader.ReadLine() ?? "";
            return resp.Contains("\"ok\":true");
        }
    }
}
