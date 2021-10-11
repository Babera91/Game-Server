const Discord = require("discord.js");
const mysql = require("mysql");
const host = 'localhost';
const user = 'root';
const password = '';
const database = 'gta';

module.exports = class everyone {
    constructor(){
            this.name = 'whitelist',
            this.alias = [''],
            this.usage = '/whitelist'
    }
 
    async run(client, message, args) {
        if (message.member.roles.cache.find(r => r.name ==="Team")) {
			var targetplayerid = "";
			var iswhitelistedornot = "";
			var whitelistedstate = "";
			if (isNaN(args[1])) {
				message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [PLAYER-ID]`');
				return;
			}
			targetplayerid = args[1];
		
			const connection = mysql.createConnection({
				host: host,
				user: user,
				password: password,
				database: database
			})
			connection.connect(function(err) {
                connection.query(`SELECT whitelisted FROM accounts WHERE id=${targetplayerid}`, function (err, result,fields) {
					if (result == undefined) {
                        message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [PLAYER-ID]`');
						return;
                    } else if (result.length === 0) {
						message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [PLAYER-ID]`');
						return;
					};
                    if (result[0].whitelisted != 1) {
						whitelistedstate = 1;
						iswhitelistedornot = "gewhitelistet."
					} else {
						whitelistedstate = 0;
						iswhitelistedornot = "von der Whitelist entfernt."
					}
					connection.query(`UPDATE accounts SET whitelisted = ${whitelistedstate} WHERE id=${targetplayerid}` , function (err, newresult, fields) {
						message.channel.send(`Der Spieler mit der ID ${targetplayerid} wurde ${iswhitelistedornot}`)
					});
                });
			})
		} else {
			message.reply("du darfst diesen Befehl nicht nutzen!")
		}
    }
}