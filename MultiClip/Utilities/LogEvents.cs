namespace MultiClip.Utilities
{
    public static class LogEvents
    {
        public static readonly EventId FatalErr = new EventId(1, nameof(FatalErr));
        public static readonly EventId DiskErr = new EventId(2, nameof(DiskErr));
        public static readonly EventId ClipbdReadErr = new EventId(3, nameof(ClipbdReadErr));
        public static readonly EventId PasteErr = new EventId(4, nameof(PasteErr));
        public static readonly EventId UiOpErr = new EventId(5, nameof(UiOpErr));
        public static readonly EventId UnknClipbdFmt = new EventId(6, nameof(UnknClipbdFmt));
        public static readonly EventId NetErr = new EventId(7, nameof(NetErr));
    }
}
