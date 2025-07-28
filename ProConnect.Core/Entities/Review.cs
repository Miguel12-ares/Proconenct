using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProConnect.Core.Entities
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; } // MongoDB ObjectId as string
        public required string ClientId { get; set; }
        public required string ProfessionalId { get; set; }
        public required string BookingId { get; set; }
        public int Rating { get; set; } // 1-5
        public required string Comment { get; set; } // Optional, max 500 chars
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
