﻿namespace AGC_Management.Helper;

public class Converter
{
    public static void SeperateIdsAndReason(string ids_and_reason, out List<ulong> ids, out string reason)
    {
        ids = new List<ulong>();
        reason = "";
        var parts = ids_and_reason.Split(' ');
        var isReasonStarted = false;

        foreach (var part in parts)
            if (!isReasonStarted)
            {
                if (part.StartsWith("<@") && part.EndsWith(">"))
                {
                    var idString = part.Substring(2, part.Length - 3);
                    if (ulong.TryParse(idString, out var id))
                        ids.Add(id);
                    else
                        break;
                }
                else if (ulong.TryParse(part, out var id))
                {
                    ids.Add(id);
                }
                else
                {
                    isReasonStarted = true;
                    reason += part + " ";
                }
            }
            else
            {
                reason += part + " ";
            }
    }
}