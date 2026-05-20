namespace CozyYard
{
    public struct CropPlantedEvent
    {
        public int GridX;
        public int GridY;
        public int CropId;
    }

    public struct CropGrowthEvent
    {
        public int GridX;
        public int GridY;
        public CropGrowthStage NewStage;
    }

    public struct CropWateredEvent
    {
        public int GridX;
        public int GridY;
    }

    public struct CropReadyEvent
    {
        public int GridX;
        public int GridY;
        public int CropId;
    }

    public struct CropWitheredEvent
    {
        public int GridX;
        public int GridY;
    }
}
