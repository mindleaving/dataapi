namespace SharedWpfControls.Objects
{
    public enum StorageLocationType
    {
        Undefined = 0, // Should never be used, except for checking for invalid serialization and other coding errors
        
        RoomWithoutClimateControl = 1,
        Refrigerator = 2,
        Freezer = 3,

        WasteBin = 4
    }
}
