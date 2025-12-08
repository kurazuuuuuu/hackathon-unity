using UnityEngine;
using Game.Data;
using Game.Network;
using Game.System.Auth;

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

        // 2. Check CognitoAuthManager
        if (CognitoAuthManager.Instance == null)
        {
            Debug.LogWarning("CognitoAuthManager instance not found. Cannot test without authentication.");
            Debug.LogWarning("Please ensure CognitoAuthManager prefab is set up in BootLoader.");
            return;
        }

        // 3. Check if authenticated (in production, user would sign in via LoginScene)
        if (!CognitoAuthManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("User is not authenticated. Please sign in via LoginScene first.");
            Debug.Log("For testing, you can manually call CognitoAuthManager.Instance.SignIn(email, password)");
            return;
        }

        Debug.Log($"Authenticated as: {CognitoAuthManager.Instance.UserId}");

        // 4. Wait for server check (optional)
        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.Log("Waiting for server connection check...");
            await ApiClient.Instance.CheckServerConnection();
        }

        if (!ApiClient.Instance.IsServerAvailable)
        {
            Debug.LogWarning("Server might not be available.");
        }

        // 5. Save
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
