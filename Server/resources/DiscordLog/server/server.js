const token = "TOKEN";

const Discord = require("discord.js");
const client = new Discord.Client();
//const TOKEN = './.env.TOKEN';
const { CommandHandler } = require("djs-commands");
const CH = new CommandHandler({
  folder: __dirname + "/commands/",
  prefix: ["/"]
});

client.on("ready", () => {
  console.log(client.user.username + " " + "ist hochgefahren.");

  const StartupEmbed = new Discord.MessageEmbed()
	.setTitle('Status')
	.setColor('#0099ff')
	.setDescription('Der Server wurde **gestartet**!')
	.setColor('#00cc00')
	.setThumbnail('https://media.discordapp.net/attachments/737017783186882700/747544875834277898/image0.png?width=1025&height=599')
	.setTimestamp();

	const channel = client.channels.cache.get('737017794763292791');
	channel.send(StartupEmbed);		

	client.user.setPresence({ game: { name: 'alt:V Multiplayer' }, status: 'online' });
});

client.on("message", message => {

  if (message.channel.type === "dm") return;
  if (message.author.type === "bot") return;
  let args = message.content.split(" ");
  let command = args[0];
  let cmd = CH.getCommand(command);
  if (!cmd) return;

  try {
    cmd.run(client, message, args);
  } catch (e) {
    console.log(e);
  }
}); 

client.login(token);
