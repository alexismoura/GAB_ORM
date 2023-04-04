using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GAB.Test.DAO;
using GAB.Test.Model;

namespace GAB.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var area = new AreaProfissional
            {
                AreaProfissionalCodigo = 0,
                AreaProfissionalDescricao = "Teste ",
                AreaProfissionalSegmento = 10
            };

            new AreaProfissionalDAO().Insert(area);
            var result = new AreaProfissionalDAO().List();
        }
    }
}
