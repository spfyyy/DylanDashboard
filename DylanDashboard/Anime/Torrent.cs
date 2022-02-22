namespace DylanDashboard.Anime
{
    public class Torrent
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int? CompletePercentage { get; set; }

        public override string ToString()
        {
            return $"{ Title } - { CompletePercentage }% - { Status }";
        }
    }
}
