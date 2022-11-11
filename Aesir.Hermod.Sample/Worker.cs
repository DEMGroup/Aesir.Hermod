using Aesir.Hermod.Publishers.Interfaces;
using Aesir.Hermod.Sample.Messages;
using System.Diagnostics;

namespace Aesir.Hermod.Sample
{
    public class Worker : BackgroundService
    {
        private readonly IMessageProducer _producer;

        public Worker(IMessageProducer producer)
        {
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var resCount = 0;
            var sentCount = 0;
            var target = DateTime.Now.AddMinutes(1);
            while(DateTime.Now < target)
            {
                sentCount += 1;
                var res = await _producer.SendWithResponseAsync<SampleMessageResult, SampleMessage>(new SampleMessage { Message = "01011110001110" }, "test-queue");
                if (res != null) resCount += 1;
            }
            Console.WriteLine($"Received/Sent {resCount}/{sentCount} in 1 minute.");
        }
    }
}