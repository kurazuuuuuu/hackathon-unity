using UnityEngine;
using Game.Data;
using Game.Network;

public class ApiTest : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("Starting API Test...");

        // 1. Create dummy data
        var userData = new UserData("TestUser");
        userData.AddCard("card_001", 3);
        userData.Decks.Add(new DeckData("My First Deck"));
        userData.CurrentDeckId = "deck_001_dummy";

        Debug.Log($"Prepared UserData: {userData.UserName}, Cards: {userData.OwnedCards.Count}");

        // 2. Wait for server check (optional, but good practice)
        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.Log("Waiting for server connection...");
            await ApiClient.Instance.CheckServerConnection();
        }

        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.LogError("Server is not available. Make sure the backend is running.");
            return;
        }

        // 3. Login (if needed, otherwise skip or mock token)
        // For this test, we assume ApiClient might already have a token or we try to save anyway.
        // If login is required, we should call Login first.
        // await ApiClient.Instance.Login("user", "email", "pass");

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
