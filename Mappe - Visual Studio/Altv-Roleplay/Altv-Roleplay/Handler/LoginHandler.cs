using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class LoginHandler : IScript
    {
        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public void OnPlayerConnect_Handler(ClassicPlayer player, string reason)
        {
            if (player == null || !player.Exists) return;
            player.SetSyncedMetaData("PLAYER_SPAWNED", false);
            player.SetSyncedMetaData("ADMINLEVEL", 0);
            player.SetPlayerIsCuffed("handcuffs", false);
            player.SetPlayerIsCuffed("ropecuffs", false);
            player.SetPlayerCurrentMinijob("None");
            player.SetPlayerCurrentMinijobRouteId(0);
            player.SetPlayerCurrentMinijobStep("None");
            player.SetPlayerCurrentMinijobActionCount(0);
            player.SetPlayerFarmingActionMeta("None");
            User.SetPlayerOnline(player, 0);
            player.EmitLocked("Client:Pedcreator:spawnPed", ServerPeds.GetAllServerPeds());
            CreateLoginBrowser(player);
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void OnPlayerDisconnected_Handler(ClassicPlayer player, string reason)
        {
            try
            {
                if (player == null) return;
                if (User.GetPlayerOnline(player) != 0) Characters.SetCharacterLastPosition(User.GetPlayerOnline(player), player.Position, player.Dimension);
                User.SetPlayerOnline(player, 0);
                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void CreateLoginBrowser(IPlayer client)
        {
            if (client == null || !client.Exists) return;
            client.Model = 0x3D843282;
            client.Dimension = 10000;
            client.Position = new Position(3120, 5349, 10);
            client.EmitLocked("Client:Login:CreateCEF"); //Login triggern
        }

        [AsyncClientEvent("Server:Login:ValidateLoginCredentials")]
        public void ValidateLoginCredentials(ClassicPlayer client, string username, string password)
        {
            if (client == null || !client.Exists) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                client.EmitLocked("Client:Login:showError", "Eines der Felder wurde nicht ordnungsgemäß ausgefüllt.");
                return;
            }

            if (!User.ExistPlayerName(username))
            {
                client.EmitLocked("Client:Login:showError", "Der eingegebene Benutzername wurde nicht gefunden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Der eingegebene Benutzername wurde nicht gefunden ({username}).");
                return;
            }

            if(!BCrypt.Net.BCrypt.Verify(password, User.GetPlayerPassword(username))) {
                client.EmitLocked("Client:Login:showError", "Das eingegebene Passwort ist falsch.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Das eingegebene Passwort ist falsch");
                return;
            }

            /*if(User.GetPlayerSocialclubId(username) != client.SocialClubId)*/
            if (User.GetPlayerSocialclubId(username) != 0) { if (User.GetPlayerSocialclubId(username) != client.SocialClubId) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 508)."); return; } }
            else { User.SetPlayerSocialID(client); }

            if(!User.IsPlayerWhitelisted(username))
            {
                client.EmitLocked("Client:Login:showError", "Dieser Benutzeraccount wurde noch nicht im Support aktiviert.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Dieser Benutzeraccount wurde noch nicht im Support aktiviert ({username}).");
                return;
            }

            if(User.GetPlayerHardwareID(client) != 0) { if(User.GetPlayerHardwareID(client) != client.HardwareIdHash) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 187)."); return; } } 
            else { User.SetPlayerHardwareID(client); }

            if(User.IsPlayerBanned(client))
            {
                client.EmitLocked("Client:Login:showError", "Dieser Benutzeraccount wurde gebannt, im Support melden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Dieser Benutzeraccount wurde gebannt, im Support melden ({username}).");
                return;
            }

            client.EmitLocked("Client:Login:SaveLoginCredentialsToStorage", username, password);
            User.SetPlayerOnline(client, 0);
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.accountId = (short)User.GetPlayerAccountId(client);
                client.Dimension = (short)User.GetPlayerAccountId(client);
            }
            SendDataToCharselectorArea(client);
            LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, true, "Erfolgreich eingeloggt.");
            stopwatch.Stop();
            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"ValidateLoginCredentials benötigte {stopwatch.Elapsed.Milliseconds}ms");
        }        

        [AsyncClientEvent("Server:Charselector:PreviewCharacter")]
        public void PreviewCharacter(IPlayer client, int charid)
        {
            if (client == null || !client.Exists) return;
            client.EmitLocked("Client:Charselector:ViewCharacter", Characters.GetCharacterGender(charid), Characters.GetCharacterSkin("facefeatures", charid), Characters.GetCharacterSkin("headblendsdata", charid), Characters.GetCharacterSkin("headoverlays", charid));
        }

        public static void SendDataToCharselectorArea(IPlayer client)
        {
            if (client == null || !client.Exists || ((ClassicPlayer)client).accountId <= 0) return;
            var charArray = Characters.GetPlayerCharacters(client);
            lock (client) 
            {
                if (client == null || !client.Exists) return;
                client.Position = new Position((float)402.778, (float)-996.9758, (float)-98);
            }
            
            client.EmitLocked("Client:Charselector:sendCharactersToCEF", charArray);
            client.EmitLocked("Client:Login:showArea", "charselect");
        }

        [AsyncClientEvent("Server:Charselector:spawnChar")]
        public async void CharacterSelectedSpawnPlace(ClassicPlayer client, string spawnstr, string charcid)
        {
            if (client == null || !client.Exists || spawnstr == null || charcid == null || client.accountId <= 0 || User.GetPlayerAccountId(client) <= 0) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charid = Convert.ToInt32(charcid);
            if (charid <= 0) return;
            if (User.GetPlayerAccountId(client) != Characters.GetCharacterAccountId(charid))
            {
                client.Kick("Login Fehler!");
                return;
            }
            string charName = Characters.GetCharacterName(charid);
            User.SetPlayerOnline(client, charid); //Online Feld = CharakterID
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.CharacterId = charid;
            }

            if (Characters.GetCharacterFirstJoin(charid) && Characters.GetCharacterFirstSpawnPlace(client, charid) == "unset")
            {
                Characters.SetCharacterFirstSpawnPlace(client, charid, spawnstr);
                CharactersInventory.AddCharacterItem(charid, "Bargeld", 10000, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Tasche", 1, "inventory");
                Characters.SetCharacterBackpack(client, "Tasche");
                //player.SetClothes(5, 0, 0, 2);
                CharactersInventory.AddCharacterItem(charid, "Sandwich", 3, "backpack"); //ToDo: Trinken hinzufuegen
                CharactersInventory.AddCharacterItem(charid, "Tablet", 1, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Smartphone", 1, "inventory");

                // First-Spawn Kleider
                if (!Characters.GetCharacterGender((int)client.GetCharacterMetaId()))
                {
                    //Männlich
                    Characters.SwitchCharacterClothes(client, 5048, false);
                    Characters.SwitchCharacterClothes(client, 7761, false);
                    Characters.SwitchCharacterClothes(client, 4860, false);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 5048);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 7761);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 4860);
                }
                else
                {
                    //Weiblich
                    Characters.SwitchCharacterClothes(client, 17018, false);
                    Characters.SwitchCharacterClothes(client, 21383, false);
                    Characters.SwitchCharacterClothes(client, 27501, false);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 17018);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 21383);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 27501);
                }
                

                switch (spawnstr)
                {
                    case "lsairport":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                    case "beach":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Beach, 0);
                        break;
                    case "sandyshores":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_SandyShores, 0);
                        break;
                    case "paletobay":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_PaletoBay, 0);
                        break;
                    case null:
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                }
            }
            lock (client)
            {
                if (client == null || !client.Exists) return;
                if (Characters.GetCharacterGender(charid)) client.Model = 0x9C9EFFD8;
                else client.Model = 0x705E61F2;
            }

            client.EmitLocked("Client:ServerBlips:LoadAllBlips", ServerBlips.GetAllServerBlips());
            client.EmitLocked("Client:ServerMarkers:LoadAllMarkers", ServerBlips.GetAllServerMarkers());
            client.EmitLocked("Client:SpawnArea:setCharSkin", Characters.GetCharacterSkin("facefeatures", charid), Characters.GetCharacterSkin("headblendsdata", charid), Characters.GetCharacterSkin("headoverlays", charid));
            Position dbPos = Characters.GetCharacterLastPosition(charid);
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.Position = dbPos;
                client.Spawn(dbPos, 0);
            }
            if (Characters.GetCharacterPedName(client.CharacterId) != "none" && !String.IsNullOrWhiteSpace(Characters.GetCharacterPedName(client.CharacterId))) client.Model = Alt.Hash(Characters.GetCharacterPedName(client.CharacterId));
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.Dimension = Characters.GetCharacterLastDimension(charid);
                client.Health = (ushort)(Characters.GetCharacterHealth(charid) + 100);
                client.Armor = (ushort)Characters.GetCharacterArmor(charid);
            }
            HUDHandler.CreateHUDBrowser(client); //HUD erstellen
            WeatherHandler.SetRealWeather(client);
            Characters.SetCharacterCorrectClothes(client, true);
            Characters.SetCharacterLastLogin(charid, DateTime.Now);
            Characters.SetCharacterCurrentFunkFrequence(charid, null);
            Alt.Log($"Eingeloggt {client.Name}");
            Alt.Emit("SaltyChat:EnablePlayer", client, (int)charid);
            client.EmitLocked("SaltyChat_OnConnected");
            client.SetSyncedMetaData("NAME", User.GetPlayerUsername(((ClassicPlayer)client).accountId) + " | " + Characters.GetCharacterName((int)client.GetCharacterMetaId()));
            if (Characters.IsCharacterUnconscious(charid))
            {
                dbPos.Y = dbPos.Y + 1;
                lock (client)
                {
                    if (client == null || !client.Exists) return;
                    client.Spawn(dbPos, 0);
                    client.EmitAsync("Client:Ragdoll:SetPedToRagdoll", true, 0);
                    client.EmitAsync("Client:Deathscreen:openCEF");
                    client.SetPlayerIsUnconscious(true);
                }
            }
            if (Characters.IsCharacterFastFarm(charid))
            {
                var fastFarmTime = Characters.GetCharacterFastFarmTime(charid) * 60000;
                client.EmitLocked("Client:Inventory:PlayEffect", "DrugsMichaelAliensFight", fastFarmTime);
                HUDHandler.SendNotification(client, 2, 2000, $"Du bist durch dein Koks noch {fastFarmTime} Minuten effektiver.");
            }
            ServerAnimations.RequestAnimationMenuContent(client);
            if (Characters.IsCharacterPhoneEquipped(charid) && CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") && CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") > 0)
            {
                client.EmitLocked("Client:Smartphone:equipPhone", true, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid), Characters.GetCharacterPhoneWallpaper(charid));
                Characters.SetCharacterPhoneEquipped(charid, true);
            }
            else if(!Characters.IsCharacterPhoneEquipped(charid) || !CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") || CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") <= 0) {
                client.EmitLocked("Client:Smartphone:equipPhone", false, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid), Characters.GetCharacterPhoneWallpaper(charid));
                Characters.SetCharacterPhoneEquipped(charid, false);
            }
            SmartphoneHandler.RequestLSPDIntranet(client);
            client.SetStreamSyncedMetaData("sharedUsername", $"{charName} ({Characters.GetCharacterAccountId(charid)})");
            client.SetSyncedMetaData("ADMINLEVEL", client.AdminLevel());
            client.SetSyncedMetaData("PLAYER_SPAWNED", true);

            if(Characters.IsCharacterInJail(charid))
            {
                HUDHandler.SendNotification(client, 1, 2500, $"Du befindest dich noch {Characters.GetCharacterJailTime(charid)} Minuten im Gefängnis.", 8000);
                lock (client)
                {
                    if (client == null || !client.Exists) return;
                    client.Position = new Position(1691.4594f, 2565.7056f, 45.556763f);
                }
                if(Characters.GetCharacterGender(charid) == false)
                {
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 11, 5, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 3, 5, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 4, 7, 15);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 6, 7, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 8, 1, 88);
                } else
                {

                }
            }
            client.updateTattoos();
            stopwatch.Stop();
            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charid} - CharacterSelectedSpawnPlace benötigte {stopwatch.Elapsed.Milliseconds}ms");          
            await Task.Delay(5000);
            Model.ServerTattoos.GetAllTattoos(client);
        }

        [AsyncClientEvent("Server:SpawnArea:SetHairs")]
        public void SetHairs(ClassicPlayer player, int hairIndex)
        {
            if (player == null || !player.Exists || hairIndex < 0) return;

            player.SetClothes(2, (ushort)hairIndex, 0, 2);
        }
    }
}
