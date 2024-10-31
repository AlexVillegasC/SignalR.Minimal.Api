using Microsoft.AspNetCore.SignalR;

namespace Paradigmas.DBZ.SignalR.Api.SignalRHub;

public class DBZCharactersStream : BackgroundService
{

    private static readonly TimeSpan Period = TimeSpan.FromSeconds(5);

    private readonly IHubContext<DBZCharactersHub, IReceiveCharacters> _context;

    public DBZCharactersStream(IHubContext<DBZCharactersHub, IReceiveCharacters> hubContext)
    {
        _context = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var dateTime = DateTime.Now;

            await _context.Clients.All.ReceiveCharacterSimulation($"{GetRandomCharacter().First().Name} {GetRandomCharacter().First().StrengthLevel}");
        }
    }

    private IEnumerable<Character> GetRandomCharacter()
    {
        string[] characters = new[] { "Goku", "Picoro", "Vegueta", "Krilin" };
        return Enumerable.Range(1, 1).Select(index =>
            new Character
            (
                characters[Random.Shared.Next(0, 4)],
                Random.Shared.Next(0, 100)
            ));
    }
}