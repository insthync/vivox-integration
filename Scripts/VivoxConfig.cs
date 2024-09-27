using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.UnityVivoxIntegration
{
    public class VivoxConfig : MonoBehaviour
    {
        [SerializeField]
        protected string _server = "";
        public virtual string Server => _server;

        [SerializeField]
        protected string _domain = "";
        public virtual string Domain => _domain;

        [SerializeField]
        protected string _issuer = "";
        public virtual string Issuer => _issuer;

        [SerializeField]
        protected string _key = "";
        public virtual string Key => _key;

        public virtual async Task LoadClient()
        {
            await Task.Yield();
        }

        public virtual async Task LoadServer()
        {
            await Task.Yield();
        }
    }
}
