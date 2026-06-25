autowatch = 1;
outlets = 2;

var events = [];
var recording = false;
var recordStart = 0;
var playbackTasks = [];
var playbackOffsetMs = 35;
var minForwardGapMs = 75;
var minSameAddressGapMs = 110;
var lastForwardTime = -999999;
var lastAddressTimes = {};
var filteredHitCount = 0;

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

    outlet(1, "playback_started", events.length, "hits", "offset_ms", playbackOffsetMs);

    for (var i = 0; i < events.length; i++) {
        scheduleEvent(events[i].time + playbackOffsetMs, events[i].address);
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

function offset(value) {
    playbackOffsetMs = Number(value) || 0;
    outlet(1, "playback_offset_ms", playbackOffsetMs);
}

function gate(value) {
    minForwardGapMs = Math.max(0, Number(value) || 0);
    outlet(1, "min_forward_gap_ms", minForwardGapMs);
}

function samegate(value) {
    minSameAddressGapMs = Math.max(0, Number(value) || 0);
    outlet(1, "min_same_address_gap_ms", minSameAddressGapMs);
}

function anything() {
    var address = messagename;

    if (address.charAt(0) !== "/") {
        return;
    }

    if (!shouldForward(address)) {
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

function shouldForward(address) {
    var hitTime = now();
    var lastAddressTime = lastAddressTimes[address];

    if (hitTime - lastForwardTime < minForwardGapMs) {
        logFiltered(address, "global_gate");
        return false;
    }

    if (lastAddressTime !== undefined && hitTime - lastAddressTime < minSameAddressGapMs) {
        logFiltered(address, "same_hit_gate");
        return false;
    }

    lastForwardTime = hitTime;
    lastAddressTimes[address] = hitTime;
    return true;
}

function logFiltered(address, reason) {
    filteredHitCount++;

    if (filteredHitCount <= 8) {
        outlet(1, "filtered_hit", address, reason);
    } else if (filteredHitCount === 9) {
        outlet(1, "filtered_hit_logging_suppressed");
    }
}

function scheduleEvent(delay, address) {
    var task = new Task(function() {
        outlet(0, address);
    }, this);

    playbackTasks.push(task);
    task.schedule(Math.max(0, delay));
}
