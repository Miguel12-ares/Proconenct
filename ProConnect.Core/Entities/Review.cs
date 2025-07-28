using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProConnect.Core.Entities
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // MongoDB ObjectId as string
        public string ClientId { get; set; }
        public string ProfessionalId { get; set; }
        public string BookingId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } // Optional, max 500 chars
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
