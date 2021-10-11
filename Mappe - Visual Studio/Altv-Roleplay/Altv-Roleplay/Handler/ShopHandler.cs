using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;

namespace Altv_Roleplay.Handler
{
    class ShopHandler : IScript
    {

        #region Shops
        [AsyncClientEvent("Server:Shop:buyItem")]
        public void buyShopItem(IPlayer player, int shopId, int amount, string itemname)
        {
            if (player == null || !player.Exists || shopId <= 0 || amount <= 0 || itemname == "") return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
            if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 3f)) { HUDHandler.SendNotification(player, 3, 5000, $"Du bist zu weit vom Shop entfernt."); return; }
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            if (ServerShops.GetShopNeededLicense(shopId) != "None" && !Characters.HasCharacterPermission(charId, ServerShops.GetShopNeededLicense(shopId))) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf."); return; }
            float itemWeight = ServerItems.GetItemWeight(itemname) * amount;
            float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
            float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
            int itemPrice = ServerShopsItems.GetShopItemPrice(shopId, itemname) * amount;
            int shopFaction = ServerShops.GetShopFaction(shopId);
            if (ServerShopsItems.GetShopItemAmount(shopId, itemname) < amount) { HUDHandler.SendNotification(player, 3, 5000, $"Soviele Gegenstände hat der Shop nicht auf Lager."); return; }
            if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast nicht genug Platz in deinen Taschen."); return; }

            if (invWeight + itemWeight <= 15f)
            {
                if (shopFaction > 0 && shopFaction != 0)
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charId)) { HUDHandler.SendNotification(player, 3, 2500, "Du hast hier keinen Zugriff drauf [CODE1-2]."); return; }
                    if (ServerFactions.GetCharacterFactionId(charId) != shopFaction) { HUDHandler.SendNotification(player, 3, 2500, $"Du hast hier keinen Zugriff drauf (Gefordert: {shopFaction} - Deine: {ServerFactions.GetCharacterFactionId(charId)}."); return; }
                    if (ServerFactions.GetFactionBankMoney(shopFaction) < itemPrice) { HUDHandler.SendNotification(player, 3, 2500, "Die Frakton hat nicht genügend Geld auf dem Fraktionskonto."); return; }
                    ServerFactions.SetFactionBankMoney(shopFaction, ServerFactions.GetFactionBankMoney(shopFaction) - itemPrice);
                    LoggingService.NewFactionLog(shopFaction, charId, 0, "shop", $"{Characters.GetCharacterName(charId)} hat {itemname} ({amount}x) für {itemPrice}$ erworben.");
                }
                else
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "inventory") < itemPrice)
                    {
                        HUDHandler.SendNotification(player, 3, 2500, "Du hast nicht genügend Geld dabei.");
                        return;
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", itemPrice, "inventory");
                }

                CharactersInventory.AddCharacterItem(charId, itemname, amount, "inventory");
                HUDHandler.SendNotification(player, 2, 5000, $"Du hast {itemname} ({amount}x) für {itemPrice} gekauft (Lagerort: Inventar).");
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - buyShopItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                return;
            }

            if (Characters.GetCharacterBackpack(charId) != -2 && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
            {
                if (shopFaction > 0 && shopFaction != 0)
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charId)) { HUDHandler.SendNotification(player, 3, 2500, "Du hast hier keinen Zugriff drauf [CODE1]."); return; }
                    if (ServerFactions.GetCharacterFactionId(charId) != shopFaction) { HUDHandler.SendNotification(player, 3, 2500, $"Du hast hier keinen Zugriff drauf (Gefordert: {shopFaction} - Deine: {ServerFactions.GetCharacterFactionId(charId)}."); return; }
                    if (ServerFactions.GetFactionBankMoney(shopFaction) < itemPrice) { HUDHandler.SendNotification(player, 3, 2500, "Die Frakton hat nicht genügend Geld auf dem Fraktionskonto."); return; }
                    ServerFactions.SetFactionBankMoney(shopFaction, ServerFactions.GetFactionBankMoney(shopFaction) - itemPrice);
                    LoggingService.NewFactionLog(shopFaction, charId, 0, "shop", $"{Characters.GetCharacterName(charId)} hat {itemname} ({amount}x) für {itemPrice}$ erworben.");
                }
                else
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "inventory") < itemPrice)
                    {
                        HUDHandler.SendNotification(player, 3, 2500, "Du hast nicht genügend Geld dabei.");
                        return;
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", itemPrice, "inventory");
                }

                CharactersInventory.AddCharacterItem(charId, itemname, amount, "backpack");
                HUDHandler.SendNotification(player, 2, 5000, $"Du hast {itemname} ({amount}x) für {itemPrice} gekauft (Lagerort: Rucksack / Tasche).");
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - buyShopItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
                return;
            }
        }

        internal static void openShop(IPlayer player, Server_Shops shopPos)
        {
            try
            {
                if (player == null || !player.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;

                if (shopPos.faction > 0 && shopPos.faction != 0)
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charId)) { HUDHandler.SendNotification(player, 3, 2500, "Kein Zugriff [1]"); return; }
                    if (ServerFactions.GetCharacterFactionId(charId) != shopPos.faction) { HUDHandler.SendNotification(player, 3, 2500, $"Kein Zugriff [{shopPos.faction} - {ServerFactions.GetCharacterFactionId(charId)}]"); return; }
                }

                if (shopPos.neededLicense != "None" && !Characters.HasCharacterPermission(charId, shopPos.neededLicense)) {
                    HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf.");
                    stopwatch.Stop();
                    if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - openShop benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }

                if (shopPos.isOnlySelling == false) {
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Shop:shopCEFCreateCEF", ServerShopsItems.GetShopShopItems(shopPos.shopId), shopPos.shopId, shopPos.isOnlySelling);
                    stopwatch.Stop();
                    if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - openShop benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }
                else if (shopPos.isOnlySelling == true) {
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Shop:shopCEFCreateCEF", ServerShopsItems.GetShopSellItems(charId, shopPos.shopId), shopPos.shopId, shopPos.isOnlySelling);
                    stopwatch.Stop();
                    if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - openShop benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    return;
                }
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - openShop benötigte {stopwatch.Elapsed.Milliseconds}ms");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Shop:robShop")]
        public async void robShop(ClassicPlayer player, int shopId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || shopId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 3f)) { HUDHandler.SendNotification(player, 3, 5000, "Du bist zu weit entfernt."); return; }
                if(player.isRobbingAShop)
                {
                    HUDHandler.SendNotification(player, 4, 2500, "Du raubst bereits einen Shop aus.");
                    return;
                }

                if(ServerShops.IsShopRobbedNow(shopId))
                {
                    HUDHandler.SendNotification(player, 3, 2500, "Dieser Shop wird bereits ausgeraubt.");
                    return;
                }

                if (ServerFactions.GetFactionDutyMemberCount(2) + ServerFactions.GetFactionDutyMemberCount(12) < 4)
                {
                    HUDHandler.SendNotification(player, 3, 2500, "Es sind weniger als 4 Polizisten im Staat.");
                    return;
                }

                ServerFactions.AddNewFactionDispatchNoName("Stiller Alarm", 2, "Ein aktiver Shopraub wurde gemeldet.", player.Position);
                ServerFactions.AddNewFactionDispatchNoName("Stiller Alarm", 12, "Ein aktiver Shopraub wurde gemeldet.", player.Position);

                foreach(var p in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.GetCharacterMetaId() > 0).ToList()) {
                    if (!ServerFactions.IsCharacterInAnyFaction((int)p.GetCharacterMetaId()) || !ServerFactions.IsCharacterInFactionDuty((int)p.GetCharacterMetaId()) || ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 2 && ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 12) continue;
                    HUDHandler.SendNotification(p, 1, 9500, "Ein stiller Alarm wurde ausgelöst.");
                }

                ServerShops.SetShopRobbedNow(shopId, true);
                player.isRobbingAShop = true;
                HUDHandler.SendNotification(player, 1, 2500, "Du raubst den Laden nun aus - warte 8 Minuten um das Geld zu erhalten.");
                await Task.Delay(480000);
                ServerShops.SetShopRobbedNow(shopId, false);
                if (player == null || !player.Exists) return;
                player.isRobbingAShop = false;
                if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 12f)) 
                { 
                    HUDHandler.SendNotification(player, 3, 5000, "Du bist zu weit entfernt, der Raub wurde abgebrochen."); 
                    return; 
                }

                int amount = new Random().Next(6000, 9000);
                HUDHandler.SendNotification(player, 2, 2500, $"Shop ausgeraubt - du erhälst {amount}$.");
                CharactersInventory.AddCharacterItem(player.CharacterId, "Bargeld", amount, "inventory");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }


        [AsyncClientEvent("Server:Shop:sellItem")]
        public void sellShopItem(IPlayer player, int shopId, int amount, string itemname)
        {
            if (player == null || !player.Exists || shopId <= 0 || amount <= 0 || itemname == "") return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
            if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 3f)) { HUDHandler.SendNotification(player, 3, 5000, "Du bist zu weit entfernt."); return; }
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            if(ServerShops.GetShopNeededLicense(shopId) != "None" && !Characters.HasCharacterPermission(charId, ServerShops.GetShopNeededLicense(shopId))) { HUDHandler.SendNotification(player, 3, 5000, "Du hast hier keinen Zugriff drauf."); return; }
            if(!CharactersInventory.ExistCharacterItem(charId, itemname, "inventory") && !CharactersInventory.ExistCharacterItem(charId, itemname, "backpack")) { HUDHandler.SendNotification(player, 3, 5000, "Diesen Gegenstand besitzt du nicht."); return; }
            int itemSellPrice = ServerShopsItems.GetShopItemPrice(shopId, itemname); //Verkaufpreis pro Item
            int invItemAmount = CharactersInventory.GetCharacterItemAmount(charId, itemname, "inventory"); //Anzahl an Items im Inventar
            int backpackItemAmount = CharactersInventory.GetCharacterItemAmount(charId, itemname, "backpack"); //Anzahl an Items im Rucksack
            if(invItemAmount + backpackItemAmount < amount) { HUDHandler.SendNotification(player, 3, 5000, "Soviele Gegenstände hast du nicht zum Verkauf dabei."); return; }


            var removeFromInventory = Math.Min(amount, invItemAmount);
            if (removeFromInventory > 0)
            {
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, removeFromInventory, "inventory");
            }

            var itemsLeft = amount - removeFromInventory;
            if (itemsLeft > 0)
            {
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemsLeft, "backpack");
            }

            HUDHandler.SendNotification(player, 2, 5000, $"Du hast {amount}x {itemname} für {itemSellPrice * amount}$ verkauft.");
            CharactersInventory.AddCharacterItem(charId, "Bargeld", amount * itemSellPrice, "inventory");
            stopwatch.Stop();
            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - sellShopItem benötigte {stopwatch.Elapsed.Milliseconds}ms");
        }
        #endregion

        #region VehicleShop

        internal static void OpenVehicleShop(IPlayer player, string shopname, int shopId)
        {
            if (player == null || !player.Exists || shopId <= 0) return;
            var array = ServerVehicleShops.GetVehicleShopItems(shopId);
            player.EmitLocked("Client:VehicleShop:OpenCEF", shopId, shopname, array);
        }

        [AsyncClientEvent("Server:VehicleShop:BuyVehicle")]
        public void BuyVehicle(IPlayer player, int shopid, string hash)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player == null || !player.Exists || shopid <= 0 || hash == "") return;
                long fHash = Convert.ToInt64(hash);
                int charId = User.GetPlayerOnline(player);
                if (charId == 0 || fHash == 0) return;
                int Price = ServerVehicleShops.GetVehicleShopPrice(shopid, fHash);
                bool PlaceFree = true;
                Position ParkOut = ServerVehicleShops.GetVehicleShopOutPosition(shopid);
                Rotation RotOut = ServerVehicleShops.GetVehicleShopOutRotation(shopid);
                foreach (IVehicle veh in Alt.GetAllVehicles().ToList()) { if (veh.Position.IsInRange(ParkOut, 2f)) { PlaceFree = false; break; } }
                if (!PlaceFree) { HUDHandler.SendNotification(player, 3, 5000, $"Der Ausladepunkt ist belegt."); return; }
                int rnd = new Random().Next(100000, 999999);
                if (ServerVehicles.ExistServerVehiclePlate($"NL{rnd}")) { BuyVehicle(player, shopid, hash); return; }
                if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "inventory") < Price) { HUDHandler.SendNotification(player, 4, 5000, $"Du hast nicht genügend Bargeld dabei ({Price}$)."); return; }
                CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", Price, "inventory");
                if (shopid == 6 )
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 2, false, 8, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                } else if (shopid == 7)
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 2, false, 9, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                } else if (shopid == 8)
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 3, false, 16, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                } else if (shopid == 9)
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 3, false, 21, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                }
                else if (shopid == 10)
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 4, false, 17, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                }
                else
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 0, 0, false, 1, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255);
                }
                CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel NL{rnd}", 2, "inventory");
                HUDHandler.SendNotification(player, 2, 5000, $"Fahrzeug erfolgreich gekauft.");
                if (!CharactersTablet.HasCharacterTutorialEntryFinished(charId, "buyVehicle"))
                {
                    CharactersTablet.SetCharacterTutorialEntryState(charId, "buyVehicle", true);
                    HUDHandler.SendNotification(player, 1, 2500, "Erfolg freigeschaltet: Mobilität");
                }
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - BuyVehicle benötigte {stopwatch.Elapsed.Milliseconds}ms");
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Clothes Shop
        public static void openClothesShop(ClassicPlayer player, int id)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerClothesShops.ExistClothesShop(id)) return;

                if (!player.HasData("clothesMenuOpen")) player.SetData("clothesMenuOpen", true);
                else
                {
                    Characters.SetCharacterCorrectClothes(player);
                    player.DeleteData("clothesMenuOpen");
                }

                player.EmitLocked("Client:Clothesstore:OpenMenu", Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId)) + 1);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Clothesstore:BuyCloth")]
        public void buyClothesShopItem(ClassicPlayer player, int clothId, bool isProp)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerClothes.ExistClothes(clothId, Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId)))) return;

                Characters.SwitchCharacterClothes(player, clothId, isProp);

                if (CharactersClothes.ExistCharacterClothes(player.CharacterId, clothId)) HUDHandler.SendNotification(player, 2, 1500, $"Du hast das Kleidungsstück angezogen.");
                else
                {
                    int price = ServerClothesShops.GetClothesPrice(player, clothId, isProp);
                    if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "inventory") < price) HUDHandler.SendNotification(player, 2, 1500, $"Du hast nicht genug Geld, um dieses Kleidungsstück zu kaufen. (${price})"); ;
                    CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", price, "inventory");
                    HUDHandler.SendNotification(player, 2, 1500, $"Du hast dir das Kleidungsstück für ${price} gekauft.");
                    CharactersClothes.CreateCharacterOwnedClothes(player.CharacterId, clothId);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Clothesstore:SetPerfectTorso")]
        public void ClothesshopSetPerfectTorso(ClassicPlayer player, int BestTorsoDrawable, int BestTorsoTexture)
        {
            try
            {
                int clothId = ServerClothes.GetClothesId(3, BestTorsoDrawable, BestTorsoTexture, Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId)));
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerClothes.ExistClothes(clothId, Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId)))) return;

                Characters.SwitchCharacterClothes(player, clothId, false);
                CharactersClothes.CreateCharacterOwnedClothes(player.CharacterId, clothId);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Tattoo Shop
        internal static void openTattooShop(ClassicPlayer player, Server_Tattoo_Shops tattooShop)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0 || tattooShop == null) return;
            int gender = Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId));
            player.Emit("Client:TattooShop:openShop", gender, tattooShop.id, CharactersTattoos.GetAccountOwnTattoos(player.CharacterId));
        }

        [AsyncClientEvent("Server:TattooShop:buyTattoo")]
        public void ClientEvent_buyTattoo(ClassicPlayer player, int shopId, int tattooId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || shopId <= 0 || tattooId <= 0 || !ServerTattoos.ExistTattoo(tattooId) || CharactersTattoos.ExistAccountTattoo(player.CharacterId, tattooId) || !ServerTattooShops.ExistTattooShop(shopId)) return;
                int price = ServerTattoos.GetTattooPrice(tattooId);
                if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "inventory") < price)
                {
                    HUDHandler.SendNotification(player, 4, 5000, $"Fehler: Du hast nicht genügend Geld dabei ({price}$).");
                    return;
                }
                CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", price, "inventory");
                ServerTattooShops.SetTattooShopBankMoney(shopId, ServerTattooShops.GetTattooShopBank(shopId) + price);
                CharactersTattoos.CreateNewEntry(player.CharacterId, tattooId);
                HUDHandler.SendNotification(player, 2, 1500, $"Du hast das Tattoo '{ServerTattoos.GetTattooName(tattooId)}' für {price}$ gekauft.");
                player.updateTattoos();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:TattooShop:deleteTattoo")]
        public void ClientEvent_deleteTattoo(ClassicPlayer player, int tattooId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || tattooId <= 0 || !CharactersTattoos.ExistAccountTattoo(player.CharacterId, tattooId)) return;
                CharactersTattoos.RemoveAccountTattoo(player.CharacterId, tattooId);
                HUDHandler.SendNotification(player, 2, 1500, $"Du hast das Tattoo '{ServerTattoos.GetTattooName(tattooId)}' erfolgreich entfernen lassen.");
                player.updateTattoos();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
        #endregion
    }
}
