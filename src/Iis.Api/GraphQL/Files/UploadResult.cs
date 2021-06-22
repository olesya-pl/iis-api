namespace IIS.Core.GraphQL.Files
{
    public class UploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        private static UploadResult _successResult = new UploadResult { Success = true };
        private static UploadResult _unsupportedResult = Error("Формат не підтримується");
        private static UploadResult _duplicatedResult = Error("Даний файл вже завантажений до системи");
        public static UploadResult Ok => _successResult;
        public static UploadResult Unsupported => _unsupportedResult;
        public static UploadResult Duplicated => _duplicatedResult;
        public static UploadResult Error(string message) => new UploadResult { Success = false, Message = message };
    }
}
