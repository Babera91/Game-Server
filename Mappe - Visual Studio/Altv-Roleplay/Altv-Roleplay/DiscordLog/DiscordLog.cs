using System;
using Discord.Webhook;
using Discord.Webhook.HookRequest;

namespace Altv_Roleplay.Handler
{
    class DiscordLog
    {
        internal static void SendEmbed(string type, string nickname, string text)
        {
            DiscordWebhook hook = new DiscordWebhook();

            switch (type)
            {
                case "adminmenu":
                    hook.HookUrl = "YOUR_WEBHOOK";
                    break;
                default:
                    hook.HookUrl = "YOUR_WEBHOOK";
                    break;
            }

            if (hook.HookUrl == "YOUR_WEBHOOK") return; //Hier YOUR_WEBHOOK nicht ersetzen

            DiscordHookBuilder builder = DiscordHookBuilder.Create(Nickname: nickname, AvatarUrl: "https://media.discordapp.net/attachments/723871259791720520/784117781271937034/Logo.png?width=519&height=519");

            DiscordEmbed embed = new DiscordEmbed(
                            Title: "Timo's Scripting Service - Logs",
                            Description: text,
                            Color: 0xf54242,
                            FooterText: "Timo's Scripting Service - Logs",
                            FooterIconUrl: "https://media.discordapp.net/attachments/723871259791720520/784117781271937034/Logo.png?width=519&height=519");
            builder.Embeds.Add(embed);

            DiscordHook HookMessage = builder.Build();
            hook.Hook(HookMessage);
        }
    }
}
