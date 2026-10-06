using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    public enum OriginalConnectionState { Disconnected, Connecting, Joining, Lobby, Playing, Faulted }

    // Owns Unity's socket lifecycle. World simulation consumes match events and
    // calls AdvanceHost; Update pumps networking even when Time.timeScale is 0.
    [DisallowMultipleComponent]
    public sealed class OriginalNetworkGame : MonoBehaviour
    {
        public TextAsset[] dataAssets = Array.Empty<TextAsset>();
        public OriginalConnectionState State { get; private set; }
        public OriginalSessionView View { get; private set; }
        public OriginalNetworkResponse LastReply { get; private set; }
        public string Error { get; private set; }
        public bool IsHost => session != null;
        public int LocalSlot => IsHost ? 1 : client?.AssignedSlot ?? 0;
        public int Port => listener?.Port ?? 0;
        public event Action<OriginalSessionView> SnapshotChanged;
        public event Action<OriginalNetworkResponse> CommandReplied;

        private OriginalGameCatalogs catalogs;
        private OriginalMapNavigation navigation;
        private readonly OriginalUnitySessionCodec codec = new OriginalUnitySessionCodec();
        private OriginalTcpHost listener;
        private OriginalSession session;
        private OriginalNetworkSession server;
        private OriginalNetworkClient client;
        private Socket connectingSocket;
        private Task<string> connecting;
        private double connectDeadline, nextSnapshot;
        private long nextSequence = 1, observedRevision = -1;
        private long lifecycleVersion;

        public void Configure(OriginalGameCatalogs value)
        {
            if (State != OriginalConnectionState.Disconnected) throw new InvalidOperationException("Disconnect before replacing game data.");
            catalogs = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void ConfigureWorld(OriginalMapNavigation value)
        {
            if (State != OriginalConnectionState.Disconnected) throw new InvalidOperationException("Disconnect before replacing navigation.");
            navigation = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Host(OriginalMatchOptions options, int seed, int port = 0, string bindAddress = "127.0.0.1")
        {
            if (State != OriginalConnectionState.Disconnected) return false;
            if (!IPAddress.TryParse(bindAddress, out var address) || !IsLocalAddress(address))
            { Error = "Выберите локальный адрес компьютера или 127.0.0.1."; return false; }
            var lifecycle = ++lifecycleVersion;
            try
            {
                EnsureCatalogs();
                var candidate = catalogs.CreateSession(options, seed, navigation);
                listener = new OriginalTcpHost(port, address);
                session = candidate;
                server = new OriginalNetworkSession(listener, session, codec);
                nextSequence = 1;
                Error = null;
                Publish(session.Snapshot());
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is SocketException || exception is InvalidOperationException)
            {
                if (lifecycle == lifecycleVersion) Fail("Не удалось создать сессию: " + exception.Message);
                return false;
            }
        }

        public bool Join(string hostAddress, int port)
        {
            if (State != OriginalConnectionState.Disconnected) return false;
            if (!IPAddress.TryParse(hostAddress, out var address) || port < 1 || port > 65535)
            { Error = "Укажите IP-адрес ведущего и порт 1-65535."; return false; }
            var lifecycle = ++lifecycleVersion;
            try
            {
                EnsureCatalogs();
                connectingSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                connecting = Connect(connectingSocket, new IPEndPoint(address, port));
                connectDeadline = Time.realtimeSinceStartupAsDouble + 10;
                State = OriginalConnectionState.Connecting;
                Error = null;
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is SocketException || exception is InvalidOperationException)
            {
                if (lifecycle == lifecycleVersion) Fail("Не удалось подключиться: " + exception.Message);
                return false;
            }
        }

        private static async Task<string> Connect(Socket socket, IPEndPoint endpoint)
        {
            try { await socket.ConnectAsync(endpoint); return null; }
            catch (SocketException exception) { return exception.SocketErrorCode.ToString(); }
            catch (ObjectDisposedException) { return "cancelled"; }
        }

        private static bool IsLocalAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address)) return true;
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = address.GetAddressBytes();
                return bytes[0] == 10 || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                    (bytes[0] == 192 && bytes[1] == 168) || (bytes[0] == 169 && bytes[1] == 254);
            }
            return address.IsIPv6LinkLocal || (address.GetAddressBytes()[0] & 0xfe) == 0xfc;
        }

        private void EnsureCatalogs()
        {
            if (catalogs == null) catalogs = OriginalGameCatalogs.Load(dataAssets);
        }

        public bool SendCommand(OriginalSessionCommandKind kind, string heroId = null, bool ready = false,
            int betSide = 0, int betStake = 0, double x = 0, double y = 0,
            OriginalWorldTargetKind targetKind = OriginalWorldTargetKind.None, int targetId = 0, string skillId = null,
            string itemId = null, int shopInstanceId = 0, OriginalInventoryBag bag = OriginalInventoryBag.Hero,
            int itemSlot = 0, long itemInstanceId = 0, int actorEntityId = 0, long targetItemInstanceId = 0)
        {
            if (State != OriginalConnectionState.Lobby && State != OriginalConnectionState.Playing) return false;
            if (kind == OriginalSessionCommandKind.Hello || !Enum.IsDefined(typeof(OriginalSessionCommandKind), kind)) return false;
            var lifecycle = lifecycleVersion;
            var command = Command(kind, heroId, ready);
            command.betSide = betSide; command.betStake = betStake;
            command.x = x; command.y = y; command.targetKind = targetKind; command.targetId = targetId;
            command.skillId = skillId;
            command.itemId = itemId; command.shopInstanceId = shopInstanceId; command.bag = bag;
            command.itemSlot = itemSlot; command.itemInstanceId = itemInstanceId;
            command.actorEntityId = actorEntityId;
            command.targetItemInstanceId = targetItemInstanceId;
            if (IsHost)
            {
                var code = server.ApplyLocal(command);
                nextSequence++;
                var snapshot = session.Snapshot();
                var reply = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Reply, code = code,
                    assignedSlot = 1, commandSequence = command.sequence,
                    acknowledgedSequence = snapshot.players[0].acknowledgedSequence };
                Publish(snapshot);
                if (lifecycle != lifecycleVersion) return true;
                Reply(reply);
                return true;
            }
            if (!client.Send(command)) return false;
            nextSequence++;
            return true;
        }

        private OriginalSessionCommand Command(OriginalSessionCommandKind kind, string heroId = null, bool ready = false) =>
            new OriginalSessionCommand { protocol = OriginalSession.Protocol, contentHash = catalogs.Fingerprint,
                sequence = nextSequence, kind = kind, heroId = heroId, ready = ready };

        public void AdvanceHost(double seconds)
        {
            if (!IsHost) throw new InvalidOperationException("Only the host can advance a match.");
            session.Advance(seconds);
        }

        public OriginalSessionEvent[] DrainHostEvents()
        {
            if (!IsHost) throw new InvalidOperationException("Only the host can consume authoritative events.");
            return session.DrainEvents();
        }

        private void Update()
        {
            var lifecycle = lifecycleVersion;
            if (State == OriginalConnectionState.Connecting)
            {
                if (Time.realtimeSinceStartupAsDouble >= connectDeadline) { Fail("Ведущий не ответил за 10 секунд."); return; }
                if (!connecting.IsCompleted) return;
                var reason = connecting.GetAwaiter().GetResult();
                connecting = null;
                if (reason != null) { Fail("Не удалось подключиться: " + reason); return; }
                client = new OriginalNetworkClient(new OriginalTcpPeer(connectingSocket, 1), codec);
                connectingSocket = null;
                nextSequence = 1;
                if (!client.Send(Command(OriginalSessionCommandKind.Hello))) { Fail("Не удалось отправить запрос входа."); return; }
                nextSequence = 2;
                State = OriginalConnectionState.Joining;
            }
            if (server != null)
            {
                server.Pump();
                if (server.FaultReason != null) { Fail(server.FaultReason); return; }
                if (Time.realtimeSinceStartupAsDouble >= nextSnapshot)
                {
                    nextSnapshot = Time.realtimeSinceStartupAsDouble + 0.1;
                    Publish(session.Snapshot());
                    if (lifecycle != lifecycleVersion) return;
                }
            }
            var activeClient = client;
            if (activeClient == null) return;
            activeClient.Pump();
            OriginalNetworkResponse response;
            while ((response = activeClient.Receive()) != null)
            {
                if (response.kind == OriginalNetworkResponseKind.Reply)
                {
                    Reply(response);
                    if (lifecycle != lifecycleVersion) return;
                    if (State == OriginalConnectionState.Joining && response.commandSequence == 1 && response.code != OriginalSessionReplyCode.Accepted)
                    { Fail("Вход отклонен: " + response.code); return; }
                }
                else
                {
                    if (response.snapshot.contentHash != catalogs.Fingerprint) { Fail("Набор правил ведущего отличается от локального."); return; }
                    Publish(response.snapshot);
                    if (lifecycle != lifecycleVersion) return;
                }
            }
            if (!activeClient.IsConnected) { Fail("Соединение с ведущим закрыто."); return; }
            if (State == OriginalConnectionState.Joining && Time.realtimeSinceStartupAsDouble >= connectDeadline)
                Fail("Ведущий не подтвердил вход за 10 секунд.");
        }

        private void Publish(OriginalSessionView view)
        {
            if (view.revision < observedRevision) return;
            observedRevision = view.revision;
            View = view;
            State = view.started ? OriginalConnectionState.Playing : OriginalConnectionState.Lobby;
            var callbacks = SnapshotChanged;
            if (callbacks == null) return;
            var lifecycle = lifecycleVersion;
            foreach (Action<OriginalSessionView> callback in callbacks.GetInvocationList())
            {
                callback(view);
                if (lifecycle != lifecycleVersion) return;
            }
        }

        private void Reply(OriginalNetworkResponse response)
        {
            LastReply = response;
            var callbacks = CommandReplied;
            if (callbacks == null) return;
            var lifecycle = lifecycleVersion;
            foreach (Action<OriginalNetworkResponse> callback in callbacks.GetInvocationList())
            {
                callback(response);
                if (lifecycle != lifecycleVersion) return;
            }
        }

        private void Fail(string reason)
        {
            CloseSockets();
            State = OriginalConnectionState.Faulted;
            Error = reason;
        }

        private void CloseSockets()
        {
            // A callback can disconnect and immediately host or join again. Any
            // caller still dispatching the old session must stop at that boundary.
            lifecycleVersion++;
            client?.Dispose(); client = null;
            server?.Dispose(); server = null;
            listener?.Dispose(); listener = null;
            connectingSocket?.Dispose(); connectingSocket = null;
            connecting = null;
            session = null;
        }

        public void Disconnect()
        {
            CloseSockets();
            State = OriginalConnectionState.Disconnected;
            View = null; LastReply = null; Error = null;
            observedRevision = -1; nextSequence = 1; nextSnapshot = 0;
        }

        private void OnDisable() { Disconnect(); }
        private void OnDestroy() { CloseSockets(); }
    }
}
