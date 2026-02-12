using Dapper.Contrib.Extensions;

namespace TFCiclo.Data.Models
{
    [Table("roles")]
    public class roles
    {
        #region Métodos privados
        private int _id = 0;
        private string _name = string.Empty;
        private string _description = string.Empty;
        #endregion

        #region Métodos públicos
        [Key]
        public int id { get => _id; set => _id = value; }
        public string name { get => _name; set => _name = value; }
        public string description { get => _description; set => _description = value; }
        #endregion
    }
}
