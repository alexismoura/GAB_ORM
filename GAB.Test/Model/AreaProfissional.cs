using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAB;

namespace GAB.Test.Model
{
    [PersistenceClass("AreaProfissional")]
    public class AreaProfissional
    {
        [PersistenceProperty("AreaProfissionalCodigo", PersistenceParameterType.IdentityKey)]
        public int AreaProfissionalCodigo { get; set; }

        [PersistenceProperty("AreaProfissionalSegmento")]
        public int AreaProfissionalSegmento { get; set; }

        [PersistenceProperty("AreaProfissionalDescricao")]
        public string AreaProfissionalDescricao { get; set; }
    }
}
