using Mirror;
using UnityEngine;
using TMPro;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMeshPro playerNameText;
        public GameObject floatingInfo;

        public NetworkAnimator networkAnim;

        public Rigidbody rb;

        public bool isGrounded;

        private Material playerMaterialClone;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            playerMaterialClone = new Material(GetComponent<Renderer>().material);
            playerMaterialClone.color = _New;
            GetComponent<Renderer>().material = playerMaterialClone;
        }

        public override void OnStartLocalPlayer()
        {
            // Camera.main.transform.SetParent(transform);
            // Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            
            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);
        }

        private void OnCollisionStay()
        {
            isGrounded = true;
        }

        private void OnCollisionLeave()
        {
            isGrounded = false;
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                // make non-local players run this
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveX, 0, moveZ);
            movement.Normalize();

            if (movement != Vector3.zero)
            {
                Quaternion toRotations = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotations, 3f);
            }

            rb.velocity = new Vector3 (moveX * 3f, rb.velocity.y, moveZ * 3f);
            if (Input.GetKey(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
                isGrounded = false;
            }

            if (rb.velocity.magnitude > 0.2f)
            {
                networkAnim.SetTrigger("Walking");
            }
        }
    }
}