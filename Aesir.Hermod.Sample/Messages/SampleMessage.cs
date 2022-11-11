using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Sample.Messages;

public class SampleMessageResult : IMessageResult<SampleMessage>
{
    public int ReceivedCount { get; set; }
}
public class SampleMessage : IMessage
{
    public string Message { get; set; } = "Hello World!";
}
