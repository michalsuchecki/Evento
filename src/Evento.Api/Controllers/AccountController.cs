using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Evento.Infrastructure.Services;
using Evento.Infrastructure.Commands.Users;
using System;

namespace Evento.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Post(Register command)
        {
            await _userService.RegisterAsync(Guid.NewGuid(), command.Email, command.Name, command.Password, command.Role);

            return Created("/account",null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Post(Login command)
        {
            return NoContent();
        }

    }
}