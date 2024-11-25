using System;
using System.Collections.Generic;

public class Game
{
    public static List<Action> VoicelineTriggerQueue = new List<Action>();

    private static bool s_VoicelineInProgress = false;

    public static void QueueVoiceLine(Action trigger)
    {
        VoicelineTriggerQueue.Add(trigger);

        if (s_VoicelineInProgress) return;

        s_VoicelineInProgress = true;

        VoicelineTriggerQueue[0].Invoke();
        VoicelineTriggerQueue.RemoveAt(0);
    }

    public static void VoicelineFinished()
    {
        s_VoicelineInProgress = false;

        if (VoicelineTriggerQueue.Count == 0) return;

        s_VoicelineInProgress = true;

        VoicelineTriggerQueue[0].Invoke();
        VoicelineTriggerQueue.RemoveAt(0);
    }
}