using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace blogs_service.src.Blogs.Application.Domain
{
    public class Blog : BaseEntity
    {
        [BsonElement("title")]
        public string? Title { get; set; }
        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonElement("descriptionMarkdown")]
        public string? DescriptionMarkdown { get; set; }
        [BsonElement("imageBase64")]
        public string? ImageBase64 { get; set; }
    }
}
