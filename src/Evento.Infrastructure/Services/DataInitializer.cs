using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace Evento.Infrastructure.Services
{
    public class DataInitializer : IDataInitializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;
        private readonly IEventService _eventService;

        public DataInitializer(IUserService userService, IEventService eventService)
        {
            _userService = userService;
            _eventService = eventService;
        }

        public async Task SeedAsync()
        {
            Logger.Info("Initializing data...");

            var tasks = new List<Task>();

            tasks.Add(_userService.RegisterAsync(Guid.NewGuid(), "admin@example.com", "Admin","secret","admin"));
            tasks.Add(_userService.RegisterAsync(Guid.NewGuid(), "user@example.com", "User","secret","user"));

            Logger.Info("Created Users: user, admin");

            for(var i = 1; i <= 10; i++)
            {
                var eventId = Guid.NewGuid();
                var eventName = $"Event #{i}";
                var eventDesc = $"{eventName} description.";
                var startDate = DateTime.UtcNow.AddHours(3);
                var endDate = startDate.AddHours(2);

                tasks.Add(_eventService.CreateAsync(eventId, eventName,eventDesc, startDate,endDate));
                tasks.Add(_eventService.AddTicketsAsync(eventId, 100, 50));

                Logger.Info($"Created event: '{eventName}'.");

                
            }

            await Task.WhenAll(tasks);
            Logger.Info("Data was initialized.");
        }
    }
}