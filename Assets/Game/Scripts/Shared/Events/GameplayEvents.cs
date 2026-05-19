namespace SpiritHealer
{
    public struct TreatmentCompletedEvent
    {
        public VisitorInstance Visitor;
        public float EfficacyScore;
        public int ReputationGained;
        public int CoinsGained;
    }

    /// <summary>来客队列或当前来客发生变化时触发。</summary>
    public struct VisitorChangedEvent { }
}
