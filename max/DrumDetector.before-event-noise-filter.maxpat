{
    "patcher": {
        "fileversion": 1,
        "appversion": {
            "major": 9,
            "minor": 1,
            "revision": 4,
            "architecture": "x64",
            "modernui": 1
        },
        "classnamespace": "box",
        "rect": [ 89.0, 103.0, 1115.0, 663.0 ],
        "boxes": [
            {
                "box": {
                    "id": "obj-44",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 121.0, 518.0, 42.0, 22.0 ],
                    "text": "/doum"
                }
            },
            {
                "box": {
                    "id": "obj-42",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 91.0, 553.0, 29.5, 22.0 ],
                    "text": "/tek"
                }
            },
            {
                "box": {
                    "id": "obj-40",
                    "maxclass": "newobj",
                    "numinlets": 3,
                    "numoutlets": 3,
                    "outlettype": [ "bang", "bang", "" ],
                    "patching_rect": [ 190.0, 554.0, 44.0, 22.0 ],
                    "text": "sel 1 0"
                }
            },
            {
                "box": {
                    "id": "obj-39",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 159.5, 584.0, 193.0, 22.0 ],
                    "text": "expr (($f1 > 2800.) || ($f2 > 9000.))"
                }
            },
            {
                "box": {
                    "id": "obj-38",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "float" ],
                    "patching_rect": [ 365.0, 452.0, 29.5, 22.0 ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-31",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "float" ],
                    "patching_rect": [ 596.0, 433.0, 29.5, 22.0 ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-29",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 529.0, 454.0, 64.0, 22.0 ],
                    "text": "print rolloff"
                }
            },
            {
                "box": {
                    "id": "obj-27",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 336.10526037216187, 489.0, 72.0, 22.0 ],
                    "text": "print spread"
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-22",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 385.0, 381.0, 50.0, 22.0 ]
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-16",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 276.0, 452.0, 50.0, 22.0 ]
                }
            },
            {
                "box": {
                    "id": "obj-12",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 384.0, 569.0, 85.0, 22.0 ],
                    "text": "print classified"
                }
            },
            {
                "box": {
                    "id": "obj-13",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "float" ],
                    "patching_rect": [ 177.0, 483.0, 29.5, 22.0 ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-10",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 581.0, 496.0, 78.0, 22.0 ],
                    "text": "print centroid"
                }
            },
            {
                "box": {
                    "id": "obj-9",
                    "maxclass": "meter~",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "float" ],
                    "patching_rect": [ 692.0000206232071, 426.6666793823242, 392.0000116825104, 404.00001204013824 ]
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-37",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 304.38596200942993, 380.70175075531006, 50.0, 22.0 ]
                }
            },
            {
                "box": {
                    "id": "obj-35",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "signal" ],
                    "patching_rect": [ 208.77192783355713, 342.9824528694153, 39.0, 22.0 ],
                    "text": "gate~"
                }
            },
            {
                "box": {
                    "id": "obj-34",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 7,
                    "outlettype": [ "float", "float", "float", "float", "float", "float", "float" ],
                    "patching_rect": [ 258.77192735671997, 292.10526037216187, 141.0, 22.0 ],
                    "text": "unpack 0. 0. 0. 0. 0. 0. 0."
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-33",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 231.0, 518.0, 50.0, 22.0 ]
                }
            },
            {
                "box": {
                    "id": "obj-30",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "" ],
                    "patching_rect": [ 292.10526037216187, 245.61403274536133, 116.0, 22.0 ],
                    "text": "fluid.spectralshape~"
                }
            },
            {
                "box": {
                    "id": "obj-8",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 439.0, 458.0, 54.0, 22.0 ],
                    "text": "print osc"
                }
            },
            {
                "box": {
                    "id": "obj-7",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 450.0, 394.0, 138.0, 22.0 ],
                    "text": "udpsend 127.0.0.1 7000"
                }
            },
            {
                "box": {
                    "id": "obj-6",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 450.0, 368.0, 89.0, 22.0 ],
                    "text": "prepend /doum"
                }
            },
            {
                "box": {
                    "id": "obj-5",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "patching_rect": [ 424.0, 280.0, 22.0, 22.0 ],
                    "text": "t b"
                }
            },
            {
                "box": {
                    "id": "obj-4",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "bang", "bang" ],
                    "patching_rect": [ 596.0, 172.0, 42.0, 22.0 ],
                    "text": "edge~"
                }
            },
            {
                "box": {
                    "id": "obj-3",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 670.0, 280.0, 55.0, 22.0 ],
                    "text": "print HIT"
                }
            },
            {
                "box": {
                    "id": "obj-2",
                    "linecount": 2,
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "signal", "" ],
                    "patching_rect": [ 538.0, 322.0, 344.0, 35.0 ],
                    "text": "fluid.ampgate~ @rampup 5 @rampdown 60 @onthreshold -18 @offthreshold -28 @minslicelength 6615"
                }
            },
            {
                "box": {
                    "id": "obj-1",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "signal", "signal" ],
                    "patching_rect": [ 478.0, 195.0, 35.0, 22.0 ],
                    "text": "adc~"
                }
            },
            {
                "box": {
                    "id": "raqs-comment-1",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 910.0, 70.0, 420.0, 20.0 ],
                    "text": "Raqs recorder: Max records audio + hit timing, then replays both to Unity"
                }
            },
            {
                "box": {
                    "id": "raqs-button-record-start",
                    "maxclass": "button",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 910.0, 115.0, 24.0, 24.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-record-start",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 940.0, 116.0, 120.0, 20.0 ],
                    "text": "START RECORD"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-record-start",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1070.0, 105.0, 62.0, 22.0 ],
                    "text": "record 1"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-record-start",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1145.0, 105.0, 252.0, 22.0 ],
                    "text": "open /Users/alaasalem/raqs_recording.wav, 1"
                }
            },
            {
                "box": {
                    "id": "raqs-button-record-stop",
                    "maxclass": "button",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 910.0, 155.0, 24.0, 24.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-record-stop",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 940.0, 156.0, 120.0, 20.0 ],
                    "text": "STOP RECORD"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-record-stop",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1070.0, 145.0, 62.0, 22.0 ],
                    "text": "record 0"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-record-stop",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1145.0, 145.0, 30.0, 22.0 ],
                    "text": "0"
                }
            },
            {
                "box": {
                    "id": "raqs-button-play",
                    "maxclass": "button",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 910.0, 205.0, 24.0, 24.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-play",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 940.0, 206.0, 120.0, 20.0 ],
                    "text": "PLAY TAKE"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-play",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1070.0, 195.0, 45.0, 22.0 ],
                    "text": "play"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-play",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1145.0, 195.0, 252.0, 22.0 ],
                    "text": "open /Users/alaasalem/raqs_recording.wav, 1"
                }
            },
            {
                "box": {
                    "id": "raqs-button-play-stop",
                    "maxclass": "button",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 910.0, 245.0, 24.0, 24.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-play-stop",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 940.0, 246.0, 125.0, 20.0 ],
                    "text": "STOP PLAYBACK"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-play-stop",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1070.0, 235.0, 62.0, 22.0 ],
                    "text": "stopplay"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-play-stop",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1145.0, 235.0, 30.0, 22.0 ],
                    "text": "0"
                }
            },
            {
                "box": {
                    "id": "raqs-button-clear",
                    "maxclass": "button",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 910.0, 295.0, 24.0, 24.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-clear",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 940.0, 296.0, 130.0, 20.0 ],
                    "text": "CLEAR HIT TAKE"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-clear",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1070.0, 285.0, 45.0, 22.0 ],
                    "text": "clear"
                }
            },
            {
                "box": {
                    "id": "raqs-loadmess-play-offset",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 910.0, 340.0, 77.0, 22.0 ],
                    "text": "loadmess 35"
                }
            },
            {
                "box": {
                    "id": "raqs-play-offset-number",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "bang" ],
                    "parameter_enable": 0,
                    "patching_rect": [ 995.0, 340.0, 58.0, 22.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-play-offset",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 1060.0, 341.0, 150.0, 20.0 ],
                    "text": "PLAY OFFSET MS"
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-play-offset",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [ "" ],
                    "patching_rect": [ 1220.0, 340.0, 61.0, 22.0 ],
                    "text": "offset $1"
                }
            },
            {
                "box": {
                    "id": "raqs-js-recorder",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [ "", "" ],
                    "patching_rect": [ 910.0, 385.0, 135.0, 22.0 ],
                    "saved_object_attributes": {
                        "filename": "raqs_hit_recorder.js",
                        "parameter_enable": 0
                    },
                    "text": "js raqs_hit_recorder.js"
                }
            },
            {
                "box": {
                    "id": "raqs-print-recorder",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 1070.0, 385.0, 112.0, 22.0 ],
                    "text": "print raqs-recorder"
                }
            },
            {
                "box": {
                    "id": "raqs-sfrecord",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [ "signal" ],
                    "patching_rect": [ 1145.0, 320.0, 75.0, 22.0 ],
                    "text": "sfrecord~ 1"
                }
            },
            {
                "box": {
                    "id": "raqs-sfplay",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 2,
                    "outlettype": [ "signal", "bang" ],
                    "patching_rect": [ 1145.0, 360.0, 62.0, 22.0 ],
                    "text": "sfplay~ 1"
                }
            },
            {
                "box": {
                    "id": "raqs-ezdac",
                    "maxclass": "ezdac~",
                    "numinlets": 2,
                    "numoutlets": 0,
                    "patching_rect": [ 1145.0, 410.0, 45.0, 45.0 ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-dac",
                    "maxclass": "comment",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [ 1205.0, 410.0, 181.0, 20.0 ],
                    "text": "click speaker if playback is silent"
                }
            }
        ],
        "lines": [
            {
                "patchline": {
                    "destination": [ "obj-2", 0 ],
                    "order": 2,
                    "source": [ "obj-1", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-30", 0 ],
                    "order": 3,
                    "source": [ "obj-1", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-9", 0 ],
                    "order": 1,
                    "source": [ "obj-1", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-sfrecord", 0 ],
                    "order": 0,
                    "source": [ "obj-1", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-33", 0 ],
                    "order": 0,
                    "source": [ "obj-13", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-39", 0 ],
                    "order": 1,
                    "source": [ "obj-13", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-38", 1 ],
                    "source": [ "obj-16", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-4", 0 ],
                    "source": [ "obj-2", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-31", 1 ],
                    "source": [ "obj-22", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-34", 0 ],
                    "source": [ "obj-30", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-29", 0 ],
                    "order": 0,
                    "source": [ "obj-31", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-39", 1 ],
                    "order": 1,
                    "source": [ "obj-31", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-10", 0 ],
                    "source": [ "obj-33", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-13", 1 ],
                    "order": 2,
                    "source": [ "obj-34", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-16", 0 ],
                    "order": 0,
                    "source": [ "obj-34", 1 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-22", 0 ],
                    "order": 0,
                    "source": [ "obj-34", 4 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-35", 0 ],
                    "order": 1,
                    "source": [ "obj-34", 4 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-35", 0 ],
                    "order": 1,
                    "source": [ "obj-34", 1 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-35", 0 ],
                    "order": 1,
                    "source": [ "obj-34", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-37", 0 ],
                    "order": 0,
                    "source": [ "obj-34", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-27", 0 ],
                    "source": [ "obj-38", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-40", 0 ],
                    "source": [ "obj-39", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-3", 0 ],
                    "order": 0,
                    "source": [ "obj-4", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-5", 0 ],
                    "order": 1,
                    "source": [ "obj-4", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-42", 0 ],
                    "source": [ "obj-40", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-44", 0 ],
                    "source": [ "obj-40", 1 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "obj-42", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "obj-44", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-13", 0 ],
                    "order": 2,
                    "source": [ "obj-5", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-31", 0 ],
                    "order": 0,
                    "source": [ "obj-5", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-38", 0 ],
                    "order": 1,
                    "source": [ "obj-5", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-8", 0 ],
                    "source": [ "obj-6", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-clear", 0 ],
                    "source": [ "raqs-button-clear", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-play-offset-number", 0 ],
                    "source": [ "raqs-loadmess-play-offset", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-play-offset", 0 ],
                    "source": [ "raqs-play-offset-number", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-play-offset", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-audio-play", 0 ],
                    "order": 0,
                    "source": [ "raqs-button-play", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-play", 0 ],
                    "order": 1,
                    "source": [ "raqs-button-play", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-audio-play-stop", 0 ],
                    "order": 0,
                    "source": [ "raqs-button-play-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-play-stop", 0 ],
                    "order": 1,
                    "source": [ "raqs-button-play-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-audio-record-start", 0 ],
                    "order": 0,
                    "source": [ "raqs-button-record-start", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-record-start", 0 ],
                    "order": 1,
                    "source": [ "raqs-button-record-start", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-audio-record-stop", 0 ],
                    "order": 0,
                    "source": [ "raqs-button-record-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-msg-js-record-stop", 0 ],
                    "order": 1,
                    "source": [ "raqs-button-record-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "obj-7", 0 ],
                    "source": [ "raqs-js-recorder", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-print-recorder", 0 ],
                    "source": [ "raqs-js-recorder", 1 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-sfplay", 0 ],
                    "source": [ "raqs-msg-audio-play", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-sfplay", 0 ],
                    "source": [ "raqs-msg-audio-play-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-sfrecord", 0 ],
                    "source": [ "raqs-msg-audio-record-start", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-sfrecord", 0 ],
                    "source": [ "raqs-msg-audio-record-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-clear", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-play", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-play-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-record-start", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-js-recorder", 0 ],
                    "source": [ "raqs-msg-js-record-stop", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-ezdac", 1 ],
                    "order": 0,
                    "source": [ "raqs-sfplay", 0 ]
                }
            },
            {
                "patchline": {
                    "destination": [ "raqs-ezdac", 0 ],
                    "order": 1,
                    "source": [ "raqs-sfplay", 0 ]
                }
            }
        ],
        "autosave": 0
    }
}
