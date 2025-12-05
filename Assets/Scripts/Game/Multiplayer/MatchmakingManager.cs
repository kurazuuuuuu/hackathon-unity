using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Game.Multiplayer
{
    /// <summary>
    /// Unity Gaming Services を使用したマッチング・セッション管理
    /// </summary>
    public class MatchmakingManager : MonoBehaviour
    {
        public static MatchmakingManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private string lobbyName = "CardBattle";
        [SerializeField] private float heartbeatInterval = 15f;
        [SerializeField] private float lobbyPollInterval = 2f;

        // 状態
        public bool IsInitialized { get; private set; }
        public bool IsInLobby { get; private set; }
        public bool IsHost { get; private set; }
        public string CurrentLobbyId { get; private set; }
        public string JoinCode { get; private set; }
        public Lobby CurrentLobby { get; private set; }

        // イベント
        public event Action OnInitialized;
        public event Action<string> OnLobbyCreated;
        public event Action<string> OnLobbyJoined;
        public event Action OnMatchStart;
        public event Action<string> OnError;
        public event Action OnPlayerJoined;

        private float heartbeatTimer;
        private float lobbyPollTimer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            await Initialize();
        }

        private void Update()
        {
            HandleHeartbeat();
            HandleLobbyPolling();
        }

        /// <summary>
        /// Unity Gaming Services を初期化
        /// </summary>
        public async Task Initialize()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                IsInitialized = true;
                OnInitialized?.Invoke();
                Debug.Log($"MatchmakingManager: 初期化完了 (PlayerId: {AuthenticationService.Instance.PlayerId})");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                Debug.LogError($"MatchmakingManager: 初期化失敗 - {e.Message}");
            }
        }

        /// <summary>
        /// ロビーを作成
        /// </summary>
        public async Task<string> CreateLobby()
        {
            try
            {
                // Relay アロケーション作成
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // ロビー作成
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                };

                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                CurrentLobbyId = CurrentLobby.Id;
                JoinCode = CurrentLobby.LobbyCode;
                IsInLobby = true;
                IsHost = true;

                // TODO: Netcode パッケージインストール後に有効化
                // SetupRelay(allocation);

                OnLobbyCreated?.Invoke(CurrentLobbyId);
                Debug.Log($"ロビー作成: {JoinCode}");
                
                return JoinCode;
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                Debug.LogError($"ロビー作成失敗: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// ロビーに参加
        /// </summary>
        public async Task<bool> JoinLobby(string joinCode)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
                CurrentLobbyId = CurrentLobby.Id;
                JoinCode = joinCode;
                IsInLobby = true;
                IsHost = false;

                // TODO: Netcode パッケージインストール後に有効化
                // string relayJoinCode = CurrentLobby.Data["RelayJoinCode"].Value;
                // JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                // SetupRelayClient(joinAllocation);

                OnLobbyJoined?.Invoke(CurrentLobbyId);
                Debug.Log($"ロビー参加: {joinCode}");
                
                return true;
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                Debug.LogError($"ロビー参加失敗: {e.Message}");
                return false;
            }
        }

        // TODO: Netcode パッケージインストール後に有効化
        /*
        /// <summary>
        /// Relay を設定（ホスト）
        /// </summary>
        private void SetupRelay(Allocation allocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
        }

        /// <summary>
        /// Relay に参加（クライアント）
        /// </summary>
        private void SetupRelayClient(JoinAllocation joinAllocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        }
        */

        /// <summary>
        /// マッチを開始
        /// </summary>
        public async Task StartMatch()
        {
            if (!IsHost) return;

            try
            {
                OnMatchStart?.Invoke();
                Debug.Log("マッチ開始");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                Debug.LogError($"マッチ開始失敗: {e.Message}");
            }
        }

        /// <summary>
        /// ロビーを退出
        /// </summary>
        public async Task LeaveLobby()
        {
            if (!IsInLobby) return;

            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobbyId, AuthenticationService.Instance.PlayerId);

                IsInLobby = false;
                IsHost = false;
                CurrentLobby = null;
                CurrentLobbyId = null;
                JoinCode = null;
                
                // TODO: Netcode パッケージインストール後に有効化
                // NetworkManager.Singleton.Shutdown();
                Debug.Log("ロビー退出");
            }
            catch (Exception e)
            {
                Debug.LogError($"ロビー退出失敗: {e.Message}");
            }
        }

        /// <summary>
        /// ハートビート送信（ロビー維持用）
        /// </summary>
        private void HandleHeartbeat()
        {
            if (!IsHost || !IsInLobby) return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = heartbeatInterval;
                LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobbyId);
            }
        }

        /// <summary>
        /// ロビー状態をポーリング
        /// </summary>
        private async void HandleLobbyPolling()
        {
            if (!IsInLobby || CurrentLobby == null) return;

            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer <= 0)
            {
                lobbyPollTimer = lobbyPollInterval;
                try
                {
                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobbyId);
                    
                    if (updatedLobby.Players.Count != CurrentLobby.Players.Count)
                    {
                        OnPlayerJoined?.Invoke();
                    }
                    
                    CurrentLobby = updatedLobby;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"ロビー更新失敗: {e.Message}");
                }
            }
        }
    }
}

