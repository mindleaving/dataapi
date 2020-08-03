namespace SharedViewModels.Objects
{
    public class ImageAnnotationStatus
    {
        public ImageAnnotationStatus(string imageFilename, bool isOk, string statusText)
        {
            ImageFilename = imageFilename;
            IsOk = isOk;
            StatusText = statusText;
        }

        public string ImageFilename { get; }
        public bool IsOk { get; }
        public string StatusText { get; }
    }
}