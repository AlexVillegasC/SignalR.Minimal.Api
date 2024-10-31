using Microsoft.AspNetCore.SignalR;

namespace Paradigmas.DBZ.SignalR.Api.SignalRHub;

public class DBZCharactersHub : Hub<IReceiveCharacters>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).ReceiveCharacterSimulation($"{GetRandomCharacter().First().Name} {GetRandomCharacter().First().StrengthLevel}");

        await base.OnConnectedAsync();
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


public interface IReceiveCharacters
{
    Task ReceiveCharacterSimulation(string message);
}
