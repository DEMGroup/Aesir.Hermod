using System.Text;
using System.Text.Json;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Models;
using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Messages;

internal static class MessageProcessing
{
    internal static void ProcessResponse(
        BasicDeliverEventArgs e,
        ExpectedResponse response
    )
    {
        try
        {
            var bodyMsg = ParseMessage(e.Body);
            if (bodyMsg == null) return;
            if (bodyMsg.Message == null)
            {
                response.Action(null);
                return;
            }

            var res = JsonSerializer.Deserialize(bodyMsg.Message, response.Type);
            if (res == null) return;

            response.Action(res);
        }
        catch
        {
            // Handle errors
        }
    }

    internal static MessageWrapper? ParseMessage(ReadOnlyMemory<byte> payload)
    {
        var bodyMsgStr = Encoding.UTF8.GetString(payload.ToArray());
        return JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);
    }
}