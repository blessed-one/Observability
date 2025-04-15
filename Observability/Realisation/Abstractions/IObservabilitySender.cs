namespace Realisation.Abstractions;

public interface IObservabilitySender
{
    Task SendAsync(string logJson);
}