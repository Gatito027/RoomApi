using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.Extensions.Configuration;

namespace RoomApi.Data
{
    public interface IAppDbContext
    {
        CollectionReference Collection(string collectionName);
        DocumentReference Document(string collectionName, string documentId);
        FirestoreDb FirestoreDb { get; }
    }

    public class AppDbContext : IAppDbContext
    {
        public FirestoreDb FirestoreDb { get; }

        public AppDbContext(IConfiguration configuration)
        {
            try
            {
                var projectId = configuration["Firebase:ProjectId"];

                if (string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("Firebase:ProjectId is required in configuration");
                }

                var credentialPath = configuration["Firebase:CredentialFile"];

                if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
                {
                    FirestoreDb = FirestoreDb.Create(projectId,
                        new FirestoreClientBuilder
                        {
                            CredentialsPath = credentialPath
                        }.Build());
                }
                else
                {
                    // Para desarrollo local o entornos cloud
                    FirestoreDb = FirestoreDb.Create(projectId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing Firestore: {ex.Message}", ex);
            }
        }

        public CollectionReference Collection(string collectionName)
        {
            return FirestoreDb.Collection(collectionName);
        }

        public DocumentReference Document(string collectionName, string documentId)
        {
            return FirestoreDb.Collection(collectionName).Document(documentId);
        }
    }
}