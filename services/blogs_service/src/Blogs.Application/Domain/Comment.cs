namespace blogs_service.src.Blogs.Application.Domain
{
    using blogs_service.src.Blogs.BuildingBlocks.Core.Domain;
    using MongoDB.Bson.Serialization.Attributes;

    public class Comment : BaseEntity
    {
        [BsonElement("blogId")]
        public string BlogId { get; set; }
        
        [BsonElement("userId")]
        public string UserId { get; set; }
        
        [BsonElement("content")]
        public string Content { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}