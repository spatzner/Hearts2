namespace Hearts;

internal delegate void RoundCompletedEventHandler(object sender, EventArgs args);

internal delegate void GameCompletedEventHandler(object sender, EventArgs args);

internal delegate void ActionRequestedEventHandler(object source, ActionRequestArgs args);