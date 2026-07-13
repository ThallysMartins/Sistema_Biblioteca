using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema_Biblioteca
{
    public class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int AnoLancamento { get; set; }
    }
}
