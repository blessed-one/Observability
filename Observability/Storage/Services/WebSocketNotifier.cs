using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Realisation;

namespace Storage.Services;

public class WebSocketNotifier
{
    private readonly List<WebSocket> _clients = new();

    public void AddClient(WebSocket ws)
    {
        lock (_clients)
            _clients.Add(ws);
    }

    public void RemoveClient(WebSocket ws)
    {
        lock (_clients)
            _clients.Remove(ws);
    }

    public async Task NotifyAllAsync(object trace)
    {
        Console.WriteLine($"Notifying { _clients.Count } clients");
        var json = JsonSerializer.Serialize(trace);
        var buffer = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(buffer);

        List<WebSocket> toRemove = new();
        lock (_clients)
        {
            foreach (var ws in _clients)
            {
                if (ws.State == WebSocketState.Open)
                    ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                else
                    toRemove.Add(ws);
            }
            foreach (var ws in toRemove)
                _clients.Remove(ws);
        }
    }
} 