using System;
using System.Threading.Tasks;
using Evento.Infrastructure.Commands.Events;
using Evento.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evento.Api.Controllers
{
    public class EventsController : ApiControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            var events = await _eventService.BrowseAsync();
            return Json(events);
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> Get(Guid eventId)
        {
            var @event = await _eventService.GetAsync(eventId);

            if(@eventId == null)
            {
                return NotFound();
            }
            
            return Json(@event);
        }

        [HttpPost]
        [Authorize(Policy = "HasAdminRole")]
        public async Task<IActionResult> Post([FromBody]CreateEvent command)
        {
            command.EventId = Guid.NewGuid();

            await _eventService.CreateAsync(command.EventId, command.Name, command.Description, command.StartDate, command.EndDate);

            await _eventService.AddTicketsAsync(command.EventId, command.Tickets, command.Price);

            return Created($"/events/{command.EventId}", null);
        }

        [HttpPut("{eventId}")]
        [Authorize(Policy = "HasAdminRole")]        
        public async Task<IActionResult> Put(Guid eventId, [FromBody]UpdateEvent command)
        {
            await _eventService.UpdateAsync(eventId, command.Name, command.Description);

            return NoContent();
        }

        [HttpDelete("{eventId}")]
        [Authorize(Policy = "HasAdminRole")]        
        public async Task<IActionResult> Delete(Guid eventId)
        {
            await _eventService.DeleteAsync(eventId);

            return NoContent();
        }
    }
}