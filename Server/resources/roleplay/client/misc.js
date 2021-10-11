import alt from 'alt-client';
import game from 'natives';

// Disable Idle Cam
alt.setInterval(() => {
    game.invalidateIdleCam(); 
    game.invalidateVehicleIdleCam();
}, 20000); 

// HideMap
/*alt.setInterval(() => {
    if (!alt.Player.local.vehicle) {
        game.displayRadar(false);
    } else {
        game.displayRadar(true);
    }
}, 1000);*/