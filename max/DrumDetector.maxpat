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
        "rect": [
            214,
            117,
            935,
            663
        ],
        "boxes": [
            {
                "box": {
                    "id": "obj-44",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        ""
                    ],
                    "patching_rect": [
                        121,
                        518,
                        42,
                        22
                    ],
                    "text": "/doum"
                }
            },
            {
                "box": {
                    "id": "obj-42",
                    "maxclass": "message",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        ""
                    ],
                    "patching_rect": [
                        91,
                        553,
                        29.5,
                        22
                    ],
                    "text": "/tek"
                }
            },
            {
                "box": {
                    "id": "obj-40",
                    "maxclass": "newobj",
                    "numinlets": 3,
                    "numoutlets": 3,
                    "outlettype": [
                        "bang",
                        "bang",
                        ""
                    ],
                    "patching_rect": [
                        190,
                        554,
                        44,
                        22
                    ],
                    "text": "sel 1 0"
                }
            },
            {
                "box": {
                    "id": "obj-39",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        ""
                    ],
                    "patching_rect": [
                        159.5,
                        584,
                        193,
                        22
                    ],
                    "text": "expr (($f1 > 2800.) || ($f2 > 9000.))"
                }
            },
            {
                "box": {
                    "id": "obj-38",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        "float"
                    ],
                    "patching_rect": [
                        365,
                        452,
                        29.5,
                        22
                    ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-31",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        "float"
                    ],
                    "patching_rect": [
                        596,
                        433,
                        29.5,
                        22
                    ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-29",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        529,
                        454,
                        64,
                        22
                    ],
                    "text": "print rolloff"
                }
            },
            {
                "box": {
                    "id": "obj-27",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        336.10526037216187,
                        489,
                        72,
                        22
                    ],
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
                    "outlettype": [
                        "",
                        "bang"
                    ],
                    "parameter_enable": 0,
                    "patching_rect": [
                        385,
                        381,
                        50,
                        22
                    ]
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-16",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [
                        "",
                        "bang"
                    ],
                    "parameter_enable": 0,
                    "patching_rect": [
                        276,
                        452,
                        50,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "obj-12",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        384,
                        569,
                        85,
                        22
                    ],
                    "text": "print classified"
                }
            },
            {
                "box": {
                    "id": "obj-13",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        "float"
                    ],
                    "patching_rect": [
                        177,
                        483,
                        29.5,
                        22
                    ],
                    "text": "f"
                }
            },
            {
                "box": {
                    "id": "obj-10",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        581,
                        496,
                        78,
                        22
                    ],
                    "text": "print centroid"
                }
            },
            {
                "box": {
                    "id": "obj-9",
                    "maxclass": "meter~",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [
                        "float"
                    ],
                    "patching_rect": [
                        47,
                        103,
                        164,
                        180
                    ]
                }
            },
            {
                "box": {
                    "format": 6,
                    "id": "obj-37",
                    "maxclass": "flonum",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [
                        "",
                        "bang"
                    ],
                    "parameter_enable": 0,
                    "patching_rect": [
                        304.38596200942993,
                        380.70175075531006,
                        50,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "obj-35",
                    "maxclass": "newobj",
                    "numinlets": 2,
                    "numoutlets": 1,
                    "outlettype": [
                        "signal"
                    ],
                    "patching_rect": [
                        208.77192783355713,
                        342.9824528694153,
                        39,
                        22
                    ],
                    "text": "gate~"
                }
            },
            {
                "box": {
                    "id": "obj-34",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 7,
                    "outlettype": [
                        "float",
                        "float",
                        "float",
                        "float",
                        "float",
                        "float",
                        "float"
                    ],
                    "patching_rect": [
                        258.77192735671997,
                        292.10526037216187,
                        141,
                        22
                    ],
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
                    "outlettype": [
                        "",
                        "bang"
                    ],
                    "parameter_enable": 0,
                    "patching_rect": [
                        231,
                        518,
                        50,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "obj-30",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [
                        "",
                        ""
                    ],
                    "patching_rect": [
                        292.10526037216187,
                        245.61403274536133,
                        116,
                        22
                    ],
                    "text": "fluid.spectralshape~"
                }
            },
            {
                "box": {
                    "id": "obj-8",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        439,
                        458,
                        54,
                        22
                    ],
                    "text": "print osc"
                }
            },
            {
                "box": {
                    "id": "obj-7",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        450,
                        394,
                        138,
                        22
                    ],
                    "text": "udpsend 127.0.0.1 7000"
                }
            },
            {
                "box": {
                    "id": "obj-6",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [
                        ""
                    ],
                    "patching_rect": [
                        450,
                        368,
                        89,
                        22
                    ],
                    "text": "prepend /doum"
                }
            },
            {
                "box": {
                    "id": "obj-5",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 1,
                    "outlettype": [
                        "bang"
                    ],
                    "patching_rect": [
                        424,
                        280,
                        22,
                        22
                    ],
                    "text": "t b"
                }
            },
            {
                "box": {
                    "id": "obj-4",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [
                        "bang",
                        "bang"
                    ],
                    "patching_rect": [
                        596,
                        172,
                        42,
                        22
                    ],
                    "text": "edge~"
                }
            },
            {
                "box": {
                    "id": "obj-3",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 0,
                    "patching_rect": [
                        670,
                        280,
                        55,
                        22
                    ],
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
                    "outlettype": [
                        "signal",
                        ""
                    ],
                    "patching_rect": [
                        538,
                        322,
                        344,
                        35
                    ],
                    "text": "fluid.ampgate~ @rampup 5 @rampdown 60 @onthreshold -30 @offthreshold -40 @minslicelength 6615"
                }
            },
            {
                "box": {
                    "id": "obj-1",
                    "maxclass": "newobj",
                    "numinlets": 1,
                    "numoutlets": 2,
                    "outlettype": [
                        "signal",
                        "signal"
                    ],
                    "patching_rect": [
                        478,
                        195,
                        35,
                        22
                    ],
                    "text": "adc~"
                }
            },
            {
                "box": {
                    "id": "raqs-comment-1",
                    "maxclass": "comment",
                    "text": "Raqs recorder: Max records audio + hit timing, then replays both to Unity",
                    "patching_rect": [
                        910,
                        70,
                        420,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-button-record-start",
                    "maxclass": "button",
                    "patching_rect": [
                        910,
                        115,
                        24,
                        24
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-record-start",
                    "maxclass": "comment",
                    "text": "START RECORD",
                    "patching_rect": [
                        940,
                        116,
                        120,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-record-start",
                    "maxclass": "message",
                    "text": "record 1",
                    "patching_rect": [
                        1070,
                        105,
                        62,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-record-start",
                    "maxclass": "message",
                    "text": "open /Users/alaasalem/raqs_recording.wav, 1",
                    "patching_rect": [
                        1145,
                        105,
                        245,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-button-record-stop",
                    "maxclass": "button",
                    "patching_rect": [
                        910,
                        155,
                        24,
                        24
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-record-stop",
                    "maxclass": "comment",
                    "text": "STOP RECORD",
                    "patching_rect": [
                        940,
                        156,
                        120,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-record-stop",
                    "maxclass": "message",
                    "text": "record 0",
                    "patching_rect": [
                        1070,
                        145,
                        62,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-record-stop",
                    "maxclass": "message",
                    "text": "0",
                    "patching_rect": [
                        1145,
                        145,
                        30,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-button-play",
                    "maxclass": "button",
                    "patching_rect": [
                        910,
                        205,
                        24,
                        24
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-play",
                    "maxclass": "comment",
                    "text": "PLAY TAKE",
                    "patching_rect": [
                        940,
                        206,
                        120,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-play",
                    "maxclass": "message",
                    "text": "play",
                    "patching_rect": [
                        1070,
                        195,
                        45,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-play",
                    "maxclass": "message",
                    "text": "open /Users/alaasalem/raqs_recording.wav, 1",
                    "patching_rect": [
                        1145,
                        195,
                        245,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-button-play-stop",
                    "maxclass": "button",
                    "patching_rect": [
                        910,
                        245,
                        24,
                        24
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-play-stop",
                    "maxclass": "comment",
                    "text": "STOP PLAYBACK",
                    "patching_rect": [
                        940,
                        246,
                        125,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-play-stop",
                    "maxclass": "message",
                    "text": "stopplay",
                    "patching_rect": [
                        1070,
                        235,
                        62,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-audio-play-stop",
                    "maxclass": "message",
                    "text": "0",
                    "patching_rect": [
                        1145,
                        235,
                        30,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-button-clear",
                    "maxclass": "button",
                    "patching_rect": [
                        910,
                        295,
                        24,
                        24
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-clear",
                    "maxclass": "comment",
                    "text": "CLEAR HIT TAKE",
                    "patching_rect": [
                        940,
                        296,
                        130,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-msg-js-clear",
                    "maxclass": "message",
                    "text": "clear",
                    "patching_rect": [
                        1070,
                        285,
                        45,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-js-recorder",
                    "maxclass": "newobj",
                    "text": "js raqs_hit_recorder.js",
                    "patching_rect": [
                        910,
                        385,
                        135,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-print-recorder",
                    "maxclass": "newobj",
                    "text": "print raqs-recorder",
                    "patching_rect": [
                        1070,
                        385,
                        112,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-sfrecord",
                    "maxclass": "newobj",
                    "text": "sfrecord~ 1",
                    "patching_rect": [
                        1145,
                        320,
                        75,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-sfplay",
                    "maxclass": "newobj",
                    "text": "sfplay~ 1",
                    "patching_rect": [
                        1145,
                        360,
                        62,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-ezdac",
                    "maxclass": "newobj",
                    "text": "ezdac~",
                    "patching_rect": [
                        1145,
                        410,
                        50,
                        22
                    ]
                }
            },
            {
                "box": {
                    "id": "raqs-comment-dac",
                    "maxclass": "comment",
                    "text": "click speaker if playback is silent",
                    "patching_rect": [
                        1205,
                        410,
                        180,
                        22
                    ]
                }
            }
        ],
        "lines": [
            {
                "patchline": {
                    "destination": [
                        "obj-2",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-1",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-30",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-1",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-9",
                        0
                    ],
                    "order": 2,
                    "source": [
                        "obj-1",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-33",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-13",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-39",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-13",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-38",
                        1
                    ],
                    "source": [
                        "obj-16",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-4",
                        0
                    ],
                    "source": [
                        "obj-2",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-31",
                        1
                    ],
                    "source": [
                        "obj-22",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-34",
                        0
                    ],
                    "source": [
                        "obj-30",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-29",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-31",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-39",
                        1
                    ],
                    "order": 1,
                    "source": [
                        "obj-31",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-10",
                        0
                    ],
                    "source": [
                        "obj-33",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-13",
                        1
                    ],
                    "order": 2,
                    "source": [
                        "obj-34",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-16",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-34",
                        1
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-22",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-34",
                        4
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-35",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-34",
                        4
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-35",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-34",
                        1
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-35",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-34",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-37",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-34",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-27",
                        0
                    ],
                    "source": [
                        "obj-38",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-40",
                        0
                    ],
                    "source": [
                        "obj-39",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-42",
                        0
                    ],
                    "source": [
                        "obj-40",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-44",
                        0
                    ],
                    "source": [
                        "obj-40",
                        1
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-3",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-4",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-5",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-4",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-13",
                        0
                    ],
                    "order": 2,
                    "source": [
                        "obj-5",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-31",
                        0
                    ],
                    "order": 0,
                    "source": [
                        "obj-5",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-38",
                        0
                    ],
                    "order": 1,
                    "source": [
                        "obj-5",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "destination": [
                        "obj-8",
                        0
                    ],
                    "source": [
                        "obj-6",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "obj-42",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "obj-44",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-js-recorder",
                        0
                    ],
                    "destination": [
                        "obj-7",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-js-recorder",
                        1
                    ],
                    "destination": [
                        "raqs-print-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-record-start",
                        0
                    ],
                    "destination": [
                        "raqs-msg-js-record-start",
                        0
                    ],
                    "order": 1
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-record-start",
                        0
                    ],
                    "destination": [
                        "raqs-msg-audio-record-start",
                        0
                    ],
                    "order": 0
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-js-record-start",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-audio-record-start",
                        0
                    ],
                    "destination": [
                        "raqs-sfrecord",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-record-stop",
                        0
                    ],
                    "destination": [
                        "raqs-msg-js-record-stop",
                        0
                    ],
                    "order": 1
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-record-stop",
                        0
                    ],
                    "destination": [
                        "raqs-msg-audio-record-stop",
                        0
                    ],
                    "order": 0
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-js-record-stop",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-audio-record-stop",
                        0
                    ],
                    "destination": [
                        "raqs-sfrecord",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-play",
                        0
                    ],
                    "destination": [
                        "raqs-msg-js-play",
                        0
                    ],
                    "order": 1
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-play",
                        0
                    ],
                    "destination": [
                        "raqs-msg-audio-play",
                        0
                    ],
                    "order": 0
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-js-play",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-audio-play",
                        0
                    ],
                    "destination": [
                        "raqs-sfplay",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-play-stop",
                        0
                    ],
                    "destination": [
                        "raqs-msg-js-play-stop",
                        0
                    ],
                    "order": 1
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-play-stop",
                        0
                    ],
                    "destination": [
                        "raqs-msg-audio-play-stop",
                        0
                    ],
                    "order": 0
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-js-play-stop",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-audio-play-stop",
                        0
                    ],
                    "destination": [
                        "raqs-sfplay",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-button-clear",
                        0
                    ],
                    "destination": [
                        "raqs-msg-js-clear",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-msg-js-clear",
                        0
                    ],
                    "destination": [
                        "raqs-js-recorder",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "obj-1",
                        0
                    ],
                    "destination": [
                        "raqs-sfrecord",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-sfplay",
                        0
                    ],
                    "destination": [
                        "raqs-ezdac",
                        0
                    ]
                }
            },
            {
                "patchline": {
                    "source": [
                        "raqs-sfplay",
                        0
                    ],
                    "destination": [
                        "raqs-ezdac",
                        1
                    ]
                }
            }
        ],
        "autosave": 0
    }
}