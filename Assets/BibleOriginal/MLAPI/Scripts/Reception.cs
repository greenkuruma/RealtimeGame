using UnityEngine;
using TMPro;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using System.Collections;

namespace RGame.MLAPI
{
    /// <summary>
    /// host起動、Cient起動を担う
    /// </summary>
    public class Reception : MonoBehaviour
    {
        const string hostIPPrefsKey = "hostIP";
        const string portNumberPrefsKey = "portNumber";

        const string defaultHostIP = "127.0.0.1";
        const int defaultPort = 7777;

        [SerializeField] GameObject receptionCanvas = default;
        [SerializeField] GameObject mainCanvas = default;
        [SerializeField] TMP_Text myIP = default;
        [SerializeField] TMP_Text usePort = default;
        [SerializeField] TMP_InputField hostIP = default;
        [SerializeField] TMP_InputField portNumber = default;

        void Start ()
        {
            myIP.text = $"my IP : {GetLocalIP ()}";
            hostIP.text = PlayerPrefs.GetString (hostIPPrefsKey, defaultHostIP);
            portNumber.text = PlayerPrefs.GetInt (portNumberPrefsKey, defaultPort).ToString ();
        }

        public void OnHost ()
        {
            Debug.Log ("Start Host!!");

            ApplySettings ();
            NetworkManager.Singleton.StartHost ();

            StartGame ();
        }

        public void OnClient ()
        {
            Debug.Log ("Start Client!!");

            ApplySettings ();
            NetworkManager.Singleton.StartClient ();

            StartGame ();
        }

        void StartGame ()
        {
            receptionCanvas.SetActive (false);
            mainCanvas.SetActive (true);
        }

        void ApplySettings ()
        {
            var port = int.Parse (portNumber.text);

            var transport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.ConnectAddress = hostIP.text;
            transport.ConnectPort = port;
            transport.ServerListenPort = port;

            usePort.text = $"port : {port}";

            SavePlayerPrefs ();
        }

        // localのIPアドレスを調べて返します
        public static string GetLocalIP ()
        {
            string ipaddress = "";
            IPHostEntry ipentry = Dns.GetHostEntry (Dns.GetHostName ());

            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipaddress = ip.ToString ();
                    break;
                }
            }
            return ipaddress;
        }

        void SavePlayerPrefs ()
        {
            var port = int.Parse (portNumber.text);

            PlayerPrefs.SetString (hostIPPrefsKey, hostIP.text);
            PlayerPrefs.SetInt (portNumberPrefsKey, port);

            PlayerPrefs.Save ();
        }

        public void OnExitGame ()
        {
            StartCoroutine (ExitGame ());
        }

        IEnumerator ExitGame ()
        {
            // ExitGame呼び出しをyieldでゆるく同期
            // Clientを先にStop
            if (NetworkManager.Singleton.IsHost)
            {
                yield return new WaitForSeconds (5f);
                NetworkManager.Singleton.StopHost ();
            }
            else
            {
                yield return new WaitForSeconds (2f);
                NetworkManager.Singleton.StopClient ();
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene ("MLAPIMain");
        }
    }
}