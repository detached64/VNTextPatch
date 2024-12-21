namespace VNTextPatch
{
    public interface ILineStatistics
    {
        int Translated { get; }
        int Checked { get; }
        int Edited { get; }
        int Total { get; set; }

        void Reset();
    }
}
