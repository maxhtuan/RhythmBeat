using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SongData
{
    public string title = "Unknown Song";
    public float bpm = 60f;
    public int divisions = 8; // MIDI divisions per quarter note
    public float totalDuration = 0f;
    public List<MeasureData> measures = new List<MeasureData>();
    public List<NoteData> allNotes = new List<NoteData>();
    public List<PhraseData> phrases = new List<PhraseData>();

    // Song metadata
    public string key = "C";
    public string mode = "major";
    public int timeSignatureBeats = 4;
    public int timeSignatureBeatType = 4;
}

[System.Serializable]
public class MeasureData
{
    public int measureNumber;
    public float startTime;
    public float duration;
    public List<NoteData> notes = new List<NoteData>();
    public List<DirectionData> directions = new List<DirectionData>();
}

[System.Serializable]
public class NoteData
{
    public string pitch; // e.g., "E4", "C5"
    public int midiNote; // MIDI note number (0-127)
    public float startTime;
    public float duration;
    public string noteType; // "quarter", "eighth", etc.
    public bool isRest;
    public bool isChord;
    public int voice = 1;
    public int staff = 1;
    public string stem = "up";

    // Gameplay properties
    public bool isHit = false;
    public bool isMissed = false;
    public float hitAccuracy = 0f;
    public GameObject noteObject; // Visual representation


    public string GetNoteName()
    {
        // Return first letter of pitch (note name)
        if (string.IsNullOrEmpty(pitch))
            return "C"; // Default fallback

        string firstLetter = this.pitch.Substring(0, 1);
        return firstLetter;
    }
}

[System.Serializable]
public class DirectionData
{
    public string directionType; // "phraseStart", "phraseEnd", "previewStart", etc.
    public float time;
    public string placement = "above";
    public string words;
}

[System.Serializable]
public class PhraseData
{
    public string phraseType; // "Chorus", "MJStart", "MJEnd", etc.
    public float startTime;
    public float endTime;
    public List<NoteData> notes = new List<NoteData>();
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
