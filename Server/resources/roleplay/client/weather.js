import * as alt from 'alt';
import * as game from 'natives';

let oldWeather = "none";

alt.onServer('Client:Weather:SetWeather', (newWeather) => {
    if (oldWeather == "none") {
        game.setWeatherTypeNowPersist(newWeather);
        oldWeather = newWeather;
        return;
    }

    let i = 0;
    let interval = alt.setInterval(() => {
        i++;
        if (i < 100) game.setWeatherTypeTransition(game.getHashKey(oldWeather), game.getHashKey(newWeather), (i / 100));
        else {
            alt.clearInterval(interval);
            oldWeather = newWeather;
        }
    }, 200)
});