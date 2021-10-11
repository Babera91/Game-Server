const Discord = require("discord.js");
const mysql = require("mysql");
const host = 'localhost';
const user = 'root';
const password = '';
const database = 'gta';

module.exports = class everyone {
    constructor(){
            this.name = 'userinfo',
            this.alias = [''],
            this.usage = '/userinfo'
    }
 
    async run(client, message, args) {
        if (message.member.roles.cache.find(r => r.name ==="Team")) {
            var targetusername = "";
            var banned = "Nein";
            var whitelisted = "Nein";
            var admin = "Nein";
            var bangrund = ""
            var adminn = "";
			
			const connection = mysql.createConnection({
				host: host,
				user: user,
				password: password,
				database: database
            })
            targetusername = args[1];
            if (isNaN(targetusername)) {
				connection.connect(function(err) {
                    connection.query(`SELECT * FROM accounts WHERE username="${targetusername}"`, function (err, result,fields) {
                        if (result == undefined) {
                            message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [NUTZERNAME ODER ID]`');
                            return;
                        } else if (result.length === 0) {
                            message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [NUTZERNAME ODER ID]`');
                            return;
                        };
                        if (result[0].ban > 0) {
                            banned = "Ja";
                        }
                         if (result[0].whitelisted > 0) {
                            whitelisted = "Ja";
                        } 
                        if (result[0].adminlevel > 0) {
                            admin = "Ja, Adminlevel " + result[0].adminlevel;
                        }  
                        
                        let UserinfoEmbed = new Discord.MessageEmbed()
                            .setAuthor("Userinformation")
                            .addField("ID", result[0].id)
                            .addField("Nutzername", result[0].username)
                            .addField("E-Mail", result[0].email)
                            .addField("Socialclub-ID", result[0].socialid)
                            .addField("Hardware-ID", result[0].hwid)
                            .addField("Whitelisted", whitelisted)
                            .addField("Gebannt", banned)
                            .addField("Admin", admin)
                        message.channel.send(UserinfoEmbed);
                        return;
                    });
                });
			}
            
            if (!isNaN(targetusername)) {
                connection.connect(function(err) {
                    connection.query(`SELECT * FROM accounts WHERE id=${targetusername}`, function (err, result,fields) {
                        if (result == undefined) {
                            message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [NUTZERNAME ODER ID]`');
                            return;
                        } else if (result.length === 0) {
                            message.channel.send('Ungültige Befehl-Nutzung, bitte mache es wie folgt: `/whitelist [NUTZERNAME ODER ID]`');
                            return;
                        };
                        if (result[0].ban > 0) {
                            banned = "Ja";
                        }
                        if (result[0].whitelisted > 0) {
                            whitelisted = "Ja";
                        } 
                        if (result[0].adminlevel > 0) {
                            admin = "Ja, Adminlevel " + result[0].adminlevel;
                        }  
                        
                        let UserinfoEmbed = new Discord.MessageEmbed()
                            .setAuthor("Userinformation")
                            .addField("ID", result[0].id)
                            .addField("Nutzername", result[0].username)
                            .addField("E-Mail", result[0].email)
                            .addField("Socialclub-ID", result[0].socialid)
                            .addField("Hardware-ID", result[0].hwid)
                            .addField("Whitelisted", whitelisted)
                            .addField("Gebannt", banned)
                            .addField("Admin", admin)
                        message.channel.send(UserinfoEmbed)
                    });
                });
            };
		} else {
			message.reply("du darfst diesen Befehl nicht nutzen!")
		}
    }
}