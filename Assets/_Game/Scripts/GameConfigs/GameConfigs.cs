using System.Collections.Generic;
using UnityEngine;

public static class GameConfigs
{
    public static Dictionary<string, string> PianoNoteColors
     = new Dictionary<string, string>
    {
        { "C", "#CB74FD" },
        { "D", "#FF8F00" },
        { "E", "#4CC901" },
        { "F", "#C02581" },
        { "G", "#6685FF" },
        { "A", "#D3D3D3" },
        { "B", "#D3D3D3" },
    };
    public static Dictionary<string, string[]> FlyingNoteColors
     = new Dictionary<string, string[]>
    {
        { "C", new string[] { "#9E5AC5", "#CA74FF" , "#D893FF" } },
        { "D", new string[] { "#C96A01", "#FF8B00" , "#FFA625" } },
        { "E", new string[] { "#1C9D01", "#4BC603" , "#74D52E" } },
        { "F", new string[] { "#9C1C63", "#C92586" , "#D4539E" } },
        { "G", new string[] { "#3E60D4", "#6783FF" , "#869FFF" } },
        { "A", new string[] { "#A0A0A0", "#C0C0C0" , "#E0E0E0" } },
        { "B", new string[] { "#A0A0A0", "#C0C0C0" , "#E0E0E0" } },
    };

    public static string GetWindowColor(string note)
    {
        if (FlyingNoteColors.ContainsKey(note))
        {
            return FlyingNoteColors[note][0];
        }
        return "#000000";
    }

    public static string GetNote2ndColor(string note)
    {
        if (FlyingNoteColors.ContainsKey(note))
        {
            return FlyingNoteColors[note][2];
        }
        return "#000000";
    }
    public static string GetNoteBaseColor(string note)
    {
        if (FlyingNoteColors.ContainsKey(note))
        {
            return FlyingNoteColors[note][1];
        }
        return "#000000";
    }
}

