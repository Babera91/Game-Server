import * as alt from 'alt';
import * as game from 'natives';

let weaponHashes = ["0x92A27487", "0x958A4A8F", "0xCD274149", "0xD8DF3C3C", "0xF9E6AA4B", "0x84BD7BFD", "0xA2719263", "0x8BB05FD7", "0x440E4788", "0x4E875F73", "0xF9DCBF2D", "0x99B507EA", "0xDD5DF8D9", "0x678B81B1", "0x19044EE0", "0x94117305", "0x3813FC08", "0xDFE37640", "0x22D8FE39", "0x2B5EF5EC", "0x5EF9FEC4", "0x97EA20B8", "0x47757124", "0x57A4368C", "0xD205520E", "0xC1B3C3D1", "0xCB96392F", "0xDC4DB296", "0x917F6C8C", "0x1B06D571", "0x99AEEB3B", "0xBFE256D4", "0xBFD21232", "0x88374054", "0x3656C8C1", "0xAF3696A1", "0x83839C4", "0xEFE7E2DF", "0xA3D4D34", "0xDB1AA450", "0x13532244", "0xBD248B55", "0x2BE6766B", "0x78A97CD0", "0x476BF155", "0xE284C527", "0x9D61E50F", "0x5A96BA4", "0xEF951FBB", "0x3AABBBAA", "0xA89CB99E", "0x1D073A89", "0x555AF99A", "0x7846A318", "0x12E82D3D", "0xAF113F99", "0xBFEFFF6D", "0x394F415C", "0x7F229F94", "0x84D6FAFD", "0x83BF0278", "0xFAD1F1C9", "0x624FE830", "0x9D1F17E6", "0xC0A3098D", "0x969C3D67", "0x7FD62962", "0xDBBD7280", "0x61012683", "0x9D07F764", "0xC472FE2", "0xA914799", "0xC734385A", "0x6A6C02E0", "0x5FC3C11", "0x781FE4A", "0x7F7497E5", "0xA284510B", "0x4DD2DC56", "0x63AB0442", "0x42BF8A85", "0x6D544C99", "0xB1CA77B1", "0xB62D1F67", "0x23C9F95C", "0xA0973D5E", "0x497FACC3", "0x93E220BD", "0x24B17070", "0xBA45E8B8", "0xAB564B93", "0xFDBC8A50", "0x787F0BB", "0x2C3731D9", "0x60EC506", "0xBA536372 ", "0xBFA49206", "0xFBAB5776"]
let currentAmmo = [];
let currentWeaponTypes = [];

alt.onServer("Client:Weapon:SetWeaponAmmo", (hash, newAmmo) => {
    if (game.getAmmoInPedWeapon(alt.Player.local, parseInt(hash)) != parseInt(newAmmo)) game.setPedAmmo(alt.Player.local, hash, newAmmo, false)
});

alt.onServer("Client:Weapon:GetWeaponAmmo", (hash, name) => {
    let ammo = game.getAmmoInPedWeapon(alt.Player.local, hash);

    alt.emitServer("Server:Weapon:SendWeaponAmmo", name, ammo);
});

alt.onServer("Client:HUD:CreateCEF",() => {
    alt.setTimeout(() => {
        weaponHashes.forEach(hash => {
            let ammo = game.getAmmoInPedWeapon(alt.Player.local, parseInt(hash));
            if (ammo <= 0) return;
            currentAmmo[weaponHashes.indexOf(hash)] = ammo;
        });

        alt.setInterval(() => {
            weaponHashes.forEach(hash => {    
                if (!game.hasPedGotWeapon(alt.Player.local, parseInt(hash), false)) return;
                let ammo = game.getAmmoInPedWeapon(alt.Player.local, parseInt(hash));
                let weaponType = game.getPedAmmoTypeFromWeapon(alt.Player.local, parseInt(hash));
                if (currentAmmo[weaponHashes.indexOf(hash)] == ammo || ammo < 0) return;
                currentAmmo[weaponHashes.indexOf(hash)] = ammo;
                currentWeaponTypes.push(weaponType);
                alt.emitServer("Server:Weapon:UpdateAmmo", parseInt(hash), ammo);
            });
    
            currentWeaponTypes = [];
        }, 45000);
    }, 5000);
});