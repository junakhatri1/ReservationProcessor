using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMqUtils;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReservationProcessor
{
    public class ReservationListener : RabbitListener
    {
        ILogger<ReservationListener> Logger;
        ReservationHttpService Service;
        public ReservationListener(ReservationHttpService service, ILogger<ReservationListener> logger, IOptionsMonitor<RabbitOptions> options) : base(options)
        {
            Logger = logger;
            QueueName = "reservations"; //this is that queue name. Make sure it matches
            ExchangeName = "";
            Service = service;
        }

        public async override Task<bool> Process(string message)
        {
            //1. Deserialize the message (JSON document)
            var request = JsonSerializer.Deserialize<Reservation>(message);
            Logger.LogInformation($"Got a reservation for {request.For}");
            //2. Decide if it is approved or denied 
            var count = request.Books.Split(',').Length;
            if(count% 2  == 0)
            {
                return await Service.MarkReservationAccepted(request);
            }
            else
            {
                return await Service.MarkReservationDenied(request);
            }
            //
            return true;
        }
    }
}
