public static class NetworkConfig
{
    // Keys
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_DIFFICULTY = "PlayerDifficulty";
    public const string KEY_RELAY_CODE = "RelayCode";

    // Datos default del jugador
    public const string PLAYER_NAME_DEFAULT = "player";

    // Datos default de la Sala
    public const int PLAYER_COUNT_DEFAULT = 4;
    public const int PLAYER_COUNT_MAX = 8;
    public const bool IS_ROOM_PRIVATE_DEFAULT = false;
    public const string DEFAULT_ROOM_NAME = "Lobby";

    // Datos default de los servers
    public const int DEFAULT_SERVER_COUNT = 20;
    public const bool SAMPLE_RESULTS_DEFAULT = true;

    // Tiempo y timeouts
    public const float LOBBY_REFRESH_RATE = 5f;
    public const float HEARTBEAT_INTERVAL = 15f;

}
