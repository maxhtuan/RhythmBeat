using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

public class DataHandler : MonoBehaviour, IService
{
    [Header("Data Settings")]
    public string xmlFileName = "song";

    public List<NoteData> LoadNotesFromXML()
    {
        List<NoteData> notes = new List<NoteData>();

        // Load XML file
        TextAsset xmlFile = Resources.Load<TextAsset>(xmlFileName);
        if (xmlFile == null)
        {
            Debug.LogError($"Could not load {xmlFileName}.xml!");
            return notes;
        }

        // Parse XML (simplified)
        var doc = XDocument.Parse(xmlFile.text);
        float currentTime = 0f;
        float xmlBpm = 60f;

        // Get BPM
        var metronome = doc.Descendants("metronome").FirstOrDefault();
        if (metronome != null)
        {
            var perMinute = metronome.Element("per-minute");
            if (perMinute != null)
            {
                xmlBpm = float.Parse(perMinute.Value);
            }
        }

        // Get divisions
        var divisions = doc.Descendants("divisions").FirstOrDefault();
        int divisionsPerQuarter = divisions != null ? int.Parse(divisions.Value) : 8;
        float secondsPerTick = 60f / (xmlBpm * divisionsPerQuarter);

        // Parse notes from Learner part only (P1)
        var learnerPart = doc.Descendants("part").FirstOrDefault(p => p.Attribute("id")?.Value == "P1");
        if (learnerPart == null)
        {
            Debug.LogError("Could not find Learner part (P1) in XML!");
            return notes;
        }

        Debug.Log("Found Learner part (P1), parsing notes...");

        // Parse notes from the Learner part only
        int notePosition = 0; // Track position for each note
        foreach (var noteElement in learnerPart.Descendants("note"))
        {
            NoteData note = new NoteData();

            // Get duration
            var durationElement = noteElement.Element("duration");
            if (durationElement != null)
            {
                int durationTicks = int.Parse(durationElement.Value);
                note.duration = durationTicks * secondsPerTick;
            }

            // Check if rest
            var restElement = noteElement.Element("rest");
            if (restElement != null)
            {
                note.isRest = true;
                note.pitch = "REST";
            }
            else
            {
                note.isRest = false;
                var pitchElement = noteElement.Element("pitch");
                if (pitchElement != null)
                {
                    var step = pitchElement.Element("step");
                    var octave = pitchElement.Element("octave");
                    if (step != null && octave != null)
                    {
                        note.pitch = step.Value + octave.Value;
                    }
                }
            }

            note.startTime = currentTime;
            note.notePosition = notePosition; // Assign position to this note
            notes.Add(note);
            currentTime += note.duration;
            notePosition++; // Increment position for next note
        }

        Debug.Log($"Loaded {notes.Count} notes, BPM: {xmlBpm}");
        return notes;
    }

    public float GetBPMFromXML()
    {
        // Load XML file
        TextAsset xmlFile = Resources.Load<TextAsset>(xmlFileName);
        if (xmlFile == null)
        {
            Debug.LogError($"Could not load {xmlFileName}.xml!");
            return 60f; // Default BPM
        }

        // Parse XML
        var doc = XDocument.Parse(xmlFile.text);
        float xmlBpm = 60f;

        // Get BPM
        var metronome = doc.Descendants("metronome").FirstOrDefault();
        if (metronome != null)
        {
            var perMinute = metronome.Element("per-minute");
            if (perMinute != null)
            {
                xmlBpm = float.Parse(perMinute.Value);
            }
        }

        return xmlBpm;
    }

    public void Initialize()
    {
        Debug.Log("DataHandler: Initialized");
    }

    public void Cleanup()
    {
        Debug.Log("DataHandler: Cleaned up");
    }
}
