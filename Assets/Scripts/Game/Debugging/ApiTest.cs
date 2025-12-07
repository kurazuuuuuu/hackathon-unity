using UnityEngine;
using Game.Data;
using Game.Network;

public class ApiTest : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("Starting API Test with Mock Data...");

        // Ensure ApiClient exists
        if (ApiClient.Instance == null)
        {
            Debug.LogWarning("ApiClient instance not found. Creating a temporary one.");
            var go = new GameObject("ApiClient");
            go.AddComponent<ApiClient>();
        }

        // 1. Create dummy data
        var userData = new UserData("TestUser");
        userData.AddCard("card_001", 3);
        userData.Decks.Add(new DeckData("My First Deck"));
        userData.CurrentDeckId = "deck_001_dummy";

        Debug.Log($"Prepared UserData: {userData.UserName}, Cards: {userData.OwnedCards.Count}");

        // 2. Set Mock Tokens (Simulate Login)
        Debug.Log("Setting Mock Tokens...");
        ApiClient.Instance.SetTokens(
            "mock_access_token",
            "mock_id_token_user_123",
            "mock_refresh_token"
        );

        // 3. Wait for server check (optional)
        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.Log("Waiting for server connection check...");
            await ApiClient.Instance.CheckServerConnection();
        }

        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.LogWarning("Server might not be available, but attempting save with mock tokens anyway...");
        }

        // 4. Save
        Debug.Log("Sending Save Request...");
        var response = await ApiClient.Instance.SaveUserData(userData);

        if (response.Success)
        {
            Debug.Log($"Save Success! Status: {response.Data.status}, UpdatedAt: {response.Data.updated_at}");
        }
        else
        {
            Debug.LogError($"Save Failed: {response.Error} (Code: {response.StatusCode})");
        }
    }
}
