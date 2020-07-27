using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Gerador_de_Updates.Utils
{
    public class Arquivo
    {
        public Arquivo(string nome, string hash)
        {
            Nome = nome;
            Hash = hash;
        }
        public Arquivo()
        {

        }
        public string Nome { get; set; }
        public string Hash { get; set; }
    }
}
