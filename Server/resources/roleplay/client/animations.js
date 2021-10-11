import * as alt from 'alt';
import * as game from 'natives';

alt.on('keyup', (key) => {
    if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true || alt.Player.local.getSyncedMeta("IsUnconscious") == true || alt.Player.local.getMeta("IsCefOpen") == true) return;
    if (key == 96) { //Numpad0 
        game.clearPedTasks(alt.Player.local.scriptID);
    } else if (key == 97) { //Numpad1
        if (alt.LocalStorage.get('Num1Hotkey') == null || alt.LocalStorage.get('Num1AnimName') == null || alt.LocalStorage.get('Num1AnimDict') == null || alt.LocalStorage.get('Num1AnimFlag') == null || alt.LocalStorage.get('Num1AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num1AnimDict'), alt.LocalStorage.get('Num1AnimName'), alt.LocalStorage.get('Num1AnimDuration'), alt.LocalStorage.get('Num1AnimFlag'));
    } else if (key == 98) { //Numpad2
        if (alt.LocalStorage.get('Num2Hotkey') == null || alt.LocalStorage.get('Num2AnimName') == null || alt.LocalStorage.get('Num2AnimDict') == null || alt.LocalStorage.get('Num2AnimFlag') == null || alt.LocalStorage.get('Num2AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num2AnimDict'), alt.LocalStorage.get('Num2AnimName'), alt.LocalStorage.get('Num2AnimDuration'), alt.LocalStorage.get('Num2AnimFlag'));
    } else if (key == 99) { //Numpad3
        if (alt.LocalStorage.get('Num3Hotkey') == null || alt.LocalStorage.get('Num3AnimName') == null || alt.LocalStorage.get('Num3AnimDict') == null || alt.LocalStorage.get('Num3AnimFlag') == null || alt.LocalStorage.get('Num3AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num3AnimDict'), alt.LocalStorage.get('Num3AnimName'), alt.LocalStorage.get('Num3AnimDuration'), alt.LocalStorage.get('Num3AnimFlag'));
    } else if (key == 100) { //Numpad4
        if (alt.LocalStorage.get('Num4Hotkey') == null || alt.LocalStorage.get('Num4AnimName') == null || alt.LocalStorage.get('Num4AnimDict') == null || alt.LocalStorage.get('Num4AnimFlag') == null || alt.LocalStorage.get('Num4AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num4AnimDict'), alt.LocalStorage.get('Num4AnimName'), alt.LocalStorage.get('Num4AnimDuration'), alt.LocalStorage.get('Num4AnimFlag'));
    } else if (key == 101) { //Numpad5 
        if (alt.LocalStorage.get('Num5Hotkey') == null || alt.LocalStorage.get('Num5AnimName') == null || alt.LocalStorage.get('Num5AnimDict') == null || alt.LocalStorage.get('Num5AnimFlag') == null || alt.LocalStorage.get('Num5AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num5AnimDict'), alt.LocalStorage.get('Num5AnimName'), alt.LocalStorage.get('Num5AnimDuration'), alt.LocalStorage.get('Num5AnimFlag'));
    } else if (key == 102) { //Numpad6 
        if (alt.LocalStorage.get('Num6Hotkey') == null || alt.LocalStorage.get('Num6AnimName') == null || alt.LocalStorage.get('Num6AnimDict') == null || alt.LocalStorage.get('Num6AnimFlag') == null || alt.LocalStorage.get('Num6AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num6AnimDict'), alt.LocalStorage.get('Num6AnimName'), alt.LocalStorage.get('Num6AnimDuration'), alt.LocalStorage.get('Num6AnimFlag'));
    } else if (key == 103) { //Numpad7
        if (alt.LocalStorage.get('Num7Hotkey') == null || alt.LocalStorage.get('Num7AnimName') == null || alt.LocalStorage.get('Num7AnimDict') == null || alt.LocalStorage.get('Num7AnimFlag') == null || alt.LocalStorage.get('Num7AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num7AnimDict'), alt.LocalStorage.get('Num7AnimName'), alt.LocalStorage.get('Num7AnimDuration'), alt.LocalStorage.get('Num7AnimFlag'));
    } else if (key == 104) { //Numpad 8
        if (alt.LocalStorage.get('Num8Hotkey') == null || alt.LocalStorage.get('Num8AnimName') == null || alt.LocalStorage.get('Num8AnimDict') == null || alt.LocalStorage.get('Num8AnimFlag') == null || alt.LocalStorage.get('Num8AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num8AnimDict'), alt.LocalStorage.get('Num8AnimName'), alt.LocalStorage.get('Num8AnimDuration'), alt.LocalStorage.get('Num8AnimFlag'));
    } else if (key == 105) { //Numpad 9
        if (alt.LocalStorage.get('Num9Hotkey') == null || alt.LocalStorage.get('Num9AnimName') == null || alt.LocalStorage.get('Num9AnimDict') == null || alt.LocalStorage.get('Num9AnimFlag') == null || alt.LocalStorage.get('Num9AnimDuration') == null) return;
        game.clearPedTasks(alt.Player.local.scriptID);
        playAnimation(alt.LocalStorage.get('Num9AnimDict'), alt.LocalStorage.get('Num9AnimName'), alt.LocalStorage.get('Num9AnimDuration'), alt.LocalStorage.get('Num9AnimFlag'));
    }
});

function playAnimation(animDict, animName, duration, flag) {
    game.requestAnimDict(animDict);
    let interval = alt.setInterval(() => {
        if (game.hasAnimDictLoaded(animDict)) {
            alt.clearInterval(interval);
            game.taskPlayAnim(alt.Player.local.scriptID, animDict, animName, 8.0, 1, duration, flag, 1, false, false, false);
        }
    }, 0);
}