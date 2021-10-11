using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Discord.Webhook.HookRequest
{
    public class DiscordHookBuilder
    {
        string _bound;

        string _nick;
        string _avatar;

        JObject _json;

        private DiscordHookBuilder(string Nickname, string AvatarUrl)
        {
            _bound = $"------------------------{DateTime.Now.Ticks.ToString("x")}";

            _nick = Nickname;
            _avatar = AvatarUrl;

            _json = new JObject();

            Embeds = new List<DiscordEmbed>();
        }

        public static DiscordHookBuilder Create(string Nickname=null, string AvatarUrl=null)
        {
            return new DiscordHookBuilder(Nickname, AvatarUrl);
        }

        public List<DiscordEmbed> Embeds { get; private set; }
        public string Message { get; set; }
        public bool  UseTTS { get; set; }

        public DiscordHook Build()
        {
            MemoryStream stream = new MemoryStream();

            byte[] boundary = Encoding.UTF8.GetBytes($"--{_bound}\r\n");
            stream.Write(boundary, 0, boundary.Length);

            _json.Add("username", _nick);
            _json.Add("avatar_url", _avatar);
            _json.Add("content", Message);
            _json.Add("tts", UseTTS);

            JArray embeds = new JArray();
            foreach(DiscordEmbed embed in Embeds) embeds.Add(embed.JsonData);
            if (embeds.Count > 0) _json.Add("embeds", embeds);

            string jsonBody = $"Content-Disposition: form-data; name=\"payload_json\"\r\n" +
                $"Content-Type: application/json\r\n\r\n" +
                $"{_json.ToString(Newtonsoft.Json.Formatting.None)}\r\n" +
                $"--{_bound}--";
            byte[] jsonBodyData = Encoding.UTF8.GetBytes(jsonBody);

            stream.Write(jsonBodyData, 0, jsonBodyData.Length);
            return new DiscordHook(stream, _bound);
        }
    }
}
