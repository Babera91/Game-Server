import * as alt from 'alt'
import * as native from 'natives'
let timeKeyPressed = 0
let vehicle
let interval

alt.on('keydown', (key) => {
    if (key === 'F'.charCodeAt(0)) {
        if (alt.Player.local.vehicle) { // If the player is in a vehicle
            let vehicle = alt.Player.local.vehicle;
            let enginestate = native.getIsVehicleEngineRunning(vehicle);

            if (!enginestate) return;

            let interval = alt.setInterval(() => {
                if (!alt.Player.local.vehicle) {
                    alt.clearInterval(interval);
                    native.setVehicleEngineOn(vehicle, true, true, false);
                }
            }, 10);
        }
    }
})