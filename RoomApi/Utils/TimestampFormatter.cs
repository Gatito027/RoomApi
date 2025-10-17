using Google.Cloud.Firestore;

namespace RoomApi.Utils
{
    public static class TimestampFormatter
    {
        public static void FormatTimestamps(Dictionary<string, object> data)
        {
            if (data.TryGetValue("CreatedAt", out var createdAtObj) && createdAtObj is Timestamp createdAt)
                data["CreatedAt"] = createdAt.ToDateTime().ToString("o");

            if (data.TryGetValue("UpdatedAt", out var updatedAtObj) && updatedAtObj is Timestamp updatedAt)
                data["UpdatedAt"] = updatedAt.ToDateTime().ToString("o");
        }
    }
}