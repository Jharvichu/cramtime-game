using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class TestPlayer : NetworkBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<Color> randomColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            randomNumber.OnValueChanged += AlCambiarElNumero;
            randomColor.OnValueChanged += AlCambiarColor;

            if (IsServer)
            {
                randomColor.Value = IsOwner ? Color.green : Color.red;
            }
        }

        public override void OnNetworkDespawn()
        {
            randomNumber.OnValueChanged -= AlCambiarElNumero;
            randomColor.OnValueChanged -= AlCambiarColor;
        }

        private void FixedUpdate() {
            if (!IsOwner) return;

            Vector3 moveDir = new Vector3(0,0,0);

            if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
            if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;  
            if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

            if (Input.GetKey(KeyCode.X))
            {
                randomNumber.Value = Random.Range(0,10);
            }

            if (Input.GetKey(KeyCode.Z))
            {
                CambiarColorServerRpc();
            }

            float moveSpeed = 3f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        private void AlCambiarElNumero(int previusValue, int newValue)
        {
            Debug.Log($"[ID:{OwnerClientId}] El número cambió de {previusValue} a {newValue}");
        }

        private void AlCambiarColor(Color previusValue, Color newValue)
        {
            Debug.Log($"[ID:{OwnerClientId}] modifico su colorrrrr.");
            _spriteRenderer.color = randomColor.Value;
        }

        [ServerRpc]
        private void CambiarColorServerRpc()
        {
            randomColor.Value = new Color(Random.value, Random.value, Random.value);
            Debug.Log($"[ID:{OwnerClientId}] cambio del color (se ejecuto desde el servidor)");
        }
    }
}
