using Iis.Domain.Materials;

namespace IIS.Core.GraphQL.Materials
{
    public static class Extensions
    {
        public static Files.FileInfo ToView(this File fileInfo)
        {
            if (fileInfo == default)
                return null;
            return new Files.FileInfo
            {
                Id = fileInfo.Id,
                Name = fileInfo.Name,
                ContentType = fileInfo.ContentType,
                IsTemporary = fileInfo.IsTemporary,
            };
        }
    }
}
