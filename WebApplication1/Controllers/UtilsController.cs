﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;
using whenAppModel.Services;

namespace WhenUp.Controllers
{
    [ApiController]
    [Route("api")]
    public class UtilsController : Controller
    {
        
        private readonly IMessageService _messagesService;
        private readonly IContactsService _contactService;
        private readonly IUsersService _userService;
        private readonly IHubContext<ChatHub> _hubContext;

        public UtilsController(IContactsService ContactService, IUsersService UserService, IMessageService MessageService, IHubContext<ChatHub> HubContext)
        {
            _contactService = ContactService;
            _userService = UserService;
            _messagesService = MessageService;
            _hubContext = HubContext;
        }

        public class UtilsPayload
        {
            public string? from { get; set; }
            public string? to { get; set; }
            public string? content { get; set; }
            public string? server { get; set; }
        }

        [HttpPost]
        [Route("invitations")]
        public async Task<IActionResult> Invitations([FromBody] UtilsPayload payload)
        {
            string? from = payload.from;
            string? to = payload.to;
            string? server = payload.server;

            await _contactService.AddContact(to, from, from, server);
            await _hubContext.Clients.Group(to).SendAsync("MessageReceived");
            return Created("Invitations", null);
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<IActionResult> Transfer([FromBody] UtilsPayload payload)
        {
            string? from = payload.from;
            string? to = payload.to;
            string? content = payload.content;

            // if from is in my server
            if (await _userService.Get(from) != null)
            {
                await _contactService.UpdateContactLast(to, from, content);
            }
            else
            {
                await _messagesService.AddMessage(from, to, content);
            }
            await _hubContext.Clients.Group(to).SendAsync("MessageReceived");

            return Created("Transfer", null);
        }
        
    }
}
