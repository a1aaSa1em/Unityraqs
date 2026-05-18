# Raqs Max-to-Unity Dance Pipeline

## What was going wrong

The old controller reacted to every drum hit immediately:

1. Max sent an OSC stroke.
2. Unity picked a random animation state from that stroke bucket.
3. Lola played about 0.65 seconds.
4. Lola returned to idle.

That makes the character look twitchy because dance phrases keep interrupting each other. Belly dance especially needs continuity: a hip drop, sway, roll, or undulation should live inside a 2-4 second phrase instead of firing as a disconnected pose.

## New Unity workflow

`Assets/Scripts/DrumDanceController.cs` now has a `BufferedPhrases` mode.

Recommended values:

- `Mode`: `BufferedPhrases`
- `PhraseBufferSeconds`: `2.5`
- `PhrasePlaySeconds`: `2.5`
- `PhraseCrossfade`: `0.25` to `0.35`
- `HoldPhraseUntilNextPhrase`: enabled

This collects a short rhythm window from Max, finds the dominant stroke, then chooses one curated phrase. The movement should feel more like a dance sentence and less like random clips.

## Recording a drum phrase

While Unity is in Play Mode:

- Press `R` to start recording incoming Max drum hits.
- Play the drum phrase in Max.
- Press `R` again to stop recording.
- Press `P` to replay the recorded phrase.
- Press `P` again to stop playback.
- Press `C` to clear the recording.

During playback, live Max input is ignored by default so the motion follows the recorded pattern exactly. The recording only lives during the current Unity Play session.

## Animator setup

1. Select `Assets/Animations/LolaAnimator.controller`.
2. Run `Tools > Raqs > Add Belly FBX States To Selected Controller`.
3. Select the object in the scene that has `DrumDanceController`.
4. In the component menu, run `Fill Suggested Belly Phrase Pools`.
5. Remove any states that do not actually look like belly dance on Lola.

The phrase pool names must match Animator state names exactly. `Tools > Raqs > Add Belly FBX States To Selected Controller` adds states named after the FBX files, such as `belly_continuous`, because many Mixamo clips import internally as generic names like `Scene`.

## Max setup

Unity listens for:

- `/doum`
- `/tek`
- `/ka`
- `/trillo`

Right now the Max patch appears to send `/doum` only. That means Unity receives everything as the same kind of hit, so the dance cannot respond differently to different drums yet.

If the Unity Console says every phrase is `Tek`, Max is sending mostly `/tek`. Fix that in Max before tuning Unity too much.

The Max side should classify the hit first, then send one of the four OSC addresses. For example:

- Low/heavy hit -> `/doum`
- Bright/high hit -> `/tek`
- Secondary sharp hit -> `/ka`
- Fast repeated roll -> `/trillo`

Avoid sending a new OSC message every audio frame. Send one message per detected hit, then let the Unity phrase buffer organize those hits.

## Curation rule

Keep each phrase pool stylistically narrow:

- Use `belly_*`, `accent_belly_*`, and `hip_drops_double` first.
- Avoid mixing hip-hop, samba, chicken dance, walking, and belly clips in the same pool.
- If a clip looks useful but too energetic, put it in a low-weight phrase or remove it.

The cleanest result will come from fewer good clips, not more random clips.

## Bailando comparison scene

Use `Tools > Raqs > Create Bailando Comparison Scene` to create
`Assets/Scenes/BailandoCompare.unity`.

The left Elmo uses the existing Max OSC + Mixamo/FBX Animator workflow.
The right Elmo uses `Assets/Scripts/BailandoPosePlayer.cs` and reads generated
Bailando poses from:

`Assets/StreamingAssets/Bailando/latest_pose.json`

The Bailando side is seeded from `export_unity_pose_json.py`, and future runs of
`C:\Users\23028727\gvhmr_workspace\Bailando\live_perf.py` now write that Unity
JSON automatically after model inference. While Unity is in Play Mode, press `B`
to reload the latest JSON without rebuilding the scene.

Unity does not listen to the microphone directly in this comparison scene. The
Max/Mixamo side listens for OSC drum messages on UDP port `7000`. If Max is not
sending `/doum`, `/tek`, `/ka`, or `/trillo`, the scene uses a small built-in
test pattern so the Mixamo side can still be checked. Press `1`, `2`, `3`, or
`4` in Play Mode to manually trigger those test strokes.
