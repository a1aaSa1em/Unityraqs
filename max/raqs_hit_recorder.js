autowatch = 1;
outlets = 2;

var events = [];
var recording = false;
var recordStart = 0;
var playbackTasks = [];

function now() {
    return new Date().getTime();
}

function record(value) {
    if (value) {
        events = [];
        recording = true;
        recordStart = now();
        outlet(1, "recording_started");
    } else {
        recording = false;
        outlet(1, "recording_stopped", events.length, "hits");
    }
}

function play() {
    stopplay();

    if (events.length === 0) {
        outlet(1, "nothing_to_play");
        return;
    }

    outlet(1, "playback_started", events.length, "hits");

    for (var i = 0; i < events.length; i++) {
        scheduleEvent(events[i].time, events[i].address);
    }
}

function stopplay() {
    for (var i = 0; i < playbackTasks.length; i++) {
        playbackTasks[i].cancel();
    }

    playbackTasks = [];
    outlet(1, "playback_stopped");
}

function clear() {
    stopplay();
    events = [];
    recording = false;
    outlet(1, "cleared");
}

function dump() {
    outlet(1, "events", events.length);

    for (var i = 0; i < events.length; i++) {
        outlet(1, i, events[i].time, events[i].address);
    }
}

function anything() {
    var address = messagename;

    if (address.charAt(0) !== "/") {
        return;
    }

    if (recording) {
        events.push({
            time: now() - recordStart,
            address: address
        });
    }

    outlet(0, address);
}

function scheduleEvent(delay, address) {
    var task = new Task(function() {
        outlet(0, address);
    }, this);

    playbackTasks.push(task);
    task.schedule(Math.max(0, delay));
}
