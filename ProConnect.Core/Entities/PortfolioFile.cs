using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProConnect.Core.Entities
{
    public class PortfolioFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [BsonElement("size")]
        public long Size { get; set; }

        [BsonElement("url")]
        public string Url { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("uploadedAt")]
        public DateTime UploadedAt { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
    }
} 