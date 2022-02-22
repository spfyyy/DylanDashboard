namespace DylanDashboard.Anime
{
    public class VideoFile
    {
        public string? FilePath { get; set; }

        public string? Name
        {
            get
            {
                if (FilePath == null)
                {
                    return null;
                }
                return Path.GetFileName(FilePath);
            }
        }

        public override string ToString()
        {
            return Name ?? "unable to get file name";
        }
    }
}
