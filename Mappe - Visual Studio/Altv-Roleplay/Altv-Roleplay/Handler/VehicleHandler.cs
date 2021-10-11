using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;

namespace Altv_Roleplay.Handler
{
    class VehicleHandler : IScript
    {
        [AsyncClientEvent("Server:VehicleTrunk:StorageItem")]
        public void VehicleTrunkStorageItem(ClassicPlayer player, int vehId, int charId, string itemName, int itemAmount, string fromContainer, string type)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player == null || !player.Exists || vehId <= 0 || charId <= 0 || itemName == "" || itemAmount <= 0 || fromContainer == "none" || fromContainer == "") return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (type != "trunk" && type != "glovebox") return;
                int cCharId = player.CharacterId;
                if (cCharId != charId) return;
                var targetVehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId);
                if (targetVehicle == null || !targetVehicle.Exists) return;
                if(!player.Position.IsInRange(targetVehicle.Position, 5f)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist zu weit entfernt."); return; }
                if (type == "trunk")
                {
                    if(player.IsInVehicle) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du von Innen an den Kofferraum kommen?"); return; }
                    if (!targetVehicle.GetVehicleTrunkState()) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Der Kofferraum ist nicht geöffnet."); return; }
                }
                else if(type == "glovebox") { if(!player.IsInVehicle) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist in keinem Fahrzeug."); return; } }
                if(!CharactersInventory.ExistCharacterItem(charId, itemName, fromContainer)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Diesen Gegenstand besitzt du nicht."); return; }
                if(CharactersInventory.GetCharacterItemAmount(charId, itemName, fromContainer) < itemAmount) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du hast nicht genügend Gegenstände davon dabei."); return; }
                if(CharactersInventory.IsItemActive(player, itemName)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Ausgerüstete Gegenstände können nicht umgelagert werden."); return; }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                float curVehWeight = 0f;
                float maxVehWeight = 0f;

                if(type == "trunk")
                {
                    curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(vehId, false);
                    maxVehWeight = ServerVehicles.GetVehicleTrunkCapacityOnHash(targetVehicle.Model);
                } else if(type == "glovebox")
                {
                    curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(vehId, true);
                    maxVehWeight = 5f;
                }

                if (curVehWeight + itemWeight > maxVehWeight) { HUDHandler.SendNotification(player, 3, 5000, $"Fehler: Soviel passt hier nicht rein (Aktuell: {curVehWeight} |  Maximum: {maxVehWeight})."); return; }
                CharactersInventory.RemoveCharacterItemAmount(charId, itemName, itemAmount, fromContainer);

                if(type == "trunk")
                {
                    ServerVehicles.AddVehicleTrunkItem(vehId, itemName, itemAmount, false);
                    HUDHandler.SendNotification(player, 2, 2500, $"Du hast den Gegenstand '{itemName} ({itemAmount}x)' in den Kofferraum gelegt.");
                    stopwatch.Stop();
                    Alt.Log($"{charId} - VehicleTrunkStorageItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                } else if(type == "glovebox")
                {
                    ServerVehicles.AddVehicleTrunkItem(vehId, itemName, itemAmount, true);
                    HUDHandler.SendNotification(player, 2, 2500, $"Du hast den Gegenstand '{itemName} ({itemAmount}x)' in das Handschuhfach gelegt.");
                    stopwatch.Stop();
                    Alt.Log($"{charId} - VehicleTrunkStorageItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }                
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:VehicleTrunk:TakeItem")]
        public void VehicleTrunkTakeItem(ClassicPlayer player, int vehId, int charId, string itemName, int itemAmount, string type)
        {
            try
            {
                if (player == null || !player.Exists || vehId <= 0 || charId <= 0 || itemName == "" || itemAmount <= 0) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (type != "trunk" && type != "glovebox") return;
                int cCharId = player.CharacterId;
                if (cCharId != charId) return;
                var targetVehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId);
                if (targetVehicle == null || !targetVehicle.Exists) return;
                if (!player.Position.IsInRange(targetVehicle.Position, 5f)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist zu weit entfernt."); return; }
                if (type == "trunk")
                {
                    if (player.IsInVehicle) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du von Innen an den Kofferraum kommen?"); return; }
                    if (!targetVehicle.GetVehicleTrunkState()) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Der Kofferraum ist nicht geöffnet."); return; }
                    if (!ServerVehicles.ExistVehicleTrunkItem(vehId, itemName, false)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Der Gegenstand existiert hier nicht."); return; }
                    if(ServerVehicles.GetVehicleTrunkItemAmount(vehId, itemName, false) < itemAmount) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Soviele Gegenstände sind nicht im Fahrzeug."); return; }
                }
                else if (type == "glovebox") { 
                    if (!player.IsInVehicle) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist in keinem Fahrzeug."); return; } 
                    if(!ServerVehicles.ExistVehicleTrunkItem(vehId, itemName, true)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Der Gegenstand existiert hier nicht."); return; }
                    if(ServerVehicles.GetVehicleTrunkItemAmount(vehId, itemName, true) < itemAmount) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Soviele Gegenstände sind nicht im Fahrzeug."); return; }
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast nicht genug Platz in deinen Taschen."); return; }
                
                if(type == "trunk")
                {
                    ServerVehicles.RemoveVehicleTrunkItemAmount(vehId, itemName, itemAmount, false);
                }
                else if(type == "glovebox")
                {
                    ServerVehicles.RemoveVehicleTrunkItemAmount(vehId, itemName, itemAmount, true);
                }

                if (invWeight + itemWeight <= 15f)
                {
                    HUDHandler.SendNotification(player, 2, 5000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Inventar).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "inventory");
                    stopwatch.Stop();
                    Alt.Log($"{charId} - VehicleTrunkTakeItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }

                if (Characters.GetCharacterBackpack(charId) != -2 && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    HUDHandler.SendNotification(player, 2, 5000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Rucksack / Tasche).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "backpack");
                    stopwatch.Stop();
                    Alt.Log($"{charId} - VehicleTrunkTakeItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OpenLicensingCEF(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if(!player.Position.IsInRange(Constants.Positions.VehicleLicensing_Position, 3f)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du hast dich zu weit entfernt."); return; }

                var vehicleList = Alt.GetAllVehicles().Where(x => x.GetVehicleId() > 0 && x.Position.IsInRange(Constants.Positions.VehicleLicensing_VehPosition, 10f) && ServerVehicles.GetVehicleOwner(x) == charId).Select(x => new
                {
                    vehId = x.GetVehicleId(),
                    ownerId = ServerVehicles.GetVehicleOwner(x),
                    vehName = ServerVehicles.GetVehicleNameOnHash(x.Model),
                    vehPlate = x.NumberplateText,
                }).ToList();

                if(vehicleList.Count <= 0) { HUDHandler.SendNotification(player, 3, 5000, "Keines deiner Fahrzeuge steht hinter dem Rathaus (an der roten Fahrzeugmarkierung)."); return; }
                player.EmitLocked("Client:VehicleLicensing:openCEF", JsonConvert.SerializeObject(vehicleList)); 
                Alt.Log($"{JsonConvert.SerializeObject(vehicleList)}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:VehicleLicensing:LicensingAction")]
        public void LicensingAction(IPlayer player, string action, int vehId, string vehPlate, string newPlate)
        {
            try
            {
                if (player == null || !player.Exists || vehId <= 0 || vehPlate == "") return;
                if (action != "anmelden" && action != "abmelden") return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                IVehicle veh = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId && x.NumberplateText == vehPlate);
                if(veh == null || !veh.Exists) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Ein unerwarteter Fehler ist aufgetreten."); return; }
                if(ServerVehicles.GetVehicleOwner(veh) != charId) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Dieses Fahrzeug gehört nicht dir."); return; }
                if(!veh.Position.IsInRange(Constants.Positions.VehicleLicensing_VehPosition, 10f)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Das Fahrzeug ist nicht am Zulassungspunkt (hinterm Rathaus)."); return; }
                if(!ServerVehicles.GetVehicleLockState(veh)) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Das Fahrzeug muss zugeschlossen sein."); return; }
                
                if(action == "anmelden")
                {
                    var notAllowedStrings = new[] { "LSPD", "DOJ", "LSFD", "ACLS", "LSF", "FIB", "LSF-", "LSPD-", "DOJ-", "LSFD-", "ACLS-", "FIB-", "NL", "EL-", "MM-", "PL-", "SWAT", "S.W.A.T", "SWAT-", "NOOSE", "N.O.O.S.E" };
                    newPlate = newPlate.Replace(" ", "-");
                    if(ServerVehicles.ExistServerVehiclePlate(newPlate)) { HUDHandler.SendNotification(player, 3, 5000, "Fehler: Dieses Nummernschild ist bereits vorhanden."); return; }
                    bool stringIsValid = Regex.IsMatch(newPlate, @"[a-zA-Z0-9-]$");
                    bool validPlate = false;
                    if (stringIsValid) validPlate = true;
                    for(var i = 0; i < notAllowedStrings.Length; i++)
                    {
                        if(newPlate.Contains(notAllowedStrings[i])) { validPlate = false; break; }
                    }
                    if(!validPlate) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Das Wunschnummernschild enthält unzulässige Zeichen."); return; }
                    if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "inventory")) { HUDHandler.SendNotification(player, 3, 5000, "Fehler: Du hast kein Bargeld dabei (250$)."); return; }
                    if (CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "inventory") < 250) { HUDHandler.SendNotification(player, 3, 5000, "Fehler: Du hast nicht genügend Bargeld dabei (250$)."); return; }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", 250, "inventory");
                    CharactersInventory.RenameCharactersItemName($"Fahrzeugschluessel {vehPlate}", $"Fahrzeugschluessel {newPlate}");
                    ServerVehicles.SetServerVehiclePlate(vehId, newPlate);
                    veh.NumberplateText = newPlate;
                    HUDHandler.SendNotification(player, 2, 5000, $"Du hast das Kennzeichen von dem Fahrzeug '{ServerVehicles.GetVehicleNameOnHash(veh.Model)}' auf {newPlate} geändert (Gebühr: 250$).");
                    return;
                } 
                else if(action == "abmelden")
                {
                    int rnd = new Random().Next(100000, 999999);
                    if(ServerVehicles.ExistServerVehiclePlate($"NL{rnd}")) { LicensingAction(player, "abmelden", vehId, vehPlate, newPlate); return; }
                    CharactersInventory.RenameCharactersItemName($"Fahrzeugschluessel {vehPlate}", $"Fahrzeugschluessel NL{rnd}");
                    ServerVehicles.SetServerVehiclePlate(vehId, $"NL{rnd}");
                    veh.NumberplateText = $"NL{rnd}";
                    HUDHandler.SendNotification(player, 2, 5000, $"Du hast das Fahrzeug '{ServerVehicles.GetVehicleNameOnHash(veh.Model)}' mit dem Kennzeichen '{vehPlate}' abgemeldet.");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Sirens:ForwardSirenMute")]
        public void SetVehicleHasMutedSirensForAll(ClassicPlayer player, int vehId, bool state)
        {
            try
            {
                Alt.EmitAllClients("Client:Sirens:setVehicleHasMutedSirensForAll", vehId, state);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("sendveharray")]
        public static void testtesttest(IPlayer player)
        {
            try
            {
                //IVehicle veh = Alt.CreateVehicle("YOSEMITE2", new AltV.Net.Data.Position(0, 0, 75), new AltV.Net.Data.Rotation(0, 0, 0));

                IVehicle veh = player.Vehicle;
                
                veh.EngineOn = true;
                veh.ModKit = 1;

                int[] possibleMods = { };
                int[] installedMods = { };

                for (var i = 0; i < 85; i++)
                {
                    Array.Resize(ref possibleMods, i);
                    Array.Resize(ref installedMods, i);
                }

                for (var i = 0; i < 49; i++)
                {
                    possibleMods[i] = veh.GetModsCount((byte)i);
                    //Console.WriteLine(i + ": " + veh.GetModsCount((byte)i));
                    installedMods[i] = veh.GetMod((byte)i);
                }

                possibleMods[49] = 15;
                installedMods[49] = 0;

                possibleMods[50] = 11;
                installedMods[50] = 0;

                possibleMods[51] = 160;
                installedMods[51] = 0;

                possibleMods[52] = 3;
                installedMods[52] = veh.WindowTint;

                possibleMods[53] = 4;
                installedMods[53] = Convert.ToInt32(veh.NumberplateIndex);

                possibleMods[54] = 0;
                installedMods[54] = 0;

                string NumberplateText = veh.NumberplateText;

                possibleMods[55] = 5;
                installedMods[55] = 0;

                possibleMods[56] = 0;
                installedMods[56] = veh.PrimaryColorRgb.R;

                possibleMods[57] = 0;
                installedMods[57] = veh.PrimaryColorRgb.G;

                possibleMods[58] = 0;
                installedMods[58] = veh.PrimaryColorRgb.B;

                possibleMods[59] = 5;
                installedMods[59] = 0;

                possibleMods[60] = 0;
                installedMods[60] = veh.SecondaryColorRgb.R;

                possibleMods[61] = 0;
                installedMods[61] = veh.SecondaryColorRgb.G;

                possibleMods[62] = 0;
                installedMods[62] = veh.SecondaryColorRgb.B;

                possibleMods[63] = 160;
                installedMods[63] = 0;

                possibleMods[64] = 100;
                installedMods[64] = 0;

                for (var i = 0; i < 16; i++)
                {
                    Array.Resize(ref possibleMods, possibleMods.Length + 1);
                    possibleMods[possibleMods.GetUpperBound(0)] = 1;

                    Array.Resize(ref installedMods, installedMods.Length + 1);
                    installedMods[installedMods.GetUpperBound(0)] = Convert.ToInt32(veh.IsExtraOn((byte)i));
                }

                possibleMods[80] = 160;
                installedMods[80] = 0;

                possibleMods[81] = 1;
                if (veh.IsNeonActive) installedMods[81] = 1;
                else installedMods[81] = 0;

                possibleMods[82] = 0;
                installedMods[82] = veh.NeonColor.R;

                possibleMods[83] = 0;
                installedMods[83] = veh.NeonColor.G;

                possibleMods[84] = 0;
                installedMods[84] = veh.NeonColor.B;

                player.Emit("sendveharray", possibleMods, installedMods, NumberplateText);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
