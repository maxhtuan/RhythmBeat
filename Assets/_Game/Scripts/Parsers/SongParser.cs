using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

public class SongParser
{
    public SongData ParseSong(string fileName)
    {
        try
        {
            // Try multiple loading methods
            TextAsset xmlFile = null;

            // Method 1: Try with extension
            xmlFile = Resources.Load<TextAsset>(fileName);

            // Method 2: Try without extension
            if (xmlFile == null)
            {
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
                xmlFile = Resources.Load<TextAsset>(fileNameWithoutExtension);
            }

            // Method 3: Try loading all TextAssets and find by name
            if (xmlFile == null)
            {
                TextAsset[] allTextAssets = Resources.LoadAll<TextAsset>("");
                foreach (var asset in allTextAssets)
                {
                    if (asset.name == fileName || asset.name == System.IO.Path.GetFileNameWithoutExtension(fileName))
                    {
                        xmlFile = asset;
                        break;
                    }
                }
            }

            if (xmlFile == null)
            {
                Debug.LogError($"Could not load XML file: {fileName}");
                Debug.LogError("Available TextAssets in Resources:");
                TextAsset[] allAssets = Resources.LoadAll<TextAsset>("");
                foreach (var asset in allAssets)
                {
                    Debug.LogError($"  - {asset.name}");
                }
                return null;
            }

            Debug.Log($"Successfully loaded XML file: {xmlFile.name}");
            XDocument doc = XDocument.Parse(xmlFile.text);
            XElement score = doc.Root;

            SongData songData = new SongData();

            // Parse basic song information
            ParseAttributes(score, songData);

            // Parse parts (measures and notes)
            ParseParts(score, songData);

            // Calculate total duration
            CalculateSongDuration(songData);

            // Parse phrases and directions
            ParsePhrases(songData);

            return songData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing song {fileName}: {e.Message}");
            return null;
        }
    }

    private void ParseAttributes(XElement score, SongData songData)
    {
        // Get first measure to extract basic attributes
        var firstMeasure = score.Descendants("measure").FirstOrDefault();
        if (firstMeasure != null)
        {
            var attributes = firstMeasure.Element("attributes");
            if (attributes != null)
            {
                // Parse divisions
                var divisions = attributes.Element("divisions");
                if (divisions != null)
                {
                    songData.divisions = int.Parse(divisions.Value);
                }

                // Parse key
                var key = attributes.Element("key");
                if (key != null)
                {
                    var fifths = key.Element("fifths");
                    if (fifths != null)
                    {
                        int fifthsValue = int.Parse(fifths.Value);
                        songData.key = ConvertFifthsToKey(fifthsValue);
                    }

                    var mode = key.Element("mode");
                    if (mode != null)
                    {
                        songData.mode = mode.Value;
                    }
                }

                // Parse time signature
                var time = attributes.Element("time");
                if (time != null)
                {
                    var beats = time.Element("beats");
                    var beatType = time.Element("beat-type");
                    if (beats != null && beatType != null)
                    {
                        songData.timeSignatureBeats = int.Parse(beats.Value);
                        songData.timeSignatureBeatType = int.Parse(beatType.Value);
                    }
                }
            }

            // Parse tempo from direction
            var direction = firstMeasure.Element("direction");
            if (direction != null)
            {
                var directionType = direction.Element("direction-type");
                if (directionType != null)
                {
                    var metronome = directionType.Element("metronome");
                    if (metronome != null)
                    {
                        var perMinute = metronome.Element("per-minute");
                        if (perMinute != null)
                        {
                            songData.bpm = float.Parse(perMinute.Value);
                        }
                    }
                }
            }
        }
    }

    private void ParseParts(XElement score, SongData songData)
    {
        var parts = score.Elements("part");
        float currentTime = 0f;

        foreach (var part in parts)
        {
            string partId = part.Attribute("id")?.Value ?? "P1";
            var measures = part.Elements("measure");

            foreach (var measure in measures)
            {
                int measureNumber = int.Parse(measure.Attribute("number")?.Value ?? "1");

                MeasureData measureData = new MeasureData
                {
                    measureNumber = measureNumber,
                    startTime = currentTime
                };

                // Parse notes in this measure
                ParseNotesInMeasure(measure, measureData, songData, currentTime);

                // Parse directions in this measure
                ParseDirectionsInMeasure(measure, measureData, currentTime);

                songData.measures.Add(measureData);

                // Calculate measure duration and advance time
                float measureDuration = CalculateMeasureDuration(measureData, songData);
                currentTime += measureDuration;
                measureData.duration = measureDuration;
            }
        }
    }

    private void ParseNotesInMeasure(XElement measure, MeasureData measureData, SongData songData, float measureStartTime)
    {
        var notes = measure.Elements("note");
        float noteTime = measureStartTime;

        foreach (var note in notes)
        {
            NoteData noteData = new NoteData();

            // Parse duration
            var duration = note.Element("duration");
            if (duration != null)
            {
                int durationValue = int.Parse(duration.Value);
                noteData.duration = ConvertDurationToSeconds(durationValue, songData.divisions, songData.bpm);
            }

            // Check if it's a rest
            var rest = note.Element("rest");
            if (rest != null)
            {
                noteData.isRest = true;
                noteData.pitch = "REST";
            }
            else
            {
                // Parse pitch
                var pitch = note.Element("pitch");
                if (pitch != null)
                {
                    var step = pitch.Element("step");
                    var octave = pitch.Element("octave");
                    if (step != null && octave != null)
                    {
                        noteData.pitch = step.Value + octave.Value;
                        noteData.midiNote = ConvertPitchToMidi(step.Value, int.Parse(octave.Value));
                    }
                }
            }

            // Parse note type
            var type = note.Element("type");
            if (type != null)
            {
                noteData.noteType = type.Value;
            }

            // Parse voice and staff
            var voice = note.Element("voice");
            if (voice != null)
            {
                noteData.voice = int.Parse(voice.Value);
            }

            var staff = note.Element("staff");
            if (staff != null)
            {
                noteData.staff = int.Parse(staff.Value);
            }

            // Check if it's part of a chord
            var chord = note.Element("chord");
            if (chord != null)
            {
                noteData.isChord = true;
            }

            // Parse stem direction
            var stem = note.Element("stem");
            if (stem != null)
            {
                noteData.stem = stem.Value;
            }

            // Set timing
            noteData.startTime = noteTime;

            // Add to collections
            measureData.notes.Add(noteData);
            songData.allNotes.Add(noteData);

            // Advance time (unless it's a chord note)
            if (!noteData.isChord)
            {
                noteTime += noteData.duration;
            }
        }
    }

    private void ParseDirectionsInMeasure(XElement measure, MeasureData measureData, float measureStartTime)
    {
        var directions = measure.Elements("direction");

        foreach (var direction in directions)
        {
            DirectionData directionData = new DirectionData();

            var placement = direction.Attribute("placement");
            if (placement != null)
            {
                directionData.placement = placement.Value;
            }

            var directionType = direction.Element("direction-type");
            if (directionType != null)
            {
                var words = directionType.Element("words");
                if (words != null)
                {
                    directionData.words = words.Value;
                    directionData.directionType = words.Value;
                }
            }

            directionData.time = measureStartTime;
            measureData.directions.Add(directionData);
        }
    }

    private void ParsePhrases(SongData songData)
    {
        foreach (var measure in songData.measures)
        {
            foreach (var direction in measure.directions)
            {
                if (direction.directionType.Contains("phraseStart"))
                {
                    PhraseData phrase = new PhraseData
                    {
                        phraseType = direction.directionType,
                        startTime = direction.time
                    };

                    // Find the corresponding phrase end
                    var endDirection = FindPhraseEnd(songData, direction.time);
                    if (endDirection != null)
                    {
                        phrase.endTime = endDirection.time;
                    }

                    // Add notes that belong to this phrase
                    phrase.notes = songData.allNotes.Where(n =>
                        n.startTime >= phrase.startTime &&
                        (phrase.endTime == 0 || n.startTime < phrase.endTime)).ToList();

                    songData.phrases.Add(phrase);
                }
            }
        }
    }

    private DirectionData FindPhraseEnd(SongData songData, float startTime)
    {
        foreach (var measure in songData.measures)
        {
            foreach (var direction in measure.directions)
            {
                if (direction.directionType.Contains("phraseEnd") && direction.time > startTime)
                {
                    return direction;
                }
            }
        }
        return null;
    }

    private void CalculateSongDuration(SongData songData)
    {
        if (songData.measures.Count > 0)
        {
            var lastMeasure = songData.measures[songData.measures.Count - 1];
            songData.totalDuration = lastMeasure.startTime + lastMeasure.duration;
        }
    }

    private float CalculateMeasureDuration(MeasureData measureData, SongData songData)
    {
        float totalDuration = 0f;
        foreach (var note in measureData.notes)
        {
            if (!note.isChord)
            {
                totalDuration += note.duration;
            }
        }
        return totalDuration;
    }

    // Utility methods
    private string ConvertFifthsToKey(int fifths)
    {
        string[] keys = { "C", "G", "D", "A", "E", "B", "F#", "C#", "F", "Bb", "Eb", "Ab", "Db", "Gb", "Cb" };
        return keys[fifths + 7]; // +7 to handle negative fifths
    }

    private float ConvertDurationToSeconds(int duration, int divisions, float bpm)
    {
        float beatsPerSecond = bpm / 60f;
        float durationInBeats = (float)duration / divisions;
        return durationInBeats / beatsPerSecond;
    }

    private int ConvertPitchToMidi(string step, int octave)
    {
        string[] steps = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int stepIndex = Array.IndexOf(steps, step);
        return stepIndex + (octave + 1) * 12;
    }
}
