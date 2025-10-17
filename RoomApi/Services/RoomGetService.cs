using Google.Cloud.Firestore;
using RoomApi.Data;
using RoomApi.Utils;

namespace RoomApi.Services
{
    public class RoomGetService
    {
        private readonly AppDbContext _db;

        public RoomGetService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Dictionary<string, object>>> ExecuteAsync()
        {
            var query = _db.Collection("roomsForms");
            var snapshot = await query.GetSnapshotAsync();

            var rooms = new List<Dictionary<string, object>>();
            foreach (var document in snapshot.Documents)
            {
                var data = document.ToDictionary();
                data["Id"] = document.Id;

                TimestampFormatter.FormatTimestamps(data);
                rooms.Add(data);
            }

            return rooms;
        }
    }
}