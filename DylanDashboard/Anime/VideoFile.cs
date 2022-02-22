namespace DylanDashboard.Anime
{
    public class VideoFile
    {
        public string FilePath { get; set; }

        public string Name
        {
            get
            {
                return Path.GetFileName(FilePath);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
