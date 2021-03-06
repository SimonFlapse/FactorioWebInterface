﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FactorioWebInterface.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace FactorioWebInterface.Services.Discord
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly IDiscordMessageHandlingService _messageHandlingService;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger<DiscordBot> _logger;

        public DiscordBot(DiscordSocketClient client,
            IConfiguration configuration,
            IDiscordMessageHandlingService messageHandlingService,
            CommandService commands,
            IServiceProvider services,
            ILogger<DiscordBot> logger)
        {
            _client = client;
            _configuration = configuration;
            _messageHandlingService = messageHandlingService;
            _commands = commands;
            _services = services;
            _logger = logger;

            _client.Log += Log;
            _messageHandlingService.CommandReceived += CommandReceived;
            _commands.CommandExecuted += CommandExecuted;
            _commands.Log += Log;
        }

        public async Task Init()
        {
            string token = _configuration[Constants.DiscordBotTokenKey];

            var ready = new AsyncManualResetEvent();
            Task ReadyCallback()
            {
                _client.Ready -= ReadyCallback;
                ready.Set();
                return Task.CompletedTask;
            }
            _client.Ready += ReadyCallback;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModuleAsync<DiscordBotCommands>(_services);

            await ready.WaitAsync();
        }

        private void CommandReceived(IDiscordMessageHandlingService sender, (SocketUserMessage message, int argPos) eventArgs)
        {
            var context = new SocketCommandContext(_client, eventArgs.message);
            _commands.ExecuteAsync(context, eventArgs.argPos, _services);
        }

        private async Task CommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
            {
                var embed = new EmbedBuilder()
                {
                    Description = $"Unknow command name see `{Constants.DiscordBotCommandPrefix}help` for available commands.",
                    Color = DiscordColors.failureColor
                }
                .Build();

                await context.Channel.SendMessageAsync(embed: embed);

                return;
            }

            if (!result.IsSuccess)
            {
                string description;
                if (result.Error == CommandError.UnmetPrecondition)
                {
                    description = "Insufficient permissions to run this command.";
                }
                else
                {
                    string commandName = command.Value.Name;
                    description = $"Invalid use of {commandName} see `{Constants.DiscordBotCommandPrefix}help {commandName}` for more information with this command.";
                }

                var embed = new EmbedBuilder()
                {
                    Description = description,
                    Color = DiscordColors.failureColor
                }.Build();

                await context.Channel.SendMessageAsync(embed: embed);
            }
        }

        private Task Log(LogMessage message)
        {
            _logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        }
    }
}
