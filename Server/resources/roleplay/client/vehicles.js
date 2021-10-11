import * as alt from 'alt';
import * as game from 'natives';

let lastInteract = 0;

alt.onServer("Client:Vehicles:ToggleDoorState", (veh, doorid, state) => {
    toggleDoor(veh, parseInt(doorid), state);
});

alt.on("gameEntityCreate", (entity) => {
    if (entity instanceof alt.Vehicle) {
        if (!entity.hasStreamSyncedMeta("IsVehicleCardealer")) return;
        if (entity.getStreamSyncedMeta("IsVehicleCardealer") == true) {
            game.freezeEntityPosition(entity.scriptID, true);
            game.setEntityInvincible(entity.scriptID, true);
        }
    }
});

function toggleDoor(vehicle, doorid, state) {
    if (state) {
        game.setVehicleDoorOpen(vehicle.scriptID, doorid, false, false);
    } else {
        game.setVehicleDoorShut(vehicle.scriptID, doorid, false);
    }
}

/* SIRENS */

let dict = {}

alt.on("enteredVehicle", (vehicle, seat) => {
    if (game.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18) game.setVehicleRadioEnabled(vehicle.scriptID, false);
});

alt.on("gameEntityCreate", (entity) => {
    if (entity instanceof alt.Vehicle && dict[entity.id] != undefined) game.setVehicleHasMutedSirens(alt.Vehicle.getByID(entity.id).scriptID, dict[entity.id]);
});

alt.everyTick(() => {
    if(!alt.Player.local.vehicle || game.getVehicleClass(alt.Player.local.vehicle.scriptID) != 18) return;
    game.disableControlAction(1, 86, true);
});

alt.on('keydown', (key) => {
    if (key === "Q".charCodeAt(0) && alt.Player.local.vehicle && alt.Player.local.scriptID == game.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, false) && game.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18) {
        if(lastInteract + 500 > Date.now()) return;
        lastInteract = Date.now();

        if(game.isVehicleSirenOn(alt.Player.local.vehicle.scriptID)){
            game.setVehicleHasMutedSirens(alt.Player.local.vehicle.scriptID, false);
            alt.emitServer("Server:Sirens:ForwardSirenMute", alt.Player.local.vehicle.id, false);
            game.setVehicleSiren(alt.Player.local.vehicle.scriptID, false);
        }else{
            game.setVehicleHasMutedSirens(alt.Player.local.vehicle.scriptID, true);
            alt.emitServer("Server:Sirens:ForwardSirenMute", alt.Player.local.vehicle.id, true);
            game.setVehicleSiren(alt.Player.local.vehicle.scriptID, true);
        }
    } else if (key === 18 && alt.Player.local.vehicle && alt.Player.local.scriptID == game.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, false) && game.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18) {
        if(lastInteract + 500 > Date.now()) return;
        lastInteract = Date.now();
        
        if(game.isVehicleSirenOn(alt.Player.local.vehicle.scriptID) && !game.isVehicleSirenAudioOn(alt.Player.local.vehicle.scriptID)) alt.emitServer("Server:Sirens:ForwardSirenMute", alt.Player.local.vehicle.id, false);
        else if(game.isVehicleSirenOn(alt.Player.local.vehicle.scriptID) && game.isVehicleSirenAudioOn(alt.Player.local.vehicle.scriptID)) alt.emitServer("Server:Sirens:ForwardSirenMute", alt.Player.local.vehicle.id, true);
    }
});

alt.onServer("Client:Sirens:setVehicleHasMutedSirensForAll", (vehId, state) => {
    dict[vehId] = state;
    game.setVehicleHasMutedSirens(alt.Vehicle.getByID(vehId).scriptID, state);
});