using Google.Cloud.Firestore;
using RoomApi.Data;
using RoomApi.Utils;

namespace RoomApi.Services
{
    public class RoomGetByIdService
    {
        private readonly AppDbContext _db;

        public RoomGetByIdService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, object>?> ExecuteAsync(string id)
        {
            var docRef = _db.Collection("roomsForms").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            var data = snapshot.ToDictionary();
            data["Id"] = snapshot.Id;

            TimestampFormatter.FormatTimestamps(data);
            return data;
        }
    }
}
