namespace MyProject.AI.Processes;

internal static class ProcessEvents
{
    internal static string ProcessStarted = nameof(ProcessStarted);
    internal static string DoneCreate = nameof(DoneCreate);
    internal static string StartedFormat = nameof(StartedFormat);
    internal static string RecheckRequired = nameof(RecheckRequired);
    internal static string EndFormat = nameof(EndFormat);

    internal static class ProcessTopics
    {
        internal static string Completed = nameof(Completed);
    }

}